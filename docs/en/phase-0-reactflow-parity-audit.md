# Phase 0 React Flow Parity Audit

## Phase 478 Refresh

This document started as the Phase 0 audit for the `0.11.0-beta` baseline. Phase 478 refreshes it against the current `master` state so the next issue wave does not duplicate work already finished by GitHub #46-#52 and their follow-up slices.

The scope of this refresh is docs and tracker state only. It does not change product code, runtime behavior, renderer contracts, or public support claims.

## Phase 490 Update

Phase 490 is GitHub #103 / `avalonia-node-map-3x0`, a stale-roadmap repair after Phases 485-489 closed. This slice is docs/tests only: it updates the bilingual parity roadmap, removes completed work from the candidate queue, and selects the accessibility breadth audit as the next open parity gap. No Core/Editor/Avalonia runtime or public API changes are in scope.

## Phase 491 Update

Phase 491 is GitHub #105 / `avalonia-node-map-44i`, the accessibility breadth audit selected by Phase 490. This slice remains docs/tests first: it audits built-in public Avalonia controls and hosted shell states against existing automation names, focusability, keyboard routes, and intentionally decorative surfaces. It does not authorize retained migration removal, public API changes, visual redesign, or full screen-reader certification claims without manual assistive-technology validation.

## Phase 492 Update

Phase 492 is GitHub #107 / `avalonia-node-map-j8v`, the retained migration removal roadmap selected after the Phase 491 accessibility breadth audit closed. This slice is inventory now, remove later: it records the retained bridge, explicit legacy import, and already-retired compatibility-only surfaces that must stay classified before any removal issue changes API shape. It does not authorize runtime behavior changes, public API deletion, public API baseline updates, or UI changes.

## Phase 493 Update

Phase 493 is GitHub #109 / `avalonia-node-map-8qv`, the custom node presenter cookbook parity proof selected after the retained migration roadmap closed. This slice is docs/tests only: it ties the partial custom-node parity row to the defended host-owned presenter route in the Custom Node Host Recipe, including `NodeBodyPresenter`, `NodeVisualPresenter`, `NodeDragHandle`, anchor maps, host-owned edge overlays, and `CUSTOM_EXTENSION_SURFACE_OK` proof markers. It does not authorize runtime behavior changes, public API changes, React Flow hooks/components parity claims, Demo UI redesign, or screenshot-gate expansion.

## Phase 494 Update

Phase 494 is GitHub #111 / `avalonia-node-map-p5z`, a localized full-window shell visual gate expansion after the custom-node presenter parity proof closed. This slice is docs/tests only: it adds shell-state-owned language/theme metadata and a Chinese Cookbook drawer capture, `shell-cookbook-default-open-zh-cn`, beside the existing English Cookbook and runtime diagnostics captures. It does not authorize strict pixel hash baselines, runtime UI changes, broad language/theme certification, flyout capture, popup capture, or context-menu capture.

## Phase 495 Update

Phase 495 is GitHub #113 / `avalonia-node-map-wzt`, a post-Phase-494 roadmap refresh after all prior next-wave entries closed. This slice is docs/tests only: it refreshes the active issue split from current GitHub and Beads evidence, records at least three concrete follow-up candidates, and keeps worktree/parallelism guidance current. It authorizes no Core/Editor/Avalonia runtime changes, no public API changes, no Demo UI redesign, no retained API deletion, no strict pixel baselines, and no broad React Flow parity claim expansion.

## Phase 497 Update

Phase 497 is GitHub #117 / `avalonia-node-map-5nl`, a closed-drawer shell visual gate expansion after the Cookbook architecture contract closed. This slice is docs/tests/manifest only: it adds `shell-cookbook-default-closed` beside the open Cookbook and runtime shell captures, asserts `expectedPaneOpen: false`, and limits required parts to visible closed-shell chrome. It does not authorize runtime UI changes, public API changes, strict pixel baselines, flyout capture, popup capture, context-menu capture, or broad language/theme certification.

## Phase 498 Update

Phase 498 is GitHub #119 / `avalonia-node-map-3um`, the retained migration removal execution gate selected after the closed-drawer shell visual gate closed. This slice is docs/tests only: it turns the Phase 492 inventory into an execution gate with exact symbols, blocker tests, support-window criteria, migration evidence, and the later `eng/public-api-baseline.txt` approval path required before any removal PR. It authorizes no retained API removal, no public API baseline change, no runtime behavior change, and no UI change.

## Phase 499 Update

Phase 499 is GitHub #121 / `avalonia-node-map-9x7`, the renderer virtualization execution-boundary proof selected after the retained migration removal execution gate closed. This slice is docs/tests only: it pins the current evidence to viewport-budgeted scene projection/rendering, records the execution proof required before any ItemsRepeater/Skia-style renderer virtualization, background graph index, or graph-size support claim expansion, and keeps `xlarge` telemetry-only. It does not authorize a renderer rewrite, benchmark harness implementation, public API change, runtime behavior change, UI change, or support-claim expansion.

## Phase 500 Update

Phase 500 is GitHub #123 / `avalonia-node-map-66t`, the selected runtime shell visual gate state chosen after the renderer virtualization execution-boundary proof closed. This slice is manifest/docs/tests only: it adds `shell-runtime-diagnostics-closed` beside the existing runtime diagnostics open state, asserts `expectedPaneOpen: false`, and limits required parts to visible closed-shell chrome. It does not authorize runtime behavior change, public API change, styling redesign, strict pixel baselines, flyout capture, popup capture, context-menu capture, broad language/theme certification, retained API removal, or renderer virtualization work.

## Phase 501 Update

Phase 501 is GitHub #125 / `avalonia-node-map-38n`, the post-Phase-500 parity follow-up queue refresh. This slice is docs/tests only: it replaces the generic Phase 501 placeholder with concrete next candidates from the current React Flow parity matrix, separating renderer virtualization execution proof, declarative API ergonomics, layout provider evidence, manual assistive-technology validation, and broader shell visual coverage into independently trackable follow-ups. It authorizes no runtime behavior change, no public API change, no UI redesign, no strict pixel baselines, no retained API removal, and no renderer virtualization implementation.

## Phase 502 Update

Phase 502 is GitHub #127 / `avalonia-node-map-mai`, the renderer virtualization execution proof selected after the post-Phase-500 queue refresh. This slice remains docs/tests only: it ties focused renderer tests, scale docs, a proof command, and artifact metadata into an executable proof contract for future work while keeping the current claim at viewport-budgeted scene projection/rendering rather than true renderer virtualization. It requires non-informational renderer thresholds before any future support claim, and it authorizes no runtime behavior change, no public API change, no UI redesign, no retained API removal, and no renderer virtualization implementation.

## Phase 503 Update

Phase 503 is GitHub #129 / `avalonia-node-map-mzu`, the declarative API ergonomics audit selected after the renderer virtualization execution proof closed. This slice remains docs/tests only: `DECLARATIVE_API_ERGONOMICS_AUDIT` maps current copyable API entry points to `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`, `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`, `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()`, `templates/astergraph-avalonia`, and `src/AsterGraph.Demo`. The current support claim stays partial: these routes are not equivalent to React Flow hooks/components and do not provide a `<ReactFlow>`-equivalent declarative DSL. This slice authorizes no runtime behavior change, no public API change, no UI redesign, no retained API removal, and no public API baseline change.

## Phase 504 Update

Phase 504 is GitHub #131 / `avalonia-node-map-8lf`, the layout provider evidence expansion selected after the declarative API ergonomics audit closed. This slice remains docs/tests only: `LAYOUT_PROVIDER_EVIDENCE_EXPANSION` ties `IGraphLayoutProvider.CreateLayoutPlan(GraphLayoutRequest)`, `GraphLayoutRequest`, `GraphLayoutPlan`, `IGraphEditorQueries.CreateLayoutPlan(...)`, `PreviewLayoutPlan`, `TryApplyLayoutPlan`, `TryApplyLayoutRequest`, `TrySnapSelectedNodesToGrid`, `TrySnapAllNodesToGrid`, `GraphEditorLayoutProviderSeamTests`, and the Cookbook `performance-viewport-route` evidence into one synchronous, host-owned proof story. It does not authorize async/cancellable provider execution, runtime behavior changes, public API changes, UI redesign, retained API removal, or a new layout engine.

## Phase 505 Update

Phase 505 is GitHub #133 / `avalonia-node-map-b4z`, the accessibility manual assistive-technology validation plan selected after the layout provider evidence expansion closed. This slice remains docs/tests only: `ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN` documents the manual assistive-technology validation checklist for the existing hosted accessibility route, keeps current headless automation, focus, keyboard, and accessible-name evidence separate from unverified live screen-reader behavior, and records the boundary as no live-region/runtime behavior change, no UI change, no public API change, no retained API removal, and no broad screen-reader certification claim.

## Phase 506 Update

Phase 506 is GitHub #135 / `avalonia-node-map-h7c`, the broader shell visual coverage planning slice selected after the accessibility manual assistive-technology validation plan closed. This slice remains docs/tests only: `SHELL_VISUAL_COVERAGE_PLANNING` maps the current five manifest-driven full-window shell captures to future flyout capture, popup capture, context-menu capture, additional language/theme variants, and pixel-baseline drift measurement. It authorizes no manifest rows, no strict pixel baselines, no runtime UI changes, no public API changes, no retained API removal, and no broad visual/language/theme certification.

## Phase 507 Update

Phase 507 is GitHub #137 / `avalonia-node-map-3tw`, the post-Phase-506 visual queue refresh. This slice is docs/tests only: it repairs the active queue after Phases 503-506 closed and converts the Phase 506 visual planning candidates into concrete next tracker candidates, GitHub #139-#143 / `avalonia-node-map-2nu`, `avalonia-node-map-0ff`, `avalonia-node-map-8lu`, `avalonia-node-map-9rq`, and `avalonia-node-map-1j4`. It authorizes no runtime UI behavior changes, no shell-state manifest rows, no strict pixel baselines, no public API changes, no retained API removal, and no broad visual/language/theme certification.

## Phase 508 Update

Phase 508 is GitHub #139 / `avalonia-node-map-2nu`, the shell flyout visual capture selected by the post-Phase-506 queue refresh. This slice is shell visual gate harness/manifest/docs/tests only: it adds `shell-cookbook-default-view-menu-flyout`, opens `PART_ViewMenu`, records `CaptureScope=full-window-shell-flyout-state`, and verifies the bounded View menu headers in generated artifact metadata. It authorizes no runtime UI behavior change, no public API change, no strict pixel baselines, no popup or context-menu coverage, no broad language/theme certification, and no retained API removal.

## Phase 489 Update

Phase 489 closed GitHub #101 / `avalonia-node-map-6sc` through PR #102 as a renderer virtualization design spike on branch `perf/renderer-virtualization-spike`. This slice was docs/tests only: it defined the proof contract required before any future ItemsRepeater/Skia-style renderer virtualization, background graph index, or graph-size claim expansion. It made no public API change and no runtime change. The current evidence remains viewport-budgeted scene projection/rendering, not a true renderer virtualization contract; `xlarge` stays telemetry-only.

## Phase 488 Update

Phase 488 closed GitHub #99 / `avalonia-node-map-ce1` through PR #100 for the P2 layout provider and background/cancel proof refresh. The slice was docs/tests-first: keep `IGraphLayoutProvider`, `PreviewLayoutPlan`, `TryApplyLayoutPlan`, `TryApplyLayoutRequest`, snap-to-grid commands, and the `background-grid-density` route tied to existing source-backed proof. Do not widen the synchronous provider seam into async or cancellable layout execution in this issue.

## Phase 487 Update

Phase 487 closed GitHub #97 / `avalonia-node-map-i8s` through PR #98 by hardening the custom-node copyable-host recipe docs and templates. It stayed inside host recipe docs, templates, Demo cookbook route text, and consistency tests with no Core/Editor/Avalonia API changes.

## Phase 486 Update

Phase 486 closed GitHub #95 / `avalonia-node-map-0xr` through PR #96 by expanding the full-window shell gate without adding strict pixel baselines or Demo runtime changes. `CookbookShellVisualGateStates.json` now drives two deterministic shell states, `shell-cookbook-default-open` and `shell-runtime-diagnostics-open`, against the same default `starter-host-route` / `ai-pipeline` fixture. The gate validates drawer state, dimensions, nonblank pixels, distinct colors, output metadata, and required named shell parts while leaving flyouts, context menus, language/theme variants, and hash baselines out of scope.

## Phase 485 Update

Phase 485 closed GitHub #93 / `avalonia-node-map-3hc` through PR #94 by clarifying the Cookbook host-copy architecture path. The completed work documents the guarded host-copy architecture route and bilingual guidance without changing Core/Editor/Avalonia runtime APIs.

## Phase 484 Update

Phase 484 refreshes the roadmap after the Phase 478-483 wave closed and after Phase 480 delivered the rotatable-node surface. The previous next-wave queue was stale because GitHub #80 / `avalonia-node-map-p480` is now closed. This update keeps the work docs-only, replaces the one-item queue with a prioritized split plan, and keeps public claims tied to current source, tests, docs, and CI evidence.

## Phase 481 Update

Phase 481 adds a deterministic full-window Cookbook shell capture alongside the existing scene PNG gate. The new gate covers the default Cookbook shell route, writes artifacts under `artifacts/test-results/cookbook-shell-visual-gate`, and validates dimensions, nonblank pixels, shell-part coverage, and metadata. Strict pixel-baseline comparison remains intentionally deferred until Skia/native drift is measured across CI hosts.

## Phase 480 Update

Phase 480 closes GitHub #80 by adding persisted node rotation as an explicit surface contract. The supported route is `GraphNodeSurfaceState.RotationDegrees` in the persisted model, `IGraphEditorCommands.TrySetNodeRotation(...)` for mutation, and `GraphEditorNodeSurfaceSnapshot.RotationDegrees` for host projection. `GraphNodeRotationGeometry` remains a geometry helper; public guidance points hosts at the command/query state contract instead of treating the helper as the primary integration API.

## Phase 479 Update

Phase 479 adds `NodeDragHandle` in `AsterGraph.Avalonia.Presentation` as the public hosted-Avalonia route for React Flow-style node drag handles. Hosts mark a control in a stock-shell custom node body presenter; the stock node shell then starts node dragging only from the marked handle or its descendants, without depending on Demo-only private hooks.

## Phase 483 Update

Phase 483 closes GitHub #82 by choosing the bounded-docs path instead of a renderer rewrite. Current evidence supports a viewport-budgeted scene projection/rendering contract: the visible-scene projector computes node/group/connection IDs, `NodeCanvasSceneHost` materializes visible node/group visuals when viewport dimensions and zoom are available, and `NodeCanvasConnectionSceneRenderer` scopes committed connection routes while preserving pending previews. This is not a true renderer virtualization contract or a 10000-node support tier; `xlarge` remains telemetry-only.

## Repository Baseline

- Current package version is `0.11.0-beta` in `Directory.Build.props`; publishable libraries are `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Publishable libraries target `net8.0;net9.0;net10.0`; Demo and test projects currently target `net9.0`.
- `AsterGraph.Demo` remains the visible demo host and Cookbook proof surface.
- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.Recipes.cs` currently contains 25 `DemoCookbookRecipe` entries.
- `tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json` currently defines 15 deterministic scene PNG capture routes.
- `tests/AsterGraph.Demo.Tests/DemoCookbookScreenshotGateTests.cs` captures canonical graph scenes through `GraphSceneImageExportService`, writes `artifacts/test-results/cookbook-screenshot-gate/metadata.json`, validates route metadata, and captures manifest-driven full-window shell states under `artifacts/test-results/cookbook-shell-visual-gate`.
- `tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json` currently defines `shell-cookbook-default-open`, `shell-cookbook-default-open-zh-cn`, `shell-cookbook-default-closed`, `shell-runtime-diagnostics-open`, and `shell-runtime-diagnostics-closed`.
- `.planning/` and `docs/plans/` are gitignored in this repository. Phase 478 treats the `.planning/*` write-set references in GitHub #79 and Beads `avalonia-node-map-p478` as tracker drift; durable planning state for this slice lives in tracked docs plus GitHub/Beads issues.

## Architecture Inventory

| Layer | Current owner | Current evidence | Phase 478 assessment |
| --- | --- | --- | --- |
| Host-facing identifiers and contracts | `src/AsterGraph.Abstractions` | Node definitions, catalogs, styling, builder helpers, compatibility policies, and plugin-neutral extension points. | Still the dependency floor for hosts; keep new parity contracts thin and explicit. |
| Persisted data model and serialization | `src/AsterGraph.Core` | `GraphDocument`, groups, scopes, edge presentation, schema migration helpers, and legacy import boundary. | Compatibility cleanup from #48 is complete; new model fields such as rotation must name schema behavior up front. |
| Session, commands, queries, state services | `src/AsterGraph.Editor` | Selection, undo/redo, clipboard, workspace save/load, connection authoring, validation, layout, events, mutations, and presentation snapshots. | Strong canonical route. New parity work should go through commands/queries rather than bypassing session ownership. |
| Layout integration | `src/AsterGraph.Editor/Runtime` and services | `GraphLayoutRequest`, `GraphLayoutPlan`, `IGraphLayoutProvider`, preview/apply evidence, command-surface cancel evidence, and snap/grid commands. | Provider seam exists and is synchronous; background/provider examples need continued proof when layout claims expand, and cancellation wording must not imply async layout cancellation. |
| Avalonia rendering and interaction | `src/AsterGraph.Avalonia` | `NodeCanvas`, scene host projection, edge renderer, interaction coordinators, automation peers, MiniMap, Background, Controls, Panel, NodeToolbar, EdgeToolbar, NodeResizer, `NodeDragHandle`, and stock rotation projection. | Built-ins are now reusable public components. Remaining UI parity gaps are broader full-window visual regression coverage, Cookbook example ergonomics, and any future claim that would require true renderer virtualization. |
| Hosted workbench shell | `GraphEditorView` and hosted factories | Header, library, canvas, inspector, validation panel, authoring tools, minimap, command projection, and status chrome. | Useful supported route, but new public features should avoid making Demo shell chrome a hidden dependency. |
| Demo/Cookbook | `src/AsterGraph.Demo` | 25 recipes, route clarity docs, built-in/interaction/lifecycle batches, scene PNG screenshot gate metadata, and five manifest-driven full-window shell visual states. | Cookbook breadth, scene coverage, English default open/closed shell captures, English runtime drawer open/closed captures, and a Chinese Cookbook drawer capture are covered. Pixel-baseline comparison plus broader flyout/popup/context-menu/theme/language shell-state coverage remain bounded future work. |
| CI and release gates | `.github/workflows`, `eng/ci.ps1`, test projects | Build/test/maintenance/contract/release/hygiene lanes, public API baseline, package validation, docs route checks, scene PNG gate tests, and manifest-driven full-window shell gate tests. | Strong text/API/scene/shell gates. Keep strict pixel baselines deferred until deterministic drift is measured. |

## React Flow Parity Matrix

| Capability | Current status | Evidence / remaining gap | Next action |
| --- | --- | --- | --- |
| Custom nodes with arbitrary Avalonia content | Partial / supported through AsterGraph idioms | Node definitions, `AsterGraphPresentationOptions.NodeBodyPresenter`, `AsterGraphPresentationOptions.NodeVisualPresenter`, `NodeDragHandle`, `GraphNodeVisual.PortAnchors`, `GraphNodeVisual.ConnectionTargetAnchors`, host-owned edge overlays from `GetConnectionGeometrySnapshots()`, the Custom Node Host Recipe, and `CUSTOM_EXTENSION_SURFACE_OK` proof markers define the defended route. The public story remains AsterGraph host-owned presenter guidance, not React Flow hooks/components parity. | Phase 493 ties the roadmap to the defended recipe and keeps the boundary docs/tests-only; future changes need a new tracker if the presenter contracts themselves change. |
| Node drag handles | Present / guarded | `NodeDragHandle` exposes a public Avalonia attached property for custom node visual/body presenters. Focused headless tests cover drag from marked handles and suppression from unmarked body surfaces when a handle exists. | Keep guarded with `StandaloneCanvas_NodeDragHandle_*` tests and public API baseline checks. |
| Node resizer | Present | `NodeResizer`, `TrySetNodeSize`, built-in component catalog, Cookbook route, and focused tests exist. | Keep guarded by built-in tests and screenshot route. |
| Rotatable nodes | Present / guarded | `GraphNodeSurfaceState.RotationDegrees`, `IGraphEditorCommands.TrySetNodeRotation(...)`, `GraphEditorNodeSurfaceSnapshot.RotationDegrees`, serializer compatibility, renderer/hit-test geometry, public API baseline, and bilingual host docs are in place. | Keep guarded by model/session/Avalonia tests, public API validation, and command/query guidance. |
| Custom edges | Present / guarded | `GraphEdgePresentation`, connection geometry snapshots, route vertices, reconnect/edit commands, labels, path kinds, markers, animation flag, and floating endpoints exist. | Keep API and renderer tests current when edge claims change. |
| Bezier, SmoothStep, Step, Straight edges | Present | `GraphEdgePathKind` maps through `GraphEditorConnectionGeometryProjector` into route styles. | Keep guarded. |
| Animated and floating edges | Present / proof-bounded | `IsAnimated` and `UsesFloatingEndpoints` flow through edge presentation and geometry snapshots. Visual animation proof remains narrower than static geometry proof. | Keep visual proof bounded; widen only with screenshot/full-window evidence. |
| Editable/reconnectable edges | Present | Reconnect and route-vertex commands plus edge toolbar evidence exist. | Keep guarded. |
| Edge labels and markers | Present | Labels and source/target marker fields are modeled in edge presentation/geometry snapshots. | Keep guarded. |
| Drag, pan, zoom | Present | Canvas pointer/wheel coordinators and viewport commands exist. | Keep guarded. |
| Marquee/box selection and multi-select | Present | `GetSelectionRectangleSnapshot`, overlay coordinator marquee selection, `SelectAll`, `SelectNone`, and `InvertSelection` have tests and Cookbook routes. | Keep guarded. |
| Connection preview and validation | Present / guarded | Pending connection, compatible targets, validation snapshots, repair commands, and `validation-prevent-cycle` fixture exist. Cycle prevention is enforced by the canonical connection completion path and exposes `GraphEditorPendingConnectionRejectionReason.WouldCreateCycle` through pending snapshots. | Keep `RuntimeSession_CompleteConnection_RejectsDirectCycleWithStableReason`, `RuntimeSession_CompleteConnection_RejectsIndirectCycleThroughNormalCommandPath`, and `RuntimeSession_TryExecuteCommand_RejectsCycleThroughConnectionsConnectRoute` guarded. |
| Context menu | Present | Menu descriptors and hosted context menu plumbing exist. | Keep guarded. |
| Undo/redo | Present | Session commands and history tests cover normal command semantics. | Keep guarded. |
| Copy/paste | Present | Clipboard commands, fragment serialization, and compatibility payload tests exist. | Keep guarded. |
| Save/restore | Present | Workspace save/load commands, serializer contracts, and compatibility import boundary exist. | Keep guarded. |
| Helper lines / snap guides | Present / proof-bounded | Snap/grid commands and projection evidence exist; docs should keep the claim tied to supported command routes. | Keep guarded. |
| Prevent cycles | Present / guarded | Direct and indirect cycles are rejected through `StartConnection` / `CompleteConnection` and descriptor-driven `connections.connect`; rejected attempts keep the pending connection, do not mutate the document, and expose `WouldCreateCycle` as the stable reason. Compatible-target queries remain type-compatibility discovery; completion is the final policy authority. | Keep guarded by Phase 482 session and diagnostics contract tests. |
| MiniMap | Present | `GraphMiniMap`, `AsterGraphMiniMapViewFactory`, lightweight projection, and Cookbook route exist. | Keep guarded. |
| Controls | Present | Standalone `AsterGraphControls`, hosted action factory, built-in catalog entry, and tests exist. | Keep guarded. |
| Background | Present | `GridBackground`, style options, grid-density tests, and Cookbook route exist. | Keep guarded. |
| Panel | Present | `AsterGraphPanel`, position enum, catalog entry, tests, and Cookbook route exist. | Keep guarded. |
| NodeToolbar / EdgeToolbar | Present | Standalone `NodeToolbar` and `EdgeToolbar` project canonical node/connection actions and have tests/routes. | Keep guarded. |
| Groups / subflows | Present / proof-bounded | Groups, composite scopes, hierarchy snapshots, promotion/expose commands, and Cookbook route exist. | Keep guarded. |
| Auto layout integration | Partial | `LAYOUT_PROVIDER_EVIDENCE_EXPANSION` ties `IGraphLayoutProvider.CreateLayoutPlan(GraphLayoutRequest)`, `GraphLayoutRequest`, `GraphLayoutPlan`, preview/apply, snap-to-grid, `GraphEditorLayoutProviderSeamTests`, command-surface cancel evidence, and Cookbook route evidence together. Provider examples are host-owned, and current layout planning is synchronous rather than async-cancellable. | Keep Phase 504 bounded to docs/tests proof unless adopter evidence justifies a new provider contract issue. |
| Virtualization / thousands of nodes | Partial / bounded | Scale baseline, visible-scene projection, MiniMap lightweight projection, and hosted performance policy exist. The current defended contract is viewport-budgeted scene projection/rendering, not ItemsRepeater/Skia-style renderer virtualization, and xlarge evidence is telemetry-only. | Keep scale docs and renderer projection tests current; add a new issue before widening this claim. |
| Declarative + code-first API | Partial | `DECLARATIVE_API_ERGONOMICS_AUDIT` ties the current code-first and hosted routes to `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`, `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`, `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()`, `templates/astergraph-avalonia`, `src/AsterGraph.Demo`, and definition builders. These are not equivalent to React Flow hooks/components, and AsterGraph does not claim a `<ReactFlow>`-equivalent declarative DSL. | Phase 503 keeps this docs/tests-only; add a later API-change tracker before introducing any declarative DSL, source generator, XAML extension, or hook-like public surface. |
| Accessibility breadth | Partial / manual validation plan defined | Keyboard navigation, automation peers, built-in action names, and shell-state focus routes are covered in targeted tests. Phase 491 adds an explicit breadth contract for standalone built-ins and hosted shell states. `ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN` now defines manual assistive-technology validation for the hosted route while keeping headless automation separate from unverified live screen-reader behavior, including dynamic screen-reader announcement evidence. | Keep Phase 505 bounded to docs/tests and source-backed controls; file a separate follow-up before changing live-region/runtime behavior or making a broad screen-reader certification claim. |
| Host events | Present | `IGraphEditorEvents`, mutation batching, and host-event Cookbook route exist. | Keep guarded. |
| Screenshot-driven UI quality | Partial / guarded planning | Scene PNG gate exists for canonical graph scenes. The full-window shell gate now captures English default Cookbook drawer, Chinese default Cookbook drawer, English default Cookbook closed shell, English runtime diagnostics drawer, and `shell-runtime-diagnostics-closed`, including host menu, drawer state, graph host, named shell parts, selected language/theme metadata, and artifact metadata. `SHELL_VISUAL_COVERAGE_PLANNING` keeps future flyout capture, popup capture, context-menu capture, additional language/theme variants, and pixel-baseline drift measurement as explicit later candidates. | Keep Phase 506 bounded to docs/tests planning: no manifest rows, no strict pixel baselines, no runtime UI changes, no public API changes, no retained API removal, and no broad visual/language/theme certification. |

## Completed Phase 0 Issue Wave

The original first wave is no longer the next work queue. These tracker items are closed and should be treated as historical context:

| GitHub | Bead | Result |
| --- | --- | --- |
| #46 | `avalonia-node-map-vqt` | Original Phase 0 parity audit and cleanup roadmap. |
| #47 | `avalonia-node-map-x24` | Public docs and removed sample route reconciliation. |
| #48 | `avalonia-node-map-25d` | Retained compatibility surface cleanup and public metadata update. |
| #49 | `avalonia-node-map-dui` | Runtime/model/session/rendering boundary split. |
| #50 | `avalonia-node-map-3qs` | React Flow-grade edge model and renderer parity. |
| #51 | `avalonia-node-map-y1e` | Built-in component catalog and reusable Avalonia component wave. |
| #52 | `avalonia-node-map-a08` | Cookbook expansion and screenshot-gate parent. |
| #59, #61, #63, #65 | `avalonia-node-map-a08.*` | Scene screenshot foundation plus built-in, interaction, and lifecycle Cookbook batches. |
| #67, #69, #71, #73, #75, #77 | `avalonia-node-map-y1e.*` | Built-in catalog, NodeToolbar, EdgeToolbar, NodeResizer, Panel, Controls, and screenshot coverage for standalone built-ins. |
| #83 | `avalonia-node-map-p481` | First full-window Cookbook shell visual gate with artifact metadata and CI lane coverage. |
| #84 | `avalonia-node-map-p482` | Guarded cycle-prevention connection policy. |
| #81 | `avalonia-node-map-p479` | Public hosted-Avalonia node drag-handle API through `NodeDragHandle`. |
| #82 | `avalonia-node-map-p483` | Large-graph virtualization claim bounded to viewport-budgeted scene projection/rendering; no renderer virtualization or 10000-node support claim. |
| #80 | `avalonia-node-map-p480` | Rotatable node model, command/query projection, renderer geometry, compatibility overloads, API baseline, and docs. |
| #91 | `avalonia-node-map-4xr` | Phase 484 roadmap refresh after rotatable nodes. |
| #93 | `avalonia-node-map-3hc` | Phase 485 Cookbook host-copy architecture path; Closed by PR #94. |
| #95 | `avalonia-node-map-0xr` | Phase 486 full-window shell visual-state expansion; Closed by PR #96. |
| #97 | `avalonia-node-map-i8s` | Phase 487 custom-node copyable-host recipe hardening; Closed by PR #98. |
| #99 | `avalonia-node-map-ce1` | Phase 488 layout provider and background/cancel proof refresh; Closed by PR #100. |
| #101 | `avalonia-node-map-6sc` | Phase 489 renderer virtualization proof-contract design spike; Closed by PR #102. |
| #103 | `avalonia-node-map-3x0` | Phase 490 stale React Flow parity roadmap repair; Closed by PR #104. |

## Phase 491 Accessibility Breadth Audit

Phase 491 records the current accessibility posture as a source-backed contract instead of a broad claim. Existing coverage is strongest for the hosted shell, canvas, nodes, ports, connections, inspector, and command surfaces. The new breadth audit adds explicit checks for standalone built-ins and hosted shell states without changing runtime behavior.

| Surface | Accessibility posture | Guarded evidence |
| --- | --- | --- |
| `AsterGraphControls` | Non-focusable container; zoom/fit/reset buttons stay keyboard-focusable and expose action names. | `AsterGraphBuiltInControlsTests` |
| `NodeToolbar` / `EdgeToolbar` | Non-focusable containers; projected node/connection action buttons stay keyboard-focusable and named from descriptors. | `AsterGraphBuiltInToolbarTests` |
| `NodeResizer` | Resize handles are named buttons and remain covered by focused built-in tests. | `AsterGraphBuiltInNodeResizerTests` |
| `GraphMiniMap` | Pointer-only overview surface; stock MiniMap and its stock drawing surface stay out of the keyboard focus path. | `GraphMiniMapStandaloneTests` |
| `AsterGraphPanel` | Overlay layout container stays non-focusable while preserving host-owned focusable children. | `AsterGraphBuiltInPanelTests` |
| `GridBackground` | Decorative canvas grid remains non-focusable. | `GridBackgroundTests` |
| Hosted validation/problems shell | Problem rows and focus buttons expose automation names and help text tied to validation targets. | `GraphEditorViewTests` |
| Hosted export, fragment, command-palette, and authoring shells | Existing interactive controls expose names and stay keyboard-focusable; command-palette focus recovery remains guarded separately. | `GraphEditorViewTests`, `GraphEditorNavigationFocusWorkflowTests` |

Remaining bounded gap: headless tests can guard names, peers, focusability, and focus return, but they do not prove live screen-reader announcements for dynamic validation/export status changes. Any future live-region or assistive-technology certification work needs a new GitHub/Beads tracker.

## Next Issue Wave

Phase 490 repaired the stale Phase 484 queue after the closed Phase 485-489 work. GitHub #103 / `avalonia-node-map-3x0` is closed by PR #104 and did not authorize runtime or public API changes.

Phase 491 closed the accessibility breadth audit while keeping live screen-reader announcement proof as a separately tracked future boundary.

Phase 492 now owns the retained migration removal roadmap as an inventory now, remove later slice. It can classify retained and compatibility-only surfaces, but any actual deletion needs a later API-change tracker tied to the v1 policy and `eng/public-api-baseline.txt`.

Phase 493 closed the custom node presenter cookbook parity proof. It records that the supported custom-node route is host-owned `NodeBodyPresenter` / `NodeVisualPresenter` guidance plus existing proof markers, not a runtime API expansion or React Flow component/hook parity claim.

Phase 494 closed localized full-window shell visual gate coverage. It added one Chinese Cookbook drawer state and shell-state language/theme metadata while leaving strict pixel baselines and broad flyout/theme/language coverage for later tracker-backed work.

Phase 495 refreshed the active queue after GitHub and Beads returned to zero open issues. Phase 496 then closed the Cookbook example architecture contract. Phase 497 closed the closed-drawer shell visual gate breadth slice through GitHub #117 / `avalonia-node-map-5nl`; it added one bounded closed shell state, not broad shell certification.

Phase 498 closed the retained migration removal execution gate through GitHub #119 / `avalonia-node-map-3um`. It only defined the gate for a later API-change issue: exact symbols, blocker tests, support-window criteria, migration evidence, and `eng/public-api-baseline.txt` approval are required before any removal PR. The slice authorized no retained API removal, no public API baseline change, no runtime behavior change, and no UI change.

Phase 499 closed the renderer virtualization execution boundary through GitHub #121 / `avalonia-node-map-9x7`. It kept the current claim to viewport-budgeted scene projection/rendering. Any future implementation issue that widens the claim must introduce non-informational renderer thresholds, a repeatable proof command, focused renderer tests, artifact metadata, and evidence that the claimed operation avoids full collection scans and full scene rebuilds.

Phase 500 closed the selected runtime shell visual gate state through GitHub #123 / `avalonia-node-map-66t`. It added one bounded runtime closed-shell capture with `expectedPaneOpen: false` and kept runtime behavior changes, strict pixel baselines, flyouts, popups, context menus, broad language/theme certification, retained API removal, and renderer virtualization work out of scope.

Phase 501 closed the post-Phase-500 parity follow-up queue refresh through GitHub #125 / `avalonia-node-map-38n`. It converted the current partial gaps into a concrete queue while keeping broad React Flow parity claims, runtime behavior changes, public API changes, UI redesign, strict pixel baselines, retained API removal, and renderer virtualization implementation out of scope.

Phase 502 now owns the renderer virtualization execution proof through GitHub #127 / `avalonia-node-map-mai`. It is current/owned work and remains docs/tests only: define the proof command and artifact metadata contract required before any true renderer virtualization claim while keeping implementation, runtime behavior change, public API change, UI redesign, retained API removal, and support-claim expansion out of scope.

Phase 503 now owns the declarative API ergonomics audit through GitHub #129 / `avalonia-node-map-mzu`. It remains docs/tests only: record the current code-first, factory, hosted-builder, template, and Demo routes without claiming React hook parity, a `<ReactFlow>`-equivalent declarative DSL, runtime behavior change, public API change, UI redesign, retained API removal, or public API baseline change.

Phase 504 now owns the layout provider evidence expansion through GitHub #131 / `avalonia-node-map-8lf`. It remains docs/tests only: record the current synchronous, host-owned `IGraphLayoutProvider` / `GraphLayoutRequest` / `GraphLayoutPlan` seam and its preview/apply/snap command evidence without claiming async/cancellable provider execution, runtime behavior change, public API change, UI redesign, retained API removal, or a new layout engine.

Phase 505 closed the accessibility manual assistive-technology validation plan through GitHub #133 / `avalonia-node-map-b4z`. It remained docs/tests only: record a manual checklist for Narrator, NVDA, and VoiceOver or platform-equivalent checks after `HOSTED_ACCESSIBILITY_OK:True` is green, while explicitly distinguishing headless automation from unverified live screen-reader behavior. It authorized no live-region/runtime behavior change, no UI change, no public API change, no retained API removal, and no broad screen-reader certification claim.

Phase 506 closed the broader shell visual coverage planning through GitHub #135 / `avalonia-node-map-h7c`. It remained docs/tests only: document how the existing five manifest-driven full-window shell captures lead into future flyout, popup, context-menu, language/theme, and drift-measurement work without adding manifest rows or widening visual certification claims in this slice.

Phase 507 closed the post-Phase-506 visual queue refresh through GitHub #137 / `avalonia-node-map-3tw`. It was docs/tests only: repair the stale current-owned table and turn the Phase 506 planning candidates into the next concrete visual-coverage queue without runtime UI behavior changes, shell-state manifest rows, strict pixel baselines, public API changes, retained API removal, or broad visual/language/theme certification.

Phase 508 now owns the shell flyout visual capture through GitHub #139 / `avalonia-node-map-2nu`. It adds one bounded View menu flyout state, `shell-cookbook-default-view-menu-flyout`, with `PART_ViewMenu`, `full-window-shell-flyout-state` metadata, and required header evidence, while keeping runtime behavior changes, public API changes, strict pixel baselines, popup coverage, context-menu coverage, broad language/theme certification, and retained API removal out of scope.

| GitHub | Bead | Title | Priority | Likely write set | Parallelism |
| --- | --- | --- | --- | --- | --- |
| #137 | `avalonia-node-map-3tw` | Phase 507: post-Phase-506 visual queue refresh | P3 | parity roadmap docs and focused docs tests | Closed slice. Repaired stale tracker wording only; no runtime UI behavior changes, shell-state manifest rows, strict pixel baselines, public API changes, retained API removal, or broad visual/language/theme certification. |
| #139 | `avalonia-node-map-2nu` | Phase 508: shell flyout visual capture | P3 | shell visual gate harness, manifest/docs/tests, generated artifact metadata | Current owned slice. Isolates one View menu flyout capture path and proves full-window artifact metadata without claiming broad shell certification. |
| #140 | `avalonia-node-map-0ff` | Phase 509: popup visual capture | P3 | shell visual gate harness, manifest/docs/tests, generated artifact metadata | Candidate after Phase 508. Keep popup coverage separate from context menus and avoid runtime redesign unless explicitly tracked. |
| #141 | `avalonia-node-map-8lu` | Phase 510: context-menu visual capture | P3 | context-menu visual harness/docs/tests, generated artifact metadata | Candidate after popup capture. Should use the existing context-menu presenter route and avoid public API changes or retained hook removal. |
| #142 | `avalonia-node-map-9rq` | Phase 511: additional language/theme shell variants | P3 | shell state manifest/docs/tests for bounded language/theme rows | Candidate after overlay capture shape is clear. Adds explicit variants only; no broad visual/language/theme certification. |
| #143 | `avalonia-node-map-1j4` | Phase 512: pixel-baseline drift measurement | P3 | drift measurement docs/tests/artifact metadata | Must precede any strict pixel baseline. Compares recorded `PngSha256` and host metadata as evidence, not pass/fail hash policy yet. |

## Recommended Parallel Worktree Plan

- `docs/phase-495-roadmap-refresh`: owns #113 / `avalonia-node-map-wzt`; refreshes this roadmap and creates at least three concrete follow-up candidates without runtime or public API changes.
- `docs/phase-496-cookbook-architecture-contract`: owned the Cookbook example architecture contract row; docs/tests-only and independent from shell visual work.
- `docs/phase-497-shell-closed-drawer-gate`: owned #117 / `avalonia-node-map-5nl`; isolated writes to shell manifest, screenshot tests, and screenshot docs.
- `docs/phase-498-retained-removal-gate`: owns #119 / `avalonia-node-map-3um`; isolate writes to retained migration removal execution criteria and keep sequential with v1/API-baseline policy work.
- `perf/phase-499-renderer-virtualization-boundary`: owned #121 / `avalonia-node-map-9x7`; it closed the renderer virtualization execution boundary proof as docs/tests-only.
- `docs/phase-500-selected-runtime-shell-state`: owns #123 / `avalonia-node-map-66t`; isolate writes to shell state manifest, screenshot docs/tests, and parity roadmap text.
- `docs/phase-501-post-phase500-queue`: owns #125 / `avalonia-node-map-38n`; isolate writes to parity roadmap docs/tests and produce the next concrete queue.
- `perf/phase-502-renderer-proof`: owns #127 / `avalonia-node-map-mai`; isolate writes to renderer proof docs/tests, proof-command wording, and artifact metadata contract.
- `docs/phase-503-declarative-api-audit`: owns #129 / `avalonia-node-map-mzu`; isolate writes to parity roadmap, Quick Start, Host Integration, and focused docs tests.
- `docs/phase-504-layout-provider-evidence`: owned #131 / `avalonia-node-map-8lf`; isolated layout provider proof docs/tests without changing provider runtime behavior.
- `docs/phase-505-accessibility-manual-validation`: owns #133 / `avalonia-node-map-b4z`; isolate writes to accessibility docs, manual validation checklist wording, and focused docs tests.
- `docs/phase-506-shell-visual-coverage-planning`: owns #135 / `avalonia-node-map-h7c`; isolate writes to shell visual planning docs/tests and do not edit shell-state manifest rows.
- `docs/phase-507-visual-queue-refresh`: owns #137 / `avalonia-node-map-3tw`; isolate writes to this parity roadmap and focused docs tests only.
- `visual/phase-508-shell-flyout-capture`: owns #139 / `avalonia-node-map-2nu`; candidate worktree for the first tracker-backed flyout visual capture.
- `visual/phase-509-popup-capture`: owns #140 / `avalonia-node-map-0ff`; candidate worktree for popup visual capture, separate from context-menu capture.
- `visual/phase-510-context-menu-capture`: owns #141 / `avalonia-node-map-8lu`; candidate worktree for context-menu visual capture through the existing context-menu presenter route.
- `visual/phase-511-language-theme-shell-variants`: owns #142 / `avalonia-node-map-9rq`; candidate worktree for bounded language/theme shell-state rows after overlay capture shape is settled.
- `visual/phase-512-pixel-drift-measurement`: owns #143 / `avalonia-node-map-1j4`; candidate worktree for drift measurement before any strict pixel-baseline gate.

## UI Verification Policy

All future UI changes to `src/AsterGraph.Avalonia` or `src/AsterGraph.Demo` should provide:

- a focused headless Avalonia test for control state, layout contract, or interaction state;
- a deterministic scene PNG capture through the Cookbook screenshot gate when the change affects graph scene rendering;
- before/after PNG artifacts and `metadata.json` for visual PRs that affect Cookbook scenes;
- a Cookbook route for each new public UI component or interaction;
- an explicit note when a UI change is structural-only and does not alter pixels.

Current coverage includes scene-level route captures plus six manifest-driven full-window shell captures: five base shell states and one bounded View menu flyout state. `DemoCookbookScreenshotGateTests`, `CookbookScreenshotGateRoutes.json`, and `CookbookShellVisualGateStates.json` prove canonical graph scenes, route metadata, the English default Cookbook open artifact, the English default Cookbook closed-drawer artifact, the Chinese default Cookbook shell artifact, the English runtime diagnostics shell artifact, the selected English runtime diagnostics closed-shell artifact, and `shell-cookbook-default-view-menu-flyout`; they still do not provide strict pixel-baseline comparisons, broad flyout coverage beyond the View menu, popup coverage, context-menu coverage, or broad theme/language coverage.

## Tracker Notes

- GitHub #79 and Beads `avalonia-node-map-p478` were created with `.planning/*` in the write set. Because `.planning/` is ignored and absent from this worktree, this refresh records that as tracker drift instead of force-adding local planning files.
- Beads is the durable local tracker for this repository. Phase 491 now owns the accessibility breadth audit with GitHub #105 / `avalonia-node-map-44i`; later follow-ups should get explicit GitHub and Beads IDs before code changes.
- Phase 492 now owns retained migration removal planning with GitHub #107 / `avalonia-node-map-j8v`; it records classification and gates, not API removal.
- Phase 493 now owns custom node presenter cookbook parity proof with GitHub #109 / `avalonia-node-map-8qv`; it records route traceability, not a presenter API expansion.
- Phase 494 now owns localized full-window shell visual gate coverage with GitHub #111 / `avalonia-node-map-p5z`; it records one localized shell visual state, not a broad visual certification.
- Phase 495 now owns the post-Phase-494 roadmap refresh with GitHub #113 / `avalonia-node-map-wzt`; it records the next issue split, not implementation.
- Phase 497 now owns the closed-drawer shell visual gate expansion with GitHub #117 / `avalonia-node-map-5nl`; it records one closed state, not broad visual certification.
- Phase 498 now owns retained migration removal execution-gate definition with GitHub #119 / `avalonia-node-map-3um`; it records pre-removal evidence criteria, not removal.
- Phase 499 now owns renderer virtualization execution-boundary proof with GitHub #121 / `avalonia-node-map-9x7`; it records proof criteria, not renderer implementation.
- Phase 500 closed selected runtime shell visual gate state with GitHub #123 / `avalonia-node-map-66t`; it records one runtime closed-shell state, not broad visual certification.
- Phase 501 closed post-Phase-500 parity follow-up queue refresh with GitHub #125 / `avalonia-node-map-38n`; it records the next issue split, not implementation.
- Phase 502 closed renderer virtualization execution proof with GitHub #127 / `avalonia-node-map-mai`; it records proof-command and artifact-metadata criteria, not renderer implementation.
- Phase 503 closed declarative API ergonomics audit with GitHub #129 / `avalonia-node-map-mzu`; it records the current copyable API routes, not a new DSL or public API.
- Phase 504 closed layout provider evidence expansion with GitHub #131 / `avalonia-node-map-8lf`; it records the current synchronous host-owned layout provider seam.
- Phase 505 closed accessibility manual assistive-technology validation planning with GitHub #133 / `avalonia-node-map-b4z`; it records manual AT checks without certification claims.
- Phase 506 closed broader shell visual coverage planning with GitHub #135 / `avalonia-node-map-h7c`; it records the future visual candidates without adding manifest rows.
- Phase 507 closed the post-Phase-506 visual queue refresh with GitHub #137 / `avalonia-node-map-3tw`; it assigns real tracker IDs to Phases 508-512.
- Phase 508 now owns the shell flyout visual capture with GitHub #139 / `avalonia-node-map-2nu`; it adds only `shell-cookbook-default-view-menu-flyout` and `full-window-shell-flyout-state` metadata.
- Phase 509 / #140 / `avalonia-node-map-0ff`, Phase 510 / #141 / `avalonia-node-map-8lu`, Phase 511 / #142 / `avalonia-node-map-9rq`, and Phase 512 / #143 / `avalonia-node-map-1j4` remain the next concrete visual-coverage candidates.
- Product code remains out of scope for Phase 478, Phase 484, Phase 490, Phase 491, Phase 492, Phase 493, Phase 494, Phase 495, Phase 497, Phase 498, Phase 499, Phase 500, Phase 501, Phase 502, Phase 503, Phase 504, Phase 505, Phase 506, Phase 507, and Phase 508 unless a focused test proves a specific missing contract.
