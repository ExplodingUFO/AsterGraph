# Phase 444: Desktop Graph Library Capability Audit

**Bead:** `avalonia-node-map-mqm.1`
**Date:** 2026-04-30
**Mode:** Read-only capability audit

## Executive Result

AsterGraph already has substantial defended library evidence: session-first runtime contracts, command/query/event APIs, Avalonia hosted controls, custom node presentation, runtime overlay snapshots, ConsumerSample/HelloWorld routes, Demo cookbook proof, ScaleSmoke budgets, and CI-sensitive proof markers.

The main v0.75 gap is not a missing product rewrite. The gap is that several capabilities are defended in separate surfaces but not yet packaged as a coherent high-performance desktop graph library contract with visible-scene proof, composable interaction contracts, extension-surface guidance, cross-platform host proof, and professional example routes.

## Capability Matrix

| Requirement | Defended Evidence | Gap | Narrow Target | Future Phase |
|-------------|-------------------|-----|---------------|--------------|
| LIB-01 | Project requirements, public API inventory, ConsumerSample proof, Demo cookbook, HelloWorld routes, package split, session-first contracts. | Evidence is broad but scattered; future phases need a single baseline handoff. | Use this audit as the baseline and keep later phases file-bounded. | 444 |
| RENDER-01 | `IGraphEditorCommands`, `GraphEditorKernelViewportCoordinator`, `ViewportMath`, `NodeCanvas`, `NodeCanvasSceneHost`, `NodeCanvasConnectionSceneRenderer`, `GraphMiniMap`, `AsterGraphWorkbenchPerformancePolicy`, `tools/AsterGraph.ScaleSmoke`, ScaleSmoke tests, scale docs. | Runtime viewport budgets are defended, but visible Avalonia scene rendering/projection budgets are weaker. Scene layers rebuild without culling/recycling evidence. Minimap freshness is a policy tradeoff, not a measured cadence. | Add measured visible-scene projection/refresh proof, viewport command latency, minimap cadence/budget tests, and a compact viewport control/readout surface using existing commands. Do not widen graph-size claims. | 445 |
| INTERACT-01 | `IGraphEditorSession`, commands/queries/events, command router, selection coordinator, connection mutation coordinator, pointer/drag coordinators, session tests, delete/reconnect/detach tests, wire selection/slicing tests, connection geometry tests, command shortcut tests/docs. | Interaction semantics are split between adapter-neutral session commands and Avalonia pointer internals. Direct wire hit selection and endpoint-specific reconnect/bend-handle editing need clearer stock interaction proof. Shortcut docs appear stale against current opt-out API. | Add smallest adapter-neutral interaction snapshot/routing primitives where useful; prove direct wire hit selection, bend-point handles over existing route commands, and current shortcut docs/tests. Avoid macro/query/scripting systems. | 446 |
| CUSTOM-01 | `IGraphNodeVisualPresenter`, `GraphNodeVisual`, `GraphNodeVisualContext`, `AsterGraphPresentationOptions`, `IGraphRuntimeOverlayProvider`, inspector presenters, parameter editor registry, ConsumerSample custom visual/overlay, runtime overlay tests, docs recipes. | Custom node, handles/anchors, edge overlay, runtime decoration, inspector, and marker vocabulary are supported in pieces but not yet presented as one extension surface. No public custom edge presenter seam; supported custom edge path should be clarified. | Document and prove one package-owned extension surface: custom node presenter lifecycle, handles as port/target anchors, custom edge through stock styling or host overlay snapshots, runtime decoration to inspector, and proof markers. | 447 |
| PLATFORM-01 | Avalonia hosted adapter, HelloWorld starter, ConsumerSample, WPF validation tests, templates, CI net8/net9/net10 awareness, package smoke precedent. | Templates do not yet scaffold optional custom presentation/runtime overlay hooks. Cross-platform wording must stay tied to tested host routes. WPF should remain validation-only. | Harden starter/template/package proof across Windows, Linux, and macOS route wording; add optional extension hook examples without claiming unsupported adapters. | 448 |
| EXAMPLE-01 | Demo cookbook catalog/projection, docs recipes, ConsumerSample proof, route clarity tests, source-backed route evidence. | Example ladder is broad but scattered. Custom extension and performance examples need clearer "which sample to copy" flow. | Consolidate cookbook/docs into a professional example suite covering rendering, viewport, interaction, customization, performance, and host integration with code, route, proof, boundary, and verification guidance. | 449 |
| PROOF-01 | Existing ConsumerSample, Demo, WPF, HelloWorld, ScaleSmoke, release closure, docs assertion, and proof marker tests. | No single v0.75 release proof cluster ties library-grade rendering, interaction, customization, packaging, examples, and requirements traceability together. | Add library-grade closure markers, traceability, performance-sensitive gates, docs alignment, and clean beads/Dolt/Git handoff. | 450 |

## Defended Capability Notes

### Rendering And Viewport

- Runtime viewport commands already cover pan, zoom, resize, reset, fit scene, fit selection, focus selection, center node, and center point.
- `ViewportMath` provides screen/world conversion and anchor-preserving zoom math.
- Avalonia `NodeCanvas` wires viewport gestures, transform updates, scene host updates, grid background, and minimap hooks.
- `GraphMiniMap` supports session-backed rendering, custom presenter injection, recentering, and lightweight projection policy.
- ScaleSmoke defends runtime/session scale budgets, but visible canvas rendering should not inherit those claims without a dedicated Avalonia proof.

### Interaction And Connection

- Session APIs expose commands, queries, events, diagnostics, automation, and mutation batching.
- Selection, multi-selection, marquee, connection selection, pending connection snapshots, compatibility feedback, delete/reconnect/detach, wire slicing, route vertices, command descriptors, disabled reasons, and shortcut routing are covered by focused tests.
- Pointer gesture state remains mostly Avalonia-internal. Future work should expose only narrow reusable interaction state, not move the whole visual gesture system into the core.

### Customization And Host Surface

- Custom node presentation and host-owned visual examples exist.
- Handles are already represented through port anchors/target anchors; later work should clarify this instead of introducing a second handle model.
- Runtime overlays and inspector metadata are already source-backed and tested.
- Custom edge support should be framed as stock renderer styling/metadata plus host overlay from geometry snapshots unless a later phase proves a new seam is necessary.

### Packaging, Examples, And Proof

- Starter, ConsumerSample, Demo cookbook, HelloWorld, WPF validation, templates, docs, and proof markers provide a strong base.
- The next product step is consolidation: examples must tell host authors what to copy and which route is supported.
- Release proof should aggregate requirement coverage rather than depend on scattered historical markers.

## Future Phase Handoff

### Phase 445 - Rendering And Viewport Performance Foundation

Primary files:
- `src/AsterGraph.Editor/Kernel/Internal/Viewport/GraphEditorKernelViewportCoordinator.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs`
- `src/AsterGraph.Editor/Viewport/ViewportMath.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasSceneHost.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Scene/NodeCanvasConnectionSceneRenderer.cs`
- `src/AsterGraph.Avalonia/Controls/GraphMiniMap.cs`
- `src/AsterGraph.Avalonia/Hosting/AsterGraphWorkbenchPerformancePolicy.cs`
- `tools/AsterGraph.ScaleSmoke/Program.cs`
- `tests/AsterGraph.ScaleSmoke.Tests/ScaleSmokeBudgetTests.cs`

First targets:
- Visible-scene projection/budget probe.
- Minimap cadence contract.
- Viewport control/readout surface using existing commands.
- Grid/background invalidation proof.

### Phase 446 - Composable Interaction And Connection Model

Primary files:
- `src/AsterGraph.Editor/Runtime/IGraphEditorSession.cs`
- `src/AsterGraph.Editor/Runtime/IGraphEditorCommands.cs`
- `src/AsterGraph.Editor/Runtime/Commands/GraphEditorSessionCommands.cs`
- `src/AsterGraph.Editor/Runtime/Queries/GraphEditorSessionQueries.cs`
- `src/AsterGraph.Editor/Kernel/Internal/CommandRouting/GraphEditorKernelCommandRouter.cs`
- `src/AsterGraph.Editor/Kernel/Internal/ConnectionMutation/GraphEditorKernelConnectionMutationCoordinator.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Interaction/NodeCanvasPointerInteractionCoordinator.cs`
- `src/AsterGraph.Avalonia/Controls/Internal/Interaction/NodeCanvasNodeDragCoordinator.cs`
- `docs/en/interactions-and-shortcuts.md`
- `tests/AsterGraph.Editor.Tests/GraphEditorSessionTests.cs`
- `tests/AsterGraph.Editor.Tests/NodeCanvasPointerInteractionCoordinatorTests.cs`

First targets:
- Adapter-neutral interaction snapshot or routing primitives only where concrete.
- Direct wire hit selection through existing command route.
- Bend-point drag handles over existing route-vertex commands.
- Shortcut documentation repair and assertion coverage.

### Phase 447 - Custom Node Edge And Overlay Extension Surface

Primary files:
- `docs/en/public-api-inventory.md`
- `src/AsterGraph.Avalonia/Presentation/AsterGraphPresentationOptions.cs`
- `src/AsterGraph.Avalonia/Presentation/IGraphNodeVisualPresenter.cs`
- `src/AsterGraph.Avalonia/Presentation/GraphNodeVisual.cs`
- `src/AsterGraph.Avalonia/Controls/NodeCanvas.axaml.cs`
- `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleNodeVisualPresenter.cs`
- `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleConnectionOverlay.cs`
- `tools/AsterGraph.ConsumerSample.Avalonia/ConsumerSampleProof.cs`
- `src/AsterGraph.Editor/Runtime/IGraphRuntimeOverlayProvider.cs`
- `docs/en/authoring-surface-recipe.md`
- `docs/en/demo-cookbook.md`

First targets:
- Cohesive custom extension guide and proof marker cluster.
- Handles/anchors vocabulary.
- Edge overlay route using geometry snapshots.
- Runtime decoration plus inspector recipe.

### Phase 448 - Cross-Platform Host Packaging And Templates

Primary files:
- `templates/astergraph-avalonia/README.md`
- `templates/astergraph-plugin/README.md`
- `tools/AsterGraph.HelloWorld.Avalonia`
- `tools/AsterGraph.ConsumerSample.Avalonia`
- `src/AsterGraph.Wpf`
- `tests/AsterGraph.HelloWorld.Tests`
- `tests/AsterGraph.ConsumerSample.Tests`
- `tests/AsterGraph.Wpf.Tests`
- `.github/workflows`

First targets:
- Copyable host route proof.
- Template extension hook examples.
- OS/framework wording tied to CI/local evidence.

### Phase 449 - Professional Examples And Documentation Suite

Primary files:
- `src/AsterGraph.Demo/Cookbook`
- `tests/AsterGraph.Demo.Tests/DemoCookbookWorkspaceProjectionTests.cs`
- `docs/en/demo-cookbook.md`
- `docs/en/host-recipe-ladder.md`
- `docs/en/consumer-sample.md`
- `docs/en/custom-node-host-recipe.md`
- `docs/en/authoring-surface-recipe.md`
- `docs/en/widened-surface-performance-recipe.md`

First targets:
- One professional example suite with code, route, proof marker, support boundary, and verification guidance.
- Preserve Demo as proof/example layer, not package API.

### Phase 450 - Library-Grade Proof And Release Gate Closure

Primary files:
- `tests/AsterGraph.ConsumerSample.Tests/ConsumerSampleProofTests.cs`
- `tests/AsterGraph.Demo.Tests/DemoProofReleaseSurfaceTests.cs`
- `tests/AsterGraph.Demo.Tests/ReleaseClosureContractTests.cs`
- `docs/en/public-api-inventory.md`
- `docs/en/project-status.md`
- `.planning/REQUIREMENTS.md`
- `.planning/ROADMAP.md`
- `.beads/issues.jsonl`

First targets:
- v0.75 proof marker cluster.
- Requirement-to-proof trace.
- Full verification gate and clean handoff.

## Read-Only Agent Handoff

- Rendering/viewport agent found strong runtime viewport proof and weaker visible Avalonia scene budget proof.
- Interaction/connection agent found strong session command contracts and identified narrow stock interaction gaps: wire hit selection, bend handles, shortcut docs, and optional compact interaction telemetry.
- Customization/package/docs agent found strong custom node/runtime overlay/inspector evidence and identified the need to consolidate the extension surface without exposing internal layers.

## Non-Goals Confirmed

- No runtime architecture rewrite.
- No compatibility or fallback layer.
- No generated runnable code execution in Demo.
- No macro/query/scripting system.
- No unsupported adapter/platform expansion.
- No new graph-size claims beyond defended evidence.
- No external project names in docs or planning artifacts.
