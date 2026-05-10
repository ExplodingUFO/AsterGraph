# Phase 0 React Flow Parity Audit

This audit starts the v1 roadmap from the current `0.11.0-beta` repository state. It is intentionally evidence-first: the goal is to turn "make AsterGraph the .NET/Avalonia React Flow" into scoped issues with clean ownership, not to hide architecture debt behind a broad refactor.

## Repository Baseline

- Current package version is `0.11.0-beta` in `Directory.Build.props`; the matching tag `v0.11.0-beta` exists.
- Publishable libraries are `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`; all target `net8.0;net9.0;net10.0`.
- `AsterGraph.Demo` is the current visible demo host and targets `net9.0`.
- The current solution contains libraries, templates, Demo, and tests. The old `tools/AsterGraph.*` sample/proof projects are not in the solution or tracked source tree.
- `docs/plans/` is gitignored, so roadmap artifacts that must travel with the repository need to live under tracked docs paths.

## Architecture Inventory

| Layer | Current owner | Evidence | Phase 0 assessment |
| --- | --- | --- | --- |
| Host-facing identifiers and contracts | `src/AsterGraph.Abstractions` | Project description covers node definitions, catalogs, styling, and compatibility policies. | Smallest contract layer today; keep this as the dependency floor for hosts. |
| Persisted data model and serialization | `src/AsterGraph.Core` | `GraphDocument` stores root nodes, connections, groups, graph scopes, and composite metadata. `GraphDocumentCompatibility` reads schema versions up to 5 and legacy payloads. | Strong start, but legacy compatibility is mixed into normal read paths and must be explicitly bounded for v1. |
| Session, commands, queries, state services | `src/AsterGraph.Editor` | `IGraphEditorCommands` exposes undo/redo, copy/paste, layout, groups, composite promotion, connection authoring, validation repair, viewport, and workspace commands. | Rich surface, but runtime/session, kernel, services, retained facade, and presentation snapshots need stricter dependency rules. |
| Layout integration | `src/AsterGraph.Editor/Runtime` and services | `GraphLayoutRequest`, `GraphLayoutPlan`, `IGraphLayoutProvider`, and layout commands exist. | Provider seam exists; a v1 plan needs background/cancel semantics and large graph stress evidence. |
| Avalonia rendering and interaction | `src/AsterGraph.Avalonia` | `NodeCanvas` uses layered background, group, connection, node, and overlay surfaces. Interaction is coordinator-based for pointer, wheel, drag, resize, context menu, and overlay behavior. | Rendering is already separated better than a monolith, but built-ins are mostly embedded inside the full `GraphEditorView` shell. |
| Hosted workbench shell | `GraphEditorView` | The shell owns header, library, canvas, inspector, validation panel, authoring tools, fragment/export sections, minimap, and status chrome. | Useful for Demo, risky as the only public composition model. Extract reusable built-in components before adding more UI. |
| Demo/Cookbook | `src/AsterGraph.Demo` | `DemoCookbookCatalog.Recipes` currently has 11 recipes. Demo tests validate catalog/docs linkage and structural visual baselines. | Good proof harness, short of the requested 20+ executable examples and screenshot gates. |
| CI and release gates | `.github/workflows`, `eng/ci.ps1`, `eng/validate-public-api-surface.ps1` | Build/test/maintenance/contract/release/hygiene lanes exist; public API baseline and package validation exist. | Strong textual/API gates. Missing image/screenshot quality gate for UI changes. |

## React Flow Parity Matrix

| Capability | Current status | Evidence / gap | Priority |
| --- | --- | --- | --- |
| Custom nodes with arbitrary Avalonia content | Partial | Node surfaces, templates, parameters, and hosted controls exist, but the public story is not yet as direct as React Flow's custom node model. | P0 after boundary cleanup |
| Node drag handles | Partial | Canvas interaction supports drag and command routing; no clear public `DragHandle` component contract. | P1 |
| Node resizer | Partial | Node/group resize behavior exists in canvas internals; it is not yet a standalone reusable `NodeResizer` component. | P1 |
| Rotatable nodes | Missing | No current rotation model or renderer contract found. | P2 |
| Custom edges | Partial | Connection geometry, route vertices, labels, and reconnect commands exist. Edge type model is not yet complete. | P1 |
| Bezier, SmoothStep, Step, Straight edges | Partial | Renderer builds paths through `ConnectionPathBuilder`; explicit React Flow-style edge type API needs to be formalized. | P1 |
| Animated/floating/editable edges | Missing/partial | Reconnect and route editing exist, but no reusable animated or floating edge contract was found. | P1 |
| Edge labels and markers | Partial | Labels exist; marker/arrowhead contract needs explicit v1 modeling and renderer coverage. | P1 |
| Drag, pan, zoom | Present | Canvas pointer/wheel coordinators and viewport commands exist. | Keep guarded |
| Marquee/box selection and multi-select | Present/partial | Selection rectangle snapshots and overlay tests exist. Need user-facing Cookbook parity examples. | P1 |
| Connection preview and validation | Present/partial | Pending connection, compatibility, validation, and repair commands exist. Need clearer parity docs/examples. | P1 |
| Context menu | Present | Menu descriptors and hosted context menu plumbing exist. | Keep guarded |
| Undo/redo | Present | Session commands expose undo/redo and tests cover history semantics. | Keep guarded |
| Copy/paste | Present | Clipboard commands and compatibility payloads exist. | Keep guarded |
| Save/restore | Present/partial | Serializer and workspace save commands exist. Need v1 migration policy for legacy read behavior. | P0 |
| Helper lines / snap guides | Present/partial | Snap guide query/overlay seams exist. Needs Cookbook example. | P1 |
| Prevent cycles | Partial | Validation exists; explicit cycle-prevention policy needs parity audit and example coverage. | P1 |
| MiniMap | Present | `GraphMiniMap` exists with standalone factory and lightweight projection mode. | Keep guarded |
| Controls | Partial | Menus/toolbars/commands exist, but no reusable floating viewport-controls component analogous to React Flow `Controls`. | P1 |
| Background | Present | `GridBackground` exists. | Keep guarded |
| Panel | Partial | Fixed shell regions and demo drawers exist; no generic positional overlay primitive. | P1 |
| NodeToolbar / EdgeToolbar | Missing/partial | Toolbar descriptors and inspector-side authoring chrome exist; no node/edge-anchored reusable components found. | P1 |
| Groups / subflows | Present/partial | Node groups, composite scopes, hierarchy snapshots, and promotion commands exist. Need route cleanup and v1 docs. | P1 |
| Auto layout integration | Partial | `IGraphLayoutProvider` seam exists; background/cancel/provider examples need hardening. | P1 |
| Virtualization / thousands of nodes | Partial | Performance policies and projection evidence exist; no true virtualization contract equivalent to ItemsRepeater/Skia-backed large graph renderer is proven. | P0/P1 |
| Declarative + code-first API | Partial | Builder/factory/session APIs exist; no Avalonia markup-first story comparable to `<ReactFlow>` plus hooks ergonomics. | P1 |
| Screenshot-driven UI quality | Missing | Existing UI tests are structural/headless; no screenshot or pixel-baseline gate was found. | P0 for UI work |

## Retained And Compatibility Cleanup Catalog

| Surface | Current evidence | v1 decision needed |
| --- | --- | --- |
| `GraphDocument` root-only constructor/deconstruction | Documented as retained for legacy hosts and plugins. | Remove from primary public API or move behind explicit migration helpers. |
| `GraphDocumentCompatibility` legacy payload readers | Reads unversioned and older versioned payloads automatically. | Bound as explicit import/migration behavior; do not keep hidden legacy reads in normal save/restore if v1 is no-compat. |
| `IGraphEditorCommands.BeginConnection(...)` | `[Obsolete]` compatibility helper over `StartConnection(...)`. | Remove once public API baseline and call sites prove replacement coverage. |
| `IGraphEditorQueries.GetCompatibleTargets(...)` | `[Obsolete]` compatibility shim over canonical target queries. | Remove or quarantine as migration-only; update docs/tests accordingly. |
| `GraphEditorCapabilitySnapshot` obsolete constructor/deconstruct | Kept for older call shapes. | Remove if no current public scenario requires positional API. |
| Avalonia retained view facade wording | `AsterGraphAvaloniaViewFactory` and options still mention retained editor facade/direct `GraphEditorView` usage. | Reword or refactor so the canonical hosted API is the only promoted route. |
| README/project-status references to removed tools | Public docs name missing `tools/AsterGraph.*` projects. | Reconcile before adding any React Flow parity marketing. |

## Target Module Boundaries

| Boundary | Should own | Must not own |
| --- | --- | --- |
| `AsterGraph.Abstractions` | Stable host contracts, identifiers, node/port definitions, capability policies, public styling contracts, and plugin-neutral extension points. | Serialization migrations, editor runtime state, Avalonia controls, or Demo-only assumptions. |
| `AsterGraph.Core` | Persisted graph model, schema versioning, import/export policy, and explicit migration helpers. | Session commands, interaction state, rendering snapshots, or hidden legacy reads in the primary save/restore path. |
| `AsterGraph.Editor` | Runtime session, command/query facade, undo/redo, selection, validation, layout requests, clipboard/workspace operations, and presentation-ready snapshots. | Avalonia visual tree types, Demo shell concerns, or compatibility shims that bypass canonical commands. |
| `AsterGraph.Avalonia` | Reusable Avalonia controls, renderers, interaction coordinators, theming hooks, built-in components, and visual projection adapters. | Persisted document mutations outside editor commands, Demo recipe orchestration, or package/release tooling. |
| `AsterGraph.Demo` | Cookbook recipes, visual verification routes, public examples, and manual exploration shell. | Library-only APIs, host compatibility shims, or private state needed by production consumers. |
| CI/release tooling | Public API baseline checks, package smoke checks, docs route checks, and screenshot/pixel gates once added. | Product feature logic or Demo-specific assertions that cannot run deterministically in automation. |

## First Issue Wave

| GitHub | Bead | Title | Priority | Parallelism |
| --- | --- | --- | --- | --- |
| #46 | `avalonia-node-map-vqt` | Phase 0: Audit React Flow parity architecture and cleanup roadmap | P0 | Current coordination issue |
| #47 | `avalonia-node-map-x24` | Phase 0 follow-up: reconcile stale public docs and removed sample routes | P1 | Can run in parallel with architecture work after this audit |
| #48 | `avalonia-node-map-25d` | Phase 1: define and remove retained compatibility surfaces for v1 | P0 | Blocks clean API and serialization work |
| #49 | `avalonia-node-map-dui` | Phase 1: split model state, session state, and rendering projection boundaries | P0 | Can proceed beside #48 with disjoint initial docs/tests |
| #50 | `avalonia-node-map-3qs` | Phase 2: implement React Flow-grade edge model and renderer parity | P1 | Depends on #49 API boundaries |
| #51 | `avalonia-node-map-y1e` | Phase 3: ship built-in React Flow-style components for Avalonia | P1 | Depends on #49; can parallelize with #50 after API seams settle |
| #52 | `avalonia-node-map-a08` | Phase 6: expand Cookbook to 20+ executable examples with screenshot gates | P1 | Can start with screenshot gate design, then fan out per example |

## Recommended Parallel Worktree Plan

- `refactor/phase-0-reactflow-audit`: current audit branch; owns this document and issue split only.
- `docs/reconcile-public-routes`: owns #47 docs and docs tests. No source API changes.
- `refactor/v1-compatibility-surface`: owns #48 Core/Editor/Avalonia public compatibility cleanup. Avoid Demo UI edits.
- `refactor/runtime-layer-boundaries`: owns #49 architecture tests, package references, and boundary docs. Avoid compatibility removal except where coordinated.
- `feature/edge-parity`: owns #50 after #49 identifies stable edge seams.
- `ui/builtin-components`: owns #51 after #49 identifies public component API seams.
- `ui/cookbook-screenshot-gates`: owns #52 screenshot harness first, then separate example branches.

## UI Verification Policy

All future UI changes to `src/AsterGraph.Avalonia` or `src/AsterGraph.Demo` should provide:

- a focused headless Avalonia test for control state, layout contract, or interaction state;
- a deterministic screenshot capture command for Demo/Cookbook when the change is visual;
- before/after screenshots for any theme, layout, component, edge, node, panel, or animation change;
- a Cookbook route for each new public UI component or interaction;
- explicit note when a UI change is structural-only and does not alter pixels.

The current repo has structural visual tests and headless control tests, but no screenshot/pixel gate. That gap is tracked by #52.

## Validation Notes

- `bd context`, `bd dolt status`, and `bd ready` were run after recovering the per-project Dolt server at `F:\CodeProjects\DotnetCore\avalonia-node-map\.beads\dolt`.
- `dotnet restore AsterGraph.sln` passes in the isolated worktree.
- An initial `dotnet build AsterGraph.sln -c Debug --no-restore` failed before restore because the new worktree had no `project.assets.json`; this was a restore prerequisite, not a compile failure.
- `dotnet build AsterGraph.sln -c Debug --no-restore -m:1 /p:BuildInParallel=false /p:UseSharedCompilation=false` passes after restore.
- No screenshot or pixel-baseline gate exists yet; that missing UI gate is tracked by #52.
