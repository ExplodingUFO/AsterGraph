# Demo Guide

`src/AsterGraph.Demo` 是 SDK 的 graph-first showcase host，不属于公开支持的包边界。

## 它在证明什么

- 主 demo 路线已经改成 `AsterGraphEditorFactory.Create(...)`
- Avalonia 表面通过 view factory 组合
- plugin trust / discovery / load 在 UI 里可见
- automation request / progress / result 在 UI 里可见
- standalone surfaces 与 presenter replacement 是真实控件，不是文字说明
- host shell 可以在中文与 English 之间切换，而且不会重建 editor session

## Host Menu Groups

- `展示`
- `视图`
- `行为`
- `运行时`
- `扩展`
- `自动化`
- `集成`
- `证明`

顶栏菜单里还有可见的语言切换入口。

## 怎么看

- `扩展`：看 candidate discovery、trust decision、load snapshot
- `自动化`：看 typed automation execution 与结果投影
- `集成`：看 `HostSample`、standalone surfaces、presenter replacement、本地化证明
- `证明`：看 host-owned shell state 与 shared runtime evidence 并排出现

## Demo 和 HostSample 的分工

- `HostSample` = 最小 canonical consumer path
- `Demo` = 完整能力展示与宿主边界说明

需要最短接入 proof 时看 `HostSample`；需要肉眼确认产品面和边界时看 `Demo`。
