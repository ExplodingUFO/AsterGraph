# 稳定化支持矩阵

这页锁定的是通往 `v1.0.0` 的 consumer-facing 支持边界。它不扩大公开面，只把当前已经防守住的包、框架、路线和适配器边界收成一张矩阵。

## 冻结边界

| 维度 | 稳定化规则 | 说明 |
| --- | --- | --- |
| 公开 SDK 包 | `AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor`、`AsterGraph.Avalonia` | 这是当前唯一受支持的公开 SDK 包边界；sample 和 proof tool 不算公开包面。 |
| 目标框架 | `net8.0`、`net9.0` | 这是当前公开 SDK 的受支持目标框架。 |
| canonical runtime 路线 | `CreateSession(...)` + `IGraphEditorSession` | 这是受支持的 runtime/custom-UI 路线。 |
| hosted 路线 | `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` | 这是当前受支持的 hosted Avalonia 路线。 |
| hosted adapter | Avalonia | 当前真正受支持的 hosted adapter 只有 Avalonia。 |
| 第二适配器 | `WPF` | 仍然只是 adapter 2 的验证目标；按 [Adapter Capability Matrix](./adapter-capability-matrix.md) 的 `partial` / `fallback` 解释，不是第二套 runtime 模型。 |
| retained 路线 | 仅用于迁移 | retained MVVM 继续保留给旧宿主分批迁移，不作为新的主接入路线。 |

## 通往 `v1.0.0` 的升级指引

- 新宿主继续优先走 canonical runtime/session 路线或 shipped Avalonia hosted 路线。
- 规划新 consumer 集成时，只按四个公开 SDK 包做依赖边界。
- `WPF` 只有在矩阵某一行明确写成 `Supported` 时，才代表那一行真的进入受支持状态；否则仍按 `partial` / `fallback` 理解。
- retained MVVM 入口只作为迁移桥接，不再作为新的 primary host surface。
- 读到 `v1.0.0` 时，应该把它理解成“把这条已经防守住的边界提升为 stable”，而不是自动扩大包、框架或 adapter 支持面。

## 发布就绪 proof

Phase 382 用 `RELEASE_READINESS_GATE_OK:True`、`SUPPORT_BOUNDARY_GATE_OK:True` 和 `BETA_CLAIM_ALIGNMENT_OK:True` 把 release readiness、support boundary 和 beta claim 口径继续绑定到这张冻结矩阵。这些 marker 不新增包、runtime route、WPF parity、marketplace、sandbox、execution engine、GA 或 `1.0` 支持承诺。

## 相关文档

- [公开 Beta 评估路径](./evaluation-path.md)
- [README](../../README.zh-CN.md)
- [Versioning](./versioning.md)
- [Quick Start](./quick-start.md)
- [Project Status](./project-status.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Host Integration](./host-integration.md)
