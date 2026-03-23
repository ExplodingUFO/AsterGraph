using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorFacadeRefactorTests
{
    [Fact]
    public void GraphEditorViewModel_RebuildsMixedParametersThroughPublicSelectionPath()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.public-path");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);

        editor.SetSelection([editor.Nodes[0], editor.Nodes[1]], editor.Nodes[0], status: null);

        Assert.Equal(2, editor.SelectedNodeParameters.Count);
        Assert.Contains(editor.SelectedNodeParameters, parameter => parameter.Key == "threshold" && parameter.HasMixedValues);
        Assert.Contains(editor.SelectedNodeParameters, parameter => parameter.Key == "enabled" && !parameter.HasMixedValues);
    }

    [Fact]
    public void GraphEditorViewModel_CachesComputedStateCommandsInSingleField()
    {
        var field = typeof(GraphEditorViewModel).GetField("_computedStateCommands", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(field);
    }

    [Fact]
    public void BuildSelectedNodeParameters_PreservesMixedValuesForSharedDefinitionSelection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.parameter-node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Parameter Node",
            "Tests",
            "Batch Edit",
            [],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "threshold",
                    "Threshold",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 0.5),
                new NodeParameterDefinition(
                    "enabled",
                    "Enabled",
                    new PortTypeId("bool"),
                    ParameterEditorKind.Boolean,
                    defaultValue: true),
            ]));

        var selectedNodes = new[]
        {
            CreateNode("node-001", definitionId, threshold: 0.25, enabled: true),
            CreateNode("node-002", definitionId, threshold: 0.75, enabled: true),
        };

        var projection = new GraphEditorInspectorProjection();
        var parameters = projection.BuildSelectedNodeParameters(
            selectedNodes,
            catalog,
            enableBatchParameterEditing: true,
            canEditNodeParameters: true,
            (_, _) => { });

        Assert.Equal(2, parameters.Count);
        Assert.Contains(parameters, parameter => parameter.Key == "threshold" && parameter.HasMixedValues);
        Assert.Contains(parameters, parameter => parameter.Key == "enabled" && !parameter.HasMixedValues);
    }

    [Fact]
    public void FormatRelatedNodes_ReturnsDistinctReadableLinesAndNoneFallback()
    {
        var outputNode = CreateNode("node-out", new NodeDefinitionId("tests.editor.facade.out"), 1, true, outputLabel: "Color");
        var inputNode = CreateNode("node-in", new NodeDefinitionId("tests.editor.facade.in"), 1, true, inputLabel: "Tint");
        var byId = new Dictionary<string, NodeViewModel>(StringComparer.Ordinal)
        {
            [outputNode.Id] = outputNode,
            [inputNode.Id] = inputNode,
        };

        var projection = new GraphEditorInspectorProjection();
        var related = projection.FormatRelatedNodes(
            [
                new ConnectionViewModel("c-001", "node-out", "out", "node-in", "in", "link", "#55D8C1"),
                new ConnectionViewModel("c-002", "node-out", "out", "node-in", "in", "link", "#55D8C1"),
            ],
            useSource: true,
            byId.GetValueOrDefault);

        Assert.Equal("Output node  ·  Color", related);

        var none = projection.FormatRelatedNodes([], useSource: false, byId.GetValueOrDefault);
        Assert.Equal("None", none);
    }

    private static NodeViewModel CreateNode(
        string nodeId,
        NodeDefinitionId definitionId,
        double threshold,
        bool enabled,
        string inputLabel = "Input",
        string outputLabel = "Output")
    {
        return new NodeViewModel(new GraphNode(
            nodeId,
            nodeId == "node-out" ? "Output node" : "Input node",
            "Tests",
            "Facade",
            "Node for projection tests.",
            new GraphPoint(120, 80),
            new GraphSize(220, 160),
            [
                new GraphPort("in", inputLabel, PortDirection.Input, "float", "#55D8C1", new PortTypeId("float")),
            ],
            [
                new GraphPort("out", outputLabel, PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
            ],
            "#55D8C1",
            definitionId,
            [
                new GraphParameterValue("threshold", new PortTypeId("float"), threshold),
                new GraphParameterValue("enabled", new PortTypeId("bool"), enabled),
            ]));
    }

    private static GraphEditorViewModel CreateEditorWithSharedDefinitionNodes(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Parameter Node",
            "Tests",
            "Public Path",
            [],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "threshold",
                    "Threshold",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 0.5),
                new NodeParameterDefinition(
                    "enabled",
                    "Enabled",
                    new PortTypeId("bool"),
                    ParameterEditorKind.Boolean,
                    defaultValue: true),
            ]));

        var document = new GraphDocument(
            "Facade Test",
            "Regression coverage for editor facade projection path.",
            [
                CreateNode("node-001", definitionId, threshold: 0.25, enabled: true).ToModel(),
                CreateNode("node-002", definitionId, threshold: 0.75, enabled: true).ToModel(),
            ],
            []);

        return new GraphEditorViewModel(document, catalog, new DefaultPortCompatibilityService());
    }
}
