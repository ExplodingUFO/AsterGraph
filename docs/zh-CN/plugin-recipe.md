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

## Trust Policy Cookbook

使用下面这些 host-owned 策略模式。它们是 policy recipe，不是 runtime fallback：

| 模式 | 适用场景 | 宿主决策 |
| --- | --- | --- |
| Local dev allow | 已知机器上的本地开发循环。 | 只允许固定本地插件目录，并在 diagnostics 里显示 `ImplicitAllow` 或 local-dev reason。 |
| Hash allowlist | 小团队共享已知插件二进制。 | 持久化 PluginTool 输出的 SHA-256 hash，只允许完全匹配的候选项。 |
| Publisher/signature policy | 组织内部发布的插件包。 | 只有 signature evidence 和 publisher metadata 符合 host policy 时才允许。 |
| Block unknown source | 默认 prerelease 或企业姿态。 | 没有 allowlist、hash 或签名匹配的候选项，在 activation 前阻止。 |
| Enterprise fixed directory | 受管桌面部署。 | 只从管理员控制的目录发现插件，并保留 allowlist import/export 记录用于审计。 |

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
