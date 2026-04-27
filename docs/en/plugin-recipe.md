# Plugin And Custom Node Recipe

This recipe shows the smallest public-boundary way to ship a plugin that adds a node definition.

## Packages

Start with:

```powershell
dotnet new install ./templates
dotnet new astergraph-plugin -n GreetingPlugin --PluginId sample.greeting

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

## Validate It Locally

```powershell
dotnet build GreetingPlugin/GreetingPlugin.csproj
dotnet run --project tools/AsterGraph.PluginTool -- validate GreetingPlugin/bin/Debug/net8.0/GreetingPlugin.dll
```

The validator is a cross-platform CLI. It reports the manifest summary, compatibility status, trust-policy outcome, signature evidence, and SHA-256 hash that a host can use for an allowlist.

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

## Trust Policy Cookbook

Use one of these host-owned patterns. They are policy recipes, not runtime fallback modes:

| Pattern | When to use it | Host decision |
| --- | --- | --- |
| Local dev allow | Inner-loop development on a known machine. | Allow a fixed local plugin directory and show `ImplicitAllow` or a local-dev reason string in diagnostics. |
| Hash allowlist | Small teams sharing known plugin binaries. | Persist the PluginTool SHA-256 hash and allow only exact matches. |
| Publisher/signature policy | Organization-published packages. | Allow only candidates whose signature evidence and publisher metadata match the host policy. |
| Block unknown source | Default prerelease or enterprise posture. | Block candidates without an allowlist, hash, or accepted signature match before activation. |
| Enterprise fixed directory | Managed desktop deployments. | Discover only from an admin-controlled directory and keep import/export allowlist records for audit. |

## Important Boundary

Plugin loading is not sandboxed. For public beta hosts:

- keep plugin directories explicit
- prefer allowlists
- validate provenance with signatures or hashes in host policy
- do not treat plugin loading as an isolation boundary

For the v1 manifest and trust-policy contract, see [Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md).

See also:

- [Plugin Host Recipe](./plugin-host-recipe.md)
- [Host Integration](./host-integration.md)
- [Extension Contracts](./extension-contracts.md)
