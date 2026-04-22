# Demo Guide

`src/AsterGraph.Demo` 是 SDK 的 graph-first showcase host，不属于公开支持的包边界。

## 它在证明什么

- 主 demo 壳层通过 `AsterGraphEditorFactory.Create(...)` 组合，但共享运行时 owner 仍然是 `Session`
- demo 只是把同一条 canonical session/runtime 路线投影成更丰富的宿主壳层，并没有引入第二套编辑模型
- Avalonia 表面通过 view factory 组合
- definition-driven inspector 分组、内建 editor 和校验提示已经能在真实 demo 节点定义上直接看到
- plugin trust / discovery / load 在 UI 里可见，而且会显示 version、target framework、fingerprint、reason 和 allowlist 导入/导出
- automation request / progress / result 在 UI 里可见
- standalone surfaces 与 presenter replacement 是真实控件，不是文字说明
- host shell 可以在中文与 English 之间切换，而且不会重建 editor session
- recent files、autosave 恢复、dirty-exit 保护、drag-and-drop 打开和布局持久化都已经变成宿主自管的 shell workflow

## 宿主菜单分组

英文界面下的菜单分组：

- `Showcase`
- `View`
- `Behavior`
- `Runtime`
- `Extensions`
- `Automation`
- `Integration`
- `Proof`

中文界面下的菜单分组：

- `展示`
- `视图`
- `行为`
- `运行时`
- `扩展`
- `自动化`
- `集成`
- `证明`

顶栏菜单里还有可见的语言切换入口。

## Proof 模式

```powershell
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof
```

预期 marker：

- `DEMO_TRUST_OK:True`
- `DEMO_SHELL_OK:True`
- `COMMAND_SURFACE_OK:True`
- `TIERED_NODE_SURFACE_OK:True`
- `FIXED_GROUP_FRAME_OK:True`
- `NON_OBSCURING_EDITING_OK:True`
- `VISUAL_SEMANTICS_OK:True`
- `HIERARCHY_SEMANTICS_OK:True`
- `COMPOSITE_SCOPE_OK:True`
- `EDGE_NOTE_OK:True`
- `EDGE_GEOMETRY_OK:True`
- `DISCONNECT_FLOW_OK:True`
- `HOST_NATIVE_METRIC:startup_ms=...`
- `HOST_NATIVE_METRIC:inspector_projection_ms=...`
- `HOST_NATIVE_METRIC:plugin_scan_ms=...`
- `HOST_NATIVE_METRIC:command_latency_ms=...`
- `DEMO_OK:True`

## 怎么看

- `扩展`：看 candidate discovery、trust decision、load snapshot 和 allowlist 决策
- `自动化`：看 typed automation execution 与结果投影
- `集成`：看 `HostSample`、standalone surfaces、presenter replacement、本地化证明
- `运行时` 和 `证明`：一起看 host-owned shell state、recent workspace、autosave 提示、threshold-driven side rails 和 shared runtime evidence
- `证明`：会把 advanced-editing split 映射到正式 surface：`Node Surface Authoring`（`TIERED_NODE_SURFACE_OK`、`NON_OBSCURING_EDITING_OK`、`VISUAL_SEMANTICS_OK`）、`Hierarchy Semantics`（`FIXED_GROUP_FRAME_OK`、`HIERARCHY_SEMANTICS_OK`）、`Composite Scope Authoring`（`COMPOSITE_SCOPE_OK`）、`Edge Semantics`（`EDGE_NOTE_OK`、`DISCONNECT_FLOW_OK`）和 `Edge Geometry Tooling`（`EDGE_GEOMETRY_OK`）。
- `视图` 和 `证明`：一起把 hierarchy、composite scope、edge semantics 和 edge geometry 维持在可见产品面上，而不是退回 retained-only 解释或第二套编辑模型。

## Demo 与其他入口样例的分工

- `Starter.Avalonia` = 第一个 hosted 脚手架；最小端到端 Avalonia 入口
- `HelloWorld` = 最小仅运行时第一跑样例
- `HelloWorld.Avalonia` = 最小 hosted-UI 第一跑样例
- `ConsumerSample.Avalonia` = canonical 路线上的真实 hosted-UI 样例，带宿主动作、参数编辑和一个可信插件
- `HostSample` = 推荐消费路线的窄范围验证样例，不是上手入口
- `Demo` = 完整能力展示与宿主边界说明

想先看第一个 hosted 入口看 `Starter.Avalonia`；想最快跑起来看 `HelloWorld` 或 `HelloWorld.Avalonia`；想先看一个真实宿主集成看 `ConsumerSample.Avalonia`；想做 proof 导向的路线验证看 `HostSample`；想肉眼检查产品面和边界时看 `Demo`。
