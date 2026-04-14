[CmdletBinding()]
param(
  [ValidateSet('restore', 'build', 'test', 'all')]
  [string]$Lane = 'all',

  [ValidateSet('all', 'net8.0', 'net9.0')]
  [string]$Framework = 'all',

  [ValidateSet('Debug', 'Release')]
  [string]$Configuration = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot 'avalonia-node-map.sln'

$publishableProjects = @(
  'src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj',
  'src/AsterGraph.Core/AsterGraph.Core.csproj',
  'src/AsterGraph.Editor/AsterGraph.Editor.csproj',
  'src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj'
)

$frameworkBuildProjects = @{
  'net8.0' = @(
    'tools/AsterGraph.PackageSmoke/AsterGraph.PackageSmoke.csproj',
    'tools/AsterGraph.ScaleSmoke/AsterGraph.ScaleSmoke.csproj'
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

function Get-Frameworks {
  if ($Framework -eq 'all') {
    return @('net8.0', 'net9.0')
  }

  return @($Framework)
}

function Resolve-ProjectPath {
  param(
    [Parameter(Mandatory = $true)]
    [string]$RelativePath
  )

  return Join-Path $repoRoot $RelativePath
}

function Invoke-Restore {
  Write-Host ''
  Write-Host '### Restore' -ForegroundColor Yellow
  Invoke-DotNet -Arguments @(
    'restore',
    $solutionPath,
    '--nologo',
    '-v',
    'minimal'
  )
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
      Invoke-DotNet -Arguments @(
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
      )
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
      Invoke-DotNet -Arguments @(
        'build',
        (Resolve-ProjectPath -RelativePath $project),
        '-c',
        $Configuration,
        '--no-restore',
        '--nologo',
        '-v',
        'minimal'
      )
    }

    foreach ($project in $frameworkTestProjects[$targetFramework]) {
      Invoke-DotNet -Arguments @(
        'test',
        (Resolve-ProjectPath -RelativePath $project),
        '-c',
        $Configuration,
        '--no-restore',
        '--nologo',
        '-v',
        'minimal'
      )
    }
  }
}

Set-Location $repoRoot
$frameworks = Get-Frameworks

switch ($Lane) {
  'restore' {
    Invoke-Restore
  }
  'build' {
    Invoke-Restore
    Invoke-Build -Frameworks $frameworks
  }
  'test' {
    Invoke-Restore
    Invoke-TestAndTooling -Frameworks $frameworks
  }
  'all' {
    Invoke-Restore
    Invoke-Build -Frameworks $frameworks
    Invoke-TestAndTooling -Frameworks $frameworks
  }
  default {
    throw "Unsupported lane: $Lane"
  }
}

Write-Host ''
Write-Host "CI lane '$Lane' completed for framework '$Framework'." -ForegroundColor Green
