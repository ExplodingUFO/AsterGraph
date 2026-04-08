using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;

namespace AsterGraph.TestPlugins;

public sealed class SamplePlugin : IGraphEditorPlugin
{
    public GraphEditorPluginDescriptor Descriptor { get; } = new(
        "tests.sample-plugin",
        "Sample Plugin",
        "Plugin fixture for canonical loading tests.",
        "1.0.0-test");

    public void Register(GraphEditorPluginBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddNodeDefinitionProvider(new SampleNodeDefinitionProvider());
        builder.AddContextMenuAugmentor(new SampleContextMenuAugmentor());
        builder.AddNodePresentationProvider(new SampleNodePresentationProvider());
        builder.AddLocalizationProvider(new SampleLocalizationProvider());
    }
}

internal sealed class SampleNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        => [new NodeDefinition(new NodeDefinitionId("tests.sample-plugin.node"), "Sample Plugin Node", "Plugins", "Fixture", [], [])];
}

internal sealed class SampleContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
{
    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        => context.StockItems;
}

internal sealed class SampleNodePresentationProvider : IGraphEditorPluginNodePresentationProvider
{
    public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        => NodePresentationState.Empty;
}

internal sealed class SampleLocalizationProvider : IGraphEditorPluginLocalizationProvider
{
    public string GetString(string key, string fallback)
        => fallback;
}
