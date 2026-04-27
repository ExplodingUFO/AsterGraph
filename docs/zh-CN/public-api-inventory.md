# Public API Inventory

这份清单是 AsterGraph 公开包 surface 的维护者地图。它不创建新的 runtime model，只给已经公开的 surface 分类，让宿主能区分 canonical API、hosted 组合 helper、retained 迁移桥、compatibility-only shim 和 internal implementation detail。

请和 [Host Integration](./host-integration.md)、[Extension Contracts](./extension-contracts.md) 一起使用。如果这些文档说法不一致，先修正文档，再扩大 public surface。

## 支持层级

| 层级 | 含义 | 消费者建议 |
| --- | --- | --- |
| Stable canonical | 定义受支持宿主接入路线的 runtime/session contract。 | 新工作和自定义 UI / 原生 shell 接入默认使用。 |
| Supported hosted helper | 建立在 canonical route 和 shipped Avalonia adapter 上的薄组合 helper。 | 当默认 Avalonia 路线足够时使用。 |
| Retained migration | 为旧宿主分批迁移保留的 MVVM / view surface。 | 只作为迁移桥使用。 |
| Compatibility-only | 已有 canonical 替代路线的 obsolete 或旧形状 helper。 | 新工作不要使用；按 replacement guidance 迁移。 |
| Internal-only | implementation detail、internal namespace、测试、样例和 proof tool。 | 不是公开支持契约。 |

## 包级清单

| 包 | Stable canonical | Supported hosted helper | Retained migration | Compatibility-only | Internal-only boundary |
| --- | --- | --- | --- | --- | --- |
| `AsterGraph.Abstractions` | 节点定义、端口定义、provider / plugin-facing contract、identifier、metadata DTO。 | 无。 | 无。 | 当前不作为主支持层级发布。 | 未通过 package docs 暴露的实现 helper。 |
| `AsterGraph.Core` | 图文档、序列化模型契约、兼容规则输入，以及 editor/session 组合使用的共享数据类型。 | 无。 | 无。 | 只有在已有 runtime-first 路线时保留旧 conversion / compatibility helper。 | Core internals 与持久化实现细节。 |
| `AsterGraph.Editor` | `AsterGraphEditorFactory.CreateSession(...)`、`IGraphEditorSession`、`IGraphEditorCommands`、`IGraphEditorQueries`、DTO / snapshot 查询、diagnostics、automation、runtime overlay snapshots/providers、plugin discovery / inspection、export services。 | `AsterGraphEditorFactory.Create(...)` 是 hosted 组合 helper，但仍返回 retained facade。 | `GraphEditorViewModel`、`GraphEditorViewModel.Session`、迁移宿主使用的 retained menu / context-menu hook。 | `IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget`，以及已有 `GetCompatiblePortTargets(...)` 或 command/query snapshot 替代物的旧 MVVM 形状 helper。 | `Runtime.Internal`、`Kernel.Internal`、projection/apply internals、proof-only helper。 |
| `AsterGraph.Avalonia` | canonical editor/session route 上的 adapter 投影，以及复用同一 runtime owner 的 hosted factory。 | `AsterGraphAvaloniaViewFactory.Create(...)`、`AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory`、`AsterGraphHostBuilder`。 | 仍使用 retained editor facade 的宿主可嵌入 `GraphEditorView`。 | 仅限把现有宿主桥接到 canonical route 的 adapter-specific compatibility glue。 | Control internals、templates、interaction session internals、visual-only implementation details。 |

## 路线映射

| 宿主路线 | 支持层级 | 主要符号 | 说明 |
| --- | --- | --- | --- |
| 仅运行时 / 自定义 UI | Stable canonical | `AsterGraphEditorFactory.CreateSession(...)`、`IGraphEditorSession`、`IGraphEditorCommands.TryCreateConnectedNodeFromPendingConnection(...)`、`IGraphEditorCommands.TryInsertNodeIntoConnection(...)`、`IGraphEditorCommands.TryDeleteSelectionAndReconnect(...)`、`IGraphEditorCommands.TryDetachSelectionFromConnections(...)`、`IGraphEditorCommands.SetConnectionSelection(...)`、`IGraphEditorCommands.TryDeleteSelectedConnections()`、`IGraphEditorCommands.TrySliceConnections(...)`、`IGraphEditorQueries.GetSelectedNodeConnectionIds()`、`IGraphEditorQueries.GetRuntimeOverlaySnapshot()`、`GraphEditorSelectionSnapshot.SelectedConnectionIds`、`IGraphRuntimeOverlayProvider`、`GraphEditorRuntimeOverlaySnapshot` | 新的自定义 UI 和原生 shell 默认从这里开始；authoring productivity 命令、wire-selection snapshot 和 host-owned runtime feedback overlay 都走 canonical session route。 |
| Shipped Avalonia UI | Supported hosted helper | `AsterGraphEditorFactory.Create(...)`、`AsterGraphAvaloniaViewFactory.Create(...)` | 使用同一个 runtime owner，不是第二套 runtime model。 |
| Thin hosted builder | Supported hosted helper | `AsterGraphHostBuilder.Create(...).BuildAvaloniaView()` | 减少常见 Avalonia 组合样板，并委托给 canonical factories。 |
| Retained migration bridge | Retained migration | `GraphEditorViewModel`、`GraphEditorView`、`GraphEditorViewModel.Session` | 只给旧宿主分批迁移使用。 |
| 旧 compatible-target query | Compatibility-only | `IGraphEditorQueries.GetCompatibleTargets(...)`、`CompatiblePortTarget` | 优先使用 `GetCompatiblePortTargets(...)` 和 `GraphEditorCompatiblePortTargetSnapshot`。 |

## Drift 规则

- 新增 public host-facing symbol 必须在发布前进入这份 inventory 分类。
- Public API 变化必须有意更新 `eng/public-api-baseline.txt`；未分类 drift 会让 release gate 失败。
- 新增 Stable canonical surface 必须同步到 [Host Integration](./host-integration.md) 或 [Extension Contracts](./extension-contracts.md)。
- 新增 retained migration 或 compatibility-only surface 必须带 replacement guidance。
- 当 canonical replacement 已存在时，compatibility-only API 必须标记 obsolete。
- README、quick-start 和 release notes 不应宣传 internal implementation details。

## 维护者检查表

发布前：

1. 检查 package docs、README、quick-start 和 release notes 是否使用同一套路由命名。
2. 检查 retained 与 compatibility-only API 是否仍指向 canonical replacement。
3. 检查 `AsterGraphHostBuilder` 是否仍被描述为 thin hosted helper，而不是 runtime model。
4. 检查 WPF wording 是否仍保持 validation-only，除非 adapter support matrix 明确改变。
