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
- `tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json` currently defines `shell-cookbook-default-open` and `shell-runtime-diagnostics-open`.
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
| Demo/Cookbook | `src/AsterGraph.Demo` | 25 recipes, route clarity docs, built-in/interaction/lifecycle batches, scene PNG screenshot gate metadata, and two manifest-driven full-window shell visual states. | Cookbook breadth, scene coverage, and default Cookbook/runtime drawer shell captures are covered. Pixel-baseline comparison and broader flyout/theme/language shell-state coverage remain bounded future work. |
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
| Auto layout integration | Partial | `IGraphLayoutProvider`, preview/apply, snap-to-grid, command-surface cancel evidence, and route evidence exist. Provider examples are host-owned, and current layout planning is synchronous rather than async-cancellable. | Keep Phase 488 bounded to docs/tests proof unless adopter evidence justifies a new provider contract issue. |
| Virtualization / thousands of nodes | Partial / bounded | Scale baseline, visible-scene projection, MiniMap lightweight projection, and hosted performance policy exist. The current defended contract is viewport-budgeted scene projection/rendering, not ItemsRepeater/Skia-style renderer virtualization, and xlarge evidence is telemetry-only. | Keep scale docs and renderer projection tests current; add a new issue before widening this claim. |
| Declarative + code-first API | Partial | Host builder, definitions, builders, templates, and session APIs exist. Avalonia markup-first ergonomics are not equivalent to React Flow hooks/components. | Keep docs honest; do not claim React hook parity. |
| Accessibility breadth | Partial / audited breadth in progress | Keyboard navigation, automation peers, built-in action names, and shell-state focus routes are covered in targeted tests. Phase 491 adds an explicit breadth contract for standalone built-ins and hosted shell states; dynamic screen-reader announcement behavior still needs manual assistive-technology validation before any stronger claim. | Keep Phase 491 bounded to docs/tests and source-backed controls; file a separate follow-up before changing runtime announcement behavior. |
| Host events | Present | `IGraphEditorEvents`, mutation batching, and host-event Cookbook route exist. | Keep guarded. |
| Screenshot-driven UI quality | Partial / guarded | Scene PNG gate exists for canonical graph scenes. The full-window shell gate now captures default Cookbook drawer state plus the runtime diagnostics drawer state, including host menu, drawer, graph host, named shell parts, and artifact metadata. Pixel-baseline comparison and broader flyout/theme/language coverage are not yet covered. | Keep Phase 486 bounded to manifest-driven shell states; add follow-ups only if visual drift evidence requires broader baselines. |

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

Phase 493 now owns the custom node presenter cookbook parity proof. It records that the supported custom-node route is host-owned `NodeBodyPresenter` / `NodeVisualPresenter` guidance plus existing proof markers, not a runtime API expansion or React Flow component/hook parity claim.

| GitHub | Bead | Title | Priority | Likely write set | Parallelism |
| --- | --- | --- | --- | --- | --- |
| #105 | `avalonia-node-map-44i` | Phase 491: audit accessibility breadth across built-ins and shell states | P2 | Avalonia built-ins, automation/focus tests, docs for keyboard/screen-reader coverage boundaries | Current docs/tests-first accessibility audit. No runtime, public API, or visual changes unless evidence proves a specific missing contract. |
| #107 | `avalonia-node-map-j8v` | Phase 492: inventory retained migration surfaces and define removal roadmap | P3 | public API inventory, stabilization support matrix, retained migration docs/tests | Docs/tests-first inventory now, remove later slice. Sequential with v1 policy and public API baseline work; do not delete retained surfaces as a side effect of parity docs work. |
| #109 | `avalonia-node-map-8qv` | Phase 493: strengthen custom node presenter cookbook parity proof | P2 | React Flow parity audit docs and focused docs tests | Docs/tests-first traceability slice. Do not change runtime behavior, public API, Demo visuals, or screenshot gates unless a later tracker proves a missing presenter contract. |

## Recommended Parallel Worktree Plan

- `docs/phase-491-accessibility-breadth-audit`: owns #105 / `avalonia-node-map-44i`; audits accessibility docs/tests across Avalonia built-ins and shell states only.
- `docs/phase-492-retained-migration-roadmap`: owns #107 / `avalonia-node-map-j8v`; inventories retained migration surfaces and removal gates only. Deletion or baseline updates need a later API-change tracker.
- `docs/phase-493-custom-node-presenter-proof`: owns #109 / `avalonia-node-map-8qv`; binds the custom-node parity row to the defended host recipe and docs tests only.

## UI Verification Policy

All future UI changes to `src/AsterGraph.Avalonia` or `src/AsterGraph.Demo` should provide:

- a focused headless Avalonia test for control state, layout contract, or interaction state;
- a deterministic scene PNG capture through the Cookbook screenshot gate when the change affects graph scene rendering;
- before/after PNG artifacts and `metadata.json` for visual PRs that affect Cookbook scenes;
- a Cookbook route for each new public UI component or interaction;
- an explicit note when a UI change is structural-only and does not alter pixels.

Current coverage includes scene-level route captures plus two manifest-driven full-window shell captures. `DemoCookbookScreenshotGateTests`, `CookbookScreenshotGateRoutes.json`, and `CookbookShellVisualGateStates.json` prove canonical graph scenes, route metadata, the default Cookbook shell artifact, and the runtime diagnostics shell artifact; they still do not provide strict pixel-baseline comparisons or broad flyout/theme/language coverage.

## Tracker Notes

- GitHub #79 and Beads `avalonia-node-map-p478` were created with `.planning/*` in the write set. Because `.planning/` is ignored and absent from this worktree, this refresh records that as tracker drift instead of force-adding local planning files.
- Beads is the durable local tracker for this repository. Phase 491 now owns the accessibility breadth audit with GitHub #105 / `avalonia-node-map-44i`; later follow-ups should get explicit GitHub and Beads IDs before code changes.
- Phase 492 now owns retained migration removal planning with GitHub #107 / `avalonia-node-map-j8v`; it records classification and gates, not API removal.
- Phase 493 now owns custom node presenter cookbook parity proof with GitHub #109 / `avalonia-node-map-8qv`; it records route traceability, not a presenter API expansion.
- Product code remains out of scope for Phase 478, Phase 484, Phase 490, Phase 491, Phase 492, and Phase 493 unless a focused test proves a specific missing contract.
