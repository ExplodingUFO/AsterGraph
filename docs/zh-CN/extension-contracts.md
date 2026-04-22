# 扩展与维护契约

这份文档发布 surface stability、compatibility retirement、extension precedence 与 lane ownership 的契约。
它不引入新的 API 稳定语义，只定义当前发布边界里 canonical-runtime 与已承诺 hosted 形态的优先级。

## 稳定性层级

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `IGraphEditorSession`
- `GetCompatiblePortTargets(...)` 这类 DTO / snapshot 查询
- runtime-boundary 上的 diagnostics / automation / plugin inspection

### Supported hosted-UI composition helper

- `AsterGraphEditorFactory.Create(...)`

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` 作为桥接 canonical runtime contract 的入口

### Compatibility-only shims

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- 其他已经存在 runtime-first 替代物的旧 MVVM 形状 helper

`retained` 与 `compatibility-only` 属于迁移路径，不是可长期扩展的“新功能落点”。
新工作应优先从 `Stable canonical surfaces` 起步，必要时经 `AsterGraphAvaloniaViewFactory` 经由 `AsterGraphEditorFactory.Create(...)` 组合 hosted UI，并与 [Host Integration](./host-integration.md) 对齐。

## 包级稳定性边界（`AsterGraph` 发布边界）

- `AsterGraph.Abstractions`：稳定发布 node / provider / 插件契约，适合作为 contract-first 集成入口。
- `AsterGraph.Core`：当前用于 `GraphDocument`、序列化、兼容层与迁移辅助，不替代 `Editor.Session` 的 canonical 运行时语义。
- `AsterGraph.Editor`：canonical runtime/session owner，承载运行时主语义与稳定交互面；新增能力优先从这里落地。
- `AsterGraph.Avalonia`：当前唯一 shipped 的官方 adapter，提供 `Create(...)` 与默认 shell/standalone composition 组合面，不构成第二套运行时 API。

canonical-first 指导是：先从 `AsterGraph.Editor` 的 canonical surface 做新功能，再决定是否叠加 `AsterGraph.Avalonia`；`retained` 只用于迁移，不是默认起点。

## 扩展优先级

- plugin trust 由 host 决定，并且发生在 activation 之前
- plugin localization 先组合，host localization 最后覆盖
- plugin node presentation 先组合，host presentation 覆盖最终字段，合并型 adornment 继续累积
- plugin command 通过 canonical session command descriptor pipeline 注册；如果和 stock command 撞 id，stock command 继续保留执行权
- runtime/session menu 当前仍以 stock descriptor 投影为主，并继续向共享 command source 收敛。
- retained `GraphEditorViewModel.BuildContextMenu(...)` 仍是 compatibility host 的最终 override 点。

## Lane Ownership

- `eng/ci.ps1 -Lane all` = framework-matrix build/test lane
- `eng/ci.ps1 -Lane contract` = focused consumer/state-contract gate
- `eng/ci.ps1 -Lane maintenance` = hotspot-refactor gate
- `eng/ci.ps1 -Lane release` = packed publish gate + smoke + coverage
- `tests/AsterGraph.Demo.Tests` = demo/sample-host lane

排查失败时，先按 lane 分类，再决定改哪一层代码。
