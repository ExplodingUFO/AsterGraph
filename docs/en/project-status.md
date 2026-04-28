# AsterGraph Project Status

## Current Status

- package baseline: `0.11.0-beta`
- matching public prerelease tag for this package line: `v0.11.0-beta`
- historical legacy repository milestone tag series: `v1.x`-style pre-launch checkpoints
- repo posture: public beta
- v0.56 adoption-readiness handoff markers: `ADOPTION_READINESS_HANDOFF_OK:True`, `ADOPTION_SCOPE_BOUNDARY_OK:True`, and `V056_MILESTONE_PROOF_OK:True`
- v0.58 authoring-depth handoff markers: `AUTHORING_DEPTH_HANDOFF_OK:True`, `AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True`, and `V058_MILESTONE_PROOF_OK:True`
- v0.59 large-graph UX baseline markers: `LARGE_GRAPH_UX_POLICY_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, and `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- v0.59 viewport LOD markers: `VIEWPORT_LOD_POLICY_OK:True`, `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`, `LARGE_GRAPH_BALANCED_UX_OK:True`, and `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- v0.59 edge interaction markers: `EDGE_INTERACTION_CACHE_OK:True`, `EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`, `SELECTED_EDGE_FEEDBACK_OK:True`, and `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- v0.59 mini-map and inspector projection markers: `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`, `INSPECTOR_NARROW_PROJECTION_OK:True`, `LARGE_GRAPH_PANEL_SCOPE_OK:True`, and `PROJECTION_PERFORMANCE_EVIDENCE_OK:True`
- v0.59 Large Graph UX handoff markers: `LARGE_GRAPH_UX_HANDOFF_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, and `V059_MILESTONE_PROOF_OK:True`
- v0.60 adapter-2 validation scope markers: `ADAPTER2_VALIDATION_SCOPE_OK:True`, `ADAPTER2_MATRIX_HANDOFF_OK:True`, and `ADAPTER2_SCOPE_BOUNDARY_OK:True`
- v0.60 WPF proof sample markers: `ADAPTER2_WPF_SAMPLE_PROOF_OK:True`, `ADAPTER2_CANONICAL_ROUTE_OK:True`, and `ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`
- public versioning guidance: [Versioning](./versioning.md)
- supported published packages:
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- frozen support boundary: [Stabilization Support Matrix](./stabilization-support-matrix.md)
- sample and proof tools:
  - `tools/AsterGraph.HelloWorld` for the quickest runtime-only first run
  - `tools/AsterGraph.Starter.Avalonia` for the shipped Avalonia starter scaffold
  - `tools/AsterGraph.HelloWorld.Avalonia` for the quickest hosted-UI first run after the starter scaffold
  - `tools/AsterGraph.ConsumerSample.Avalonia` for one realistic hosted integration before the full Demo shell
  - `tools/AsterGraph.HostSample` for the post-ladder proof harness on the canonical adoption route
  - `tools/AsterGraph.PackageSmoke` for packed-package proof
  - `tools/AsterGraph.ScaleSmoke` for the public scale baseline and state-continuity proof
- synthetic dry-run fixtures in [Synthetic Adoption Intake Dry-Run Fixtures](./adoption-intake-dry-run.md) are maintainer/internal rehearsal only, not external validation, and do not widen support or capability claims
- canonical adoption path:
  - runtime-only hosts use `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
  - Avalonia UI hosts use `AsterGraphEditorFactory.Create(...)` plus `AsterGraphAvaloniaViewFactory.Create(...)`
- active adapter validation target: `WPF` as adapter 2 under the [Adapter Capability Matrix](./adapter-capability-matrix.md)

## What Is Already Stable Enough To Evaluate

- the four-package SDK boundary
- kernel/session-first runtime ownership
- default Avalonia shell plus standalone surfaces
- runtime inspection surface for trusted, loaded, and blocked outcomes
- command/trust timeline and perf overlay in the showcase surface
- demo scenario presets remain host-owned proof catalog entries: `DEMO_SCENARIO_PRESETS_OK:True`
- ConsumerSample snippets remain host-owned proof catalog entries: `GRAPH_SNIPPET_CATALOG_OK:True`, `GRAPH_SNIPPET_INSERT_OK:True`, `FRAGMENT_LIBRARY_SEARCH_OK:True`, `FRAGMENT_LIBRARY_PREVIEW_OK:True`, `FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK:True`, `FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK:True`
- ConsumerSample authoring flow proof stays bounded to existing session commands: `AUTHORING_FLOW_PROOF_OK:True`, `AUTHORING_FLOW_HANDOFF_OK:True`, `AUTHORING_FLOW_SCOPE_BOUNDARY_OK:True`
- experience handoff proof is summarized without widening the support boundary: `EXPERIENCE_POLISH_HANDOFF_OK:True`, `FEATURE_ENHANCEMENT_PROOF_OK:True`, `EXPERIENCE_SCOPE_BOUNDARY_OK:True`
- authoring-depth proof summarizes the v0.58 port, validation, toolbar, and fragment-library convenience polish without new runtime model claims: `AUTHORING_DEPTH_HANDOFF_OK:True`, `AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True`, `V058_MILESTONE_PROOF_OK:True`
- large-graph UX baseline proof summarizes hosted performance mode, LOD policy, and widened-surface metrics without creating a new graph-size support tier: `LARGE_GRAPH_UX_POLICY_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, `LARGE_GRAPH_UX_PROOF_BASELINE_OK:True`
- viewport LOD proof keeps selected/hovered affordances on hosted workbench policy, not the runtime graph contract: `VIEWPORT_LOD_POLICY_OK:True`, `SELECTED_HOVERED_ADORNER_SCOPE_OK:True`, `LARGE_GRAPH_BALANCED_UX_OK:True`, `VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True`
- edge interaction proof summarizes existing edge quick-tool, toolbar, and geometry evidence on the hosted proof route without creating a runtime renderer contract: `EDGE_INTERACTION_CACHE_OK:True`, `EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True`, `SELECTED_EDGE_FEEDBACK_OK:True`, `EDGE_RENDERING_SCOPE_BOUNDARY_OK:True`
- panel projection proof summarizes mini-map lightweight projection and inspector narrow projection evidence on the hosted proof route without creating a broad graph subscription contract: `MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True`, `INSPECTOR_NARROW_PROJECTION_OK:True`, `LARGE_GRAPH_PANEL_SCOPE_OK:True`, `PROJECTION_PERFORMANCE_EVIDENCE_OK:True`
- v0.59 Large Graph UX handoff proof keeps phases 371-374 tied to existing hosted workbench evidence without expanding graph-size support claims: `LARGE_GRAPH_UX_HANDOFF_OK:True`, `LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True`, `V059_MILESTONE_PROOF_OK:True`
- adapter-2 validation scope proof keeps WPF on the same canonical route and matrix vocabulary without widening public WPF support or parity claims: `ADAPTER2_VALIDATION_SCOPE_OK:True`, `ADAPTER2_MATRIX_HANDOFF_OK:True`, `ADAPTER2_SCOPE_BOUNDARY_OK:True`
- WPF proof sample evidence keeps `AsterGraph.HelloWorld.Wpf` copyable and tied to the canonical session/runtime route without creating a second onboarding path: `ADAPTER2_WPF_SAMPLE_PROOF_OK:True`, `ADAPTER2_CANONICAL_ROUTE_OK:True`, `ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`
- graph-surface usability proof markers:
  - `COMMAND_SURFACE_OK:True`
  - `TIERED_NODE_SURFACE_OK:True`
  - `FIXED_GROUP_FRAME_OK:True`
  - `NON_OBSCURING_EDITING_OK:True`
  - `VISUAL_SEMANTICS_OK:True`
- advanced-editing closure markers:
  - `HIERARCHY_SEMANTICS_OK:True`
  - `COMPOSITE_SCOPE_OK:True`
  - `EDGE_NOTE_OK:True`
  - `EDGE_GEOMETRY_OK:True`
  - `DISCONNECT_FLOW_OK:True`
- plugin discovery, trust policy, loading, and inspection
- automation execution through `IGraphEditorSession.Automation`
- contract, maintenance, and release proof lanes
- packed `HostSample` compatibility proof under `.NET 10` in the release lane
- public API guidance proof stays with the template/plugin proof in the release story: `PUBLIC_API_SURFACE_OK`, `PUBLIC_API_SCOPE_OK`, `PUBLIC_API_GUIDANCE_OK`, `ASTERGRAPH_TEMPLATE_SMOKE_OK`, `TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK`

## Current Priorities

The current public-repo priority is turning the public beta into a coherent SDK surface rather than a pile of disconnected feature slices:

- public docs stay under `README.md`, `README.zh-CN.md`, `docs/en`, and `docs/zh-CN`
- advanced editing is described as canonical capability modules, not retained-only behavior
- source, tests, samples, proof tools, workflows, and governance files remain visible
- internal workflow traces and local-only files do not remain part of the tracked public repo surface

## Near-Term Roadmap

- keep the canonical runtime/session surface stable while broadening official capability modules and proof guidance
- keep public beta documentation and proof guidance easy to follow as advanced editing closes
- maintain hosted CI parity across the supported proof lanes
- continue the retained compatibility migration window without abrupt public breaks
- keep the shipped starter scaffold, runtime inspection surface, command/trust timeline, and perf overlay aligned with the canonical session-first route
- validate `WPF` as adapter 2 on the same canonical route and publish Avalonia/WPF status using `Supported`, `Partial`, and `Fallback`; do not read `Partial` or `Fallback` as parity

## External Capability Readiness Gate

Use this section as the single external capability readiness gate for release notes, maintainer replies, and beta intake. Maintainer-seeded rehearsal evidence is recorded in the adoption feedback loop, but only real external reports on the same bounded risk count toward the 3-5 gate. Every public claim below stays tied to route-level evidence instead of parity aspirations or internal confidence.

Every real external report used for this gate must carry the same intake vocabulary: report type, adopter context, route, version, proof markers, friction, support-bundle attachment note, and claim-expansion status. A single report does not widen public claims; support or capability expansion needs 3-5 real external reports clustered on the same bounded risk.

Current intake gate markers: `ADOPTION_INTAKE_EVIDENCE_OK:True`, `SUPPORT_BUNDLE_INTAKE_HANDOFF_OK:True`, and `REAL_EXTERNAL_REPORT_GATE_OK:True`.

### Externally proven now

| Claim | Route-level evidence |
| --- | --- |
| Canonical runtime/session route and the maintained evaluator ladder are externally proven on the defended beta line. | `tools/AsterGraph.HelloWorld`, `tools/AsterGraph.Starter.Avalonia`, `tools/AsterGraph.HelloWorld.Avalonia`, `tools/AsterGraph.ConsumerSample.Avalonia`, `tools/AsterGraph.HostSample`, `HOST_SAMPLE_OK`, `CONSUMER_SAMPLE_OK`, `GRAPH_SNIPPET_CATALOG_OK`, `GRAPH_SNIPPET_INSERT_OK`, `FRAGMENT_LIBRARY_SEARCH_OK`, `FRAGMENT_LIBRARY_PREVIEW_OK`, `FRAGMENT_LIBRARY_RECENTS_FAVORITES_OK`, `FRAGMENT_LIBRARY_SCOPE_BOUNDARY_OK`, `AUTHORING_FLOW_PROOF_OK`, `AUTHORING_FLOW_HANDOFF_OK`, `AUTHORING_FLOW_SCOPE_BOUNDARY_OK`, `EXPERIENCE_POLISH_HANDOFF_OK`, `FEATURE_ENHANCEMENT_PROOF_OK`, `EXPERIENCE_SCOPE_BOUNDARY_OK`, `AUTHORING_DEPTH_HANDOFF_OK`, `AUTHORING_DEPTH_SCOPE_BOUNDARY_OK`, `V058_MILESTONE_PROOF_OK` |
| The showcase authoring surface and host-owned runtime feedback are externally proven as a bounded beta host experience. | `src/AsterGraph.Demo`, `tools/AsterGraph.ConsumerSample.Avalonia`, `DEMO_OK:True`, `DEMO_SCENARIO_PRESETS_OK:True`, `COMMAND_SURFACE_OK:True`, `COMPOSITE_SCOPE_OK:True`, `EDGE_NOTE_OK:True`, `EDGE_GEOMETRY_OK:True`, `DISCONNECT_FLOW_OK:True`, `RUNTIME_DEBUG_PANEL_INTERACTION_OK:True`, `RUNTIME_LOG_LOCATE_OK:True`, `RUNTIME_LOG_EXPORT_OK:True`, `AI_PIPELINE_MOCK_RUNNER_POLISH_OK:True`, `AI_PIPELINE_PAYLOAD_PREVIEW_OK:True`, `AI_PIPELINE_ERROR_DEBUG_EVIDENCE_OK:True` |
| Packaged consumer proof is externally proven without widening the SDK boundary. | `tools/AsterGraph.PackageSmoke`, `PACKAGE_SMOKE_OK`, `HOST_SAMPLE_NET10_OK` |
| Scale proof is externally proven at defended `baseline`/`large` tiers and 5000-node `stress`: performance, authoring, SVG export, conservative PNG/JPEG raster export, and reload are defended. | `tools/AsterGraph.ScaleSmoke`, `SCALE_PERFORMANCE_BUDGET_OK:baseline:True`, `SCALE_PERFORMANCE_BUDGET_OK:large:True`, `SCALE_PERFORMANCE_BUDGET_OK:stress:True`, `SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800`, `SCALE_RASTER_EXPORT_STRESS_OK:True` |

### Validation-only or bounded claims

| Claim | Current public stance | Route-level evidence |
| --- | --- | --- |
| `WPF` as adapter 2 | Validation-only and not Avalonia parity or public WPF support. WPF support expansion stays blocked until 3-5 real external reports cluster on the same bounded risk. Current evidence is limited to the bounded hosted shell accessibility and performance/export-breadth paths. | `HELLOWORLD_WPF_OK`, `HOSTED_ACCESSIBILITY_BASELINE_OK`, `HOSTED_ACCESSIBILITY_FOCUS_OK`, `HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK`, `HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK`, `HOSTED_ACCESSIBILITY_OK`, `ADAPTER2_PERFORMANCE_BASELINE_OK`, `ADAPTER2_EXPORT_BREADTH_OK`, `ADAPTER2_PROJECTION_BUDGET_OK`, `ADAPTER2_COMMAND_BUDGET_OK`, `ADAPTER2_SCENE_BUDGET_OK`, `ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`, `ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`, [Adapter Capability Matrix](./adapter-capability-matrix.md) |
| Retained route | Migration-only bridge, not a new primary host path. | [Retained-To-Session Migration Recipe](./retained-migration-recipe.md), [Stabilization Support Matrix](./stabilization-support-matrix.md) |
| Stress raster export budget | PNG/JPEG export at 5000 nodes has conservative defended redlines; this is a regression guard, not a fast-export claim. | `SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800`, `SCALE_RASTER_EXPORT_STRESS_OK:True`, [ScaleSmoke Baseline](./scale-baseline.md) |
| XLarge telemetry | 10000-node ScaleSmoke is telemetry-only and not a support or virtualization claim. | `SCALE_TIER_BUDGET:xlarge:nodes=10000:selection=512:moves=128:budget=informational-only`, [ScaleSmoke Baseline](./scale-baseline.md) |

### Deferred until more adopter evidence

- faster defended claims beyond the conservative 5000-node `stress` raster export gates
- new hosted adapters or widened adapter claims beyond Avalonia plus the current `WPF` validation lane
- marketplace, remote install/update, unload lifecycle, or sandboxed plugin stories
- algorithm execution engine or workflow scripting UI stories beyond host-owned runtime feedback display evidence
- stable / GA / `1.0` support guarantees
- GA prep checklist: adoption evidence, API drift, support boundary, and release proof gates must all be reviewed before any GA or `1.0` language.
- Release-candidate proof handoff markers: `API_RELEASE_CANDIDATE_PROOF_OK:True`, `PUBLIC_API_GUIDANCE_HANDOFF_OK:True`, and `RELEASE_BOUNDARY_STABILITY_OK:True`.
- the current 0.xx alpha/beta hardening line is `Adoption Readiness / Release Candidate Hygiene`: keep the public recommendation, API drift, support boundary, and release proof gates aligned before any release-candidate, GA, or `1.0` language; `ADOPTION_RECOMMENDATION_CURRENT_OK:True` and `CLAIM_HYGIENE_BOUNDARY_OK:True` are the proof handoff markers
- seeded rehearsals do not count toward the 3-5 gate
- if a new report does not fit one of the proven or bounded rows above, route it through the [Adoption Feedback Loop](./adoption-feedback.md) and the [Beta Support Bundle](./support-bundle.md) instead of widening the claim ad hoc; the claim-expansion status remains a triage field until the 3-5 real external reports gate is met

## Public Entry Matrix

The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`; `HostSample` comes after that ladder as the proof harness.

- `tools/AsterGraph.HelloWorld` = first-run runtime-only sample
- `tools/AsterGraph.Starter.Avalonia` = shipped Avalonia starter scaffold
- `tools/AsterGraph.HelloWorld.Avalonia` = first-run hosted-UI sample after the starter scaffold
- `tools/AsterGraph.ConsumerSample.Avalonia` = realistic hosted integration sample
- `tools/AsterGraph.Starter.Wpf` = validation-only adapter-2 composition sample
- `tools/AsterGraph.HelloWorld.Wpf` = validation-only adapter-2 proof sample
- `tools/AsterGraph.HostSample` = post-ladder canonical adoption proof
- `tools/AsterGraph.PackageSmoke` = packed-package consumption proof
- `tools/AsterGraph.ScaleSmoke` = larger-graph baseline plus history/state-continuity proof
- `src/AsterGraph.Demo` = showcase host for visual/manual inspection

## Public Entry Points

- [Versioning](./versioning.md)
- [Beta Evaluation Path](./evaluation-path.md) = single route ladder from first install to realistic hosted proof
- [Quick Start](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [ScaleSmoke Baseline](./scale-baseline.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Plugin And Custom Node Recipe](./plugin-recipe.md)
- [Retained-To-Session Migration Recipe](./retained-migration-recipe.md)
- [Alpha Status](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Stabilization Support Matrix](./stabilization-support-matrix.md)
- [Demo Guide](./demo-guide.md)
