using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Plugins;

namespace AsterGraphPlugin;

public sealed class SamplePlugin : IGraphEditorPlugin
{
    public GraphEditorPluginDescriptor Descriptor { get; } = new(
        "PluginId",
        "AsterGraph Sample Plugin",
        description: "Adds one sample node definition through the public plugin contract.");

    public void Register(GraphEditorPluginBuilder builder)
    {
        builder.AddNodeDefinitionProvider(new SampleNodeDefinitionProvider());
    }
}

internal sealed class SampleNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        =>
        [
            new NodeDefinition(
                new NodeDefinitionId("PluginId.node"),
                "Plugin Node",
                "Plugin",
                "Sample node contributed by an AsterGraph plugin.",
                [new PortDefinition("in", "Input", new PortTypeId("signal"), "#f3b36b")],
                [new PortDefinition("out", "Output", new PortTypeId("signal"), "#6ad5c4")]),
        ];
}
