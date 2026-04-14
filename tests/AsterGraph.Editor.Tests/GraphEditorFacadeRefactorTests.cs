using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorFacadeRefactorTests
{
    [Fact]
    public void EditorAssembly_ContainsDedicatedSelectionCoordinator()
    {
        var coordinatorType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorSelectionCoordinator");

        Assert.NotNull(coordinatorType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedSelectionStateSynchronizer()
    {
        var synchronizerType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorSelectionStateSynchronizer");

        Assert.NotNull(synchronizerType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedSelectionProjectionApplier()
    {
        var applierType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorSelectionProjectionApplier");

        Assert.NotNull(applierType);
    }

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

        var projection = new GraphEditorSelectionProjection();
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

        var projection = new GraphEditorSelectionProjection();
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

    [Fact]
    public void SetHostContext_RaisesHostContextPropertyChangedOnlyWhenValueChanges()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.host-context");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var changedProperties = new List<string?>();
        editor.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        var hostContextA = new TestGraphHostContext(new object(), null);
        var hostContextB = new TestGraphHostContext(new object(), null);

        editor.SetHostContext(hostContextA);
        editor.SetHostContext(hostContextA);
        editor.SetHostContext(hostContextB);

        Assert.Equal(
            2,
            changedProperties.Count(propertyName => propertyName == nameof(GraphEditorViewModel.HostContext)));
    }

    [Fact]
    public void SelectedNodesCollectionChange_ReconcilesPrimarySelectionAndNodeFlags()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.selection-collection");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var first = editor.Nodes[0];
        var second = editor.Nodes[1];

        editor.SelectedNodes.Add(first);
        editor.SelectedNodes.Add(second);

        Assert.True(first.IsSelected);
        Assert.True(second.IsSelected);
        Assert.Same(first, editor.SelectedNode);

        editor.SelectedNodes.Remove(second);

        Assert.True(first.IsSelected);
        Assert.False(second.IsSelected);
        Assert.Same(first, editor.SelectedNode);
    }

    [Fact]
    public void NodesCollectionChange_CoercesSelectionToExistingNodes()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.selection-coerce");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var first = editor.Nodes[0];
        var second = editor.Nodes[1];

        editor.SetSelection([first, second], second, status: null);
        editor.Nodes.Remove(second);

        Assert.Equal([first], editor.SelectedNodes);
        Assert.Same(first, editor.SelectedNode);
        Assert.True(first.IsSelected);
        Assert.Equal("1 inputs  ·  1 outputs", editor.SelectionCaption);
        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);
        Assert.Equal("None", editor.InspectorDownstream);
    }

    [Fact]
    public void SelectedNodeChange_RebuildsInspectorProjectionAndBatchParameterProjection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.selection-primary-refresh");
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            firstTitle: "Source node",
            secondTitle: "Target node");
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];

        editor.Connections.Add(new ConnectionViewModel("c-001", source.Id, "out", target.Id, "in", "link", "#55D8C1"));
        editor.SetSelection([source, target], source, status: null);

        Assert.Equal("2 nodes selected  ·  primary Source node", editor.SelectionCaption);
        Assert.Equal("0 incoming  ·  1 outgoing", editor.InspectorConnections);
        Assert.Contains("Target node", editor.InspectorDownstream, StringComparison.Ordinal);
        Assert.Contains("Input", editor.InspectorDownstream, StringComparison.Ordinal);
        Assert.Equal(2, editor.SelectedNodeParameters.Count);

        editor.SelectedNode = target;

        Assert.Equal("2 nodes selected  ·  primary Target node", editor.SelectionCaption);
        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Equal(2, editor.SelectedNodeParameters.Count);
    }

    [Fact]
    public void ConnectionsCollectionChange_RefreshesInspectorProjectionForSelectedNode()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.connection-refresh");
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            firstTitle: "Source node",
            secondTitle: "Target node");
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];
        var connection = new ConnectionViewModel("c-001", source.Id, "out", target.Id, "in", "link", "#55D8C1");

        editor.SelectSingleNode(target, updateStatus: false);

        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);

        editor.Connections.Add(connection);

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);

        editor.Connections.Remove(connection);

        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);
    }

    [Fact]
    public void SelectedNodeChange_RaisesEditableParameterProjectionPropertyChanged()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.parameter-projection-notify");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var first = editor.Nodes[0];
        var second = editor.Nodes[1];
        var changedProperties = new List<string?>();

        editor.SetSelection([first, second], first, status: null);
        editor.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        editor.SelectedNode = second;

        Assert.Contains(nameof(GraphEditorViewModel.HasEditableParameters), changedProperties);
        Assert.Contains(nameof(GraphEditorViewModel.HasBatchEditableParameters), changedProperties);
    }

    private static NodeViewModel CreateNode(
        string nodeId,
        NodeDefinitionId definitionId,
        double threshold,
        bool enabled,
        string? title = null,
        string inputLabel = "Input",
        string outputLabel = "Output")
    {
        return new NodeViewModel(new GraphNode(
            nodeId,
            title ?? (nodeId == "node-out" ? "Output node" : "Input node"),
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

    private static GraphEditorViewModel CreateEditorWithSharedDefinitionNodes(
        NodeDefinitionId definitionId,
        string firstTitle = "Input node",
        string secondTitle = "Input node")
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
                CreateNode("node-001", definitionId, threshold: 0.25, enabled: true, title: firstTitle).ToModel(),
                CreateNode("node-002", definitionId, threshold: 0.75, enabled: true, title: secondTitle).ToModel(),
            ],
            []);

        return new GraphEditorViewModel(document, catalog, new DefaultPortCompatibilityService());
    }

    private sealed record TestGraphHostContext(object Owner, object? TopLevel) : IGraphHostContext
    {
        public IServiceProvider? Services => null;
    }
}
