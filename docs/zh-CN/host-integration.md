# Host Integration 指南

这份文档展开 canonical host 路线，但不重新定义第二套路由树。

## Canonical Routes

1. 只要 runtime / 自定义 UI  
   `AsterGraphEditorFactory.CreateSession(...)`
2. 使用默认 Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. retained migration  
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

`AsterGraphCanvasViewFactory`、`AsterGraphInspectorViewFactory`、`AsterGraphMiniMapViewFactory` 这些独立表面，属于第 2 条路线下的高级组合细节，不是第四条主路线。

## Consumer Route Matrix

| 需求 | 起始包 | canonical 入口 | 验证方式 |
| --- | --- | --- | --- |
| runtime-only / custom UI | `AsterGraph.Abstractions`, `AsterGraph.Editor` | `CreateSession(...)` | packed `HostSample` |
| 默认 Avalonia UI | `AsterGraph.Avalonia` | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | packed `HostSample` |
| plugin trust / discovery | `AsterGraph.Editor` | `DiscoverPluginCandidates(...)` + `PluginTrustPolicy` | `eng/ci.ps1 -Lane contract` |
| automation | `AsterGraph.Editor` | `IGraphEditorSession.Automation.Execute(...)` | `eng/ci.ps1 -Lane contract` |
| retained migration | `AsterGraph.Editor`（嵌 `GraphEditorView` 时再加 `AsterGraph.Avalonia`） | retained constructor 路线 | `eng/ci.ps1 -Lane contract` |

## 最小 Consumer Host

`tools/AsterGraph.HostSample` 是最小可运行 proof，用来证明：

- `CreateSession(...)`
- `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`

它故意保持窄范围，不替代：

- `AsterGraph.PackageSmoke`
- `AsterGraph.ScaleSmoke`
- `AsterGraph.Demo`

## 状态契约

面向宿主的 save/history/dirty 规则见 [State Contracts](./state-contracts.md)。

短版：

- save 建立 clean baseline
- undo 离开保存态后会变 dirty
- redo 回到保存态后会恢复 clean
- no-op interaction 不能制造伪 dirty / undo 状态
- retained 与 runtime mutation 仍共享同一个 kernel-owned history/save authority

## 扩展契约

稳定性与优先级规则见 [Extension Contracts](./extension-contracts.md)。

重要默认项：

- canonical surfaces 是 `CreateSession(...)`、`Create(...)`、`IGraphEditorSession` 和 DTO/snapshot queries
- retained `GraphEditorViewModel` / `GraphEditorView` 仍是受支持的 migration facade
- host localization 在 plugin localization 之后执行，所以 host override 最终生效
- runtime/session menu composition 与 retained augmentor composition 不同；新代码优先走 runtime 路线

## Release Verification

推荐 proof-ring 入口：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release
```

各工具职责：

- `AsterGraph.HostSample` = 最小 consumer proof
- `AsterGraph.PackageSmoke` = packed package consumption proof
- `AsterGraph.ScaleSmoke` = 大图与 history/readiness proof
- `AsterGraph.Demo` = 可视化 showcase host
