[CmdletBinding()]
param(
  [ValidateSet('restore', 'build', 'test', 'maintenance', 'release', 'all')]
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
$coverageResultsRoot = Join-Path $artifactsRoot 'test-results\release'
$coverageOutputPath = Join-Path $artifactsRoot 'coverage\release-summary.json'
$dotnetCliHome = Join-Path $repoRoot '.dotnet-cli-home'
$coverageRunSettingsPath = Join-Path $repoRoot 'tests/coverage.runsettings'
$coverageReportScriptPath = Join-Path $repoRoot 'eng/coverage-report.ps1'
$packageSmokeProject = 'tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj'
$scaleSmokeProject = 'tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj'
$editorTestsProject = 'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj'
$fallbackPackageCache = Join-Path $env:USERPROFILE '.nuget\packages'
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

  Invoke-DotNet -Arguments @(
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
  )
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

  Invoke-DotNet -Arguments @(
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
  )
}

function Invoke-CoverageValidation {
  Reset-Directory -Path $coverageResultsRoot
  Ensure-Directory -Path (Split-Path -Parent $coverageOutputPath)

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
}

function Invoke-ReleaseValidation {
  if ($Framework -ne 'all') {
    Write-Warning "Release lane validates the full release surface. Ignoring -Framework $Framework and using all."
  }

  $releaseFrameworks = @('net8.0', 'net9.0')

  Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $releaseFrameworks)
  Invoke-Packages
  Invoke-PackageSmoke
  Invoke-ScaleSmoke
  Invoke-CoverageValidation
}

function Invoke-MaintenanceValidation {
  if ($Framework -ne 'all') {
    Write-Warning "Maintenance lane validates the hotspot refactor surface across the required frameworks. Ignoring -Framework $Framework and using all."
  }

  Invoke-RestoreProjects -Projects @(
    $editorTestsProject
  )

  Write-Host ''
  Write-Host '### Run maintenance editor validation lane' -ForegroundColor Yellow

  Invoke-DotNet -Arguments (@(
    'test',
    (Resolve-ProjectPath -RelativePath $editorTestsProject),
    '-c',
    $Configuration,
    '--no-restore',
    '--nologo',
    '-v',
    'minimal'
  ) + $singleProcessBuildArguments + $buildStabilityProperties)
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
