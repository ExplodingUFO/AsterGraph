# Phase 469 Handoff: Cookbook Component Showcase

Bead: `avalonia-node-map-v78.4.5`

Next phase: Phase 470 `Cookbook Component Showcase`

## What Phase 469 Proved

Phase 469 closed `SPACE-01` with source-backed evidence that spatial authoring is a coherent workbench flow on the existing AsterGraph session route.

Use these anchors in Phase 470:

| Cookbook scenario need | Phase 469 anchor |
| --- | --- |
| Show spatial commands as supported component commands | `23d315e`: `CommandRegistry_SpatialAuthoringCommandsExposeCanonicalRouteMenuAndToolPlacements`; `GraphEditorKernel_SpatialAuthoringCommands_RunThroughCanonicalCommandRoute` |
| Show selection transforms composing with snap, groups, and routes | `a9dbbd2`: `Commands_ComposeSelectionMoveSnapGroupRouteAndLayoutResetConstraints`; `Queries_RejectEmptySelectionTransformWithoutProjectingStaleGroupOrRouteState` |
| Show hosted command state staying fresh after spatial workflows | `d9cbf7e`: `HostedCommandSurface_RefreshesAfterLayoutPlanApply`; `HostedCommandSurface_RefreshesAfterPendingConnectionStartAndCancel` |
| Show the stock Avalonia workbench preserving keyboard ownership | `e7bd128`: `AuthoringToolsChrome_SelectionLayoutActionRestoresCanvasFocusAndKeyboardRouting`; `BorderResizeHandles_PointerPress_FocusesCanvasAndKeepsDeleteShortcutRouted`; `GroupResizeHandlePointerPress_FocusesCanvasAndKeepsDeleteShortcutRouted` |

## Supported Route To Document

Phase 470 should describe the spatial authoring recipe as:

1. Discover commands from canonical descriptors/registry entries.
2. Project those commands into selection context menus, hosted selection tools, command palette, and shortcut surfaces.
3. Execute through the session command route, not through Demo-only button logic.
4. Read session query snapshots for selection transform, snap guides, groups, routes, and command enablement.
5. Let the stock Avalonia host recover focus to the canvas after spatial actions so keyboard routing remains coherent.

The cookbook should make the stock workbench behavior visible while keeping runtime ownership in `AsterGraph.Editor` session contracts and Avalonia ownership limited to projection, focus, pointer capture, and control lifecycle.

## Suggested Phase 470 Cookbook Scenarios

1. Spatial command route scenario:
   - Start from a selected multi-node graph.
   - Show align, distribute, snap, and group commands as descriptor-backed menu/tool actions.
   - Cite `CommandRegistry_SpatialAuthoringCommandsExposeCanonicalRouteMenuAndToolPlacements` and `GraphEditorKernel_SpatialAuthoringCommands_RunThroughCanonicalCommandRoute`.

2. Selection transform coherence scenario:
   - Demonstrate constrained move, snap guide preview, snap commit, group membership preservation, and route reset behavior after layout apply.
   - Cite `Commands_ComposeSelectionMoveSnapGroupRouteAndLayoutResetConstraints`.

3. Hosted workbench freshness scenario:
   - Show layout apply enabling undo and pending connection cancel enabling/disabling from command state.
   - Cite `HostedCommandSurface_RefreshesAfterLayoutPlanApply` and `HostedCommandSurface_RefreshesAfterPendingConnectionStartAndCancel`.

4. Focus recovery scenario:
   - Show spatial toolbar and resize interactions returning focus to the canvas so Delete and canvas shortcuts still route correctly.
   - Cite the three focus recovery tests from `e7bd128`.

## Boundaries For Phase 470

Do not reopen these non-goals:

- no UI-only runtime ownership for spatial workflows;
- no fallback behavior or compatibility shim;
- no broad spatial rewrite or second interaction runtime;
- no second workflow engine, macro/query layer, generated runnable code execution, marketplace, or sandbox story;
- no cookbook claim that Demo owns semantics beyond projection/proof.

Phase 470 should be a cookbook/docs/demo proof layer over the closed Phase 467, 468, and 469 implementation evidence. For the spatial route, the recipe should point readers to canonical command descriptors, command execution, session snapshots, and stock hosted behavior rather than inventing a parallel example workflow.

## Verification Carried Forward

Phase 469 synthesis validation:

- `git diff --check`
- prohibited external-name scan over the touched planning artifacts

Phase 470 should add its own focused cookbook/docs/proof checks after it updates cookbook assets.
