[CmdletBinding()]
param(
  [Parameter(Mandatory = $true)]
  [string]$RepoRoot,

  [Parameter(Mandatory = $true)]
  [string]$ProofRoot,

  [Parameter(Mandatory = $true)]
  [string]$OutputPath,

  [string]$PublicTag,

  [string]$GeneratedNotesPath,

  [string]$CoverageSummaryPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-PackageVersion {
  param(
    [Parameter(Mandatory = $true)]
    [string]$PropsPath
  )

  [xml]$props = Get-Content -LiteralPath $PropsPath -Raw
  $versionNode = $props.SelectSingleNode('/Project/PropertyGroup/Version')
  $version = if ($null -ne $versionNode) { $versionNode.InnerText } else { $null }
  if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Could not read package version from $PropsPath"
  }

  return $version.Trim()
}

function Get-LatestLegacyMilestoneTag {
  param(
    [Parameter(Mandatory = $true)]
    [string]$WorkingDirectory
  )

  $tags = & git -C $WorkingDirectory tag --list 'v1.*' --sort=-version:refname
  if ($LASTEXITCODE -ne 0) {
    throw 'Failed to enumerate legacy milestone tags.'
  }

  return ($tags | Select-Object -First 1)
}

function Get-FirstMatchingLine {
  param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,

    [Parameter(Mandatory = $true)]
    [string]$Pattern
  )

  if (-not (Test-Path -LiteralPath $FilePath)) {
    return $null
  }

  $match = Select-String -Path $FilePath -Pattern $Pattern | Select-Object -First 1
  if ($null -eq $match) {
    return $null
  }

  return $match.Line
}

function Get-MatchingLines {
  param(
    [Parameter(Mandatory = $true)]
    [string]$FilePath,

    [Parameter(Mandatory = $true)]
    [string]$Pattern
  )

  if (-not (Test-Path -LiteralPath $FilePath)) {
    return @()
  }

  return @(Select-String -Path $FilePath -Pattern $Pattern | ForEach-Object { $_.Line })
}

function Get-CoverageMarker {
  param(
    [string]$CoverageSummaryJsonPath,
    [string]$ProofRootPath
  )

  $coverageMarkerPath = Join-Path $ProofRootPath 'coverage-report.txt'
  $coverageMarker = Get-FirstMatchingLine -FilePath $coverageMarkerPath -Pattern 'COVERAGE_REPORT_OK'
  if ($coverageMarker) {
    return $coverageMarker
  }

  if ([string]::IsNullOrWhiteSpace($CoverageSummaryJsonPath) -or -not (Test-Path -LiteralPath $CoverageSummaryJsonPath)) {
    return $null
  }

  $coverage = Get-Content -LiteralPath $CoverageSummaryJsonPath -Raw | ConvertFrom-Json
  return "COVERAGE_REPORT_OK:{0}:{1}:{2}" -f $coverage.coveredLines, $coverage.totalLines, $coverage.lineRate
}

$resolvedRepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
$resolvedProofRoot = [System.IO.Path]::GetFullPath($ProofRoot)
$resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$propsPath = Join-Path $resolvedRepoRoot 'Directory.Build.props'
$packageVersion = Get-PackageVersion -PropsPath $propsPath
$expectedTag = "v$packageVersion"
$effectiveTag = if ([string]::IsNullOrWhiteSpace($PublicTag)) { $expectedTag } else { $PublicTag.Trim() }

if ($effectiveTag -ne $expectedTag) {
  throw "Public prerelease tag '$effectiveTag' does not match package version '$expectedTag'."
}

$legacyTag = Get-LatestLegacyMilestoneTag -WorkingDirectory $resolvedRepoRoot
$generatedNotes = if (-not [string]::IsNullOrWhiteSpace($GeneratedNotesPath) -and (Test-Path -LiteralPath $GeneratedNotesPath)) {
  (Get-Content -LiteralPath $GeneratedNotesPath -Raw).Trim()
} else {
  $null
}

$proofLines = @()
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-repo-hygiene.txt') -Pattern 'PUBLIC_REPO_HYGIENE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hostsample-packed.txt') -Pattern 'HOST_SAMPLE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'consumer-sample.txt') -Pattern 'CONSUMER_SAMPLE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'DEMO_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'COMMAND_SURFACE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'TIERED_NODE_SURFACE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'FIXED_GROUP_FRAME_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'NON_OBSCURING_EDITING_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'VISUAL_SEMANTICS_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'HIERARCHY_SEMANTICS_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'CUSTOM_TEMPLATE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'TOOL_PROVIDER_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'NATIVE_INTERACTION_A11Y_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'DEMO_GESTURE_PROOF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'COMPOSITE_SCOPE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'EDGE_NOTE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'EDGE_GEOMETRY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'DISCONNECT_FLOW_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'SCENARIO_LAUNCH_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'SCENARIO_TOUR_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'AI_PIPELINE_MOCK_RUNNER_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'AI_PIPELINE_RUNTIME_OVERLAY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'AI_PIPELINE_ERROR_STATE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'AI_PIPELINE_MOCK_RUNNER_POLISH_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'AI_PIPELINE_PAYLOAD_PREVIEW_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_COMMAND_RECOVERY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_COMMAND_ACTION_GUIDANCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_COMMAND_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'DISCOVERY_TO_ACTION_FLOW_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'DISCOVERY_EMPTY_STATE_GUIDANCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'DISCOVERY_ACTION_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_EXPORT_AFFORDANCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_SHARE_EVIDENCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_EXPORT_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_FEATURE_DEPTH_HANDOFF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'WORKBENCH_FEATURE_DEPTH_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'V065_MILESTONE_PROOF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'API_SURFACE_BASELINE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'API_CANONICAL_ROUTES_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'demo-proof.txt') -Pattern 'API_PACKAGE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hostsample-net10-packed.txt') -Pattern 'HOST_SAMPLE_NET10_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'package-smoke.txt') -Pattern 'PACKAGE_SMOKE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'template-smoke.txt') -Pattern 'ASTERGRAPH_TEMPLATE_SMOKE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'template-smoke.txt') -Pattern 'TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'template-smoke.txt') -Pattern 'TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'template-smoke.txt') -Pattern 'TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-api-surface.txt') -Pattern 'PUBLIC_API_SURFACE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-api-surface.txt') -Pattern 'PUBLIC_API_SCOPE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-api-surface.txt') -Pattern 'PUBLIC_API_GUIDANCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-api-surface.txt') -Pattern 'PUBLIC_API_DIFF_GATE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-api-surface.txt') -Pattern 'PUBLIC_API_USAGE_GUIDANCE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'public-api-surface.txt') -Pattern 'PUBLIC_API_STABILITY_SCOPE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'HOSTED_ACCESSIBILITY_BASELINE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'HOSTED_ACCESSIBILITY_FOCUS_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'HOSTED_ACCESSIBILITY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_PERFORMANCE_BASELINE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_EXPORT_BREADTH_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_PROJECTION_BUDGET_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_COMMAND_BUDGET_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_SCENE_BUDGET_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_VALIDATION_SCOPE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_MATRIX_HANDOFF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_WPF_SAMPLE_PROOF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_CANONICAL_ROUTE_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_RECIPE_ALIGNMENT_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_PROOF_BUDGET_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_VALIDATION_HANDOFF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'ADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'V060_MILESTONE_PROOF_OK'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'hello-world-wpf-proof.txt') -Pattern 'HELLOWORLD_WPF_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'wpf-adapter-capability-matrix.txt') -Pattern 'ADAPTER_CAPABILITY_MATRIX'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_TIER_BUDGET'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_EXPORT_BUDGET:'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_PERFORMANCE_BUDGET_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_AUTHORING_BUDGET_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_EXPORT_BUDGET_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_RASTER_EXPORT_STRESS_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'EXPORT_PROGRESS_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'EXPORT_CANCEL_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'EXPORT_SCOPE_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'EXPORT_SELECTION_SCOPE_OK'
$proofLines += Get-MatchingLines -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_PERF_SUMMARY'
$proofLines += Get-FirstMatchingLine -FilePath (Join-Path $resolvedProofRoot 'scale-smoke.txt') -Pattern 'SCALE_HISTORY_CONTRACT_OK'
$proofLines += Get-CoverageMarker -CoverageSummaryJsonPath $CoverageSummaryPath -ProofRootPath $resolvedProofRoot
$proofLines = $proofLines | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }

$environmentLines = @(
  '- validation lanes: GitHub-hosted Windows release validation plus GitHub-hosted Linux matrix validation',
  '- medium host proof: `AsterGraph.ConsumerSample.Avalonia` on the hosted-UI route with host actions, parameter editing, and one trusted plugin',
  '- trusted plugin proof: pair `CONSUMER_SAMPLE_TRUST_OK` with `ASTERGRAPH_PLUGIN_VALIDATE_OK` and the plugin trust contract before treating a third-party plugin artifact as loadable',
  '- showcase proof: `AsterGraph.Demo --proof` for host-native shell workflows, non-obscuring editing, and graph-surface visual semantics',
  '- packed consumer proofs: `HostSample`, `.NET 10` packed consumer path, `PackageSmoke`, and generated template/plugin validation',
  '- API adoption proof: `validate-public-api-surface.ps1` pairs `PUBLIC_API_SURFACE_OK`, `PUBLIC_API_SCOPE_OK`, `PUBLIC_API_GUIDANCE_OK`, `PUBLIC_API_DIFF_GATE_OK:True`, `PUBLIC_API_USAGE_GUIDANCE_OK:True`, and `PUBLIC_API_STABILITY_SCOPE_OK:True` near generated template/plugin validation in the release proof story; `API_RELEASE_CANDIDATE_PROOF_OK:True`, `PUBLIC_API_GUIDANCE_HANDOFF_OK:True`, and `RELEASE_BOUNDARY_STABILITY_OK:True` keep release-candidate wording tied to the current package boundary',
  '- scale proof source: `ScaleSmoke` defended `baseline`/`large` tiers plus `stress` performance/authoring/SVG/raster-export gates; `stress` PNG/JPEG export uses conservative defended redlines'
)

$builder = [System.Text.StringBuilder]::new()
[void]$builder.AppendLine("# AsterGraph $effectiveTag")
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Release Header")
[void]$builder.AppendLine()
[void]$builder.AppendLine(('- installable package version: `{0}`' -f $packageVersion))
[void]$builder.AppendLine(('- matching public tag: `{0}`' -f $effectiveTag))
if (-not [string]::IsNullOrWhiteSpace($legacyTag)) {
  [void]$builder.AppendLine(('- historical repo checkpoint reference: `{0}` (legacy, not installable)' -f $legacyTag))
}
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Proof Summary")
[void]$builder.AppendLine()
foreach ($line in $proofLines) {
  [void]$builder.AppendLine("- $line")
}
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Support Story")
[void]$builder.AppendLine()
[void]$builder.AppendLine("- frozen support boundary story: [Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)")
[void]$builder.AppendLine("- adapter matrix story: [Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md)")
[void]$builder.AppendLine("- external capability readiness gate: [Project Status](./docs/en/project-status.md) for externally proven now, validation-only or bounded claims, and deferred until more adopter evidence")
[void]$builder.AppendLine("- current 0.xx alpha/beta hardening line: `Adoption Readiness / Release Candidate Hygiene` means the public recommendation, API drift, support boundary, and release proof gates stay aligned before release-candidate, GA, or `1.0` language; `ADOPTION_RECOMMENDATION_CURRENT_OK:True`, `CLAIM_HYGIENE_BOUNDARY_OK:True`, `RELEASE_READINESS_GATE_OK:True`, `SUPPORT_BOUNDARY_GATE_OK:True`, `BETA_CLAIM_ALIGNMENT_OK:True`, `ADOPTION_API_STABILIZATION_HANDOFF_OK:True`, `ADOPTION_API_SCOPE_BOUNDARY_OK:True`, and `V061_MILESTONE_PROOF_OK:True` are the proof handoff markers; `xlarge` remains telemetry-only, not a 10000-node support or virtualization claim")
[void]$builder.AppendLine("- release evidence contract: route, version, proof markers, friction, and support-bundle attachment note")
[void]$builder.AppendLine("- `HELLOWORLD_WPF_OK` remains adapter-2 validation only and does not widen the public publish/package boundary")
[void]$builder.AppendLine("- support-bundle attachment note: [Beta Support Bundle](./docs/en/support-bundle.md)")
[void]$builder.AppendLine("- [Adoption Feedback Loop](./docs/en/adoption-feedback.md)")
[void]$builder.AppendLine("- [Adopter Triage Checklist](./docs/en/adopter-triage.md)")
[void]$builder.AppendLine()
[void]$builder.AppendLine("## Validation Environment")
[void]$builder.AppendLine()
foreach ($line in $environmentLines) {
  [void]$builder.AppendLine($line)
}
[void]$builder.AppendLine()
[void]$builder.AppendLine("## References")
[void]$builder.AppendLine()
[void]$builder.AppendLine("- [Versioning](./docs/en/versioning.md)")
[void]$builder.AppendLine("- [Project Status](./docs/en/project-status.md)")
[void]$builder.AppendLine("- [Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)")
[void]$builder.AppendLine("- [Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md)")
[void]$builder.AppendLine("- [Demo Guide](./docs/en/demo-guide.md)")
[void]$builder.AppendLine("- [ScaleSmoke Baseline](./docs/en/scale-baseline.md)")
[void]$builder.AppendLine("- [Public Launch Checklist](./docs/en/public-launch-checklist.md)")

if (-not [string]::IsNullOrWhiteSpace($generatedNotes)) {
  [void]$builder.AppendLine()
  [void]$builder.AppendLine("## Generated Change Notes")
  [void]$builder.AppendLine()
  [void]$builder.AppendLine($generatedNotes)
}

$outputDirectory = Split-Path -Parent $resolvedOutputPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -LiteralPath $outputDirectory)) {
  New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

$builder.ToString().TrimEnd() + [Environment]::NewLine | Set-Content -LiteralPath $resolvedOutputPath
Write-Host ('PRERELEASE_NOTES_OK:{0}:{1}' -f $packageVersion, $effectiveTag)
