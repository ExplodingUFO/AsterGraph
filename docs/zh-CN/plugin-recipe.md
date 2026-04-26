# Plugin 与自定义节点 Recipe

这份 recipe 展示的是：怎样只用公开包边界，做一个最小插件，并让它注册一个节点定义。

## 包

先装：

```powershell
dotnet new install ./templates
dotnet new astergraph-plugin -n GreetingPlugin --PluginId sample.greeting

dotnet add package AsterGraph.Abstractions --prerelease
dotnet add package AsterGraph.Editor --prerelease
```

只有宿主还要嵌入默认 UI 时，才再加 `AsterGraph.Avalonia`。

## 最小插件形状

```csharp
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Plugins;

public sealed class GreetingPlugin : IGraphEditorPlugin
{
    public GraphEditorPluginDescriptor Descriptor { get; } =
        new("sample.greeting", "Greeting Plugin", "Adds one sample node definition.");

    public void Register(GraphEditorPluginBuilder builder)
    {
        builder.AddNodeDefinitionProvider(new GreetingNodeProvider());
    }
}

public sealed class GreetingNodeProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        => [
            new NodeDefinition(
                new NodeDefinitionId("sample.greeting.node"),
                "Greeting Node",
                "Samples",
                "Example plugin-defined node.",
                [],
                [new PortDefinition("out", "Output", new PortTypeId("string"), "#6AD5C4")])
        ];
}
```

## 本地验证

```powershell
dotnet build GreetingPlugin/GreetingPlugin.csproj
dotnet run --project tools/AsterGraph.PluginTool -- validate GreetingPlugin/bin/Debug/net8.0/GreetingPlugin.dll
```

validator 是跨平台 CLI。它会输出 manifest 摘要、兼容性状态、trust-policy 结果、签名证据和 SHA-256 哈希，宿主可以把这些信息接入 allowlist。

## 直接注册

```csharp
var editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
{
    Document = document,
    NodeCatalog = catalog,
    CompatibilityService = compatibilityService,
    PluginRegistrations =
    [
        GraphEditorPluginRegistration.FromPlugin(new GreetingPlugin())
    ]
});
```

## 从程序集或包注册

如果宿主是从磁盘加载插件，而不是直接 `new` 出来：

- 用 `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`
- 通过宿主自己的 trust policy 评估候选项
- 把批准的包变成 `GraphEditorPluginRegistration`
- 只对可信代码启用 in-process 插件加载

## 重要边界

插件加载没有 sandbox。对 public beta 宿主，更稳妥的做法是：

- 固定插件目录
- 用 allowlist
- 在宿主策略里做签名或哈希校验
- 不要把插件加载当成隔离边界

更细的 v1 manifest 和 trust-policy 合同见 [插件信任契约 v1](./plugin-trust-contracts.md)。

另见：

- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
