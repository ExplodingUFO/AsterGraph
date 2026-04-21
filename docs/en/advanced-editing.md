# Advanced Editing Guide

`v0.6.0-alpha` closes advanced editing as a small set of official host-facing surfaces layered on top of the canonical session/runtime route. The stock Avalonia adapter projects those surfaces visually, but the editing model itself stays on `CreateSession(...)` + `IGraphEditorSession`.

## Official Advanced-Editing Modules

| Module | Canonical seam | Stock Avalonia projection | Demo proof markers |
| --- | --- | --- | --- |
| `Node Surface Authoring` | `GetNodeSurfaceSnapshots()`, `TrySetNodeSize(...)`, `TrySetNodeParameterValue(...)`, and selected-node parameter snapshots | tiered node cards, node-side parameter editors, threshold-driven authoring chrome | `TIERED_NODE_SURFACE_OK`, `NON_OBSCURING_EDITING_OK`, `VISUAL_SEMANTICS_OK` |
| `Hierarchy Semantics` | `GetHierarchyStateSnapshot()`, `GetNodeGroups()`, `GetNodeGroupSnapshots()`, `TrySetNodeGroupCollapsed(...)`, `TrySetNodeGroupPosition(...)`, `TrySetNodeGroupSize(...)`, `TrySetNodeGroupMemberships(...)` | fixed group frames, content-area membership, group collapse state, and frame drag/resize chrome | `FIXED_GROUP_FRAME_OK`, `HIERARCHY_SEMANTICS_OK` |
| `Composite Scope Authoring` | `TryWrapSelectionToComposite(...)`, `TryPromoteNodeGroupToComposite(...)`, `TryExposeCompositePort(...)`, `TryUnexposeCompositePort(...)`, `TryEnterCompositeChildGraph(...)`, `TryReturnToParentGraphScope(...)`, `GetScopeNavigationSnapshot()`, `GetCompositeNodeSnapshots()` | breadcrumb navigation, composite authoring actions, and scope-return controls | `COMPOSITE_SCOPE_OK` |
| `Edge Semantics` | `TrySetConnectionNoteText(...)`, `TryReconnectConnection(...)`, disconnect commands, and the pending-connection snapshot | edge note editing, disconnect affordances, and reconnect workflows | `EDGE_NOTE_OK`, `DISCONNECT_FLOW_OK` |
| `Edge Geometry Tooling` | `GetConnectionGeometrySnapshots()`, `TryInsertConnectionRouteVertex(...)`, `TryMoveConnectionRouteVertex(...)`, `TryRemoveConnectionRouteVertex(...)` | route-vertex authoring tools and routed edge rendering | `EDGE_GEOMETRY_OK` |

## How To Use The Split

1. Start from the runtime/session seam, not from retained presenter types.
2. Treat `AsterGraph.Avalonia` as the first adapter that projects these surfaces, not as the source of truth for editing semantics.
3. Use the proof markers above when you need to confirm that a host build still exposes the full advanced-editing story after customization.

## Sample And Proof Anchors

- `src/AsterGraph.Demo` is the visual host that keeps all five advanced-editing modules visible together.
- `docs/en/demo-guide.md` maps the proof markers to the same module names used here.
- `docs/en/host-integration.md` keeps the route-to-seam matrix aligned with these modules so hosts do not need retained-only explanations.

## What This Does Not Mean

- It does not create a second editing model underneath Avalonia.
- It does not expand the supported package boundary beyond `AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, and `AsterGraph.Avalonia`.
- It does not turn retained compatibility surfaces into the guidance path for new work.
