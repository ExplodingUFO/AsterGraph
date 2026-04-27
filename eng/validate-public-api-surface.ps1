[CmdletBinding()]
param(
  [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
  [string]$BaselinePath,
  [string]$ProofPath = '',
  [ValidateSet('Debug', 'Release')]
  [string]$Configuration = 'Release',
  [string]$Framework = 'net9.0',
  [switch]$UpdateBaseline
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($BaselinePath)) {
  $BaselinePath = Join-Path $PSScriptRoot 'public-api-baseline.txt'
}

$publishableAssemblies = @(
  'AsterGraph.Abstractions',
  'AsterGraph.Core',
  'AsterGraph.Editor',
  'AsterGraph.Avalonia'
)

$userHome = if ([string]::IsNullOrWhiteSpace($env:USERPROFILE)) { $env:HOME } else { $env:USERPROFILE }
$fallbackPackageCache = Join-Path $userHome '.nuget/packages'

$dotnetAssemblyDirectories = @()
$dotnetRoots = @()
if (-not [string]::IsNullOrWhiteSpace($env:DOTNET_ROOT)) {
  $dotnetRoots += $env:DOTNET_ROOT
}

$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
if ($dotnetCommand -and -not [string]::IsNullOrWhiteSpace($dotnetCommand.Source)) {
  $dotnetRoots += (Split-Path -Parent $dotnetCommand.Source)
}

foreach ($dotnetRoot in ($dotnetRoots | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)) {
  $refRoot = Join-Path $dotnetRoot "packs/Microsoft.NETCore.App.Ref"
  if (Test-Path -LiteralPath $refRoot) {
    $dotnetAssemblyDirectories += Get-ChildItem -LiteralPath $refRoot -Directory |
      ForEach-Object { Join-Path $_.FullName "ref/$Framework" } |
      Where-Object { Test-Path -LiteralPath $_ }
  }

  $sharedRoot = Join-Path $dotnetRoot "shared/Microsoft.NETCore.App"
  if (Test-Path -LiteralPath $sharedRoot) {
    $dotnetAssemblyDirectories += Get-ChildItem -LiteralPath $sharedRoot -Directory |
      ForEach-Object { $_.FullName }
  }
}

$dotnetAssemblyDirectories = @($dotnetAssemblyDirectories | Select-Object -Unique)
$metadataLoadContextImported = $false

function Resolve-AssemblyPath {
  param([string]$AssemblyName)

  $assemblyPath = Join-Path $RepoRoot "src/$AssemblyName/bin/$Configuration/$Framework/$AssemblyName.dll"
  if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Public API assembly is missing: $assemblyPath. Build publishable packages before running this validation."
  }

  return [System.IO.Path]::GetFullPath($assemblyPath)
}

function Get-PublicApiAssetAssemblyPaths {
  foreach ($assemblyName in $publishableAssemblies) {
    $assetsPath = Join-Path $RepoRoot "src/$assemblyName/obj/project.assets.json"
    if (-not (Test-Path -LiteralPath $assetsPath)) {
      continue
    }

    $assets = Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json
    $targetProperty = $assets.targets.PSObject.Properties |
      Where-Object { $_.Name -eq $Framework } |
      Select-Object -First 1
    if (-not $targetProperty) {
      continue
    }

    foreach ($packageProperty in $targetProperty.Value.PSObject.Properties) {
      $separatorIndex = $packageProperty.Name.LastIndexOf('/')
      if ($separatorIndex -le 0) {
        continue
      }

      $packageId = $packageProperty.Name.Substring(0, $separatorIndex)
      $packageVersion = $packageProperty.Name.Substring($separatorIndex + 1)
      $packageRoot = Join-Path (Join-Path $fallbackPackageCache $packageId.ToLowerInvariant()) $packageVersion.ToLowerInvariant()
      if (-not (Test-Path -LiteralPath $packageRoot)) {
        continue
      }

      foreach ($assetGroupName in @('compile', 'runtime')) {
        $assetGroupProperty = $packageProperty.Value.PSObject.Properties[$assetGroupName]
        if (-not $assetGroupProperty) {
          continue
        }

        foreach ($assetProperty in $assetGroupProperty.Value.PSObject.Properties) {
          if ($assetProperty.Name -eq '_._' -or -not $assetProperty.Name.EndsWith('.dll', [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
          }

          $assetPath = Join-Path $packageRoot $assetProperty.Name
          if (Test-Path -LiteralPath $assetPath) {
            [System.IO.Path]::GetFullPath($assetPath)
          }
        }
      }
    }
  }
}

function Import-MetadataLoadContextAssembly {
  $loadedMetadataAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -eq 'System.Reflection.MetadataLoadContext' } |
    Select-Object -First 1
  if ($script:metadataLoadContextImported -or $loadedMetadataAssembly) {
    $script:metadataLoadContextImported = $true
    return
  }

  $candidatePaths = @()
  foreach ($dotnetRoot in ($dotnetRoots | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)) {
    $sdkRoot = Join-Path $dotnetRoot 'sdk'
    if (-not (Test-Path -LiteralPath $sdkRoot)) {
      continue
    }

    $candidatePaths += Get-ChildItem -LiteralPath $sdkRoot -Directory |
      Sort-Object Name -Descending |
      ForEach-Object { Join-Path $_.FullName 'System.Reflection.MetadataLoadContext.dll' } |
      Where-Object { Test-Path -LiteralPath $_ }
  }

  foreach ($candidatePath in ($candidatePaths | Select-Object -Unique)) {
    try {
      Add-Type -Path $candidatePath
      $script:metadataLoadContextImported = $true
      return
    }
    catch {
      continue
    }
  }

  throw 'System.Reflection.MetadataLoadContext.dll could not be loaded from the installed .NET SDK. Install the .NET SDK before running public API validation.'
}

function New-PublicApiMetadataContext {
  param([string[]]$AssemblyPaths)

  Import-MetadataLoadContextAssembly

  $metadataAssemblyPaths = [System.Collections.Generic.List[string]]::new()
  foreach ($assemblyPath in $AssemblyPaths) {
    $metadataAssemblyPaths.Add([System.IO.Path]::GetFullPath($assemblyPath))
  }

  foreach ($assetAssemblyPath in Get-PublicApiAssetAssemblyPaths) {
    $metadataAssemblyPaths.Add($assetAssemblyPath)
  }

  $metadataDirectories = ($AssemblyPaths | ForEach-Object { Split-Path -Parent $_ }) + $dotnetAssemblyDirectories |
    Select-Object -Unique
  foreach ($directory in $metadataDirectories) {
    if (-not (Test-Path -LiteralPath $directory)) {
      continue
    }

    foreach ($assemblyFile in Get-ChildItem -LiteralPath $directory -Filter '*.dll' -File) {
      $metadataAssemblyPaths.Add($assemblyFile.FullName)
    }
  }

  $uniqueAssemblyPaths = [System.Collections.Generic.List[string]]::new()
  $seenAssemblyIdentities = @{}
  foreach ($path in ($metadataAssemblyPaths | Select-Object -Unique)) {
    try {
      $assemblyName = [System.Reflection.AssemblyName]::GetAssemblyName($path)
      $publicKeyToken = [System.BitConverter]::ToString($assemblyName.GetPublicKeyToken())
      $assemblyIdentity = "$($assemblyName.Name)|$($assemblyName.Version)|$($assemblyName.CultureName)|$publicKeyToken"
    }
    catch {
      $assemblyIdentity = $path
    }

    if ($seenAssemblyIdentities.ContainsKey($assemblyIdentity)) {
      continue
    }

    $seenAssemblyIdentities[$assemblyIdentity] = $true
    $uniqueAssemblyPaths.Add($path)
  }

  $resolverType = [type]::GetType('System.Reflection.PathAssemblyResolver, System.Reflection.MetadataLoadContext', $true)
  $contextType = [type]::GetType('System.Reflection.MetadataLoadContext, System.Reflection.MetadataLoadContext', $true)
  $resolver = [Activator]::CreateInstance($resolverType, $uniqueAssemblyPaths)

  return [Activator]::CreateInstance($contextType, $resolver, 'System.Runtime')
}

function Get-ObsoleteMessage {
  param($Member)

  $obsoleteAttribute = $Member.CustomAttributes |
    Where-Object { $_.AttributeType.FullName -eq 'System.ObsoleteAttribute' } |
    Select-Object -First 1

  if (-not $obsoleteAttribute -or $obsoleteAttribute.ConstructorArguments.Count -lt 1) {
    throw "Expected ObsoleteAttribute on $($Member.Name)."
  }

  return [string]$obsoleteAttribute.ConstructorArguments[0].Value
}

function Format-TypeName {
  param([Type]$Type)

  if ($Type.get_IsGenericParameter()) {
    return $Type.Name
  }

  if ($Type.get_IsArray()) {
    return "$(Format-TypeName -Type $Type.GetElementType())[]"
  }

  if ($Type.get_IsByRef()) {
    return "$(Format-TypeName -Type $Type.GetElementType())&"
  }

  if ($Type.get_IsGenericType()) {
    $definitionName = $Type.GetGenericTypeDefinition().FullName
    if ([string]::IsNullOrWhiteSpace($definitionName)) {
      $definitionName = $Type.Name
    }

    $tickIndex = $definitionName.IndexOf('`')
    if ($tickIndex -ge 0) {
      $definitionName = $definitionName.Substring(0, $tickIndex)
    }

    $arguments = $Type.GetGenericArguments() | ForEach-Object { Format-TypeName -Type $_ }
    return "$definitionName<$($arguments -join ',')>"
  }

  if (-not [string]::IsNullOrWhiteSpace($Type.FullName)) {
    return $Type.FullName
  }

  return $Type.Name
}

function Get-TypeKind {
  param([Type]$Type)

  if ($Type.get_IsInterface()) {
    return 'interface'
  }

  if ($Type.get_IsEnum()) {
    return 'enum'
  }

  if ($Type.get_IsValueType()) {
    return 'struct'
  }

  if ($Type.get_IsAbstract() -and $Type.get_IsSealed()) {
    return 'static class'
  }

  if ($Type.get_IsAbstract()) {
    return 'abstract class'
  }

  return 'class'
}

function Format-Parameters {
  param([System.Reflection.ParameterInfo[]]$Parameters)

  $formatted = $Parameters | ForEach-Object {
    $prefix = if ($_.IsOut) { 'out ' } elseif ($_.ParameterType.get_IsByRef()) { 'ref ' } else { '' }
    "$prefix$(Format-TypeName -Type $_.ParameterType)"
  }

  return "($($formatted -join ','))"
}

function Get-PublicApiLines {
  $assemblyPaths = $publishableAssemblies | ForEach-Object { Resolve-AssemblyPath -AssemblyName $_ }
  $metadataContext = New-PublicApiMetadataContext -AssemblyPaths $assemblyPaths

  try {
    $lines = [System.Collections.Generic.List[string]]::new()
    $bindingFlags = [System.Reflection.BindingFlags]'Public,Instance,Static,DeclaredOnly'

    foreach ($assemblyPath in $assemblyPaths) {
      $assembly = $metadataContext.LoadFromAssemblyPath($assemblyPath)
      $assemblyName = $assembly.GetName().Name
      $lines.Add("A:$assemblyName")

      foreach ($type in ($assembly.GetExportedTypes() | Sort-Object FullName)) {
        if ($type.FullName -like '*<*') {
          continue
        }

        $typeName = (Format-TypeName -Type $type).Replace('+', '.')
        $lines.Add("T:$(Get-TypeKind -Type $type) $typeName")

        foreach ($constructor in ($type.GetConstructors($bindingFlags) | Sort-Object { $_.ToString() })) {
          $lines.Add("C:$typeName$(Format-Parameters -Parameters $constructor.GetParameters())")
        }

        foreach ($property in ($type.GetProperties($bindingFlags) | Sort-Object Name)) {
          $accessors = @()
          if ($property.GetMethod -and $property.GetMethod.IsPublic) { $accessors += 'get' }
          if ($property.SetMethod -and $property.SetMethod.IsPublic) { $accessors += 'set' }
          $lines.Add("P:$typeName.$($property.Name):$(Format-TypeName -Type $property.PropertyType):$($accessors -join ',')")
        }

        foreach ($event in ($type.GetEvents($bindingFlags) | Sort-Object Name)) {
          $lines.Add("E:$typeName.$($event.Name):$(Format-TypeName -Type $event.EventHandlerType)")
        }

        foreach ($field in ($type.GetFields($bindingFlags) | Where-Object { -not $_.IsSpecialName } | Sort-Object Name)) {
          $lines.Add("F:$typeName.$($field.Name):$(Format-TypeName -Type $field.FieldType)")
        }

        foreach ($method in ($type.GetMethods($bindingFlags) | Where-Object { -not $_.IsSpecialName } | Sort-Object Name, { $_.ToString() })) {
          $lines.Add("M:$typeName.$($method.Name)$(Format-Parameters -Parameters $method.GetParameters()):$(Format-TypeName -Type $method.ReturnType)")
        }
      }
    }

    return $lines.ToArray() | Sort-Object
  }
  finally {
    $metadataContext.Dispose()
  }
}

function Assert-ContainsText {
  param(
    [string]$Path,
    [string[]]$ExpectedText
  )

  $contents = Get-Content -LiteralPath $Path -Raw
  foreach ($text in $ExpectedText) {
    if (-not $contents.Contains($text, [System.StringComparison]::Ordinal)) {
      throw "Required public API guidance is missing from ${Path}: $text"
    }
  }
}

function Assert-WarningGuidance {
  $englishInventory = Join-Path $RepoRoot 'docs/en/public-api-inventory.md'
  $chineseInventory = Join-Path $RepoRoot 'docs/zh-CN/public-api-inventory.md'
  $extensionContracts = Join-Path $RepoRoot 'docs/en/extension-contracts.md'

  Assert-ContainsText -Path $englishInventory -ExpectedText @(
    'Retained migration',
    'Compatibility-only',
    'GraphEditorViewModel',
    'IGraphEditorQueries.GetCompatibleTargets(...)',
    'CompatiblePortTarget',
    'GetCompatiblePortTargets(...)',
    'GraphEditorCompatiblePortTargetSnapshot',
    'Compatibility-only APIs must be marked obsolete when a canonical replacement exists.'
  )

  Assert-ContainsText -Path $chineseInventory -ExpectedText @(
    'Retained migration',
    'Compatibility-only',
    'GraphEditorViewModel',
    'IGraphEditorQueries.GetCompatibleTargets(...)',
    'CompatiblePortTarget',
    'GetCompatiblePortTargets(...)',
    'GraphEditorCompatiblePortTargetSnapshot',
    'compatibility-only API 必须标记 obsolete'
  )

  Assert-ContainsText -Path $extensionContracts -ExpectedText @(
    'GraphEditorViewModel',
    'IGraphEditorQueries.GetCompatibleTargets(...)',
    'CompatiblePortTarget',
    'Compatibility-only APIs',
    'replacement guidance',
    'GetCompatiblePortTargets(...)'
  )

  $assemblyPaths = $publishableAssemblies | ForEach-Object { Resolve-AssemblyPath -AssemblyName $_ }
  $metadataContext = New-PublicApiMetadataContext -AssemblyPaths $assemblyPaths
  try {
    $editorAssemblyPath = Resolve-AssemblyPath -AssemblyName 'AsterGraph.Editor'
    $editorAssembly = $metadataContext.LoadFromAssemblyPath($editorAssemblyPath)
    $queriesType = $editorAssembly.GetType('AsterGraph.Editor.Runtime.IGraphEditorQueries', $true)
    $compatibilityMethod = $queriesType.GetMethods() |
      Where-Object {
        $_.Name -eq 'GetCompatibleTargets' -and
        (($_.GetParameters() | ForEach-Object { $_.ParameterType.FullName }) -join ',') -eq 'System.String,System.String'
      } |
      Select-Object -First 1
    if (-not $compatibilityMethod) {
      throw 'IGraphEditorQueries.GetCompatibleTargets(string,string) was not found.'
    }

    $compatibilityMessage = Get-ObsoleteMessage -Member $compatibilityMethod
    if (-not $compatibilityMessage.Contains('Compatibility-only shim', [System.StringComparison]::Ordinal) -or
        -not $compatibilityMessage.Contains('GetCompatiblePortTargets', [System.StringComparison]::Ordinal)) {
      throw 'IGraphEditorQueries.GetCompatibleTargets must warn as compatibility-only and name GetCompatiblePortTargets as the replacement.'
    }

    $targetType = $editorAssembly.GetType('AsterGraph.Editor.Menus.CompatiblePortTarget', $true)
    $targetMessage = Get-ObsoleteMessage -Member $targetType
    if (-not $targetMessage.Contains('Retained compatibility shim', [System.StringComparison]::Ordinal) -or
        -not $targetMessage.Contains('GraphEditorCompatiblePortTargetSnapshot', [System.StringComparison]::Ordinal)) {
      throw 'CompatiblePortTarget must warn as a retained compatibility shim and name GraphEditorCompatiblePortTargetSnapshot as the replacement.'
    }
  }
  finally {
    $metadataContext.Dispose()
  }
}

$currentLines = Get-PublicApiLines

if ($UpdateBaseline) {
  $baselineDirectory = Split-Path -Parent $BaselinePath
  if (-not [string]::IsNullOrWhiteSpace($baselineDirectory) -and -not (Test-Path -LiteralPath $baselineDirectory)) {
    New-Item -ItemType Directory -Path $baselineDirectory -Force | Out-Null
  }

  $currentLines | Set-Content -LiteralPath $BaselinePath
  Write-Host "PUBLIC_API_BASELINE_UPDATED:$($currentLines.Count):$BaselinePath"
  exit 0
}

if (-not (Test-Path -LiteralPath $BaselinePath)) {
  throw "Public API baseline is missing: $BaselinePath"
}

$baselineLines = Get-Content -LiteralPath $BaselinePath
$differences = Compare-Object -ReferenceObject $baselineLines -DifferenceObject $currentLines

if ($differences) {
  Write-Error "Public API surface drift detected. Update $BaselinePath intentionally and classify new host-facing symbols in docs/en/public-api-inventory.md."
  $differences |
    Sort-Object SideIndicator, InputObject |
    Select-Object -First 80 |
    ForEach-Object { Write-Error "$($_.SideIndicator) $($_.InputObject)" }
  exit 1
}

Assert-WarningGuidance

$successMarker = "PUBLIC_API_SURFACE_OK:$($currentLines.Count):$Framework"
if (-not [string]::IsNullOrWhiteSpace($ProofPath)) {
  $proofDirectory = Split-Path -Parent $ProofPath
  if (-not [string]::IsNullOrWhiteSpace($proofDirectory) -and -not (Test-Path -LiteralPath $proofDirectory)) {
    New-Item -ItemType Directory -Path $proofDirectory -Force | Out-Null
  }

  @(
    $successMarker,
    'PUBLIC_API_GUIDANCE_OK:True'
  ) | Set-Content -LiteralPath $ProofPath
}

Write-Host $successMarker
