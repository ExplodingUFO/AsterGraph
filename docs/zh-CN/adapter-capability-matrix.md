# Adapter Capability Matrix

这页文档锁定 `v0.9.0-beta Second Adapter Validation` 的 `Phase 154` 合同。

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

路线与能力地图看 [Host Integration](./host-integration.md)，adapter 边界看 [Architecture](./architecture.md)，当前 Avalonia-first 的上手路径看 [Quick Start](./quick-start.md)。
