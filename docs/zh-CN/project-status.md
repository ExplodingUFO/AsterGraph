# AsterGraph 项目状态

## 当前状态

- 包版本基线：`0.2.0-alpha.3`
- 最新对外 SemVer prerelease 标签：`v0.2.0-alpha.3`
- 最新历史仓库里程碑标签：`v1.9`
- 仓库阶段：公开 Alpha
- 对外版本说明：[Versioning](./versioning.md)
- 当前公开支持的发布包：
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- 示例与验证工具：
  - `tools/AsterGraph.HelloWorld`：最快的 runtime-only 第一跑样例
  - `tools/AsterGraph.HelloWorld.Avalonia`：最快的默认 UI 第一跑样例
  - `tools/AsterGraph.ConsumerSample.Avalonia`：介于 HelloWorld 和 Demo 之间的真实宿主样例
  - `tools/AsterGraph.HostSample`：最小接入验证样例
  - `tools/AsterGraph.PackageSmoke`：打包消费验证
  - `tools/AsterGraph.ScaleSmoke`：公开的大图基线与状态连续性验证
- 推荐接入路径：
  - runtime-only 宿主使用 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
  - Avalonia UI 宿主使用 `AsterGraphEditorFactory.Create(...)` 加 `AsterGraphAvaloniaViewFactory.Create(...)`

## 已经足够对外评估的部分

- 四个可发布 SDK 包边界
- kernel/session-first 的运行时状态所有权
- 默认 Avalonia 壳层与 standalone surfaces
- v1.23 图面可用性 proof marker：
  - `COMMAND_SURFACE_OK:True`
  - `TIERED_NODE_SURFACE_OK:True`
  - `FIXED_GROUP_FRAME_OK:True`
  - `NON_OBSCURING_EDITING_OK:True`
  - `VISUAL_SEMANTICS_OK:True`
- v0.6.0-alpha advanced-editing 收口 marker：
  - `HIERARCHY_SEMANTICS_OK:True`
  - `COMPOSITE_SCOPE_OK:True`
  - `EDGE_NOTE_OK:True`
  - `EDGE_GEOMETRY_OK:True`
  - `DISCONNECT_FLOW_OK:True`
- plugin discovery、trust policy、loading、inspection
- `IGraphEditorSession.Automation`
- contract、maintenance、release 验证 lanes
- release lane 里的 `.NET 10` 打包 `HostSample` 兼容性验证

## 当前优先事项

当前对外仓库的重点，是把 public alpha 收口成一套连贯可评估的 SDK surface，而不是一堆分散功能：

- 对外文档入口集中在 `README.md`、`README.zh-CN.md`、`docs/en`、`docs/zh-CN`
- advanced editing 要继续被描述成 canonical capability modules，而不是 retained-only 行为
- 源码、测试、sample、proof tool、workflow、governance 文件继续公开保留
- 内部工作流痕迹和本地环境文件不再作为公开仓库跟踪内容

## 近期路线

- 继续保持 canonical runtime/session surface 稳定，同时扩展 official capability modules 与 proof guidance
- 在 advanced editing 收口时保持 public alpha 文档和验证指引清晰可执行
- 继续维护托管 CI 与核心验证 lane 的一致性
- 在不突然 breaking 的前提下继续保留兼容迁移窗口

## 公开入口分工

- `tools/AsterGraph.HelloWorld` = runtime-only 第一跑样例
- `tools/AsterGraph.HelloWorld.Avalonia` = 默认 UI 第一跑样例
- `tools/AsterGraph.ConsumerSample.Avalonia` = 真实 hosted-UI 宿主样例
- `tools/AsterGraph.HostSample` = 最小推荐接入验证
- `tools/AsterGraph.PackageSmoke` = 打包消费验证
- `tools/AsterGraph.ScaleSmoke` = 大图基线加历史记录与状态连续性验证
- `src/AsterGraph.Demo` = 可视化展示宿主

## 对外入口

- [Versioning](./versioning.md)
- [Quick Start](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [ScaleSmoke 基线](./scale-baseline.md)
- [Advanced Editing Guide](./advanced-editing.md)
- [Adoption Feedback Loop](./adoption-feedback.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)
- [Alpha 状态](./alpha-status.md)
- [Public Launch Checklist](./public-launch-checklist.md)
- [Host Integration](./host-integration.md)
- [Demo Guide](./demo-guide.md)
