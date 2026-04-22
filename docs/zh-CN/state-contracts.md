# History / Save / Dirty 契约

这份文档发布 AsterGraph 在 history、save、dirty 语义上的宿主可依赖契约。

## 适用范围

这份契约同时适用于：

- 以 `IGraphEditorSession` 为根的 canonical runtime/session 路线
- 以 `GraphEditorViewModel` 为根的 retained compatibility bridge 路线
- retained interaction 与 runtime/session command 混合操作同一状态的场景

## 契约

1. 成功保存后，当前状态变 clean。
2. undo 离开保存态时，编辑器应变 dirty。
3. redo 回到保存态时，编辑器应恢复 clean。
4. no-op interaction 不能制造伪 dirty / undo 状态。
5. retained 与 runtime mutation 共享同一个 kernel-owned history/save authority。

## 证明面

官方 proof：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release
```

该 lane 会覆盖：

- `GraphEditorHistoryInteractionTests`
- `GraphEditorSaveBoundaryTests`
- `GraphEditorHistorySemanticTests`

更大一点的 readiness proof 还会通过 `tools/AsterGraph.ScaleSmoke` 输出 `SCALE_HISTORY_CONTRACT_OK`。

## 对宿主的建议

- 把 `SaveWorkspace()` 视为 clean baseline 的建立点。
- 不要假设每次交互都会形成有效 history step。
- 新接入优先使用 canonical session boundary，并把 retained 路线只当作迁移支持。
