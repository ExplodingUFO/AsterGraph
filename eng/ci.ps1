[CmdletBinding()]
param(
  [ValidateSet('restore', 'build', 'test', 'maintenance', 'contract', 'release', 'all')]
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
$hostSampleProjectProofPath = Join-Path $proofArtifactsRoot 'hostsample-project.txt'
$hostSamplePackedProofPath = Join-Path $proofArtifactsRoot 'hostsample-packed.txt'
$hostSampleNet10PackedProofPath = Join-Path $proofArtifactsRoot 'hostsample-net10-packed.txt'
$packageSmokeProofPath = Join-Path $proofArtifactsRoot 'package-smoke.txt'
$scaleSmokeProofPath = Join-Path $proofArtifactsRoot 'scale-smoke.txt'
$dotnetCliHome = Join-Path $repoRoot '.dotnet-cli-home'
$coverageRunSettingsPath = Join-Path $repoRoot 'tests/coverage.runsettings'
$coverageReportScriptPath = Join-Path $repoRoot 'eng/coverage-report.ps1'
$hostSampleProject = 'tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj'
$packageSmokeProject = 'tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj'
$scaleSmokeProject = 'tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj'
$editorTestsProject = 'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj'
$userHome = if ([string]::IsNullOrWhiteSpace($env:USERPROFILE)) { $env:HOME } else { $env:USERPROFILE }
$fallbackPackageCache = Join-Path $userHome '.nuget/packages'
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
    $hostSampleProject,
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
    'tests/AsterGraph.Serialization.Tests/AsterGraph.Serialization.Tests.csproj'
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

function Invoke-DotNetCapture {
  param(
    [Parameter(Mandatory = $true)]
    [string[]]$Arguments,

    [Parameter(Mandatory = $true)]
    [string]$CapturePath
  )

  $captureDirectory = Split-Path -Parent $CapturePath
  if (-not [string]::IsNullOrWhiteSpace($captureDirectory)) {
    Ensure-Directory -Path $captureDirectory
  }

  Write-Host "==> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
  & dotnet @Arguments 2>&1 | Tee-Object -FilePath $CapturePath
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
    '--nologo'
  ) -CapturePath $scaleSmokeProofPath
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
      $editorTestsProject
    )
  }

  Invoke-HostSample

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
