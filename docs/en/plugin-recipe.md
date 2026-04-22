# Plugin And Custom Node Recipe

This recipe shows the smallest public-boundary way to ship a plugin that adds a node definition.

## Packages

Start with:

```powershell
dotnet add package AsterGraph.Abstractions --prerelease
dotnet add package AsterGraph.Editor --prerelease
```

Add `AsterGraph.Avalonia` only if the host also embeds the shipped UI.

## Minimal Plugin Shape

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
    public IReadOnlyList<NodeDefinition> GetDefinitions()
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

## Register It Directly

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

## Register It From Assembly Or Package

If the host is loading plugins from disk instead of constructing them directly:

- use `AsterGraphEditorFactory.DiscoverPluginCandidates(...)`
- evaluate candidates through your host trust policy
- turn approved packages into `GraphEditorPluginRegistration`
- keep plugin loading in-process only for trusted code

## Important Boundary

Plugin loading is not sandboxed. For public-beta hosts:

- keep plugin directories explicit
- prefer allowlists
- validate provenance with signatures or hashes in host policy
- do not treat plugin loading as an isolation boundary

For the v1 manifest and trust-policy contract, see [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md).

See also:

- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
