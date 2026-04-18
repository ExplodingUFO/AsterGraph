# Consumer Sample

`tools/AsterGraph.ConsumerSample.Avalonia` 是介于最小 `HelloWorld.Avalonia` 路线和完整 `AsterGraph.Demo` 展示宿主之间的中等 hosted-UI 样例。

## 它证明什么

这个样例保留了一个真实宿主窗口，但不会膨胀成完整 showcase shell。它会同时展示：

- 一条宿主自管的动作栏
- 一组宿主自定义节点
- 通过 canonical session commands/queries 做共享参数编辑
- 一个可信插件注册与可见的 load snapshot
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
- `CONSUMER_SAMPLE_OK:True`

## 什么时候看它

当你想在跳到完整 `Demo` 之前，先看一个真实宿主集成时，就看 `ConsumerSample.Avalonia`。

如果你需要的是更窄的入口，请用：

- `HelloWorld` = 最小 runtime-only 第一跑
- `HelloWorld.Avalonia` = 最小 hosted-UI 第一跑
- `HostSample` = 推荐路线验证用 proof harness
- `PackageSmoke` = 打包消费验证
- `ScaleSmoke` = 规模与状态连续性验证
- `Demo` = 完整展示宿主

## 集成说明

这个样例刻意控制在可复制范围内：

- 宿主动作在编辑器壳层之外
- 插件信任策略保持显式且由宿主管理
- 参数编辑走 `IGraphEditorSession.Commands` 和 `IGraphEditorSession.Queries`
- 插件加载仍是进程内执行，不提供沙箱或不受信任代码隔离

## 相关文档

- [Quick Start](./quick-start.md)
- [Host Integration](./host-integration.md)
- [Plugin 与自定义节点 Recipe](./plugin-recipe.md)
- [Alpha Status](./alpha-status.md)
