[CmdletBinding()]
param(
  [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
  [string]$Configuration = 'Release',
  [string]$AsterGraphVersion = '0.11.0-beta',
  [string]$PackageSource = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$templateRoot = Join-Path $RepoRoot 'templates'
$smokeRoot = Join-Path $RepoRoot 'artifacts/template-smoke'
$dotnetCliHome = Join-Path $RepoRoot '.dotnet-cli-home'

function Invoke-DotNet {
  param([Parameter(Mandatory = $true)][string[]]$Arguments)

  Write-Host "==> dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
  & dotnet @Arguments
  if ($LASTEXITCODE -ne 0) {
    throw "dotnet command failed with exit code ${LASTEXITCODE}: dotnet $($Arguments -join ' ')"
  }
}

if (Test-Path -LiteralPath $smokeRoot) {
  Remove-Item -LiteralPath $smokeRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $smokeRoot | Out-Null
New-Item -ItemType Directory -Path $dotnetCliHome -Force | Out-Null
$env:DOTNET_CLI_HOME = $dotnetCliHome

Invoke-DotNet -Arguments @('new', 'install', $templateRoot, '--force')

$avaloniaOutput = Join-Path $smokeRoot 'SmokeAvalonia'
$pluginOutput = Join-Path $smokeRoot 'SmokePlugin'

Invoke-DotNet -Arguments @('new', 'astergraph-avalonia', '-n', 'SmokeAvalonia', '-o', $avaloniaOutput, '--AsterGraphVersion', $AsterGraphVersion)
Invoke-DotNet -Arguments @('new', 'astergraph-plugin', '-n', 'SmokePlugin', '-o', $pluginOutput, '--PluginId', 'smoke.plugin', '--AsterGraphVersion', $AsterGraphVersion)

$restoreProperties = @('/p:NuGetAudit=false')
if (-not [string]::IsNullOrWhiteSpace($PackageSource)) {
  $resolvedPackageSource = (Resolve-Path -LiteralPath $PackageSource).Path
  $nugetConfigPath = Join-Path $smokeRoot 'NuGet.Config'
  @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local-astergraph" value="$resolvedPackageSource" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local-astergraph">
      <package pattern="AsterGraph.*" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content -LiteralPath $nugetConfigPath
}

Invoke-DotNet -Arguments (@('build', (Join-Path $pluginOutput 'SmokePlugin.csproj'), '-c', $Configuration, '--nologo', '-v', 'minimal') + $restoreProperties)
Invoke-DotNet -Arguments (@('build', (Join-Path $avaloniaOutput 'SmokeAvalonia.csproj'), '-c', $Configuration, '--nologo', '-v', 'minimal') + $restoreProperties)

$pluginAssembly = Join-Path $pluginOutput "bin/$Configuration/net8.0/SmokePlugin.dll"
Invoke-DotNet -Arguments @('run', '--project', (Join-Path $RepoRoot 'tools/AsterGraph.PluginTool/AsterGraph.PluginTool.csproj'), '-c', $Configuration, '--no-restore', '--', 'validate', $pluginAssembly)

Write-Host 'ASTERGRAPH_TEMPLATE_SMOKE_OK:True' -ForegroundColor Green
