# 公开 Beta 评估路径

这份文档只做一件事：把 public beta 从第一次安装带到真实宿主 proof，收口成一条 hosted route ladder。它只有一次明确的 intake handoff：从受防守路线到受限 intake 记录的一次明确 handoff，先经过 [Beta Support Bundle](./support-bundle.md) 处理附件备注，再进入 [Adoption Feedback Loop](./adoption-feedback.md) 里的受限 intake 记录。
如果你是在评估 AsterGraph 这个 SDK，就看这页；如果你是在维护 release 基础设施，再去看其他维护者文档。

如果你在评估插件信任，把 `src/AsterGraph.Demo` 当成受防守的 hosted trust hop。先看 [插件信任契约 v1](./plugin-trust-contracts.md) 和 [Plugin 与自定义节点 Recipe](./plugin-recipe.md)，再停在这条 route ladder。
如果你在跟着这条受防守路线做本地证据记录，就先把 [Beta Support Bundle](./support-bundle.md) 放在第 3 步旁边。

## 先锁边界

- 新评估优先走 shipped Avalonia 路线，或者 canonical runtime/session 路线
- `WPF` 只用于验证，不是第二条上手路径，也不是 parity 承诺
- retained `GraphEditorViewModel` / `GraphEditorView` 仅用于迁移
- `Demo` 和 release harness 放在后面做补充验证，不是第一跳

## 推荐阶梯

按下面顺序走。每一步只解决一个问题，并把你交给下一步。

这条 hosted route ladder 是 `templates/astergraph-avalonia -> src/AsterGraph.Demo -> src/AsterGraph.Demo -- --proof`。

| 步骤 | 运行什么 | 为什么现在跑它 | 下一步看什么 |
| --- | --- | --- | --- |
| 1 | `templates/astergraph-avalonia` | 先确认第一个 hosted 端到端入口和最小脚手架 | 壳层能跑起来后，再去看最小 stock sample |
| 2 | `src/AsterGraph.Demo` | 再确认最小 shipped Avalonia surface，不掺额外宿主逻辑 | 确认后进入真实宿主 proof |
| 3 | `src/AsterGraph.Demo -- --proof` | 在受防守的路线上验证 host-owned actions、trusted plugin、参数编辑、command projection 和 hosted accessibility semantics | 期待看到 `CONSUMER_SAMPLE_OK:True`、`COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_OK:True` 以及 `HOST_NATIVE_METRIC:*`；这一步只负责 proof handoff，受限 intake 记录继续放在 [Beta Support Bundle](./support-bundle.md)、[Hosted Accessibility Recipe](./hosted-accessibility-recipe.md) 和 [Adoption Feedback Loop](./adoption-feedback.md) 里 |
| 4 | release validation lane | 在真实宿主样例已经看懂之后，再验证下游消费、template smoke、规模基线和 adapter-2 evidence | 期待 CI/release proof artifacts 里有 `HOST_SAMPLE_OK:True`、`HOST_SAMPLE_AUTOMATION_OK:True`、`HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True` 和 `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True`；这一步必须放在 `src/AsterGraph.Demo -- --proof` 之后 |

如果你是刻意评估 runtime-only 路线，可以在步骤 1 之后先跑 `src/AsterGraph.Demo -- --proof`，替代步骤 2；然后再回到同一条真实宿主 proof 阶梯。

如果 `CONSUMER_SAMPLE_PARAMETER_OK` 或 `CONSUMER_SAMPLE_METADATA_PROJECTION_OK` 失败，就把失败的 proof-marker 行和 support bundle 的 `parameterSnapshots` 行一起保留在同一条受限 intake 记录里。
如果你要走一条 screen-reader-ready 的本地评估路径，就把第 3 步的 support bundle 和 release-lane proof 行继续放在同一条受限 intake 记录里。这仍然只是本地证据，不扩大支持承诺。
如果你在受防守的 Avalonia proof 之后还要做有边界的 adapter-2 跟进验证，就使用 release validation lane，并把 `HOSTED_ACCESSIBILITY_BASELINE_OK:True`、`HOSTED_ACCESSIBILITY_FOCUS_OK:True`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_OK:True`、`ADAPTER2_PERFORMANCE_BASELINE_OK:True`、`ADAPTER2_PROJECTION_BUDGET_OK:True:none`、`ADAPTER2_COMMAND_BUDGET_OK:True:none`、`ADAPTER2_SCENE_BUDGET_OK:True:none`、`ADAPTER2_WPF_SAMPLE_PROOF_OK:True`、`ADAPTER2_CANONICAL_ROUTE_OK:True`、`ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True` 和 `HELLOWORLD_WPF_OK:True` 继续放在同一条本地记录里；细则见 [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md) 和 [Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md)。

## 命令

```powershell
# 第一个 hosted 确认
dotnet new astergraph-avalonia -n MyGraphHost

# 最小 shipped Avalonia surface
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --scenario ai-pipeline

# 防守 beta 路线上的真实宿主 proof
dotnet run --project src/AsterGraph.Demo/AsterGraph.Demo.csproj --nologo -- --proof

# 只有在受防守的 Avalonia proof 之后才做有边界的 adapter-2 accessibility 验证
dotnet test tests/AsterGraph.Demo.Tests/AsterGraph.Demo.Tests.csproj --filter FullyQualifiedName~ReleaseClosureContractTests
```

## 什么算成功

- 对大多数评估者来说，步骤 3 才是真正的 hosted proof gate
- `src/AsterGraph.Demo` 应该输出 `CONSUMER_SAMPLE_OK:True`
- 同一轮输出里也应该有 `COMMAND_SURFACE_OK:True`
- 同一轮输出里也应该有 `HOSTED_ACCESSIBILITY_OK:True`、`HOSTED_ACCESSIBILITY_AUTOMATION_NAVIGATION_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_DIAGNOSTICS_OK:True` 和 `HOSTED_ACCESSIBILITY_AUTOMATION_OK:True`
- 同一轮输出里也应该有四条 `HOST_NATIVE_METRIC:*`
- release validation 应该输出 `HOST_SAMPLE_OK:True`、`HOST_SAMPLE_AUTOMATION_OK:True`、`HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True` 和 `HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK:True`
- adapter-2 release evidence 应该在 validation-only 的 adapter-2 通道上输出 `HOSTED_ACCESSIBILITY_BASELINE_OK:True`、`HOSTED_ACCESSIBILITY_FOCUS_OK:True`、`HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`、`HOSTED_ACCESSIBILITY_OK:True`、`ADAPTER2_PERFORMANCE_BASELINE_OK:True`、`ADAPTER2_PROJECTION_BUDGET_OK:True:none`、`ADAPTER2_COMMAND_BUDGET_OK:True:none`、`ADAPTER2_SCENE_BUDGET_OK:True:none`、`ADAPTER2_WPF_SAMPLE_PROOF_OK:True`、`ADAPTER2_CANONICAL_ROUTE_OK:True`、`ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True` 和 `HELLOWORLD_WPF_OK:True`

## 不要误读

- `WPF` 仅用于验证，不代表支持边界扩大
- retained MVVM 仅用于迁移，不是推荐评估路线
- `src/AsterGraph.Demo -- --proof` 是 proof mode，不是第一个产品化体验
- release validation lane 是维护者证据，不是额外的采用者样例路线

## 相关文档

- [快速开始](./quick-start.md)
- `src/AsterGraph.Demo`
- [Host Integration](./host-integration.md)
- [稳定化支持矩阵](./stabilization-support-matrix.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
- [Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)
- [Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md)
