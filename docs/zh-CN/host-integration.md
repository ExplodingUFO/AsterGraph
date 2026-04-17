# Host Integration 指南

这份文档只展开受支持的宿主路线，不把第一次接入和维护者 proof 文档混在一起。

## Canonical Routes

1. 只要运行时 / 自定义 UI  
   `AsterGraphEditorFactory.CreateSession(...)`
2. 使用默认 Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. retained 迁移  
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

`AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory` 这些独立表面都属于第 2 条路线下的组合细节，不是第四条主路线。

## Consumer Route Matrix

| 需求 | 起始包 | canonical 入口 | 第一个样例 |
| --- | --- | --- | --- |
| 仅运行时 / 自定义 UI | `AsterGraph.Editor`（自定义节点时再加 `AsterGraph.Abstractions`） | `CreateSession(...)` | `tools/AsterGraph.HelloWorld` |
| 默认 Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | `tools/AsterGraph.HostSample` |
| plugin trust / discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `src/AsterGraph.Demo` |
| automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `src/AsterGraph.Demo` |
| retained 迁移 | `AsterGraph.Editor`（嵌入 `GraphEditorView` 时再加 `AsterGraph.Avalonia`） | retained constructor 路线 | 仅用于迁移 |

## 样例分工

- `AsterGraph.HelloWorld` = 仅运行时第一跑样例
- `AsterGraph.HostSample` = 推荐的仅运行时和默认 UI 两条路线的窄范围验证样例
- `AsterGraph.PackageSmoke` = 打包消费验证
- `AsterGraph.ScaleSmoke` = 规模、历史记录与状态连续性验证
- `AsterGraph.Demo` = 用于可视检查的完整展示宿主

## 状态契约

面向宿主的 save/history/dirty 规则见 [State Contracts](./state-contracts.md)。

短版：

- save 建立 clean baseline
- undo 离开保存态后会变 dirty
- redo 回到保存态后会恢复 clean
- no-op interaction 不能制造伪 dirty / undo 状态
- retained 与 runtime mutation 共享同一个 kernel-owned history/save authority

## 扩展契约

稳定性与优先级规则见 [Extension Contracts](./extension-contracts.md)。

重要默认项：

- 推荐的稳定入口是 `CreateSession(...)`、`Create(...)`、`IGraphEditorSession` 和 DTO/snapshot queries
- retained `GraphEditorViewModel` / `GraphEditorView` 仍是受支持的 migration facade
- host localization 在 plugin localization 之后执行，所以 host override 最终生效
- runtime/session menu composition 与 retained augmentor composition 不同；新代码优先走运行时路线

## 插件信任边界

插件信任由宿主负责。AsterGraph 可以帮助你发现候选、做 allow/block 策略、检查结果，但不提供沙箱或不受信任代码隔离。

更深的 proof、CI lane 和 release gate，请看 [CONTRIBUTING.md](../../CONTRIBUTING.md) 和 [Public Launch Checklist](./public-launch-checklist.md)。
