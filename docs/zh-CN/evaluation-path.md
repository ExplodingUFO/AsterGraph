# 公开 Beta 评估路径

这份文档只做一件事：把 public beta 从第一次安装带到真实宿主 proof，收口成一条 hosted route ladder。
如果你是在评估 AsterGraph 这个 SDK，就看这页；如果你是在维护 release 基础设施，再去看其他维护者文档。

如果你在评估插件信任，把 `AsterGraph.ConsumerSample.Avalonia` 当成受防守的 hosted trust hop。先看 [插件信任契约 v1](./plugin-trust-contracts.md) 和 [Plugin 与自定义节点 Recipe](./plugin-recipe.md)，再停在这条 route ladder。
如果你在跟着这条受防守路线做本地证据记录，就先把 [Beta Support Bundle](./support-bundle.md) 放在第 3 步旁边。

## 先锁边界

- 新评估优先走 shipped Avalonia 路线，或者 canonical runtime/session 路线
- `WPF` 只用于验证，不是第二条上手路径，也不是 parity 承诺
- retained `GraphEditorViewModel` / `GraphEditorView` 仅用于迁移
- `Demo` 和 release harness 放在后面做补充验证，不是第一跳

## 推荐阶梯

按下面顺序走。每一步只解决一个问题，并把你交给下一步。

这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。

| 步骤 | 运行什么 | 为什么现在跑它 | 下一步看什么 |
| --- | --- | --- | --- |
| 1 | `AsterGraph.Starter.Avalonia` | 先确认第一个 hosted 端到端入口和最小脚手架 | 壳层能跑起来后，再去看最小 stock sample |
| 2 | `AsterGraph.HelloWorld.Avalonia` | 再确认最小 shipped Avalonia surface，不掺额外宿主逻辑 | 确认后进入真实宿主 proof |
| 3 | `AsterGraph.ConsumerSample.Avalonia -- --proof` | 在防守的 beta 路线上验证 host-owned actions、trusted plugin、参数编辑和 command projection | 期待看到 `CONSUMER_SAMPLE_OK:True`、`COMMAND_SURFACE_OK:True`、`HOST_NATIVE_METRIC:*`，并可选生成 [Beta Support Bundle](./support-bundle.md) |
| 4 | `AsterGraph.HostSample` | 在真实宿主样例已经看懂之后，再用 proof harness 做路线验证 | 期待看到 `HOST_SAMPLE_OK:True`；这一步必须放在 `ConsumerSample.Avalonia` 之后 |

如果你是刻意评估 runtime-only 路线，可以在步骤 1 之后先跑 `AsterGraph.HelloWorld`，替代步骤 2；然后再回到同一条真实宿主 proof 阶梯。

## 命令

```powershell
# 第一个 hosted 确认
dotnet run --project tools/AsterGraph.Starter.Avalonia/AsterGraph.Starter.Avalonia.csproj --nologo

# 最小 shipped Avalonia surface
dotnet run --project tools/AsterGraph.HelloWorld.Avalonia/AsterGraph.HelloWorld.Avalonia.csproj --nologo

# 防守 beta 路线上的真实宿主 proof
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof

# 只有在真实宿主样例之后才跑 proof harness
dotnet run --project tools/AsterGraph.HostSample/AsterGraph.HostSample.csproj --nologo
```

## 什么算成功

- 对大多数评估者来说，步骤 3 才是真正的 hosted proof gate
- `ConsumerSample.Avalonia` 应该输出 `CONSUMER_SAMPLE_OK:True`
- 同一轮输出里也应该有 `COMMAND_SURFACE_OK:True`
- 同一轮输出里也应该有四条 `HOST_NATIVE_METRIC:*`
- `HostSample` 应该输出 `HOST_SAMPLE_OK:True`

## 不要误读

- `WPF` 仅用于验证，不代表支持边界扩大
- retained MVVM 仅用于迁移，不是推荐评估路线
- `HostSample` 是 proof harness，不是第一个产品化体验
- `Demo` 适合在路线和 proof 预期已经清楚之后再看

## 相关文档

- [快速开始](./quick-start.md)
- [Consumer Sample](./consumer-sample.md)
- [Host Integration](./host-integration.md)
- [稳定化支持矩阵](./stabilization-support-matrix.md)
- [Adapter Capability Matrix](./adapter-capability-matrix.md)
