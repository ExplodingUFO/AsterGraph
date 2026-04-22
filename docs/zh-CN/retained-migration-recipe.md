# Retained 到 Session 的迁移 Recipe

只有在现有宿主要分批迁移时才选 retained。这份 recipe 适合这样的宿主：现在还在直接 `new GraphEditorViewModel(...)`，但想逐步迁到推荐的 runtime/session 路线，又不想一次性重写完。retained 路线只是桥接，不是最终目的地。

这是唯一一个 bounded 的 retained recipe 集合。只有在现有宿主还在构造 `GraphEditorViewModel` 或 `GraphEditorView` 时才使用这份文档；否则请从 `CreateSession(...)` 或 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` 开始。

## 目标状态

新代码应该落到：

- 仅运行时 / 自定义 UI：`AsterGraphEditorFactory.CreateSession(...)`
- 默认 Avalonia UI：`AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
- `IGraphEditorSession.Commands`、`Queries`、`Events` 和 diagnostics snapshot

如果你是在做新工作，请先看 [Quick Start](./quick-start.md) 或 [Host Integration](./host-integration.md)，不要从 retained 桥接开始。

## 第一步：先把 Editor Options 收到一处

把 document、catalog、compatibility、storage、plugin、localization、diagnostics 这些输入集中到宿主自己的一个 `AsterGraphEditorOptions` 构造入口里。

## 第二步：新逻辑别再走 ViewModel 专属 helper

宿主新增功能时：

- 优先用 `editor.Session.Commands`
- 优先用 DTO/snapshot query
- `GraphEditorViewModel` 只保留成 UI bridge，等宿主其余部分慢慢迁走

### 补充说明：兼容桥接

新增逻辑优先走 canonical 的运行时命令链路（建议统一用 `connections.disconnect-*`，目前可用实例为 `connections.disconnect-all`），再考虑 retained 的兼容入口。

迁移窗口内仍可保留的兼容 shim：

- 兼容查询：`GetCompatibleTargets(...)` 与 `CompatiblePortTarget`
- 推荐替代：`GetCompatiblePortTargets(...)` 与 `GraphEditorCompatiblePortTargetSnapshot`

在临时兼容阶段，保持以下 retained 辅助方法仅作过渡：

- `TrySetNodeExpansionState(...)`
- `TrySetNodeGroupExtraPadding(...)`

## 第三步：先迁不依赖 Avalonia 的调用方

如果宿主里有一块根本不需要 Avalonia 控件，先迁成：

```csharp
var session = AsterGraphEditorFactory.CreateSession(options);
```

这通常是最快能减少 retained surface 依赖的一步。

## 第四步：默认 UI 也换到 factory 路线

如果宿主还想继续用默认 Avalonia 壳层，就把：

```csharp
var editor = new GraphEditorViewModel(...);
var view = new GraphEditorView { Editor = editor };
```

改成：

```csharp
var editor = AsterGraphEditorFactory.Create(options);
var view = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
{
    Editor = editor,
});
```

这样 UI 仍然能沿用 retained bridge，但 runtime ownership 已经回到共享 kernel/session 路线。

## 第五步：最后再清 compatibility-only query

`GetCompatibleTargets(...)` 和 `CompatiblePortTarget` 只在宿主还需要 MVVM 形状结果时临时保留。新代码应该已经切到 `GetCompatiblePortTargets(...)` 这类 DTO/snapshot API。

## 成功标准

只要宿主满足下面这些条件，基本就算离开了 migration-critical path：

- 新 commands / queries 都落到 `IGraphEditorSession`
- UI 组合走 factory 路线，而不是直接 retained 构造
- compatibility-only shim 只服务尚未迁动的旧宿主代码
- 新功能从 canonical session 路线或 shipped Avalonia 路线起步，而不是从 retained 构造起步

另见：

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
