# Adapter Capability Matrix

这页文档锁定当前 adapter capability 合同。
它不是第二套 API 稳定定义，而是在 [extension contracts](./extension-contracts.md) 已定义的 canonical 稳定合同之上，给出 Avalonia/WPF 的 portability / projection 能力边界。
默认对新接入宿主的 onboarding 仍然是 Avalonia-first：WPF 仅用于同一条 canonical session/runtime 路线的 adapter-2 validation-only 可移植性验证，不会形成第二条上手路径、公开 WPF support 扩大或 parity 承诺。

`WPF` 是 adapter 2。这个里程碑的目标是在现有 canonical session/runtime 路线上验证第二个官方 adapter，而不是新增 adapter 专属 runtime API，也不是再开一条新的宿主运行时路线。

## 锁定目标

- adapter 1 继续是 Avalonia
- adapter 2 锁定为 `WPF`
- 宿主侧 canonical root 继续是 `AsterGraph.Editor` 提供的 `AsterGraphEditorFactory.CreateSession(...)` + `IGraphEditorSession`
- `Create(...)` 仍然只是叠在同一个 runtime owner 之上的 hosted-adapter 组合 helper

当前真正 shipped 的 hosted adapter 仍然只有 Avalonia。`WPF` 只是锁定的 adapter-2 validation-only 验证目标，不代表已经承诺功能对齐、公开 WPF support 扩大或第二条 onboarding 路线。

## Matrix Vocabulary

公开文档描述 Avalonia/WPF 支持情况时，只使用下面这三种标签：

| Label | 含义 |
| --- | --- |
| `Supported` | 该 adapter 已经能通过文档说明的主支持路线，在 canonical session/query 路线上提供对应 stock surface |
| `Partial` | 能力仍然建立在同一条 canonical session/query + host 自有 projection 路线上，但该 adapter 还存在明确范围限制或缺少某个 stock projection |
| `Fallback` | 宿主继续停留在同一条 canonical session/query + host 自有 projection 路线上，通过已文档化的 path / sample / proof harness 改走更底层路径，而不是依赖新一套 adapter 专属 runtime API |

retained 迁移不是 `Fallback`。retained 仍然只是 legacy host 的 compatibility bridge。

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

`Fallback` 不是“退回 retained MVVM”，也不允许引入 `WPF` 专属 runtime API。它的含义是：宿主仍然停留在同一条 canonical session/query + host 自有 projection seam 上，只是临时改走已经被 sample 或 proof harness 证明过的更底层路径。

公开 beta 对外文案 `must not exceed` 下面这些行级状态。只要 WPF 某一行仍是 `Partial` 或 `Fallback`，release note 和公开文档就必须把这个缺口写明，不能暗示已经对齐。

## Phase 157 能力矩阵

| Matrix row | Avalonia | WPF | 证据锚点 | WPF 的 fallback 规则 |
| --- | --- | --- | --- | --- |
| Canonical runtime/session route | `Supported` | `Supported` | `CreateSession(...)`、`HOST_SAMPLE_OK`、`CONSUMER_SAMPLE_OK`、`DEMO_OK`、`HELLOWORLD_WPF_OK` | 无。canonical seam 不变；自定义 shell 时保持 canonical runtime/session 路线不变。 |
| Hosted full editor shell | `Supported` | `Partial` | `tools/AsterGraph.Starter.Avalonia`、`tools/AsterGraph.HelloWorld.Avalonia`、`tools/AsterGraph.Starter.Wpf`、`tools/AsterGraph.HelloWorld.Wpf` | 宿主负责组装 shell；当 stock 级别不足时将 `AsterGraph.Wpf.Controls.GraphEditorView`（来自 `AsterGraphWpfViewFactory`）当普通内容控件使用。 |
| Standalone surfaces | `Supported` | `Partial` | Avalonia 有独立 canvas/inspector/minimap 工厂；WPF 目前仅在 `tools/AsterGraph.Starter.Wpf` 与 `tools/AsterGraph.HelloWorld.Wpf` 里验证 `GraphEditorView` | 通过 `IGraphEditorSession` 快照在 host 层自建 canvas / inspector / minimap 类表面。 |
| Command and tool projection | `Supported` | `Partial` | `COMMAND_SURFACE_OK`、`CONSUMER_SAMPLE_OK`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK`、`ADAPTER2_COMMAND_BUDGET_OK`，以及 `HELLOWORLD_WPF_OK` | 走 canonical session 的共享 command descriptor（`GetCommandDescriptors`）并由 host action rail 做映射；WPF 的 command budget 证据只算 hosted validation shell 的有界证明，不引入 WPF 专属 command 运行时。 |
| Authoring presentation | `Supported` | `Partial` | `DEMO_OK`（完整作者态能力证明）、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK`、`ADAPTER2_PROJECTION_BUDGET_OK`、`ADAPTER2_SCENE_BUDGET_OK` 与 WPF shell 的 inspector 摘要显示 | 用 host 的 WPF 表现层按 session/query 快照投影 node/edge/inspector/parameter chrome。 |
| Platform integration | `Supported` | `Partial` | `HOST_SAMPLE_OK` 的 runtime seam 校验、`HOSTED_ACCESSIBILITY_BASELINE_OK`、`HOSTED_ACCESSIBILITY_FOCUS_OK`、`ADAPTER2_PERFORMANCE_BASELINE_OK`，以及 `AsterGraph.Wpf` 的基础 platform seam 绑定 | focus/clipboard/pointer/wheel/theme/host-context 由 host 实现并通过兼容 seam 注入，不引入 adapter 级 runtime 语义。 |
| Proof and sample coverage | `Supported` | `Supported` | `HOST_SAMPLE_OK`、`CONSUMER_SAMPLE_OK`、`DEMO_OK`、`COMMAND_SURFACE_OK`、`HOSTED_ACCESSIBILITY_OK`、`ADAPTER2_PERFORMANCE_BASELINE_OK`、`ADAPTER2_PROJECTION_BUDGET_OK`、`ADAPTER2_COMMAND_BUDGET_OK`、`ADAPTER2_SCENE_BUDGET_OK`、`HELLOWORLD_WPF_OK` | 无。 |

路线与能力地图看 [Host Integration](./host-integration.md)，adapter 边界看 [Architecture](./architecture.md)，当前 Avalonia-first 的上手路径看 [Quick Start](./quick-start.md)。
