# Demo Guide

`src/AsterGraph.Demo` 是 SDK 的 graph-first showcase host，不属于公开支持的包边界。

## 它在证明什么

- 主 demo 路线已经改成 `AsterGraphEditorFactory.Create(...)`
- Avalonia 表面通过 view factory 组合
- plugin trust / discovery / load 在 UI 里可见
- automation request / progress / result 在 UI 里可见
- standalone surfaces 与 presenter replacement 是真实控件，不是文字说明
- host shell 可以在中文与 English 之间切换，而且不会重建 editor session

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

## 怎么看

- `扩展`：看 candidate discovery、trust decision、load snapshot
- `自动化`：看 typed automation execution 与结果投影
- `集成`：看 `HostSample`、standalone surfaces、presenter replacement、本地化证明
- `证明`：看 host-owned shell state 与 shared runtime evidence 并排出现

## Demo 与其他入口样例的分工

- `HelloWorld` = 最小仅运行时第一跑样例
- `HelloWorld.Avalonia` = 最小 hosted-UI 第一跑样例
- `ConsumerSample.Avalonia` = 带宿主动作、参数编辑和一个可信插件的真实 hosted-UI 样例
- `HostSample` = 推荐消费路线的窄范围验证样例
- `Demo` = 完整能力展示与宿主边界说明

想最快跑起来看 `HelloWorld` 或 `HelloWorld.Avalonia`；想先看一个真实宿主集成看 `ConsumerSample.Avalonia`；想做 proof 导向的路线验证看 `HostSample`；想肉眼检查产品面和边界时看 `Demo`。
