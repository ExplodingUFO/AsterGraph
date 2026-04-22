# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` 是位于 canonical session/runtime 路线上的中等 hosted-UI 样例，排在 starter 脚手架和最小 `HelloWorld.Avalonia` 路线之后、完整 `AsterGraph.Demo` 展示宿主之前。

## 它证明什么

这个样例保留了一个真实宿主窗口，但不会膨胀成完整 showcase shell。它会同时展示：

- 一条宿主自管的动作栏
- 一组宿主自定义节点
- 通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 把共享 command descriptor 投到宿主动作栏
- 一个 plugin command 也走同一条动作路径，而不是样例私有的菜单占位项
- 通过 canonical session commands/queries 做共享参数编辑
- 一个可信插件注册，以及可见的 provenance、trust reason 和 allowlist 导入/导出
- 基于 factory 的默认 Avalonia hosted-UI 路线

## 如何运行

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

如果你想跑 proof 模式：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

预期 marker：

- `CONSUMER_SAMPLE_HOST_ACTION_OK:True`
- `CONSUMER_SAMPLE_PLUGIN_OK:True`
- `CONSUMER_SAMPLE_PARAMETER_OK:True`
- `CONSUMER_SAMPLE_WINDOW_OK:True`
- `CONSUMER_SAMPLE_TRUST_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOST_NATIVE_METRIC:startup_ms=...`
- `HOST_NATIVE_METRIC:inspector_projection_ms=...`
- `HOST_NATIVE_METRIC:plugin_scan_ms=...`
- `HOST_NATIVE_METRIC:command_latency_ms=...`
- `CONSUMER_SAMPLE_OK:True`

## 什么时候看它

当你想在 `HelloWorld.Avalonia` 之后、跳到完整 `Demo` 之前，先看一个真实宿主集成时，就看 `ConsumerSample.Avalonia`。

如果你需要的是更窄的入口，请用：

- `Starter.Avalonia` = 第一个 hosted 脚手架，也是最小端到端 Avalonia 入口
- `HelloWorld` = 最小 runtime-only 第一跑
- `HelloWorld.Avalonia` = 最小 hosted-UI 第一跑
- `HostSample` = 推荐路线验证用 proof harness
- `PackageSmoke` = 打包消费验证
- `ScaleSmoke` = 规模与状态连续性验证
- `Demo` = 完整展示宿主

## 集成说明

这个样例刻意控制在可复制范围内：

- 宿主动作在编辑器壳层之外，并且直接复用共享 command descriptor，而不是再维护一份独立动作表
- plugin 动作也通过共享 command descriptor 进入同一条宿主动作路径，不需要额外的 sample 专属 plumbing
- 插件信任策略保持显式且由宿主管理，并把 provenance、reason 和 allowlist 持久化放在一起
- 参数编辑走 `IGraphEditorSession.Commands` 和 `IGraphEditorSession.Queries`
- allowlist 决策可以导出/导入，不需要重建整条 trust-policy 流程
- 插件加载仍是进程内执行，不提供沙箱或不受信任代码隔离

更细的 v1 manifest 和 trust-policy 合同见 [插件信任契约 v1](./plugin-trust-contracts.md)。

## 可直接照抄的接法

如果你想在自己的宿主里做同等级别的接入，可以按这个顺序抄：

- command rail：通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 消费共享 command descriptor
- trust workflow：把 `GraphEditorPluginDiscoveryOptions`、provenance snapshot 和宿主自管 allowlist policy 放在同一层
- parameter editing：只通过 `IGraphEditorSession.Commands` 修改当前选中节点参数
- proof mode：输出 `COMMAND_SURFACE_OK` 和四条 `HOST_NATIVE_METRIC:*`，这样你能和官方 sample 做横向比较

## 相关文档

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Sample README](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Alpha Status](./alpha-status.md)
