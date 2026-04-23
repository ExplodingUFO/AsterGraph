# Advanced Editing Guide

`v0.6.0-alpha` 把 advanced editing 收口成几条正式的宿主能力面，全部建立在 canonical session/runtime 路线上。默认 Avalonia adapter 负责把这些 surface 投影成可视 UI，但编辑语义本身仍然以 `CreateSession(...)` + `IGraphEditorSession` 为准。

## Official Advanced-Editing Modules

| Module | Canonical seam | 默认 Avalonia 投影 | Demo proof markers |
| --- | --- | --- | --- |
| `Node Surface Authoring` | `GetNodeSurfaceSnapshots()`、`TrySetNodeSize(...)`、`TrySetNodeParameterValue(...)` 以及 selected-node parameter snapshots | 分层节点卡片、节点旁路参数编辑器、threshold-driven authoring chrome | `TIERED_NODE_SURFACE_OK`、`NON_OBSCURING_EDITING_OK`、`VISUAL_SEMANTICS_OK` |
| `Hierarchy Semantics` | `GetHierarchyStateSnapshot()`、`GetNodeGroups()`、`GetNodeGroupSnapshots()`、`TrySetNodeGroupCollapsed(...)`、`TrySetNodeGroupPosition(...)`、`TrySetNodeGroupSize(...)`、`TrySetNodeGroupMemberships(...)` | 固定组框、内容区 membership、组折叠状态和 frame drag/resize chrome | `FIXED_GROUP_FRAME_OK`、`HIERARCHY_SEMANTICS_OK` |
| `Composite Scope Authoring` | `TryWrapSelectionToComposite(...)`、`TryPromoteNodeGroupToComposite(...)`、`TryExposeCompositePort(...)`、`TryUnexposeCompositePort(...)`、`TryEnterCompositeChildGraph(...)`、`TryReturnToParentGraphScope(...)`、`GetScopeNavigationSnapshot()`、`GetCompositeNodeSnapshots()` | breadcrumb navigation、composite authoring actions 和 scope-return controls | `COMPOSITE_SCOPE_OK` |
| `Edge Semantics` | `TrySetConnectionNoteText(...)`、`TryReconnectConnection(...)`、disconnect commands 和 pending-connection snapshot | 边注解编辑、断开 affordance 和 reconnect workflow | `EDGE_NOTE_OK`、`DISCONNECT_FLOW_OK` |
| `Edge Geometry Tooling` | `GetConnectionGeometrySnapshots()`、`TryInsertConnectionRouteVertex(...)`、`TryMoveConnectionRouteVertex(...)`、`TryRemoveConnectionRouteVertex(...)` | route-vertex authoring tools 和 routed edge rendering | `EDGE_GEOMETRY_OK` |

## 使用方式

1. 从 runtime/session seam 起步，而不是从 retained presenter 类型起步。
2. 把 `AsterGraph.Avalonia` 视为第一套 adapter 投影，而不是编辑语义本体。
3. 当宿主做了自定义后，继续用上面的 proof markers 检查 advanced-editing story 是否仍然完整可见。

## 样例与 proof 锚点

- `src/AsterGraph.Demo` 是把这 5 条 advanced-editing module 一起公开展示出来的可视宿主。
- `docs/zh-CN/demo-guide.md` 会用同一套 module 名称解释 proof marker。
- `docs/zh-CN/host-integration.md` 会把 route-to-seam matrix 与这里对齐，避免宿主只能靠 retained-only 说明理解能力面。
- [Authoring Surface Recipe](./authoring-surface-recipe.md) 是 hosted Avalonia 路线下可直接复制的 recipe，适合替换节点、端口、参数和边展示，但不改变 session 路线。

## 这不意味着什么

- 这不表示 Avalonia 下面还藏着第二套编辑模型。
- 这不表示受支持的包边界会超出 `AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor`、`AsterGraph.Avalonia`。
- 这也不表示 retained compatibility surface 会重新成为新代码的推荐路径。
