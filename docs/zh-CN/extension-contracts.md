# 扩展与维护契约

这份文档发布 surface stability、compatibility retirement、extension precedence 与 lane ownership 的契约。

## 稳定性层级

### Stable canonical surfaces

- `AsterGraphEditorFactory.CreateSession(...)`
- `AsterGraphEditorFactory.Create(...)`
- `IGraphEditorSession`
- `GetCompatiblePortTargets(...)` 这类 DTO / snapshot 查询
- runtime-boundary 上的 diagnostics / automation / plugin inspection

### Retained migration surfaces

- `GraphEditorViewModel`
- `GraphEditorView`
- `GraphEditorViewModel.Session` 作为桥接 canonical runtime contract 的入口

### Compatibility-only shims

- `IGraphEditorQueries.GetCompatibleTargets(...)`
- `CompatiblePortTarget`
- 其他已经存在 runtime-first 替代物的旧 MVVM 形状 helper

## 扩展优先级

- plugin trust 由 host 决定，并且发生在 activation 之前
- plugin localization 先组合，host localization 最后覆盖
- plugin node presentation 先组合，host presentation 覆盖最终字段，合并型 adornment 继续累积
- plugin command 通过 canonical session command descriptor pipeline 注册；如果和 stock command 撞 id，stock command 继续保留执行权
- runtime/session menu 当前仍以 stock descriptor 投影为主，并继续向共享 command source 收敛
- retained `GraphEditorViewModel.BuildContextMenu(...)` 仍是 compatibility host 的最终 override 点

## Lane Ownership

- `eng/ci.ps1 -Lane all` = framework-matrix build/test lane
- `eng/ci.ps1 -Lane contract` = focused consumer/state-contract gate
- `eng/ci.ps1 -Lane maintenance` = hotspot-refactor gate
- `eng/ci.ps1 -Lane release` = packed publish gate + smoke + coverage
- `tests/AsterGraph.Demo.Tests` = demo/sample-host lane

排查失败时，先按 lane 分类，再决定改哪一层代码。
