# Phase 0 React Flow Parity Audit

## Phase 478 Refresh

This document started as the Phase 0 audit for the `0.11.0-beta` baseline. Phase 478 refreshes it against the current `master` state so the next issue wave does not duplicate work already finished by GitHub #46-#52 and their follow-up slices.

The scope of this refresh is docs and tracker state only. It does not change product code, runtime behavior, renderer contracts, or public support claims.

## Repository Baseline

- Current package version is `0.11.0-beta` in `Directory.Build.props`; publishable libraries are `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- Publishable libraries target `net8.0;net9.0;net10.0`; Demo and test projects currently target `net9.0`.
- `AsterGraph.Demo` remains the visible demo host and Cookbook proof surface.
- `src/AsterGraph.Demo/Cookbook/DemoCookbookCatalog.Recipes.cs` currently contains 25 `DemoCookbookRecipe` entries.
- `tests/AsterGraph.Demo.Tests/CookbookScreenshotGateRoutes.json` currently defines 15 deterministic scene PNG capture routes.
- `tests/AsterGraph.Demo.Tests/DemoCookbookScreenshotGateTests.cs` captures canonical graph scenes through `GraphSceneImageExportService`, writes `artifacts/test-results/cookbook-screenshot-gate/metadata.json`, and validates route metadata.
- `.planning/` and `docs/plans/` are gitignored in this repository. Phase 478 treats the `.planning/*` write-set references in GitHub #79 and Beads `avalonia-node-map-p478` as tracker drift; durable planning state for this slice lives in tracked docs plus GitHub/Beads issues.

## Architecture Inventory

| Layer | Current owner | Current evidence | Phase 478 assessment |
| --- | --- | --- | --- |
| Host-facing identifiers and contracts | `src/AsterGraph.Abstractions` | Node definitions, catalogs, styling, builder helpers, compatibility policies, and plugin-neutral extension points. | Still the dependency floor for hosts; keep new parity contracts thin and explicit. |
| Persisted data model and serialization | `src/AsterGraph.Core` | `GraphDocument`, groups, scopes, edge presentation, schema migration helpers, and legacy import boundary. | Compatibility cleanup from #48 is complete; new model fields such as rotation must name schema behavior up front. |
| Session, commands, queries, state services | `src/AsterGraph.Editor` | Selection, undo/redo, clipboard, workspace save/load, connection authoring, validation, layout, events, mutations, and presentation snapshots. | Strong canonical route. New parity work should go through commands/queries rather than bypassing session ownership. |
| Layout integration | `src/AsterGraph.Editor/Runtime` and services | `GraphLayoutRequest`, `GraphLayoutPlan`, `IGraphLayoutProvider`, preview/apply/cancel evidence, and snap/grid commands. | Provider seam exists; background/cancel/provider examples need continued proof when layout claims expand. |
| Avalonia rendering and interaction | `src/AsterGraph.Avalonia` | `NodeCanvas`, scene host projection, edge renderer, interaction coordinators, automation peers, MiniMap, Background, Controls, Panel, NodeToolbar, EdgeToolbar, and NodeResizer. | Built-ins are now reusable public components. Remaining UI parity gaps are drag-handle API, rotation, and full-window visual regression coverage. |
| Hosted workbench shell | `GraphEditorView` and hosted factories | Header, library, canvas, inspector, validation panel, authoring tools, minimap, command projection, and status chrome. | Useful supported route, but new public features should avoid making Demo shell chrome a hidden dependency. |
| Demo/Cookbook | `src/AsterGraph.Demo` | 25 recipes, route clarity docs, built-in/interaction/lifecycle batches, and scene PNG screenshot gate metadata. | Cookbook breadth and scene coverage are no longer missing. Full-window shell capture and pixel-baseline comparison remain gaps. |
| CI and release gates | `.github/workflows`, `eng/ci.ps1`, test projects | Build/test/maintenance/contract/release/hygiene lanes, public API baseline, package validation, docs route checks, and scene PNG gate tests. | Strong text/API/scene gates. Add full-window visual regression only after a deterministic capture plan is proven. |

## React Flow Parity Matrix

| Capability | Current status | Evidence / remaining gap | Next action |
| --- | --- | --- | --- |
| Custom nodes with arbitrary Avalonia content | Partial / supported through AsterGraph idioms | Node definitions, `IGraphNodeVisualPresenter`, authoring-surface docs, templates, and hosted controls exist. The public story is still less direct than React Flow's custom node component model. | Keep improving docs and samples with concrete host-owned visual presenter examples. |
| Node drag handles | Gap | Node dragging, marquee, selection, and command routing exist, but no stable public `DragHandle` or selector-style contract was found. | GitHub #81 / `avalonia-node-map-p479`. |
| Node resizer | Present | `NodeResizer`, `TrySetNodeSize`, built-in component catalog, Cookbook route, and focused tests exist. | Keep guarded by built-in tests and screenshot route. |
| Rotatable nodes | Missing | Source search found viewport transforms and selection transforms, but no persisted node rotation model, command, renderer contract, or tests. | GitHub #80 / `avalonia-node-map-p480`. |
| Custom edges | Present / guarded | `GraphEdgePresentation`, connection geometry snapshots, route vertices, reconnect/edit commands, labels, path kinds, markers, animation flag, and floating endpoints exist. | Keep API and renderer tests current when edge claims change. |
| Bezier, SmoothStep, Step, Straight edges | Present | `GraphEdgePathKind` maps through `GraphEditorConnectionGeometryProjector` into route styles. | Keep guarded. |
| Animated and floating edges | Present / proof-bounded | `IsAnimated` and `UsesFloatingEndpoints` flow through edge presentation and geometry snapshots. Visual animation proof remains narrower than static geometry proof. | Keep visual proof bounded; widen only with screenshot/full-window evidence. |
| Editable/reconnectable edges | Present | Reconnect and route-vertex commands plus edge toolbar evidence exist. | Keep guarded. |
| Edge labels and markers | Present | Labels and source/target marker fields are modeled in edge presentation/geometry snapshots. | Keep guarded. |
| Drag, pan, zoom | Present | Canvas pointer/wheel coordinators and viewport commands exist. | Keep guarded. |
| Marquee/box selection and multi-select | Present | `GetSelectionRectangleSnapshot`, overlay coordinator marquee selection, `SelectAll`, `SelectNone`, and `InvertSelection` have tests and Cookbook routes. | Keep guarded. |
| Connection preview and validation | Present / policy gap | Pending connection, compatible targets, validation snapshots, repair commands, and `validation-prevent-cycle` fixture exist. Hard cycle-prevention policy still needs an explicit public contract. | GitHub #84 / `avalonia-node-map-p482`. |
| Context menu | Present | Menu descriptors and hosted context menu plumbing exist. | Keep guarded. |
| Undo/redo | Present | Session commands and history tests cover normal command semantics. | Keep guarded. |
| Copy/paste | Present | Clipboard commands, fragment serialization, and compatibility payload tests exist. | Keep guarded. |
| Save/restore | Present | Workspace save/load commands, serializer contracts, and compatibility import boundary exist. | Keep guarded. |
| Helper lines / snap guides | Present / proof-bounded | Snap/grid commands and projection evidence exist; docs should keep the claim tied to supported command routes. | Keep guarded. |
| Prevent cycles | Partial | Validation examples and route fixtures exist, but a hard policy and invalid-reason contract need dedicated tests. | GitHub #84 / `avalonia-node-map-p482`. |
| MiniMap | Present | `GraphMiniMap`, `AsterGraphMiniMapViewFactory`, lightweight projection, and Cookbook route exist. | Keep guarded. |
| Controls | Present | Standalone `AsterGraphControls`, hosted action factory, built-in catalog entry, and tests exist. | Keep guarded. |
| Background | Present | `GridBackground`, style options, grid-density tests, and Cookbook route exist. | Keep guarded. |
| Panel | Present | `AsterGraphPanel`, position enum, catalog entry, tests, and Cookbook route exist. | Keep guarded. |
| NodeToolbar / EdgeToolbar | Present | Standalone `NodeToolbar` and `EdgeToolbar` project canonical node/connection actions and have tests/routes. | Keep guarded. |
| Groups / subflows | Present / proof-bounded | Groups, composite scopes, hierarchy snapshots, promotion/expose commands, and Cookbook route exist. | Keep guarded. |
| Auto layout integration | Partial | `IGraphLayoutProvider`, preview/apply/cancel, snap-to-grid, and route evidence exist. Provider examples and long-running cancellation semantics should stay explicit. | Backlog after higher-risk parity gaps unless adopter evidence raises it. |
| Virtualization / thousands of nodes | Partial / bounded | Scale baseline, visible-scene projection, MiniMap lightweight projection, and hosted performance policy exist. A true renderer virtualization contract is not proven, and xlarge evidence is telemetry-only. | GitHub #82 / `avalonia-node-map-p483`. |
| Declarative + code-first API | Partial | Host builder, definitions, builders, templates, and session APIs exist. Avalonia markup-first ergonomics are not equivalent to React Flow hooks/components. | Keep docs honest; do not claim React hook parity. |
| Accessibility breadth | Partial | Keyboard navigation and automation peers are covered in targeted tests. A broad accessibility audit across all built-ins and shell states remains weaker. | Add issue only if adopter/release evidence needs it. |
| Host events | Present | `IGraphEditorEvents`, mutation batching, and host-event Cookbook route exist. | Keep guarded. |
| Screenshot-driven UI quality | Partial | Scene PNG gate exists for canonical graph scenes. Full hosted window, panels, flyouts, shell chrome, and pixel-baseline comparison are not yet covered. | GitHub #83 / `avalonia-node-map-p481`. |

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

## Next Issue Wave

| GitHub | Bead | Title | Priority | Likely write set | Parallelism |
| --- | --- | --- | --- | --- | --- |
| #81 | `avalonia-node-map-p479` | Phase 479: define public node drag-handle API | P1 | `AsterGraph.Abstractions` or `AsterGraph.Avalonia`, canvas interaction routing, tests, docs | Can start after Phase 478. Avoid overlapping with rotation changes in node transform/hit-test code. |
| #80 | `avalonia-node-map-p480` | Phase 480: add rotatable node model and rendering contract | P2 | `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`, serialization/API/tests/docs | Keep separate from drag-handle work because both may touch node hit testing and adorners. |
| #83 | `avalonia-node-map-p481` | Phase 481: add full-window Cookbook visual regression gate | P1 | `tests/AsterGraph.Demo.Tests`, optional Demo capture seam, docs, optional CI scripts | Can run in parallel with editor-only policy work. Useful before heavy visual changes. |
| #84 | `avalonia-node-map-p482` | Phase 482: harden cycle-prevention connection policy | P1 | `AsterGraph.Editor` validation policy, tests, Demo/Cookbook docs | Smallest product-code parity slice; can run independently of UI work. |
| #82 | `avalonia-node-map-p483` | Phase 483: prove or bound large-graph rendering virtualization | P1 | `AsterGraph.Avalonia` renderer/projection if implementing; otherwise scale docs/tests | Start as an evidence/decision branch before any renderer rewrite. |

Recommended next branch: `avalonia-node-map-p481` / GitHub #83 if the next milestone will touch visible UI, because it adds the missing full-window proof gate before drag-handle or rotation work changes pixels. If the next branch needs to stay editor-only, start `avalonia-node-map-p482` / GitHub #84 instead.

## Recommended Parallel Worktree Plan

- `docs/phase478-parity-refresh`: owns this audit refresh, Chinese mirror, tracker split, and Phase 478 closure only.
- `ui/full-window-cookbook-gate`: owns #83 / `avalonia-node-map-p481`. Avoid product API changes.
- `feature/cycle-prevention-policy`: owns #84 / `avalonia-node-map-p482`. Avoid Avalonia shell work except docs/examples.
- `feature/node-drag-handle-api`: owns #81 / `avalonia-node-map-p479`. Coordinate with rotation if both touch hit testing or adorners.
- `feature/rotatable-nodes`: owns #80 / `avalonia-node-map-p480`. Do not mix with drag-handle API unless a shared transform abstraction is explicitly approved.
- `perf/rendering-virtualization-boundary`: owns #82 / `avalonia-node-map-p483`. Start by proving or bounding the claim before touching renderer internals.

## UI Verification Policy

All future UI changes to `src/AsterGraph.Avalonia` or `src/AsterGraph.Demo` should provide:

- a focused headless Avalonia test for control state, layout contract, or interaction state;
- a deterministic scene PNG capture through the Cookbook screenshot gate when the change affects graph scene rendering;
- before/after PNG artifacts and `metadata.json` for visual PRs that affect Cookbook scenes;
- a Cookbook route for each new public UI component or interaction;
- an explicit note when a UI change is structural-only and does not alter pixels.

Current coverage is scene-level, not full-window. `DemoCookbookScreenshotGateTests` and `CookbookScreenshotGateRoutes.json` prove canonical graph scenes and route metadata; they do not capture the hosted shell, panels, flyouts, or pixel-baseline comparisons. That remaining gap is tracked by GitHub #83 / `avalonia-node-map-p481`.

## Tracker Notes

- GitHub #79 and Beads `avalonia-node-map-p478` were created with `.planning/*` in the write set. Because `.planning/` is ignored and absent from this worktree, this refresh records that as tracker drift instead of force-adding local planning files.
- Beads is the durable local tracker for this repository. The refreshed next wave has explicit Beads IDs and GitHub issue links.
- Product code remains out of scope for Phase 478.
