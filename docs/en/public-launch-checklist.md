# AsterGraph Public Launch Checklist

Use this checklist immediately before making the repository public or pushing the public prerelease tag that matches the package version.

## 1. Visibility And Branch Policy

- confirm the default branch is the intended public branch
- enable branch protection for the default branch
- require the `ci` workflow checks that represent the shipped gate
- confirm release permissions and NuGet publication permissions are limited to maintainers

## 2. Public Repo Surface

- confirm `README.md` and `README.zh-CN.md` both point to the current public beta docs and entry matrix
- confirm `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, and `SECURITY.md` are present and still accurate
- confirm issue templates and the pull request template are enabled in `.github`
- confirm repository description, topics, and homepage match the current prerelease narrative

## 3. Required Validation

Run the maintained entrypoints, not ad-hoc local commands:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

## 4. Proof Artifact Review

Review these release artifacts before opening the repo or announcing the prerelease:

- `artifacts/proof/public-repo-hygiene.txt`
- `artifacts/proof/hostsample-packed.txt`
- `artifacts/proof/consumer-sample.txt`
- `artifacts/proof/demo-proof.txt`
- `artifacts/proof/hostsample-net10-packed.txt`
- `artifacts/proof/package-smoke.txt`
- `artifacts/proof/template-smoke.txt`
- `artifacts/proof/public-api-surface.txt`
- `artifacts/proof/scale-smoke.txt`
- `artifacts/proof/coverage-report.txt`
- `artifacts/coverage/release-summary.json`

Expected high-signal markers:

- `PUBLIC_REPO_HYGIENE_OK:True`
- `HOST_SAMPLE_OK:True`
- `CONSUMER_SAMPLE_OK:True`
- `DEMO_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_BASELINE_OK:True`
- `HOSTED_ACCESSIBILITY_FOCUS_OK:True`
- `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`
- `HOSTED_ACCESSIBILITY_OK:True`
- `ADAPTER2_PERFORMANCE_BASELINE_OK:True`
- `ADAPTER2_EXPORT_BREADTH_OK:True`
- `ADAPTER2_PROJECTION_BUDGET_OK:True:none`
- `ADAPTER2_COMMAND_BUDGET_OK:True:none`
- `ADAPTER2_SCENE_BUDGET_OK:True:none`
- `HELLOWORLD_WPF_OK:True`
- `TIERED_NODE_SURFACE_OK:True`
- `FIXED_GROUP_FRAME_OK:True`
- `NON_OBSCURING_EDITING_OK:True`
- `VISUAL_SEMANTICS_OK:True`
- `HIERARCHY_SEMANTICS_OK:True`
- `COMPOSITE_SCOPE_OK:True`
- `EDGE_NOTE_OK:True`
- `EDGE_GEOMETRY_OK:True`
- `DISCONNECT_FLOW_OK:True`
- `ADAPTER_CAPABILITY_MATRIX_FORMAT:1`
- `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`
- `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- `HOST_SAMPLE_NET10_OK:True`
- `PACKAGE_SMOKE_OK:True`
- `ASTERGRAPH_TEMPLATE_SMOKE_OK:True`
- `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True`
- `TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True`
- `TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True`
- `PUBLIC_API_SURFACE_OK:...:net9.0`
- `PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia`
- `PUBLIC_API_GUIDANCE_OK:True`
- `SCALE_PERFORMANCE_BUDGET_OK:baseline:True:...`
- `SCALE_PERFORMANCE_BUDGET_OK:large:True:...`
- `SCALE_PERFORMANCE_BUDGET_OK:stress:True:...`
- `SCALE_AUTHORING_BUDGET_OK:stress:True:...`
- `SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800`
- `SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only`
- `SCALE_RASTER_EXPORT_STRESS_OK:True`
- `EXPORT_PROGRESS_OK:True`
- `EXPORT_CANCEL_OK:True`
- `EXPORT_SCOPE_OK:True`
- `EXPORT_SELECTION_SCOPE_OK:True`
- `SCALE_PERF_SUMMARY:stress:...`
- `SCALE_HISTORY_CONTRACT_OK:...`
- `COVERAGE_REPORT_OK:...`
- `ADOPTION_INTAKE_EVIDENCE_OK:True`
- `SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True`
- `REAL_EXTERNAL_REPORT_GATE_OK:True`
- `API_RELEASE_CANDIDATE_PROOF_OK:True`
- `PUBLIC_API_GUIDANCE_HANDOFF_OK:True`
- `RELEASE_BOUNDARY_STABILITY_OK:True`
- `ADOPTION_READINESS_HANDOFF_OK:True`
- `ADOPTION_SCOPE_BOUNDARY_OK:True`
- `V056_MILESTONE_PROOF_OK:True`
- `AUTHORING_DEPTH_HANDOFF_OK:True`
- `AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True`
- `V058_MILESTONE_PROOF_OK:True`
- `LARGE_GRAPH_UX_POLICY_OK:True`
- `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`
- `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- `VIEWPORT_LOD_POLICY_OK:True`
- `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`
- `LARGE_GRAPH_BALANCED_UX_OK:True`
- `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- `EDGE_INTERACTION_CACHE_OK:True`
- `EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`
- `SELECTED_EDGE_FEEDBACK_OK:True`
- `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`
- `INSPECTOR_NARROW_PROJECTION_OK:True`
- `LARGE_GRAPH_PANEL_SCOPE_OK:True`
- `PROJECTION_PERFORMANCE_EVIDENCE_OK:True`
- synthetic dry-run records from [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md) are maintainer/internal rehearsal only; do not count them toward the 3-5 real external report gate, and do not widen support or capability claims from them
- every beta intake record includes report type, adopter context, route, version, proof markers, friction, support-bundle attachment note, and claim-expansion status; a single report does not widen public claims

## 5. Public Prerelease Tag

- confirm the working tree is clean
- push the release branch or `master` state that should back the tag
- create and push the public tag that matches the package version
- watch `.github/workflows/release.yml` from start to finish
- remember that the prerelease workflow now enforces an exact tag-to-package-version match
- confirm the generated prerelease notes begin with the automated header block:
  - installable package version
  - matching public tag
  - optional legacy historical repo checkpoint reference
- confirm the generated prerelease notes also publish the proof summary block, not only workflow artifacts
- confirm the proof summary keeps public API guidance proof next to template/plugin proof: `ASTERGRAPH_TEMPLATE_SMOKE_OK:True`, `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True`, `PUBLIC_API_SURFACE_OK:...:net9.0`, `PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia`, and `PUBLIC_API_GUIDANCE_OK:True`
- confirm runtime feedback proof stays host-owned and visible in release notes: `RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`, `RUNTIME_LOG_LOCATE_OK:True`, `RUNTIME_LOG_EXPORT_OK:True`, `AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`, `AI_PIPELINE_PAYLOAD_PREVIEW_OK:True`, and `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True`
- confirm runtime feedback copy does not imply an algorithm execution engine, workflow scripting UI, plugin marketplace, sandboxing, WPF parity, or GA / `1.0` support language
- confirm the generated notes and announcement text explicitly carry the frozen support boundary story and the adapter matrix story, plus `HOSTED_ACCESSIBILITY_BASELINE_OK:True`, `HOSTED_ACCESSIBILITY_FOCUS_OK:True`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`, `HOSTED_ACCESSIBILITY_OK:True`, `ADAPTER2_PERFORMANCE_BASELINE_OK:True`, `ADAPTER2_EXPORT_BREADTH_OK:True`, `ADAPTER2_PROJECTION_BUDGET_OK:True:none`, `ADAPTER2_COMMAND_BUDGET_OK:True:none`, `ADAPTER2_SCENE_BUDGET_OK:True:none`, `HELLOWORLD_WPF_OK:True`, `ADAPTER_CAPABILITY_MATRIX_FORMAT:1`, `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`, and `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`
- treat `HELLOWORLD_WPF_OK` as adapter-2 validation only; do not present it as Avalonia/WPF parity or public WPF support when describing the adapter story
- keep WPF support expansion validation-only and out of public WPF support language until 3-5 real external reports cluster on the same bounded risk
- use [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md) when you need the bounded handoff from defended Avalonia accessibility proof to validation-only WPF verification
- use [Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md) when you need the bounded handoff from defended Avalonia hosted metrics to validation-only WPF performance verification
- confirm every beta intake record uses the same bounded schema: report type, adopter context, route, version, proof markers, friction, support-bundle attachment note, and claim-expansion status
- confirm claim-expansion status is treated as triage input until 3-5 real external reports cluster on the same bounded risk
- GA prep checklist: adoption evidence, API drift, support boundary, and release proof gates must all stay explicit before any GA or `1.0` messaging
- repeat the current 0.xx alpha/beta hardening handoff in release messaging: `Adoption Readiness / Release Candidate Hygiene` means the public recommendation, API drift, support boundary, and release proof gates stay aligned before release-candidate, GA, or `1.0` language; include `ADOPTION_RECOMMENDATION_CURRENT_OK:True` and `CLAIM_HYGIENE_BOUNDARY_OK:True`
- keep `xlarge` described as telemetry-only; do not present it as a 10000-node support promise or virtualization commitment
- if `NUGET_API_KEY` is configured, confirm package publication succeeds
- if `NUGET_API_KEY` is not configured, confirm the workflow reports a deliberate NuGet publish skip instead of a failure
- do not present legacy `v1.x`-style historical milestone checkpoints as the current public package version; use [Versioning](./versioning.md) as the public rule
- in the first screen of release notes, list the installable package version first, the matching public tag second, and any legacy `v1.x`-style milestone reference only as historical context

For a maintainer-driven manual beta publish without pushing a new tag:

- add the `NUGET_API_KEY` repository secret in GitHub
- open `Actions > prerelease > Run workflow`
- set `publish_to_nuget` to `true`
- optionally set `release_ref` to the branch or `v*` tag that should be packed
- keep using the committed package version; the manual path publishes the version already checked into the repository
- leave GitHub prerelease creation tag-driven; manual dispatch is only the NuGet publish escape hatch

## 6. Public Entry Guidance

Keep the consumer entry story explicit in release notes and public announcements:

- `tools/AsterGraph.HelloWorld` = fastest runtime-only first-run sample
- `tools/AsterGraph.HelloWorld.Avalonia` = fastest hosted-UI first-run sample
- `tools/AsterGraph.ConsumerSample.Avalonia` = realistic hosted-UI consumer sample with one host action rail and one trusted plugin
- `tools/AsterGraph.Starter.Wpf` = validation-only adapter-2 composition sample, not onboarding
- `tools/AsterGraph.HelloWorld.Wpf` = validation-only adapter-2 proof sample, not parity
- `tools/AsterGraph.HostSample` = minimal consumer proof
- `tools/AsterGraph.PackageSmoke` = packaged-consumption proof
- `tools/AsterGraph.ScaleSmoke` = scale baseline plus history/state-continuity proof
- `src/AsterGraph.Demo` = showcase host
- `docs/en/versioning.md` = package version versus historical repository-tag guidance
- `docs/en/project-status.md` = external capability readiness gate for externally proven now, validation-only or bounded claims, and deferred until more adopter evidence
- `docs/en/evaluation-path.md` = single route ladder from first install to realistic hosted proof
- `docs/en/quick-start.md` = canonical adoption path
- `docs/en/stabilization-support-matrix.md` = frozen support boundary and upgrade guidance
- `docs/en/adapter-capability-matrix.md` = adapter matrix story and validation matrix
- `docs/en/alpha-status.md` = historical alpha reference for the current beta support story
- `docs/en/advanced-editing.md` = advanced-editing capability split and proof map
- `docs/en/adoption-feedback.md` = bounded public beta intake loop and reuse of support-bundle attachment notes
- `docs/en/adopter-triage.md` = adopter triage checklist for the one beta evidence contract
- `docs/en/support-bundle.md` = required support bundle format and collection workflow
- `docs/en/adoption-intake-dry-run.md` = synthetic rehearsal fixtures that stay internal and never widen support claims
