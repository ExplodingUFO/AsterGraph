[CmdletBinding()]
param(
  [ValidateSet('restore', 'build', 'test', 'maintenance', 'contract', 'release', 'hygiene', 'all')]
  [string]$Lane = 'all',

  [ValidateSet('all', 'net8.0', 'net9.0')]
  [string]$Framework = 'all',

  [ValidateSet('Debug', 'Release')]
  [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$artifactsRoot = Join-Path $repoRoot 'artifacts'
$packagesOutputPath = Join-Path $artifactsRoot 'packages'
$proofArtifactsRoot = Join-Path $artifactsRoot 'proof'
$coverageResultsRoot = Join-Path $artifactsRoot 'test-results\release'
$coverageOutputPath = Join-Path $artifactsRoot 'coverage\release-summary.json'
$coverageMarkerOutputPath = Join-Path $proofArtifactsRoot 'coverage-report.txt'
$prereleaseNotesOutputPath = Join-Path $proofArtifactsRoot 'prerelease-notes.md'
$publicRepoHygieneProofPath = Join-Path $proofArtifactsRoot 'public-repo-hygiene.txt'
$hostSampleProjectProofPath = Join-Path $proofArtifactsRoot 'hostsample-project.txt'
$consumerSampleProofPath = Join-Path $proofArtifactsRoot 'consumer-sample.txt'
$hostSamplePackedProofPath = Join-Path $proofArtifactsRoot 'hostsample-packed.txt'
$hostSampleNet10PackedProofPath = Join-Path $proofArtifactsRoot 'hostsample-net10-packed.txt'
$packageSmokeProofPath = Join-Path $proofArtifactsRoot 'package-smoke.txt'
$scaleSmokeProofPath = Join-Path $proofArtifactsRoot 'scale-smoke.txt'
$demoProofPath = Join-Path $proofArtifactsRoot 'demo-proof.txt'
$dotnetCliHome = Join-Path $repoRoot '.dotnet-cli-home'
$coverageRunSettingsPath = Join-Path $repoRoot 'tests/coverage.runsettings'
$coverageReportScriptPath = Join-Path $repoRoot 'eng/coverage-report.ps1'
$prereleaseNotesScriptPath = Join-Path $repoRoot 'eng/write-prerelease-notes.ps1'
$defaultVsTestConnectionTimeoutSeconds = 180
$hostSampleProject = 'tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj'
$consumerSampleProject = 'tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj'
$helloWorldProject = 'tools/AsterGraph.HelloWorld/AsterGraph.HelloWorld.csproj'
$helloWorldAvaloniaProject = 'tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj'
$helloWorldWpfProject = 'tools/AsterGraph.HelloWorld.Wpf/AsterGraph.HelloWorld.Wpf.csproj'
$asterGraphWpfProject = 'src/AsterGraph.Wpf/AsterGraph.Wpf.csproj'
$asterGraphWpfTestsProject = 'tests/AsterGraph.Wpf.Tests/AsterGraph.Wpf.Tests.csproj'
$starterAvaloniaProject = 'tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj'
$packageSmokeProject = 'tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj'
$scaleSmokeProject = 'tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj'
$demoProject = 'src/AsterGraph.Demo/AsterGraph.Demo.csproj'
$editorTestsProject = 'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj'
$consumerSampleTestsProject = 'tests/AsterGraph.ConsumerSample.Tests/AsterGraph.ConsumerSample.Tests.csproj'
$helloWorldTestsProject = 'tests/AsterGraph.HelloWorld.Tests/AsterGraph.HelloWorld.Tests.csproj'
$scaleSmokeTestsProject = 'tests/AsterGraph.ScaleSmoke.Tests/AsterGraph.ScaleSmoke.Tests.csproj'
$userHome = if ([string]::IsNullOrWhiteSpace($env:USERPROFILE)) { $env:HOME } else { $env:USERPROFILE }
$fallbackPackageCache = Join-Path $userHome '.nuget/packages'
$isWindowsHost = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
$helloWorldWpfFramework = 'net8.0-windows'
$asterGraphWpfFramework = 'net9.0-windows'
$asterGraphWpfTestsFramework = 'net9.0-windows7.0'
$singleProcessBuildArguments = @(
  '-m:1'
)
$buildStabilityProperties = @(
  '/p:BuildInParallel=false',
  '/p:UseSharedCompilation=false',
  '/p:UsedAvaloniaProducts='
)

$publishableProjects = @(
  'src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj',
  'src/AsterGraph.Core/AsterGraph.Core.csproj',
  'src/AsterGraph.Editor/AsterGraph.Editor.csproj',
  'src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj'
)

$frameworkBuildProjects = @{
  'net8.0' = @(
    $helloWorldProject,
    $helloWorldAvaloniaProject,
    $starterAvaloniaProject,
    $hostSampleProject,
    $consumerSampleProject,
    $packageSmokeProject,
    $scaleSmokeProject
  )
  'net9.0' = @(
    'tests/AsterGraph.TestPlugins/AsterGraph.TestPlugins.csproj',
    'src/AsterGraph.Demo/AsterGraph.Demo.csproj'
  )
}

$frameworkTestProjects = @{
  'net8.0' = @(
    'tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj',
    $helloWorldTestsProject,
    $consumerSampleTestsProject,
    $scaleSmokeTestsProject
  )
  'net9.0' = @(
    'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj',
    'tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj'
  )
}

$maintenanceTestFilter = @(
  'FullyQualifiedName~GraphEditorMutationCompatibilityTests',
  'FullyQualifiedName~GraphEditorFacadeMutationParityTests',
  'FullyQualifiedName~GraphEditorSessionTests',
  'FullyQualifiedName~GraphEditorServiceSeamsTests',
  'FullyQualifiedName~GraphEditorHistorySemanticTests',
  'FullyQualifiedName~GraphEditorHistoryInteractionTests',
  'FullyQualifiedName~GraphEditorSaveBoundaryTests',
  'FullyQualifiedName~GraphEditorMigrationCompatibilityTests',
  'FullyQualifiedName~GraphEditorFacadeRefactorTests',
  'FullyQualifiedName~GraphEditorKernelCommandRouterTests',
  'FullyQualifiedName~GraphEditorViewModelProjectionTests',
  'FullyQualifiedName~NodeCanvasStandaloneTests',
  'FullyQualifiedName~NodeCanvasNodeDragCoordinatorTests',
  'FullyQualifiedName~NodeCanvasPointerInteractionCoordinatorTests'
) -join '|'

$contractTestFilter = @(
  'FullyQualifiedName~GraphEditorSessionTests',
  'FullyQualifiedName~GraphEditorProofRingTests',
  'FullyQualifiedName~GraphEditorSurfaceCompositionTests',
  'FullyQualifiedName~GraphEditorServiceSeamsTests',
  'FullyQualifiedName~GraphEditorDiagnosticsContractsTests',
  'FullyQualifiedName~GraphEditorDiagnosticsInspectionTests',
  'FullyQualifiedName~GraphEditorDiagnosticsInstrumentationTests',
  'FullyQualifiedName~GraphEditorAutomationContractsTests',
  'FullyQualifiedName~GraphEditorAutomationExecutionTests',
  'FullyQualifiedName~GraphEditorPluginContractsTests',
  'FullyQualifiedName~GraphEditorPluginDiscoveryTests',
  'FullyQualifiedName~GraphEditorPluginInspectionContractsTests',
  'FullyQualifiedName~GraphEditorPluginLoadingTests',
  'FullyQualifiedName~GraphEditorPluginPackageStagingTests',
  'FullyQualifiedName~GraphEditorMigrationCompatibilityTests',
  'FullyQualifiedName~GraphEditorHistorySemanticTests',
  'FullyQualifiedName~GraphEditorHistoryInteractionTests',
  'FullyQualifiedName~GraphEditorSaveBoundaryTests'
) -join '|'

function Invoke-DotNet {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Arguments
  )

  Write-Host "==> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
  & dotnet @Arguments

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($Arguments -join ' ')"
  }
}

function Test-GitTrackedPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RelativePath
  )

  $null = & git -C $repoRoot ls-files --error-unmatch -- $RelativePath 2>$null
  $isTracked = $LASTEXITCODE -eq 0
  $global:LASTEXITCODE = 0
  return $isTracked
}

function Invoke-DotNetCapture {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Arguments,

    [Parameter(Mandatory = $true)]
    [string]$CapturePath,

    [switch]$Append
  )

  $captureDirectory = Split-Path -Parent $CapturePath
  if (-not [string]::IsNullOrWhiteSpace($captureDirectory)) {
    Ensure-Directory -Path $captureDirectory
  }

  Write-Host "==> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
  if ($Append) {
    & dotnet @Arguments 2>&1 | Tee-Object -FilePath $CapturePath -Append
  }
  else {
    & dotnet @Arguments 2>&1 | Tee-Object -FilePath $CapturePath
  }
  $exitCode = $LASTEXITCODE

  if ($exitCode -ne 0) {
    throw "dotnet command failed with exit code ${exitCode}: dotnet $($Arguments -join ' ')"
  }
}

function Reset-BuildServers {
  Write-Host "==> dotnet build-server shutdown" -ForegroundColor Cyan
  & dotnet build-server shutdown

  if ($LASTEXITCODE -ne 0) {
    throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet build-server shutdown"
  }
}

function Resolve-ProjectPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RelativePath
  )

  return Join-Path $repoRoot $RelativePath
}

function Ensure-Directory {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path
  )

  if (-not (Test-Path -LiteralPath $Path)) {
    New-Item -ItemType Directory -Path $Path -Force | Out-Null
  }
}

function Reset-Directory {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Path
  )

  $resolvedPath = [System.IO.Path]::GetFullPath($Path)
  $resolvedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot)

  if (-not $resolvedPath.StartsWith($resolvedRepoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to clear path outside the repo root: $resolvedPath"
  }

  if (Test-Path -LiteralPath $resolvedPath) {
    Remove-Item -LiteralPath $resolvedPath -Recurse -Force
  }

  New-Item -ItemType Directory -Path $resolvedPath -Force | Out-Null
}

function Initialize-RepoToolingEnvironment {
  Ensure-Directory -Path $artifactsRoot
  Ensure-Directory -Path $proofArtifactsRoot
  Ensure-Directory -Path $dotnetCliHome

  $env:DOTNET_CLI_HOME = $dotnetCliHome
  $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
  $env:DOTNET_NOLOGO = '1'
  $env:MSBuildEnableWorkloadResolver = 'false'

  if ([string]::IsNullOrWhiteSpace($env:VSTEST_CONNECTION_TIMEOUT)) {
    $env:VSTEST_CONNECTION_TIMEOUT = $defaultVsTestConnectionTimeoutSeconds.ToString([System.Globalization.CultureInfo]::InvariantCulture)
  }

  Write-Host "Using VSTEST_CONNECTION_TIMEOUT=$($env:VSTEST_CONNECTION_TIMEOUT)" -ForegroundColor DarkGray
}

function Get-Frameworks {
  if ($Framework -eq 'all') {
    return @('net8.0', 'net9.0')
  }

  return @($Framework)
}

function Get-UniqueProjects {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Projects
  )

  $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
  $orderedProjects = [System.Collections.Generic.List[string]]::new()

  foreach ($project in $Projects) {
    if ($seen.Add($project)) {
      $orderedProjects.Add($project)
    }
  }

  return $orderedProjects.ToArray()
}

function Get-DefaultRestoreProjects {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Frameworks
  )

  $projects = [System.Collections.Generic.List[string]]::new()

  foreach ($project in $publishableProjects) {
    $projects.Add($project)
  }

  foreach ($targetFramework in $Frameworks) {
    foreach ($project in $frameworkBuildProjects[$targetFramework]) {
      $projects.Add($project)
    }

    foreach ($project in $frameworkTestProjects[$targetFramework]) {
      $projects.Add($project)
    }
  }

  if ($isWindowsHost) {
    if (Test-Path -LiteralPath (Resolve-ProjectPath -RelativePath $asterGraphWpfProject)) {
      $projects.Add($asterGraphWpfProject)
    }

    if (Test-Path -LiteralPath (Resolve-ProjectPath -RelativePath $asterGraphWpfTestsProject)) {
      $projects.Add($asterGraphWpfTestsProject)
    }

    $wpfBootstrapProjectPath = Resolve-ProjectPath -RelativePath $helloWorldWpfProject
    if (Test-Path -LiteralPath $wpfBootstrapProjectPath) {
      $projects.Add($helloWorldWpfProject)
    }
  }

  return Get-UniqueProjects -Projects $projects.ToArray()
}

function Get-ReleaseValidationTestProjects {
  $projects = [System.Collections.Generic.List[string]]::new()

  foreach ($targetFramework in @('net8.0', 'net9.0')) {
    foreach ($project in $frameworkTestProjects[$targetFramework]) {
      $projects.Add($project)
    }
  }

  return Get-UniqueProjects -Projects $projects.ToArray()
}

function Get-RestoreArguments {
  param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [hashtable]$Properties = @{}
  )

  $arguments = @(
    'restore',
    $ProjectPath,
    '--nologo',
    '-v',
    'minimal',
    '-m:1',
    '/p:RestoreUseStaticGraphEvaluation=false',
    '/p:RestoreDisableParallel=true',
    '/p:NuGetAudit=false',
    '--ignore-failed-sources'
  )

  if (Test-Path -LiteralPath $fallbackPackageCache) {
    $arguments += "/p:RestoreFallbackFolders=$fallbackPackageCache"
  }

  foreach ($propertyName in ($Properties.Keys | Sort-Object)) {
    $arguments += "/p:${propertyName}=$($Properties[$propertyName])"
  }

  return $arguments
}

function Invoke-RestoreProjects {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Projects,

    [hashtable]$Properties = @{}
  )

  Write-Host ''
  Write-Host '### Restore' -ForegroundColor Yellow

  foreach ($project in (Get-UniqueProjects -Projects $Projects)) {
    $projectPath = Resolve-ProjectPath -RelativePath $project
    Invoke-DotNet -Arguments (Get-RestoreArguments -ProjectPath $projectPath -Properties $Properties)
  }
}

function Invoke-Build {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Frameworks
  )

  foreach ($targetFramework in $Frameworks) {
    Write-Host ''
    Write-Host "### Build packages for $targetFramework" -ForegroundColor Yellow

    foreach ($project in $publishableProjects) {
      Invoke-DotNet -Arguments (@(
        'build',
        (Resolve-ProjectPath -RelativePath $project),
        '-c',
        $Configuration,
        '-f',
        $targetFramework,
        '--no-restore',
        '--nologo',
        '-v',
        'minimal'
      ) + $singleProcessBuildArguments + $buildStabilityProperties)
    }
  }

  Invoke-WindowsHelloWorldWpfSlice
}

function Invoke-TestAndTooling {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Frameworks
  )

  foreach ($targetFramework in $Frameworks) {
    Write-Host ''
    Write-Host "### Validate lane for $targetFramework" -ForegroundColor Yellow

    foreach ($project in $frameworkBuildProjects[$targetFramework]) {
      Invoke-DotNet -Arguments (@(
        'build',
        (Resolve-ProjectPath -RelativePath $project),
        '-c',
        $Configuration,
        '--no-restore',
        '--nologo',
        '-v',
        'minimal'
      ) + $singleProcessBuildArguments + $buildStabilityProperties)
    }

    foreach ($project in $frameworkTestProjects[$targetFramework]) {
      Invoke-DotNet -Arguments (@(
        'test',
        (Resolve-ProjectPath -RelativePath $project),
        '-c',
        $Configuration,
        '--no-restore',
        '--nologo',
        '-v',
        'minimal'
      ) + $singleProcessBuildArguments + $buildStabilityProperties)
    }
  }

  Invoke-WindowsHelloWorldWpfSlice
}

function Invoke-WindowsHelloWorldWpfSlice {
  if (-not $isWindowsHost) {
    return
  }

  $asterGraphWpfProjectPath = Resolve-ProjectPath -RelativePath $asterGraphWpfProject
  if (Test-Path -LiteralPath $asterGraphWpfProjectPath) {
    Write-Host ''
    Write-Host '### Validate WPF library package surface (Windows)' -ForegroundColor Yellow

    Invoke-DotNet -Arguments (@(
      'build',
      $asterGraphWpfProjectPath,
      '-c',
      $Configuration,
      '--framework',
      $asterGraphWpfFramework,
      '--no-restore',
      '--nologo',
      '-v',
      'minimal'
    ) + $singleProcessBuildArguments + $buildStabilityProperties)
  }

  Write-Host ''
  Write-Host '### Validate WPF bootstrap sample (Windows)' -ForegroundColor Yellow
  $projectPath = Resolve-ProjectPath -RelativePath $helloWorldWpfProject

  if (Test-Path -LiteralPath $projectPath) {
    Invoke-DotNet -Arguments (@(
      'build',
      $projectPath,
      '-c',
      $Configuration,
      '--framework',
      $helloWorldWpfFramework,
      '--no-restore',
      '--nologo',
      '-v',
      'minimal'
    ) + $singleProcessBuildArguments + $buildStabilityProperties)
  }

  Write-Host ''
  Write-Host '### Validate WPF tests (Windows)' -ForegroundColor Yellow
  $testsProjectPath = Resolve-ProjectPath -RelativePath $asterGraphWpfTestsProject

  if (Test-Path -LiteralPath $testsProjectPath) {
    Invoke-DotNet -Arguments (@(
      'test',
      $testsProjectPath,
      '-c',
      $Configuration,
      '--framework',
      $asterGraphWpfTestsFramework,
      '--no-restore',
      '--nologo',
      '--no-build',
      '-v',
      'minimal'
    ) + $singleProcessBuildArguments + $buildStabilityProperties)
  }
}

function Invoke-Packages {
  Reset-Directory -Path $packagesOutputPath

  Write-Host ''
  Write-Host '### Pack publishable packages and run package validation' -ForegroundColor Yellow
  Reset-BuildServers

  foreach ($project in $publishableProjects) {
    Invoke-DotNet -Arguments (@(
      'pack',
      (Resolve-ProjectPath -RelativePath $project),
      '-c',
      $Configuration,
      '--no-restore',
      '--nologo',
      '-v',
      'minimal',
      '--disable-build-servers',
      '-o',
      $packagesOutputPath
    ) + $singleProcessBuildArguments + $buildStabilityProperties)
  }
}

function Invoke-PackageSmoke {
  Write-Host ''
  Write-Host '### Run PackageSmoke against packed packages' -ForegroundColor Yellow

  Invoke-RestoreProjects -Projects @($packageSmokeProject) -Properties @{ UsePackedAsterGraphPackages = 'true' }

  Invoke-DotNet -Arguments (@(
    'build',
    (Resolve-ProjectPath -RelativePath $packageSmokeProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-restore',
    '--nologo',
    '-v',
    'minimal',
    '/p:UsePackedAsterGraphPackages=true'
  ) + $singleProcessBuildArguments + $buildStabilityProperties)

  Invoke-DotNetCapture -Arguments @(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $packageSmokeProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-build',
    '--no-restore',
    '--nologo',
    '/p:UsePackedAsterGraphPackages=true'
  ) -CapturePath $packageSmokeProofPath
}

function Assert-RepoPathExists {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RelativePath
  )

  $path = Resolve-ProjectPath -RelativePath $RelativePath
  if (-not (Test-Path -LiteralPath $path)) {
    throw "Required public repo path is missing: $RelativePath"
  }
}

function Assert-RepoPathNotTracked {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RelativePath
  )

  if (Test-GitTrackedPath -RelativePath $RelativePath) {
    throw "Internal-only path is still tracked by git: $RelativePath"
  }
}

function Assert-GitIgnoreContains {
  param(
    [Parameter(Mandatory = $true)]
    [string]$ExpectedEntry
  )

  $gitIgnorePath = Join-Path $repoRoot '.gitignore'
  if (-not (Select-String -Path $gitIgnorePath -SimpleMatch -Pattern $ExpectedEntry -Quiet)) {
    throw "Expected .gitignore entry was not found: $ExpectedEntry"
  }
}

function Invoke-HostSample {
  param(
    [switch]$UsePackedPackages,

    [ValidateSet('net8.0', 'net10.0')]
    [string]$TargetFramework = 'net8.0'
  )

  if ($TargetFramework -eq 'net10.0' -and -not $UsePackedPackages) {
    throw '.NET 10 HostSample proof requires packed-package consumption.'
  }

  $propertyArguments = @()
  $restoreProperties = @{}
  $modeLabel = if ($TargetFramework -eq 'net10.0') { 'packed packages (.NET 10)' } else { 'project references' }

  if ($UsePackedPackages) {
    if ($TargetFramework -eq 'net8.0') {
      $modeLabel = 'packed packages'
    }

    $restoreProperties = @{ UsePackedAsterGraphPackages = 'true' }
    $propertyArguments += '/p:UsePackedAsterGraphPackages=true'
    if ($TargetFramework -eq 'net10.0') {
      $restoreProperties.EnableNet10ConsumerProof = 'true'
      $propertyArguments += '/p:EnableNet10ConsumerProof=true'
    }

    Invoke-RestoreProjects -Projects @($hostSampleProject) -Properties $restoreProperties
  }

  Write-Host ''
  Write-Host "### Run HostSample ($modeLabel)" -ForegroundColor Yellow

  $capturePath = switch ($TargetFramework) {
    'net10.0' { $hostSampleNet10PackedProofPath }
    default {
      if ($UsePackedPackages) { $hostSamplePackedProofPath } else { $hostSampleProjectProofPath }
    }
  }

  Invoke-DotNet -Arguments (@(
    'build',
    (Resolve-ProjectPath -RelativePath $hostSampleProject),
    '-c',
    $Configuration,
    '--framework',
    $TargetFramework,
    '--no-restore',
    '--nologo',
    '-v',
    'minimal'
  ) + $propertyArguments + $singleProcessBuildArguments + $buildStabilityProperties)

  Invoke-DotNetCapture -Arguments (@(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $hostSampleProject),
    '-c',
    $Configuration,
    '--framework',
    $TargetFramework,
    '--no-build',
    '--no-restore',
    '--nologo'
  ) + $propertyArguments) -CapturePath $capturePath

  if ($TargetFramework -eq 'net10.0') {
    Add-Content -LiteralPath $capturePath -Value 'HOST_SAMPLE_NET10_OK:True'
  }
}

function Invoke-ConsumerSampleProof {
  Write-Host ''
  Write-Host '### Run medium consumer sample proof' -ForegroundColor Yellow

  Invoke-DotNet -Arguments (@(
    'build',
    (Resolve-ProjectPath -RelativePath $consumerSampleProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-restore',
    '--nologo',
    '-v',
    'minimal'
  ) + $singleProcessBuildArguments + $buildStabilityProperties)

  Invoke-DotNetCapture -Arguments @(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $consumerSampleProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-build',
    '--no-restore',
    '--nologo',
    '--',
    '--proof'
  ) -CapturePath $consumerSampleProofPath
}

function Invoke-ScaleSmoke {
  Write-Host ''
  Write-Host '### Run ScaleSmoke readiness proof' -ForegroundColor Yellow

  Invoke-DotNet -Arguments (@(
    'build',
    (Resolve-ProjectPath -RelativePath $scaleSmokeProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-restore',
    '--nologo',
    '-v',
    'minimal'
  ) + $singleProcessBuildArguments + $buildStabilityProperties)

  Invoke-DotNetCapture -Arguments @(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $scaleSmokeProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-build',
    '--no-restore',
    '--nologo',
    '--',
    '--tier',
    'baseline'
  ) -CapturePath $scaleSmokeProofPath

  Invoke-DotNetCapture -Arguments @(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $scaleSmokeProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-build',
    '--no-restore',
    '--nologo',
    '--',
    '--tier',
    'large'
  ) -CapturePath $scaleSmokeProofPath -Append

  Invoke-DotNetCapture -Arguments @(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $scaleSmokeProject),
    '-c',
    $Configuration,
    '--framework',
    'net8.0',
    '--no-build',
    '--no-restore',
    '--nologo',
    '--',
    '--tier',
    'stress',
    '--samples',
    '3'
  ) -CapturePath $scaleSmokeProofPath -Append
}

function Invoke-DemoProof {
  Write-Host ''
  Write-Host '### Run Demo usability proof' -ForegroundColor Yellow

  Invoke-DotNet -Arguments (@(
    'build',
    (Resolve-ProjectPath -RelativePath $demoProject),
    '-c',
    $Configuration,
    '--framework',
    'net9.0',
    '--no-restore',
    '--nologo',
    '-v',
    'minimal'
  ) + $singleProcessBuildArguments + $buildStabilityProperties)

  Invoke-DotNetCapture -Arguments @(
    'run',
    '--project',
    (Resolve-ProjectPath -RelativePath $demoProject),
    '-c',
    $Configuration,
    '--framework',
    'net9.0',
    '--no-build',
    '--no-restore',
    '--nologo',
    '--',
    '--proof'
  ) -CapturePath $demoProofPath
}

function Invoke-CoverageValidation {
  Reset-Directory -Path $coverageResultsRoot
  Ensure-Directory -Path (Split-Path -Parent $coverageOutputPath)
  Ensure-Directory -Path $proofArtifactsRoot

  Write-Host ''
  Write-Host '### Collect coverage and validate publishable-surface reporting' -ForegroundColor Yellow

  foreach ($project in (Get-ReleaseValidationTestProjects)) {
    Invoke-DotNet -Arguments (@(
      'test',
      (Resolve-ProjectPath -RelativePath $project),
      '-c',
      $Configuration,
      '--no-restore',
      '--nologo',
      '-v',
      'minimal',
      '--settings',
      $coverageRunSettingsPath,
      '--results-directory',
      $coverageResultsRoot,
      '--collect',
      'XPlat Code Coverage'
    ) + $singleProcessBuildArguments + $buildStabilityProperties)
  }

  & $coverageReportScriptPath `
    -RepoRoot $repoRoot `
    -ResultsRoot $coverageResultsRoot `
    -OutputPath $coverageOutputPath

  if ($LASTEXITCODE -ne 0) {
    throw "coverage-report script failed with exit code ${LASTEXITCODE}"
  }

  $coverageSummary = Get-Content -LiteralPath $coverageOutputPath -Raw | ConvertFrom-Json
  "COVERAGE_REPORT_OK:{0}:{1}:{2}" -f $coverageSummary.coveredLines, $coverageSummary.totalLines, $coverageSummary.lineRate |
    Set-Content -LiteralPath $coverageMarkerOutputPath
}

function Invoke-PrereleaseNotesValidation {
  param(
    [string]$PublicTag
  )

  Write-Host ''
  Write-Host '### Generate prerelease notes proof' -ForegroundColor Yellow

  & $prereleaseNotesScriptPath `
    -RepoRoot $repoRoot `
    -ProofRoot $proofArtifactsRoot `
    -OutputPath $prereleaseNotesOutputPath `
    -CoverageSummaryPath $coverageOutputPath `
    -PublicTag $PublicTag

  if ($LASTEXITCODE -ne 0) {
    throw "write-prerelease-notes script failed with exit code ${LASTEXITCODE}"
  }

  [xml]$props = Get-Content -LiteralPath (Join-Path $repoRoot 'Directory.Build.props') -Raw
  $versionNode = $props.SelectSingleNode('/Project/PropertyGroup/Version')
  $packageVersion = if ($null -ne $versionNode) { $versionNode.InnerText } else { $null }
  $expectedTag = if ([string]::IsNullOrWhiteSpace($PublicTag)) { "v$packageVersion" } else { $PublicTag }
  $noteText = Get-Content -LiteralPath $prereleaseNotesOutputPath -Raw

  foreach ($requiredText in @(
    "installable package version: ``$packageVersion``",
    "matching public tag: ``$expectedTag``",
    '## Proof Summary',
    'PUBLIC_REPO_HYGIENE_OK:True',
    'HOST_SAMPLE_OK:True',
    'CONSUMER_SAMPLE_OK:True',
    'DEMO_OK:True',
    'COMMAND_SURFACE_OK:True',
    'TIERED_NODE_SURFACE_OK:True',
    'FIXED_GROUP_FRAME_OK:True',
    'NON_OBSCURING_EDITING_OK:True',
    'VISUAL_SEMANTICS_OK:True',
    'HIERARCHY_SEMANTICS_OK:True',
    'CUSTOM_TEMPLATE_OK:True',
    'TOOL_PROVIDER_OK:True',
    'NATIVE_INTERACTION_A11Y_OK:True',
    'COMPOSITE_SCOPE_OK:True',
    'EDGE_NOTE_OK:True',
    'EDGE_GEOMETRY_OK:True',
    'DISCONNECT_FLOW_OK:True',
    'HOST_SAMPLE_NET10_OK:True',
    'PACKAGE_SMOKE_OK:True',
    'SCALE_TIER_BUDGET:baseline',
    'SCALE_PERFORMANCE_BUDGET_OK:baseline:True:',
    'SCALE_TIER_BUDGET:large',
    'SCALE_PERFORMANCE_BUDGET_OK:large:True:',
    'SCALE_TIER_BUDGET:stress',
    'SCALE_PERFORMANCE_BUDGET_OK:stress:True:informational-only',
    'SCALE_PERF_SUMMARY:stress:',
    'SCALE_HISTORY_CONTRACT_OK:',
    'COVERAGE_REPORT_OK:'
  )) {
    if (-not $noteText.Contains($requiredText, [System.StringComparison]::Ordinal)) {
      throw "Generated prerelease notes are missing required text: $requiredText"
    }
  }
}

function Invoke-ContractValidation {
  param(
    [switch]$SkipRestore
  )

  if ($Framework -ne 'all') {
    Write-Warning "Contract lane validates the cross-target consumer and contract surface. Ignoring -Framework $Framework and using all."
  }

  Reset-Directory -Path $proofArtifactsRoot

  if (-not $SkipRestore) {
    Invoke-RestoreProjects -Projects @(
      $hostSampleProject,
      $consumerSampleProject,
      $demoProject,
      $editorTestsProject
    )
  }

  Invoke-HostSample
  Invoke-ConsumerSampleProof
  Invoke-DemoProof

  Write-Host ''
  Write-Host '### Run focused contract and proof regression surface' -ForegroundColor Yellow

  Invoke-DotNet -Arguments (@(
    'test',
    (Resolve-ProjectPath -RelativePath $editorTestsProject),
    '-c',
    $Configuration,
    '--no-restore',
    '--nologo',
    '-v',
    'minimal',
    '--filter',
    $contractTestFilter
  ) + $singleProcessBuildArguments + $buildStabilityProperties)
}

function Invoke-ReleaseValidation {
  if ($Framework -ne 'all') {
    Write-Warning "Release lane validates the full release surface. Ignoring -Framework $Framework and using all."
  }

  $releaseFrameworks = @('net8.0', 'net9.0')
  Reset-Directory -Path $proofArtifactsRoot

  Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $releaseFrameworks)
  Invoke-ContractValidation -SkipRestore
  Invoke-Packages
  Invoke-HostSample -UsePackedPackages
  Invoke-HostSample -UsePackedPackages -TargetFramework net10.0
  Invoke-PackageSmoke
  Invoke-ScaleSmoke
  Invoke-CoverageValidation
  Invoke-PublicRepoHygieneValidation -PreserveExistingProofArtifacts
  Invoke-PrereleaseNotesValidation
}

function Invoke-MaintenanceValidation {
  if ($Framework -ne 'all') {
    Write-Warning "Maintenance lane validates the hotspot refactor surface across the required frameworks. Ignoring -Framework $Framework and using all."
  }

  Invoke-RestoreProjects -Projects @(
    $scaleSmokeProject,
    $editorTestsProject
  )

  Write-Host ''
  Write-Host '### Run hotspot-sensitive editor regression surface' -ForegroundColor Yellow

  Invoke-DotNet -Arguments (@(
    'test',
    (Resolve-ProjectPath -RelativePath $editorTestsProject),
    '-c',
    $Configuration,
    '--no-restore',
    '--nologo',
    '-v',
    'minimal',
    '--filter',
    $maintenanceTestFilter
  ) + $singleProcessBuildArguments + $buildStabilityProperties)

  Invoke-ScaleSmoke
}

function Invoke-PublicRepoHygieneValidation {
  param(
    [switch]$PreserveExistingProofArtifacts
  )

  if ($Framework -ne 'all') {
    Write-Warning "Hygiene lane is framework-agnostic. Ignoring -Framework $Framework and validating the tracked repo surface."
  }

  if ($PreserveExistingProofArtifacts) {
    Ensure-Directory -Path $proofArtifactsRoot
  }
  else {
    Reset-Directory -Path $proofArtifactsRoot
  }

  Write-Host ''
  Write-Host '### Validate public repo hygiene surface' -ForegroundColor Yellow

  foreach ($path in @(
    '.planning',
    'AGENTS.md',
    'CLAUDE.md',
    'build.log',
    'docs/plans'
  )) {
    Assert-RepoPathNotTracked -RelativePath $path
  }

  foreach ($path in @(
    'README.md',
    'README.zh-CN.md',
    'tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj',
    'docs/en/project-status.md',
    'docs/en/consumer-sample.md',
    'docs/en/adoption-feedback.md',
    'docs/en/alpha-status.md',
    'docs/en/public-launch-checklist.md',
    'docs/en/scale-baseline.md',
    'docs/en/plugin-recipe.md',
    'docs/en/retained-migration-recipe.md',
    'docs/zh-CN/project-status.md',
    'docs/zh-CN/consumer-sample.md',
    'docs/zh-CN/adoption-feedback.md',
    'docs/zh-CN/alpha-status.md',
    'docs/zh-CN/public-launch-checklist.md',
    'docs/zh-CN/scale-baseline.md',
    'docs/zh-CN/plugin-recipe.md',
    'docs/zh-CN/retained-migration-recipe.md',
    'docs/en/quick-start.md',
    'docs/zh-CN/quick-start.md',
    'CONTRIBUTING.md',
    'CODE_OF_CONDUCT.md',
    'SECURITY.md',
    'global.json',
    '.github/ISSUE_TEMPLATE/adoption_feedback.yml'
  )) {
    Assert-RepoPathExists -RelativePath $path
  }

  foreach ($entry in @(
    '/build.log',
    '/.planning/',
    '/AGENTS.md',
    '/CLAUDE.md',
    '/docs/plans/'
  )) {
    Assert-GitIgnoreContains -ExpectedEntry $entry
  }

  'PUBLIC_REPO_HYGIENE_OK:True' | Set-Content -LiteralPath $publicRepoHygieneProofPath
  Write-Host 'PUBLIC_REPO_HYGIENE_OK:True' -ForegroundColor Green
}

Set-Location $repoRoot
Initialize-RepoToolingEnvironment
$frameworks = Get-Frameworks

switch ($Lane) {
  'restore' {
    Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $frameworks)
  }
  'build' {
    Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $frameworks)
    Invoke-Build -Frameworks $frameworks
  }
  'test' {
    Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $frameworks)
    Invoke-TestAndTooling -Frameworks $frameworks
  }
  'maintenance' {
    Invoke-MaintenanceValidation
  }
  'contract' {
    Invoke-ContractValidation
  }
  'release' {
    Invoke-ReleaseValidation
  }
  'hygiene' {
    Invoke-PublicRepoHygieneValidation
  }
  'all' {
    Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $frameworks)
    Invoke-Build -Frameworks $frameworks
    Invoke-TestAndTooling -Frameworks $frameworks
  }
  default {
    throw "Unsupported lane: $Lane"
  }
}

Write-Host ''
Write-Host "CI lane '$Lane' completed for framework '$Framework'." -ForegroundColor Green
