# Phase 469 Summary: Layout And Spatial Authoring Workbench

Bead: `avalonia-node-map-v78.4.5`

Status: complete

Requirement: `SPACE-01`

## Scope

Phase 469 closed the layout and spatial authoring workbench proof after the four implementation beads under `avalonia-node-map-v78.4`. The phase stayed on the existing session command/query route and the stock Avalonia hosted workbench projection. It did not add a UI-only spatial runtime, fallback behavior, a broad spatial rewrite, or a second workflow engine.

## Proof Matrix

| Bead | Observed commit | Files | Proof added |
| --- | --- | --- | --- |
| `469.1` spatial command proof | `23d315e` `test: prove spatial command routing` | `tests/AsterGraph.Editor.Tests/GraphEditorCommandRegistryTests.cs`; `tests/AsterGraph.Editor.Tests/GraphEditorKernelCommandRouterTests.cs` | `CommandRegistry_SpatialAuthoringCommandsExposeCanonicalRouteMenuAndToolPlacements` proves group, layout, distribution, and snap commands expose kernel descriptors plus command-route, context-menu, and selection-tool placements. `GraphEditorKernel_SpatialAuthoringCommands_RunThroughCanonicalCommandRoute` proves selection transform, group create, align, distribute, and snap execute through `TryExecuteCommand`. |
| `469.2` selection/group/snap coherence proof | `a9dbbd2` `test: prove selection transform coherence` | `tests/AsterGraph.Editor.Tests/GraphEditorSelectionTransformContractsTests.cs` | `Commands_ComposeSelectionMoveSnapGroupRouteAndLayoutResetConstraints` proves constrained selection move, snap guides, snap commit, group membership, manual route preservation, and layout route reset compose through session state. `Queries_RejectEmptySelectionTransformWithoutProjectingStaleGroupOrRouteState` prevents stale selection/group/route projection for empty selection. |
| `469.3` hosted command freshness proof | `d9cbf7e` `Add hosted command freshness proofs` | `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs` | `HostedCommandSurface_RefreshesAfterLayoutPlanApply` proves header undo state refreshes after a layout plan apply. `HostedCommandSurface_RefreshesAfterPendingConnectionStartAndCancel` proves command-palette cancel state refreshes as pending connection state starts and clears. |
| `469.4` focus recovery proof | `e7bd128` `Prove focus after spatial layout interactions` | `src/AsterGraph.Avalonia/Controls/GraphEditorView.axaml.cs`; `tests/AsterGraph.Editor.Tests/GraphEditorViewTests.cs`; `tests/AsterGraph.Editor.Tests/NodeCanvasStandaloneTests.cs` | `AuthoringToolsChrome_SelectionLayoutActionRestoresCanvasFocusAndKeyboardRouting`, `BorderResizeHandles_PointerPress_FocusesCanvasAndKeepsDeleteShortcutRouted`, and `GroupResizeHandlePointerPress_FocusesCanvasAndKeepsDeleteShortcutRouted` prove spatial toolbar and resize interactions return keyboard routing to the canvas. |

The branch also contains v0.78 wave coordination commits (`8d5dfac`, `7a49ed4`, `c330793`, `478a084`). They are bead bookkeeping and do not change the Phase 469 production proof surface.

## Supported Spatial Route

The supported route is:

1. Canonical command descriptors and registry entries describe spatial operations.
2. Context menus, selection tools, keyboard surfaces, and hosted actions project those descriptors.
3. Spatial operations execute through `IGraphEditorCommands.TryExecuteCommand` / kernel command routing.
4. Session queries project selection transform, snap guide, group, route, and command-state snapshots.
5. Avalonia hosted controls recover focus to `NodeCanvas` after spatial toolbar and resize interactions so keyboard commands continue on the same canonical command route.

This route now covers:

- canonical command descriptor/execution for `groups.create`, alignment, distribution, snap, and `selection.transform.move`;
- focus recovery after selection tool clicks, node resize handles, and group resize handles;
- selection, group, snap, and route coherence when move/snap/layout operations compose;
- hosted command freshness after layout apply and pending connection start/cancel.

## Non-Goals

Phase 469 did not add:

- UI-only runtime ownership for layout, transform, snap, group, or route state;
- fallback behavior, compatibility shims, or degraded alternate routes;
- a broad spatial rewrite or replacement interaction engine;
- a second workflow engine, macro/query layer, generated runnable Demo code, marketplace behavior, or sandboxing;
- adapter-specific spatial behavior outside the canonical session contracts.

## Result

`SPACE-01` is satisfied for v0.78 planning purposes: layout, alignment, snapping, groups/subgraphs, selection transforms, and spatial editing now compose as one workbench route through existing session contracts, with stock Avalonia projection proof and focus/command freshness coverage.

Phase 470 can use these proof anchors when turning the cookbook into a component showcase. It should present spatial authoring as a code-plus-demo recipe over the canonical command/query route, not as a separate Demo-owned runtime.
