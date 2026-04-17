# AsterGraph

AsterGraph 是一个面向 .NET 的模块化节点图编辑器工具包，包含可复用的编辑器运行时层、Avalonia UI 壳层，以及面向宿主的扩展与诊断边界。

## Public Alpha

- 当前包版本基线：`0.2.0-alpha.1`
- English docs：[`docs/en/`](./docs/en/)
- 中文文档：[`docs/zh-CN/`](./docs/zh-CN/)
- 项目当前状态：[`docs/zh-CN/project-status.md`](./docs/zh-CN/project-status.md)
- Alpha 范围、已知限制与稳定性说明：[`docs/zh-CN/alpha-status.md`](./docs/zh-CN/alpha-status.md)
- 对外发布检查清单：[`docs/zh-CN/public-launch-checklist.md`](./docs/zh-CN/public-launch-checklist.md)
- 公开仓库 hygiene 门禁：`pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release`

公开入口分工：

- `tools/AsterGraph.HostSample` = 最小 canonical consumer path
- `tools/AsterGraph.PackageSmoke` = packaged-consumption proof
- `tools/AsterGraph.ScaleSmoke` = 规模 / history / 状态连续性 proof
- `src/AsterGraph.Demo` = 可视化 showcase host

## 当前能力范围

当前已提供：

- 节点拖拽、选择、框选、多选编辑
- 画布缩放、平移、缩略图、待完成连线预览
- 保存 / 加载
- Undo / Redo
- Copy / Paste
- 对齐 / 分布
- Selection fragment 导入导出
- 节点定义 provider 注册
- runtime plugin registration / trust policy / candidate discovery / load inspection
- `IGraphEditorSession.Automation`
- 运行时诊断与 inspection snapshot

当前明确不做：

- 算法执行引擎
- 插件 marketplace 或远程安装 / 更新
- 插件卸载生命周期
- 沙箱或不受信任代码隔离保证
- 专用脚本语言或 automation 可视化编排器

## 包边界

公开支持的 SDK 包只有四个：

- `AsterGraph.Abstractions`
- `AsterGraph.Core`
- `AsterGraph.Editor`
- `AsterGraph.Avalonia`

`AsterGraph.Demo` 只是 sample host，不属于 SDK 支持边界。

## 推荐入口

新宿主按这三条路径接入：

1. 只要 runtime / 自定义 UI  
   `AsterGraphEditorFactory.CreateSession(...)`
2. 使用默认 Avalonia UI  
   `AsterGraphEditorFactory.Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`
3. 迁移旧宿主  
   `new GraphEditorViewModel(...)` + `new GraphEditorView { Editor = editor }`

更完整的分流和命令见：

- [Quick Start](./docs/zh-CN/quick-start.md)
- [Project Status](./docs/zh-CN/project-status.md)
- [Host Integration](./docs/zh-CN/host-integration.md)
- [State Contracts](./docs/zh-CN/state-contracts.md)
- [Extension Contracts](./docs/zh-CN/extension-contracts.md)
- [Demo Guide](./docs/zh-CN/demo-guide.md)
- [Public Launch Checklist](./docs/zh-CN/public-launch-checklist.md)

## Demo

`src/AsterGraph.Demo` 是 graph-first showcase host。

当前窗口菜单分组是：

- `展示`
- `视图`
- `行为`
- `运行时`
- `扩展`
- `自动化`
- `集成`
- `证明`

并且支持中英文切换，用来证明：

- host shell 文案不是硬编码死在 UI 里
- runtime localization 可以通过 `SetLocalizationProvider(...)` 切换
- demo 主路径已经走 canonical factory/session/view-factory 组合，而不是旧的 retained 构造路径

## Proof Ring

官方 proof ring 入口：

- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane hygiene -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane all -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane contract -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane maintenance -Framework all -Configuration Release`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\eng\ci.ps1 -Lane release -Framework all -Configuration Release`

最小 consumer sample、package smoke、scale readiness proof 分别是：

- `tools/AsterGraph.HostSample`
- `tools/AsterGraph.PackageSmoke`
- `tools/AsterGraph.ScaleSmoke`
