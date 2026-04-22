# Adapter Capability Matrix

这页文档锁定 `v0.9.0-beta Second Adapter Validation` 的 adapter capability 合同。

`WPF` 是 adapter 2。这个里程碑的目标是在现有 canonical session/runtime 路线上验证第二个官方 adapter，而不是新增 adapter 专属 runtime API，也不是再开一条新的宿主运行时路线。

## 锁定目标

- adapter 1 继续是 Avalonia
- adapter 2 锁定为 `WPF`
- 宿主侧 canonical root 继续是 `AsterGraph.Editor` 提供的 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
- `Create(...)` 仍然只是叠在同一个 runtime owner 之上的 hosted-adapter 组合 helper

当前真正 shipped 的 hosted adapter 仍然只有 Avalonia。`WPF` 只是下一个里程碑里锁定的验证目标，在矩阵真正填出来之前，不代表已经承诺功能对齐。

## Matrix Vocabulary

公开文档描述 Avalonia/WPF 支持情况时，只使用下面这三种标签：

| Label | 含义 |
| --- | --- |
| `supported` | 该 adapter 已经能通过文档说明的主支持路线提供对应 stock surface |
| `partial` | 能力仍然建立在同一条 canonical 路线上，但该 adapter 还存在明确范围限制或缺少某个 stock projection |
| `fallback` | 宿主继续停留在同一条 canonical session/runtime seam 上，改走更底层、已文档化的 path / sample / proof harness，而不是依赖 stock adapter surface |

retained 迁移不是 `fallback`。retained 仍然只是 legacy host 的 compatibility bridge。

## Matrix Categories

后续公开矩阵统一使用下面这些行：

| Matrix row | 覆盖内容 |
| --- | --- |
| Canonical runtime/session route | `CreateSession(...)`、`IGraphEditorSession`、DTO/snapshot queries、diagnostics、automation、plugin inspection |
| Hosted full editor shell | 叠在 `Create(...)` 之上的 stock adapter-hosted editor 组合 |
| Standalone surfaces | adapter package 公开的 stock canvas、inspector、mini map |
| Command and tool projection | 由共享 descriptor 投影出的 menu、toolbar、shortcut、palette、tool |
| Authoring presentation | adapter 提供的 node、edge、group、inspector、parameter-editor、geometry chrome |
| Platform integration | 保留在 adapter package 内部的 clipboard、focus、pointer、wheel、theme、host-context glue |
| Proof and sample coverage | 在不改变 runtime route 的前提下证明 adapter surface 的 starter / sample / proof tool |

## Fallback Rule

`fallback` 不是“退回 retained MVVM”，也不允许引入 `WPF` 专属 runtime API。它的含义是：宿主仍然停留在同一条 canonical seam 上，只是临时改走已经被 sample 或 proof harness 证明过的更底层路径。

## Phase 157 能力矩阵

| Matrix row | Avalonia | WPF | 证据锚点 | WPF 的 fallback 规则 |
| --- | --- | --- | --- | --- |
| Canonical runtime/session route | `supported` | `supported` | `CreateSession(...)`、`HOST_SAMPLE_OK`、`CONSUMER_SAMPLE_OK`、`DEMO_OK`、`HELLOWORLD_WPF_OK` | 无。canonical seam 不变；自定义 shell 时保持 canonical runtime/session 路线不变。 |
| Hosted full editor shell | `supported` | `partial` | `tools/AsterGraph.Starter.Avalonia`、`tools/AsterGraph.HelloWorld.Avalonia`、`tools/AsterGraph.Starter.Wpf`、`tools/AsterGraph.HelloWorld.Wpf` | 宿主负责组装 shell；当 stock 级别不足时将 `AsterGraph.Wpf.Controls.GraphEditorView`（来自 `AsterGraphWpfViewFactory`）当普通内容控件使用。 |
| Standalone surfaces | `supported` | `partial` | Avalonia 有独立 canvas/inspector/minimap 工厂；WPF 目前仅在 `tools/AsterGraph.Starter.Wpf` 与 `tools/AsterGraph.HelloWorld.Wpf` 里验证 `GraphEditorView` | 通过 `IGraphEditorSession` 快照在 host 层自建 canvas / inspector / minimap 类表面。 |
| Command and tool projection | `supported` | `partial` | `COMMAND_SURFACE_OK`、`CONSUMER_SAMPLE_OK`，以及 `HELLOWORLD_WPF_OK`（WPF proof 会输出 `COMMAND_SURFACE_OK`） | 走 canonical session 的共享 command descriptor（`GetCommandDescriptors`）并由 host action rail 做映射，不引入 WPF 专属 command 运行时。 |
| Authoring presentation | `supported` | `partial` | `DEMO_OK`（完整作者态能力证明）与 WPF shell 的 inspector 摘要显示 | 用 host 的 WPF 表现层按 session/query 快照投影 node/edge/inspector/parameter chrome。 |
| Platform integration | `supported` | `partial` | `HOST_SAMPLE_OK` 的 runtime seam 校验 + `AsterGraph.Wpf` 的基础 platform seam 绑定 | focus/clipboard/pointer/wheel/theme/host-context 由 host 实现并通过兼容 seam 注入，不引入 adapter 级 runtime 语义。 |
| Proof and sample coverage | `supported` | `supported` | `HOST_SAMPLE_OK`、`CONSUMER_SAMPLE_OK`、`DEMO_OK`、`COMMAND_SURFACE_OK`、`HELLOWORLD_WPF_OK` | 无。 |

路线与能力地图看 [Host Integration](./host-integration.md)，adapter 边界看 [Architecture](./architecture.md)，当前 Avalonia-first 的上手路径看 [Quick Start](./quick-start.md)。
