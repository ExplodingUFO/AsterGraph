# Retained 到 Session 的迁移 Recipe

只有在现有宿主要分批迁移时才选 retained。这份 recipe 是迁移期专用，不是长期首选扩展路径。它适合这样的宿主：现在还在直接 `new GraphEditorViewModel(...)`，但想逐步迁到 canonical 的 runtime/session seam，又不想一次性重写完。

只有在现有 retained 宿主切片上才复制这份 recipe。它仍然次于 canonical session/runtime 路线和 shipped Avalonia 路线，也不会新增新的兼容性承诺。

这是唯一一个 bounded 的 retained recipe 集合。只有在现有宿主还在构造 `GraphEditorViewModel` 或 `GraphEditorView` 时才使用这份文档；否则请从 `CreateSession(...)` 或 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 开始。

## 目标状态

新代码应该落到：

- 仅运行时 / 自定义 UI：`AsterGraphEditorFactory.CreateSession(...)`
- 默认 Avalonia UI：`AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
- `IGraphEditorSession.Commands`、`Queries`、`Events` 和 diagnostics snapshot

如果你是在做新工作，请先看 [Quick Start](./quick-start.md) 或 [Host Integration](./host-integration.md)，不要从 retained 桥接开始。

## 第 0 阶段：先保留 retained 桥接

暂时保留：

- `GraphEditorViewModel`
- `GraphEditorView`
- 宿主自己负责的 `AsterGraphEditorOptions` 组装

替换成 canonical seam：

- 新的 runtime-only 切片先走 `AsterGraphEditorFactory.CreateSession(...)`
- 新的 command / query 走 `IGraphEditorSession`

这个阶段之后的归属：

- 宿主仍然负责 shell 组合和 options wiring
- session 负责 commands、queries、events、diagnostics 和 automation
- retained 桥接只负责把 UI 表面接起来

## 第 1 阶段：把 command 和 query 的归属移入 session

暂时保留：

- 还没有迁移到别处的 retained view-model 桥接
- 如果宿主还需要 shipped Avalonia shell，就先保留它

替换成 canonical seam：

- 新 action 直接走 `editor.Session.Commands`
- query 改成 DTO / snapshot query，不再依赖 retained projection
- 只要是 runtime-only 调用方，就优先用 `CreateSession(...)`

这个阶段期间还能保留的兼容桥接：

- retained connection discovery：`GetCompatibleTargets(...)` 和 `CompatiblePortTarget`
- canonical 替换：`GetCompatiblePortTargets(...)` 和 `GraphEditorCompatiblePortTargetSnapshot`
- 临时 node-group helper：`TrySetNodeExpansionState(...)` 和 `TrySetNodeGroupExtraPadding(...)`
- canonical connection control：`connections.disconnect-*`，尤其是 `connections.disconnect-all`

这个阶段之后的归属：

- commands 和 queries 都由 session 负责
- 宿主代码只负责 orchestration 和 composition
- compatibility shim 只服务旧代码，不再接收新行为

## 第 2 阶段：替换 retained-only helper 并退役桥接专属行为

暂时保留：

- 只给尚未迁移的旧代码保留 `GraphEditorViewModel` 和 `GraphEditorView`

替换成 canonical seam：

- shipped Avalonia UI 改成 `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
- compatibility projection 改成 `GetCompatiblePortTargets(...)` 和 snapshot API
- 新工作统一从 session/runtime 路线开始

这个阶段之后的归属：

- UI 组合由 factory 负责
- commands 和 queries 由 session 负责
- retained helper 只剩 legacy-only 角色，不再是扩展路径

## 证据交接

retained 迁移的证据也走和其他 public 文档一样的 defended hosted beta 路线：

- 运行 `AsterGraph.ConsumerSample.Avalonia -- --proof`
- 如果需要本地产物，就按 [Beta Support Bundle](./support-bundle.md) 的合同附上 `artifacts/consumer-support-bundle.json`
- 复查同一组公开 proof marker：`CONSUMER_SAMPLE_OK:True`、`COMMAND_SURFACE_OK:True`、`HOST_NATIVE_METRIC:*`，以及 support-bundle marker `SUPPORT_BUNDLE_OK:True` 和 `SUPPORT_BUNDLE_PATH:...`

retained 桥接不会新增单独的 retained 支持边界，也不会引入 retained-only 的证据 lane，所以维护者可以直接用公开 proof 文档和 bundle 文档来审核。

## 成功标准

宿主只要满足下面这些条件，就基本已经离开 migration-critical path：

- 新 commands 和 queries 都落到 `IGraphEditorSession`
- UI 组合走 factory 路线，而不是直接 retained 构造
- compatibility-only shim 只服务尚未迁动的旧宿主代码
- 新功能从 canonical session 路线或 shipped Avalonia 路线起步，而不是从 retained 构造起步

另见：

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
