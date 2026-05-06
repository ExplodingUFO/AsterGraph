[CmdletBinding()]
param(
  [ValidateSet('restore', 'build', 'test', 'maintenance', 'contract', 'release', 'hygiene', 'all')]
  [string]$Lane = 'all',

  [ValidateSet('all', 'net8.0', 'net9.0', 'net10.0')]
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
$demoProofPath = Join-Path $proofArtifactsRoot 'demo-proof.txt'
$templateSmokeProofPath = Join-Path $proofArtifactsRoot 'template-smoke.txt'
$publicApiSurfaceProofPath = Join-Path $proofArtifactsRoot 'public-api-surface.txt'
$dotnetCliHome = Join-Path $repoRoot '.dotnet-cli-home'
$coverageRunSettingsPath = Join-Path $repoRoot 'tests/coverage.runsettings'
$coverageReportScriptPath = Join-Path $repoRoot 'eng/coverage-report.ps1'
$prereleaseNotesScriptPath = Join-Path $repoRoot 'eng/write-prerelease-notes.ps1'
$publicVersioningValidationScriptPath = Join-Path $repoRoot 'eng/validate-public-versioning.ps1'
$publicApiSurfaceValidationScriptPath = Join-Path $repoRoot 'eng/validate-public-api-surface.ps1'
$templateSmokeScriptPath = Join-Path $repoRoot 'eng/template-smoke.ps1'
$defaultVsTestConnectionTimeoutSeconds = 180
$demoProject = 'src/AsterGraph.Demo/AsterGraph.Demo.csproj'
$editorTestsProject = 'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj'
$userHome = if ([string]::IsNullOrWhiteSpace($env:USERPROFILE)) { $env:HOME } else { $env:USERPROFILE }
$fallbackPackageCache = Join-Path $userHome '.nuget/packages'
$isWindowsHost = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)
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
  'net8.0' = @()
  'net9.0' = @(
    'tests/AsterGraph.TestPlugins/AsterGraph.TestPlugins.csproj',
    'src/AsterGraph.Demo/AsterGraph.Demo.csproj'
  )
  'net10.0' = @()
}

$frameworkTestProjects = @{
  'net8.0' = @(
    'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj'
  )
  'net9.0' = @(
    'tests/AsterGraph.Editor.Tests/AsterGraph.Editor.Tests.csproj',
    'tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj'
  )
  'net10.0' = @()
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

function Convert-TextToCapabilityStatus {
  param([string]$Value)

  if ([string]::IsNullOrWhiteSpace($Value)) {
    return 'MISSING'
  }

  $parsed = $false
  if ([bool]::TryParse($Value, [ref]$parsed)) {
    if ($parsed) {
      return 'PASS'
    }

    return 'FAIL'
  }

  return 'MISSING'
}

function Get-ProofMarkerLine {
  param([string]$ProofText, [string]$Marker)

  $match = [regex]::Match($ProofText, "(?m)^$([regex]::Escape($Marker)):(.+)$")
  if ($match.Success) {
    return $match.Groups[1].Value.Trim()
  }

  return $null
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
    return @('net8.0', 'net9.0', 'net10.0')
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

function Get-PackageVersion {
  [xml]$buildProps = Get-Content -LiteralPath (Join-Path $repoRoot 'Directory.Build.props')
  $versionNode = $buildProps.SelectSingleNode('//Version')
  $version = if ($null -eq $versionNode) { $null } else { $versionNode.InnerText }
  if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'Directory.Build.props does not define Version.'
  }

  return $version.Trim()
}

function Invoke-TemplateSmoke {
  Write-Host ''
  Write-Host '### Run template and plugin-tool smoke' -ForegroundColor Yellow

  $packageVersion = Get-PackageVersion
  & $templateSmokeScriptPath `
    -RepoRoot $repoRoot `
    -Configuration $Configuration `
    -AsterGraphVersion $packageVersion `
    -PackageSource $packagesOutputPath `
    -ProofPath $templateSmokeProofPath

  if ($LASTEXITCODE -ne 0) {
    throw "template smoke failed with exit code ${LASTEXITCODE}"
  }
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
      'XPlat Code Coverage',
      '--blame-hang-timeout',
      '5m',
      '--blame-hang-dump-type',
      'mini'
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
    'GRAPH_ERROR_HELP_TARGET_OK:True',
    'GRAPH_PROBLEM_INSPECTOR_HELP_TARGET_OK:True',
    'REPAIR_HELP_REVIEW_LOOP_OK:True',
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
    'DEMO_GESTURE_PROOF_OK:True',
    'COMPOSITE_SCOPE_OK:True',
    'EDGE_NOTE_OK:True',
    'EDGE_GEOMETRY_OK:True',
    'DISCONNECT_FLOW_OK:True',
    'SCENARIO_LAUNCH_OK:True',
    'SCENARIO_TOUR_OK:True',
    'AI_PIPELINE_MOCK_RUNNER_OK:True',
    'AI_PIPELINE_RUNTIME_OVERLAY_OK:True',
    'AI_PIPELINE_ERROR_STATE_OK:True',
    'AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True',
    'AI_PIPELINE_PAYLOAD_PREVIEW_OK:True',
    'AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True',
    'WORKBENCH_COMMAND_RECOVERY_OK:True',
    'WORKBENCH_COMMAND_ACTION_GUIDANCE_OK:True',
    'WORKBENCH_COMMAND_SCOPE_BOUNDARY_OK:True',
    'DISCOVERY_TO_ACTION_FLOW_OK:True',
    'DISCOVERY_EMPTY_STATE_GUIDANCE_OK:True',
    'DISCOVERY_ACTION_SCOPE_BOUNDARY_OK:True',
    'WORKBENCH_EXPORT_AFFORDANCE_OK:True',
    'WORKBENCH_SHARE_EVIDENCE_OK:True',
    'WORKBENCH_EXPORT_SCOPE_BOUNDARY_OK:True',
    'WORKBENCH_FEATURE_DEPTH_HANDOFF_OK:True',
    'WORKBENCH_FEATURE_DEPTH_SCOPE_BOUNDARY_OK:True',
    'V065_MILESTONE_PROOF_OK:True',
    'API_SURFACE_BASELINE_OK:True',
    'API_CANONICAL_ROUTES_OK:True',
    'API_PACKAGE_BOUNDARY_OK:True',
    'ASTERGRAPH_TEMPLATE_SMOKE_OK:True',
    'TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True',
    'TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True',
    'TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True',
    'PUBLIC_API_SURFACE_OK:',
    'PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia',
    'PUBLIC_API_GUIDANCE_OK:True',
    'PUBLIC_API_DIFF_GATE_OK:True',
    'PUBLIC_API_USAGE_GUIDANCE_OK:True',
    'PUBLIC_API_STABILITY_SCOPE_OK:True',
    'ADOPTION_API_STABILIZATION_HANDOFF_OK:True',
    'ADOPTION_API_SCOPE_BOUNDARY_OK:True',
    'V061_MILESTONE_PROOF_OK:True',
    'COVERAGE_REPORT_OK:'
  )) {
    if (-not $noteText.Contains($requiredText, [System.StringComparison]::Ordinal)) {
      throw "Generated prerelease notes are missing required text: $requiredText"
    }
  }
}

function Invoke-PublicVersioningValidation {
  param(
    [string]$PublicTag
  )

  Write-Host ''
  Write-Host '### Validate public versioning surface' -ForegroundColor Yellow

  $arguments = @{
    RepoRoot = $repoRoot
  }

  if (-not [string]::IsNullOrWhiteSpace($PublicTag)) {
    $arguments['PublicTag'] = $PublicTag
  }

  & $publicVersioningValidationScriptPath @arguments

  if ($LASTEXITCODE -ne 0) {
    throw "validate-public-versioning script failed with exit code $LASTEXITCODE"
  }
}

function Invoke-PublicApiSurfaceValidation {
  Write-Host ''
  Write-Host '### Validate public API surface drift' -ForegroundColor Yellow

  & $publicApiSurfaceValidationScriptPath `
    -RepoRoot $repoRoot `
    -Configuration $Configuration `
    -Framework 'net9.0' `
    -ProofPath $publicApiSurfaceProofPath

  if ($LASTEXITCODE -ne 0) {
    throw "validate-public-api-surface script failed with exit code $LASTEXITCODE"
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
      $demoProject,
      $editorTestsProject
    )
  }

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

  $releaseFrameworks = @('net8.0', 'net9.0', 'net10.0')
  Reset-Directory -Path $proofArtifactsRoot

  Invoke-RestoreProjects -Projects (Get-DefaultRestoreProjects -Frameworks $releaseFrameworks)
  Invoke-ContractValidation -SkipRestore
  Invoke-Packages
  Invoke-TemplateSmoke
  Invoke-PublicVersioningValidation
  Invoke-PublicApiSurfaceValidation
  Invoke-CoverageValidation
  Invoke-PublicRepoHygieneValidation -PreserveExistingProofArtifacts
  Invoke-PrereleaseNotesValidation
}

function Invoke-MaintenanceValidation {
  if ($Framework -ne 'all') {
    Write-Warning "Maintenance lane validates the hotspot refactor surface across the required frameworks. Ignoring -Framework $Framework and using all."
  }

  Invoke-RestoreProjects -Projects @(
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
    'docs/en/project-status.md',
    'docs/en/adoption-feedback.md',
    'docs/en/alpha-status.md',
    'docs/en/public-launch-checklist.md',
    'docs/en/scale-baseline.md',
    'docs/en/plugin-recipe.md',
    'docs/en/retained-migration-recipe.md',
    'docs/zh-CN/project-status.md',
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
