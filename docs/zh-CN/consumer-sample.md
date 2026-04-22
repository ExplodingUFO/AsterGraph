# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` 是位于 canonical session/runtime 路线上的中等 hosted-UI 样例，排在 starter 脚手架和最小 `HelloWorld.Avalonia` 路线之后、完整 `AsterGraph.Demo` 展示宿主之前。

它是三条宿主管线 seam 的可复制 recipe：

- action rail / command projection
- plugin trust workflow
- parameter-editing composition

这是把宿主自管 seam 复制到你自己的应用里的受防守 beta 路线。把 action projection、trust workflow 和 parameter-editing composition 保持在宿主里，只复制样例自有的展示细节。

它只停留在 canonical session/runtime model 上，不引入第二套 editor model、sandbox，或更大的 plugin ecosystem。

## 它证明什么

这个样例保留了一个真实宿主窗口，但不会膨胀成完整 showcase shell。它会同时展示：

- 一条宿主自管的动作栏，通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 从共享 command descriptor 投到宿主层
- 一组宿主自定义节点，这部分是样例自有且可替换的
- 一个 plugin command 也走同一条动作路径，而不是样例私有的菜单占位项
- 通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 和 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 做共享参数编辑
- 一个可信插件注册，以及可见的 provenance、trust reason 和 allowlist 导入/导出
- 基于 factory 的默认 Avalonia hosted-UI 路线

## 复制这些宿主自管 seam

- action rail / command projection：宿主动作放在编辑器壳层之外，并且通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 投影共享 command descriptor
- plugin trust workflow：把 `GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshot 和宿主自管 allowlist policy 放在同一层
- parameter-editing composition：只通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 和 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 修改当前选中节点参数

## 替换这些样例自有内容

下面这些样例自有内容保持在你自己的 app 内部即可：

- review/audit 节点族
- action ids/titles
- 窗口布局和叙述文本
- defended markers 之外的 proof 文案

## 信任与证明速查

可复制的信任与证明参考：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

公开 marker 预期如下：

- `CONSUMER_SAMPLE_TRUST_OK:True`
- `COMMAND_SURFACE_OK:True`
- `HOST_NATIVE_METRIC:*`
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`

support bundle 只保留本地证据，不会扩大 support 边界。

## 如何运行

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo
```

如果你想跑 proof 模式：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof
```

如果你要生成本地 support bundle：

```powershell
dotnet run --project tools/AsterGraph.ConsumerSample.Avalonia/AsterGraph.ConsumerSample.Avalonia.csproj --nologo -- --proof --support-bundle artifacts/consumer-support-bundle.json --support-note "what you were trying to validate"
```

## Proof Handoff

如果你是从 starter recipe 复制过来的，就在这里完成 proof handoff：跑 `AsterGraph.ConsumerSample.Avalonia -- --proof` 来验证受防守的宿主路线。

如果你需要可复查的本地证据，就附上 `artifacts/consumer-support-bundle.json`；bundle marker 会指向 `SUPPORT_BUNDLE_PATH:...`，对应解析后的路径。它应当只作为本地证据，不应扩大支持边界。

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
- `SUPPORT_BUNDLE_OK:True`
- `SUPPORT_BUNDLE_PATH:...`
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

- action rail / command projection：宿主动作在编辑器壳层之外，并且通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 和 `AsterGraphHostedActionFactory.CreateProjection(...)` 复用共享 command descriptor
- plugin trust workflow：把 `GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshot 和 allowlist 导入/导出放在同一层；插件信任策略保持显式且由宿主管理，通过 discovery snapshot、reason 字符串和 allowlist 导入/导出保持可见，allowlist 决策可以导出/导入而不需要重建宿主 trust-policy 流程。
- parameter-editing composition：通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 读选中节点参数，并通过 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 写回
- 插件加载仍是进程内执行，不提供 sandbox 或不受信任代码隔离
- review/audit 节点族、action ids/titles、窗口布局、叙述文本，以及 defended markers 之外的 proof 文案，都是样例自有内容

更细的 v1 manifest 和 trust-policy 合同见 [插件信任契约 v1](./plugin-trust-contracts.md)。

## 可直接照抄的接法

如果你想在自己的宿主里做同等级别的接入，可以按这个顺序抄：

- action rail / command projection：通过 `AsterGraphHostedActionFactory.CreateCommandActions(...)` 消费共享 command descriptor，并用 `AsterGraphHostedActionFactory.CreateProjection(...)` 组合宿主动作
- plugin trust workflow：把 `GraphEditorPluginDiscoveryOptions`、`AsterGraphEditorOptions.PluginTrustPolicy`、provenance snapshot 和宿主自管 allowlist policy 放在同一层
- parameter-editing composition：只通过 `IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()` 和 `IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)` 修改当前选中节点参数
- proof mode：输出 `COMMAND_SURFACE_OK` 和四条 `HOST_NATIVE_METRIC:*`，这样你能和官方 sample 做横向比较
- support bundle：在 proof mode 上额外附带 `--support-bundle`，生成本地 JSON 证据包给 support/feedback 使用
- sample-owned content：review/audit 节点族、action ids/titles 和 proof labels 应该保持在你的 app 内部，不要写成 canonical contract

## 相关文档

- [Quick Start](./quick-start.md)
- [Beta Support Bundle](./support-bundle.md)
- [Host Integration](./host-integration.md)
- [Sample README](../../tools/AsterGraph.ConsumerSample.Avalonia/README.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Alpha Status](./alpha-status.md)
