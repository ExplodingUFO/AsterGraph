# AsterGraph 项目状态

## 当前状态

- 包版本基线：`0.11.0-beta`
- 与当前包版本配对的对外 SemVer prerelease 标签：`v0.11.0-beta`
- 历史仓库里程碑标签系列：`v1.x` 风格的公开前检查点
- 仓库阶段：公开 Beta（稳定化收口）
- 对外版本说明：[Versioning](./versioning.md)
- 当前公开支持的发布包：
  - `AsterGraph.Abstractions`
  - `AsterGraph.Core`
  - `AsterGraph.Editor`
  - `AsterGraph.Avalonia`
- 示例与验证工具：
  - `tools/AsterGraph.HelloWorld`：最快的 runtime-only 第一跑样例
  - `tools/AsterGraph.Starter.Avalonia`：shipped 的 Avalonia starter scaffold
  - `tools/AsterGraph.HelloWorld.Avalonia`：在 starter scaffold 之后的默认 UI 第一跑样例
  - `tools/AsterGraph.ConsumerSample.Avalonia`：介于 HelloWorld 和 Demo 之间的真实宿主样例
  - `tools/AsterGraph.HostSample`：这条 canonical adoption route 在 ladder 之后的 proof harness
  - `tools/AsterGraph.PackageSmoke`：打包消费验证
  - `tools/AsterGraph.ScaleSmoke`：公开的大图基线与状态连续性验证
- 推荐接入路径：
  - runtime-only 宿主使用 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
  - Avalonia UI 宿主使用 `AsterGraphEditorFactory.Create(...)` 加 `AsterGraphAvaloniaViewFactory.Create(...)`
- 当前已锁定的 adapter 2 验证目标：`WPF`，合同见 [Adapter Capability Matrix](./adapter-capability-matrix.md)；未写成 `Supported` 的行都不应被解读成与 Avalonia 已经对齐

## 已经足够对外评估的部分

- 四个可发布 SDK 包边界
- kernel/session-first 的运行时状态所有权
- 默认 Avalonia 壳层与 standalone surfaces
- trusted / loaded / blocked 的 runtime inspection surface
- command/trust timeline 和 perf overlay 这类 showcase surface
- 图面可用性 proof marker：
  - `COMMAND_SURFACE_OK:True`
  - `TIERED_NODE_SURFACE_OK:True`
  - `FIXED_GROUP_FRAME_OK:True`
  - `NON_OBSCURING_EDITING_OK:True`
  - `VISUAL_SEMANTICS_OK:True`
- advanced-editing 收口 marker：
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

当前对外仓库的重点，是把 public beta 收口成一套连贯可评估的 SDK surface，而不是一堆分散功能：

- 对外文档入口集中在 `README.md`、`README.zh-CN.md`、`docs/en`、`docs/zh-CN`
- advanced editing 要继续被描述成 canonical capability modules，而不是 retained-only 行为
- 源码、测试、sample、proof tool、workflow、governance 文件继续公开保留
- 内部工作流痕迹和本地环境文件不再作为公开仓库跟踪内容

## 近期路线

- 继续保持 canonical runtime/session surface 稳定，同时扩展 official capability modules 与 proof guidance
- 在 advanced editing 收口时保持 public beta 文档和验证指引清晰可执行
- 继续维护托管 CI 与核心验证 lane 的一致性
- 在不突然 breaking 的前提下继续保留兼容迁移窗口
- 让 shipped starter scaffold、runtime inspection surface、command/trust timeline 和 perf overlay 继续对齐 canonical session-first 路线
- 在不扩 runtime surface 的前提下继续验证 `WPF` 作为 adapter 2，并用 `supported` / `partial` / `fallback` 发布 Avalonia/WPF 的公开状态；不要把 `partial` / `fallback` 写成 parity

## 外部能力就绪闸门

当 release notes、维护者回复或 beta intake 需要回答“当前到底有哪些能力已经被外部证据证明”时，就统一引用这一节。维护者种子预演证据记录在 adoption feedback loop 里，但只有同一个受限风险上的真实外部报告才计入 3 到 5 的门槛。下面每条公开声明都必须回到路线级证据，而不是内部信心或 parity 想象。

### 当前已被外部证据证明

| 声明 | 路线级证据 |
| --- | --- |
| canonical runtime/session 路线和维护中的评估阶梯，已经在当前防守住的 beta 线上被外部证据证明。 | `tools/AsterGraph.HelloWorld`、`tools/AsterGraph.Starter.Avalonia`、`tools/AsterGraph.HelloWorld.Avalonia`、`tools/AsterGraph.ConsumerSample.Avalonia`、`tools/AsterGraph.HostSample`、`HOST_SAMPLE_OK`、`CONSUMER_SAMPLE_OK` |
| showcase authoring surface 已经作为有边界的 beta 宿主体验被外部证据证明。 | `src/AsterGraph.Demo`、`DEMO_OK`、`COMMAND_SURFACE_OK`、`COMPOSITE_SCOPE_OK`、`EDGE_NOTE_OK`、`EDGE_GEOMETRY_OK`、`DISCONNECT_FLOW_OK` |
| 打包后的 consumer proof 已被外部证据证明，而且没有扩大 SDK 边界。 | `tools/AsterGraph.PackageSmoke`、`PACKAGE_SMOKE_OK`、`HOST_SAMPLE_NET10_OK` |
| Scale proof 只在当前防守住的 beta tier 上被外部证据证明。 | `tools/AsterGraph.ScaleSmoke`、`SCALE_PERFORMANCE_BUDGET_OK:baseline:True`、`SCALE_PERFORMANCE_BUDGET_OK:large:True`、`SCALE_PERF_SUMMARY:stress` |

### 仅验证通过或受边界约束的声明

| 声明 | 当前公开口径 | 路线级证据 |
| --- | --- | --- |
| `WPF` 作为 adapter 2 | 只算 validation-only，不代表 Avalonia parity，也不是公开 WPF support。 | `HELLOWORLD_WPF_OK`、`ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS`、`ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS`、[Adapter Capability Matrix](./adapter-capability-matrix.md) |
| retained 路线 | 只作为迁移桥，不是新的 primary host path。 | [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)、[稳定化支持矩阵](./stabilization-support-matrix.md) |
| stress 规模遥测 | 只算 informational，不是 defended budget 声明。 | `SCALE_PERF_SUMMARY:stress`、[ScaleSmoke 基线](./scale-baseline.md) |

### 在更多采用者证据出现前继续延后

- 超出当前 `baseline` 与 `large` tier 的更大 defended performance 承诺
- 除 Avalonia 加当前 `WPF` 验证通道之外的新 hosted adapter 或更宽的 adapter 声明
- marketplace、远程安装/更新、unload lifecycle、sandboxed plugin 这类故事
- stable / GA / `1.0` 级别的支持保证
- 下一条 `0.xx` alpha/beta 线继续优先做可复制的 host-owned 参数/元数据打磨；只有当 3 到 5 条真实外部报告聚焦在同一个受限风险上时，才考虑把 defended large-tier performance 或更宽的参数/元数据编辑提上来
- 维护者种子预演证据不计入 3 到 5 的门槛
- 如果新的报告放不进上面的“已证明”或“受边界约束”两类，就走 [Adoption Feedback Loop](./adoption-feedback.md) 和 [Beta Support Bundle](./support-bundle.md)，不要临时扩大公开声明

## 公开入口分工

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`；`HostSample` 放在这条 ladder 之后，作为 proof harness。

- `tools/AsterGraph.HelloWorld` = runtime-only 第一跑样例
- `tools/AsterGraph.Starter.Avalonia` = shipped 的 Avalonia starter scaffold
- `tools/AsterGraph.HelloWorld.Avalonia` = 在 starter scaffold 之后的默认 UI 第一跑样例
- `tools/AsterGraph.ConsumerSample.Avalonia` = 真实 hosted-UI 宿主样例
- `tools/AsterGraph.HostSample` = 这条 ladder 之后的 canonical adoption proof
- `tools/AsterGraph.PackageSmoke` = 打包消费验证
- `tools/AsterGraph.ScaleSmoke` = 大图基线加历史记录与状态连续性验证
- `src/AsterGraph.Demo` = 可视化展示宿主

## 对外入口

- [Versioning](./versioning.md)
- [公开 Beta 评估路径](./evaluation-path.md) = 从第一次安装到真实宿主 proof 的单一路径
- [稳定化支持矩阵](./stabilization-support-matrix.md)
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
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Demo Guide](./demo-guide.md)
