using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.TestPlugins;

public sealed class SamplePlugin : IGraphEditorPlugin
{
    internal static readonly NodeDefinitionId DefinitionId = new("tests.sample-plugin.node");
    public const string AddNodeCommandId = "tests.sample-plugin.add-node";

    public GraphEditorPluginDescriptor Descriptor { get; } = new(
        "tests.sample-plugin",
        "Sample Plugin",
        "Plugin fixture for canonical loading tests.",
        "1.0.0-test");

    public void Register(GraphEditorPluginBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddNodeDefinitionProvider(new SampleNodeDefinitionProvider());
        builder.AddCommandContributor(new SampleCommandContributor());
        builder.AddNodePresentationProvider(new SampleNodePresentationProvider());
        builder.AddLocalizationProvider(new SampleLocalizationProvider());
    }
}

internal sealed class SampleNodeDefinitionProvider : INodeDefinitionProvider
{
    public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
        =>
            [
                new NodeDefinition(
                    SamplePlugin.DefinitionId,
                    "Sample Plugin Node",
                    "Plugins",
                    "Fixture",
                    [],
                    [],
                    [
                        new NodeParameterDefinition(
                            "mode",
                            "Mode",
                            new PortTypeId("enum"),
                            ParameterEditorKind.Enum,
                            defaultValue: "sample",
                            constraints: new ParameterConstraints(
                                AllowedOptions:
                                [
                                    new ParameterOptionDefinition("sample", "Sample"),
                                    new ParameterOptionDefinition("diagnostic", "Diagnostic"),
                                ]),
                            groupName: "Plugin",
                            helpText: "Sample plugin metadata exposed by PluginTool reports."),
                    ]),
            ];
}

internal sealed class SampleCommandContributor : IGraphEditorPluginCommandContributor
{
    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors(GraphEditorPluginCommandContext context)
        =>
            [
                new GraphEditorCommandDescriptorSnapshot(
                    SamplePlugin.AddNodeCommandId,
                    "Add Sample Plugin Node",
                    "plugin",
                    iconKey: "plugin",
                    defaultShortcut: "Ctrl+Shift+P",
                    source: GraphEditorCommandSourceKind.Plugin,
                    isEnabled: true),
            ];

    public bool TryExecuteCommand(GraphEditorPluginCommandExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var beforeCount = context.Session.Queries.CreateDocumentSnapshot().Nodes.Count;
        context.Session.Commands.AddNode(SamplePlugin.DefinitionId, new GraphPoint(640, 220));
        return context.Session.Queries.CreateDocumentSnapshot().Nodes.Count == beforeCount + 1;
    }
}

internal sealed class SampleNodePresentationProvider : IGraphEditorPluginNodePresentationProvider
{
    public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
        => new(
            SubtitleOverride: "Plugin Subtitle",
            TopRightBadges:
            [
                new NodeAdornmentDescriptor("Plugin", "#6AD5C4"),
            ]);
}

internal sealed class SampleLocalizationProvider : IGraphEditorPluginLocalizationProvider
{
    public string GetString(string key, string fallback)
        => key == "editor.menu.canvas.addNode"
            ? "Plugin Add Node"
            : fallback;
}
