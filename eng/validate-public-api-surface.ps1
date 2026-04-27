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
$resolvedDependencyCache = @{}

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

function Resolve-AssemblyPath {
  param([string]$AssemblyName)

  $assemblyPath = Join-Path $RepoRoot "src/$AssemblyName/bin/$Configuration/$Framework/$AssemblyName.dll"
  if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Public API assembly is missing: $assemblyPath. Build publishable packages before running this validation."
  }

  return [System.IO.Path]::GetFullPath($assemblyPath)
}

function Select-MatchingAssemblyFile {
  param($Candidates, $RequestedAssemblyName)

  foreach ($candidateFile in $Candidates) {
    try {
      $candidateName = [System.Reflection.AssemblyName]::GetAssemblyName($candidateFile.FullName)
      if ($candidateName.Name -eq $RequestedAssemblyName.Name -and $candidateName.Version -eq $RequestedAssemblyName.Version) {
        return $candidateFile
      }
    }
    catch {
      continue
    }
  }

  return $null
}

function New-PublicApiAssemblyResolver {
  param([string[]]$AssemblyDirectories)

  return {
    param($context, $assemblyName)

    foreach ($directory in $AssemblyDirectories) {
      $candidate = Join-Path $directory "$($assemblyName.Name).dll"
      if (Test-Path -LiteralPath $candidate) {
        return $context.LoadFromAssemblyPath([System.IO.Path]::GetFullPath($candidate))
      }
    }

    foreach ($directory in $dotnetAssemblyDirectories) {
      $candidate = Join-Path $directory "$($assemblyName.Name).dll"
      if (Test-Path -LiteralPath $candidate) {
        try {
          $candidateName = [System.Reflection.AssemblyName]::GetAssemblyName($candidate)
          if ($candidateName.Name -eq $assemblyName.Name -and $candidateName.Version -eq $assemblyName.Version) {
            return $context.LoadFromAssemblyPath([System.IO.Path]::GetFullPath($candidate))
          }
        }
        catch {
          continue
        }
      }
    }

    if ($resolvedDependencyCache.ContainsKey($assemblyName.Name)) {
      return $context.LoadFromAssemblyPath($resolvedDependencyCache[$assemblyName.Name])
    }

    if (Test-Path -LiteralPath $fallbackPackageCache) {
      $packageDirectory = Join-Path $fallbackPackageCache $assemblyName.Name.ToLowerInvariant()
      if (Test-Path -LiteralPath $packageDirectory) {
        $candidate = Select-MatchingAssemblyFile `
          -Candidates (Get-ChildItem -LiteralPath $packageDirectory -Filter "$($assemblyName.Name).dll" -Recurse -File) `
          -RequestedAssemblyName $assemblyName
        if ($candidate) {
          $resolvedDependencyCache[$assemblyName.Name] = $candidate.FullName
          return $context.LoadFromAssemblyPath($candidate.FullName)
        }
      }

      $candidate = Select-MatchingAssemblyFile `
        -Candidates (Get-ChildItem -LiteralPath $fallbackPackageCache -Filter "$($assemblyName.Name).dll" -Recurse -File) `
        -RequestedAssemblyName $assemblyName
      if ($candidate) {
        $resolvedDependencyCache[$assemblyName.Name] = $candidate.FullName
        return $context.LoadFromAssemblyPath($candidate.FullName)
      }
    }

    return $null
  }.GetNewClosure()
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
  $assemblyDirectories = $assemblyPaths | ForEach-Object { Split-Path -Parent $_ } | Select-Object -Unique
  $resolver = New-PublicApiAssemblyResolver -AssemblyDirectories $assemblyDirectories

  [System.Runtime.Loader.AssemblyLoadContext]::Default.add_Resolving($resolver)

  try {
    $lines = [System.Collections.Generic.List[string]]::new()
    $bindingFlags = [System.Reflection.BindingFlags]'Public,Instance,Static,DeclaredOnly'

    foreach ($assemblyPath in $assemblyPaths) {
      $assembly = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromAssemblyPath($assemblyPath)
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
    [System.Runtime.Loader.AssemblyLoadContext]::Default.remove_Resolving($resolver)
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

  $editorAssemblyPath = Resolve-AssemblyPath -AssemblyName 'AsterGraph.Editor'
  $resolver = New-PublicApiAssemblyResolver -AssemblyDirectories @((Split-Path -Parent $editorAssemblyPath))
  [System.Runtime.Loader.AssemblyLoadContext]::Default.add_Resolving($resolver)
  try {
    $editorAssembly = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromAssemblyPath($editorAssemblyPath)
    $queriesType = $editorAssembly.GetType('AsterGraph.Editor.Runtime.IGraphEditorQueries', $true)
    $compatibilityMethod = $queriesType.GetMethod('GetCompatibleTargets', [Type[]]@([string], [string]))
    $compatibilityMessage = $compatibilityMethod.GetCustomAttributes([ObsoleteAttribute], $false)[0].Message
    if (-not $compatibilityMessage.Contains('Compatibility-only shim', [System.StringComparison]::Ordinal) -or
        -not $compatibilityMessage.Contains('GetCompatiblePortTargets', [System.StringComparison]::Ordinal)) {
      throw 'IGraphEditorQueries.GetCompatibleTargets must warn as compatibility-only and name GetCompatiblePortTargets as the replacement.'
    }

    $targetType = $editorAssembly.GetType('AsterGraph.Editor.Menus.CompatiblePortTarget', $true)
    $targetMessage = $targetType.GetCustomAttributes([ObsoleteAttribute], $false)[0].Message
    if (-not $targetMessage.Contains('Retained compatibility shim', [System.StringComparison]::Ordinal) -or
        -not $targetMessage.Contains('GraphEditorCompatiblePortTargetSnapshot', [System.StringComparison]::Ordinal)) {
      throw 'CompatiblePortTarget must warn as a retained compatibility shim and name GraphEditorCompatiblePortTargetSnapshot as the replacement.'
    }
  }
  finally {
    [System.Runtime.Loader.AssemblyLoadContext]::Default.remove_Resolving($resolver)
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
