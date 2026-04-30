# Phase 439 Cookbook Scenario Coverage Matrix

**Milestone:** v0.74.0-beta Cookbook Scenario Depth And Component Polish
**Parent bead:** `avalonia-node-map-h3e.1`
**Generated:** 2026-04-30

## Coverage Summary

The active cookbook has five professional routes. Every route has at least one code anchor, demo/graph anchor, documentation anchor, scenario point set, proof marker, support boundary, route posture, and deferred gap. The current weakness is not missing base coverage; it is that the coverage posture is implicit across several files rather than visible as a first-class matrix.

| Recipe | Category | Graph / Demo Posture | Code Posture | Docs Posture | Proof Posture | Route Status | Scenario Kinds | Main Gap | Later Phase |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `starter-host-route` | StarterHost | AI pipeline Demo scenario anchor exists through `DemoGraphFactory.AiPipelineScenario`; graph cue is textual, not scenario-selectable. | Starter Avalonia host code is source-backed through `tools/AsterGraph.Starter.Avalonia/Program.cs`. | English and Chinese cookbook rows exist; route references host recipe ladder. | `FIVE_MINUTE_ONBOARDING_OK` is present and validated by catalog/proof tests. | Supported SDK route. | HostCodeExample, GraphOperations, SupportEvidence | Add selected-scenario cue that can point the visible graph to onboarding/pipeline context. | 440 |
| `authoring-surface-route` | Authoring | Demo presenter and tour anchors exist; graph operations are represented by edge overlays/previews. | ConsumerSample authoring and command projection anchors are source-backed. | Cookbook links authoring surface recipe and proof evidence. | `AUTHORING_SURFACE_OK` is present and validated. | Supported SDK route. | GraphOperations, NodeMetadata, SupportEvidence | Classify code anchors as supported API/sample route versus Demo presenter scaffolding. | 441 |
| `plugin-trust-route` | PluginTrust | Demo extensions projection exposes plugin trust state; no visible graph cue beyond detail text. | Demo plugin trust workspace and ConsumerSample proof anchors exist. | Cookbook links plugin host recipe. | `PLUGIN_TRUST_EVIDENCE_PANEL_OK` is present and validated. | Proof/demo route. | NodeMetadata, ValidationRuntimeOverlay, SupportEvidence | Make proof/demo boundary more visible and avoid implying sandbox/marketplace support. | 441 |
| `diagnostics-support-route` | DiagnosticsSupport | Runtime timeline and diagnostics projection anchors exist; graph command timeline is textual. | ConsumerSample support bundle and Demo runtime projection anchors exist. | Cookbook links support bundle docs. | `RUNTIME_OVERLAY_SUPPORT_BUNDLE_OK` is present and validated. | Proof/demo route. | ValidationRuntimeOverlay, SupportEvidence, GraphOperations | Add scenario cue for runtime/validation overlay state without telemetry or remote sync claims. | 440 |
| `review-help-route` | ReviewHelp | Demo proof surface projection exists; repair/help is not graph-highlightable. | ConsumerSample repair/help proof and validation feedback anchors exist. | Cookbook links feature catalog review/help row. | `REPAIR_HELP_REVIEW_LOOP_OK` is present and validated. | Proof/demo route. | ValidationRuntimeOverlay, SupportEvidence, HostCodeExample | Connect validation feedback scenario to visible graph/detail state and proof docs. | 440 / 443 |

## Existing Test Coverage

| Test File | Coverage Role |
| --- | --- |
| `DemoCookbookCatalogTests.cs` | Required metadata, categories, real anchors, scenario kind coverage, proof marker evidence, no view-model aggregation. |
| `DemoCookbookWorkspaceProjectionTests.cs` | Left navigation, selected recipe projection, graph-above-code content, route posture, filtered navigation. |
| `DemoCookbookProofClosureTests.cs` | Public cookbook proof markers, Demo proof integration, narrow ownership boundary. |
| `DemoCookbookDocsTests.cs` | English/Chinese cookbook recipe indexing, entry doc links, support boundaries, scenario depth proof docs. |
| `DemoCookbookVisualBaselineTests.cs` | Graph-above-detail layout and visual baseline. |
| `DemoCookbookNavigationTests.cs` / `DemoCookbookNavigationPolishTests.cs` | Navigation selection/filter feedback and graph preservation. |
| `DemoCookbookDetailReadabilityTests.cs` | Detail panel readability and line projection. |
| `DemoCookbookInteractionStateTests.cs` | Empty/deferred/unavailable interaction states. |

## Gap Backlog For Later Phases

| Gap | Evidence | Target Phase | Recommended Acceptance |
| --- | --- | --- | --- |
| Scenario points are not independently selectable. | UI audit: scenario points are displayed as text in scenario detail mode only. | 440 | Selected scenario projection exists and updates bounded content cues while preserving live graph host. |
| Graph cues are textual only. | `GraphAnchors` are converted from `DemoAnchors`; no node/connection/viewport/runtime cue is modeled. | 440 | Scenario cue model can reference visible graph/demo context without executing code. |
| Code anchors lack route classification. | Catalog has anchors, but not supported SDK/sample/demo-only classification. | 441 | Code panel can distinguish supported SDK route, sample host code, and Demo-only scaffolding. |
| Route posture is category-derived. | `CreateRoutePosture` maps by category, not recipe-specific details. | 441 | Professional recipes can express route clarity without widening support claims. |
| Demo Guide proof mode omits cookbook proof markers. | Docs/proof audit: `DemoProof` emits cookbook markers, demo-guide docs do not list the full set. | 443 | Demo Guide lists `DEMO_COOKBOOK_OK` and cookbook closure marker expectations. |
| Proof markers are strings without owner classification. | `ProofMarkers` are string arrays; tests verify existence but not owner/test/doc strength. | 443 | Proof closure maps markers to owning tests/docs locales. |

## No-God-Code Boundary

Keep the matrix as planning/proof context. Do not collapse route posture, scenario cues, code route classification, and docs/proof ownership into one large production registry. Later phases should extend narrow existing seams:

- `DemoCookbookWorkspaceProjection` for projection/cues.
- `MainWindowViewModel.CookbookDetails` for selected detail/scenario state.
- `DemoCookbookCatalogTests` for catalog contract validation.
- `DemoCookbookDocsTests` and `DemoCookbookProofClosureTests` for docs/proof closure.
