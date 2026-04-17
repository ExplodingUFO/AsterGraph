# AsterGraph 项目状态

## 当前状态

- 包版本基线：`0.2.0-alpha.1`
- 仓库阶段：公开 Alpha
- 当前公开支持的发布包：
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- 示例与验证工具：
  - `tools/AsterGraph.HostSample`：最小接入路径
  - `tools/AsterGraph.PackageSmoke`：打包消费验证
  - `tools/AsterGraph.ScaleSmoke`：规模与状态连续性验证
- 推荐接入路径：
  - runtime-only 宿主使用 `AsterGraphEditorFactory.CreateSession(...)`
  - Avalonia UI 宿主使用 `AsterGraphEditorFactory.Create(...)` 加 `AsterGraphAvaloniaViewFactory.Create(...)`

## 已经足够对外评估的部分

- 四个可发布 SDK 包边界
- kernel/session-first 的运行时状态所有权
- 默认 Avalonia 壳层与 standalone surfaces
- plugin discovery、trust policy、loading、inspection
- `IGraphEditorSession.Automation`
- contract、maintenance、release proof lanes
- release lane 里的 `.NET 10` 打包 `HostSample` 兼容性验证

## 当前优先事项

当前对外仓库的重点不是继续加运行时功能，而是让仓库公开面保持干净、可理解、可贡献：

- 对外文档入口集中在 `README.md`、`README.zh-CN.md`、`docs/en`、`docs/zh-CN`
- 源码、测试、sample、proof tool、workflow、governance 文件继续公开保留
- 内部工作流痕迹和本地环境文件不再作为公开仓库跟踪内容

## 近期路线

- 保持公开 Alpha 文档和验证指引清晰可执行
- 继续维护托管 CI 与核心验证 lane 的一致性
- 在不突然 breaking 的前提下继续保留兼容迁移窗口
- 以后续 alpha 使用反馈决定下一轮真正的产品功能里程碑

## 公开入口分工

- `tools/AsterGraph.HostSample` = 最小推荐接入验证
- `tools/AsterGraph.PackageSmoke` = 打包消费验证
- `tools/AsterGraph.ScaleSmoke` = 大图、历史记录和状态连续性验证
- `src/AsterGraph.Demo` = 可视化展示宿主

## 对外入口

- [Quick Start](./quick-start.md)
- [Alpha 状态](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Demo Guide](./demo-guide.md)
