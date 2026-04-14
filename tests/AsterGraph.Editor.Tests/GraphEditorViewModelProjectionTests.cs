global using AsterGraph.Editor.Hosting;

using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorViewModelProjectionTests
{
    [Fact]
    public void EditorAssembly_ContainsDedicatedKernelProjectionApplier()
    {
        var projectionType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorKernelProjectionApplier");

        Assert.NotNull(projectionType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedDocumentLoadCoordinator()
    {
        var coordinatorType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorDocumentLoadCoordinator");

        Assert.NotNull(coordinatorType);
    }

    [Fact]
    public void SelectionProjection_ProjectInspectorState_UsesConnectionSummariesAndSelectionCaption()
    {
        var definitionId = new NodeDefinitionId("tests.editor.projection.selection");
        var sourceNode = CreateNode("source-001", definitionId, "Source Node", inputLabel: "Input", outputLabel: "Output");
        var targetNode = CreateNode("target-001", definitionId, "Target Node", inputLabel: "Input", outputLabel: "Result");
        var byId = new Dictionary<string, NodeViewModel>(StringComparer.Ordinal)
        {
            [sourceNode.Id] = sourceNode,
            [targetNode.Id] = targetNode,
        };
        var projection = new GraphEditorSelectionProjection();
        ConnectionViewModel[] incomingConnections =
        [
            new ConnectionViewModel("c-001", sourceNode.Id, "out", targetNode.Id, "in", "link", "#55D8C1"),
        ];

        var state = projection.ProjectInspectorState(
            [targetNode],
            targetNode,
            _ => incomingConnections,
            _ => [],
            byId.GetValueOrDefault);

        Assert.Equal("1 incoming  ·  0 outgoing", state.InspectorConnections);
        Assert.Equal("Source Node  ·  Output", state.InspectorUpstream);
        Assert.Equal("None", state.InspectorDownstream);
        Assert.Equal("1 inputs  ·  1 outputs", state.SelectionCaption);
    }

    [Fact]
    public void GraphEditorViewModel_ApplyKernelDocument_RebuildsDocumentProjectionAndClearsSelectionState()
    {
        var initialDefinitionId = new NodeDefinitionId("tests.editor.projection.initial");
        var replacementDefinitionId = new NodeDefinitionId("tests.editor.projection.replacement");
        var editor = CreateEditor(CreateDocument(
            "Initial Graph",
            "Initial document.",
            initialDefinitionId,
            sourceNodeId: "initial-source",
            targetNodeId: "initial-target",
            includeConnection: false));

        editor.SelectSingleNode(editor.FindNode("initial-source"), updateStatus: false);

        var replacementDocument = CreateDocument(
            "Replacement Graph",
            "Replacement document.",
            replacementDefinitionId,
            sourceNodeId: "replacement-source",
            targetNodeId: "replacement-target",
            includeConnection: true);

        editor.ApplyKernelDocument(replacementDocument, "Loaded replacement.", markClean: true);

        Assert.Equal("Replacement Graph", editor.Title);
        Assert.Equal("Replacement document.", editor.Description);
        Assert.Equal(2, editor.Nodes.Count);
        Assert.Single(editor.Connections);
        Assert.Null(editor.FindNode("initial-source"));
        Assert.NotNull(editor.FindNode("replacement-source"));
        Assert.Empty(editor.SelectedNodes);
        Assert.Null(editor.SelectedNode);
        Assert.Empty(editor.SelectedNodeParameters);
        Assert.Equal("No selection", editor.SelectionCaption);
        Assert.Equal("Select a node to inspect its connection summary.", editor.InspectorConnections);
        Assert.False(editor.IsDirty);
        Assert.Equal("Loaded replacement.", editor.StatusMessage);
    }

    [Fact]
    public void GraphEditorViewModel_ApplyKernelDocument_WhenMarkCleanFalse_LeavesDocumentDirty()
    {
        var initialDefinitionId = new NodeDefinitionId("tests.editor.projection.initial-dirty");
        var replacementDefinitionId = new NodeDefinitionId("tests.editor.projection.replacement-dirty");
        var editor = CreateEditor(CreateDocument(
            "Initial Graph",
            "Initial document.",
            initialDefinitionId,
            sourceNodeId: "initial-source",
            targetNodeId: "initial-target",
            includeConnection: false));

        var replacementDocument = CreateDocument(
            "Replacement Graph",
            "Replacement document.",
            replacementDefinitionId,
            sourceNodeId: "replacement-source",
            targetNodeId: "replacement-target",
            includeConnection: true);

        editor.ApplyKernelDocument(replacementDocument, "Loaded replacement.", markClean: false);

        Assert.Equal("Replacement Graph", editor.Title);
        Assert.Single(editor.Connections);
        Assert.True(editor.IsDirty);
        Assert.Equal("Loaded replacement.", editor.StatusMessage);
    }

    [Fact]
    public void GraphEditorViewModel_ApplyKernelSelection_UsesRebuiltIndexesForInspectorProjection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.projection.selection-rebuild");
        var editor = CreateEditor(CreateDocument(
            "Projection Graph",
            "Projection document.",
            definitionId,
            sourceNodeId: "source-node",
            targetNodeId: "target-node",
            includeConnection: true));

        editor.ApplyKernelSelection(["target-node"], "target-node");

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Equal("None", editor.InspectorDownstream);
        Assert.Single(editor.SelectedNodes);
        Assert.Equal("target-node", editor.SelectedNode!.Id);
    }

    [Fact]
    public void GraphEditorViewModel_ApplyKernelViewport_UpdatesViewportProjection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.projection.viewport");
        var editor = CreateEditor(CreateDocument(
            "Viewport Graph",
            "Viewport document.",
            definitionId,
            sourceNodeId: "source-node",
            targetNodeId: "target-node",
            includeConnection: false));

        editor.ApplyKernelViewport(new GraphEditorViewportSnapshot(1.35, 240, -80, 1280, 720));

        Assert.Equal(1.35, editor.Zoom);
        Assert.Equal(240, editor.PanX);
        Assert.Equal(-80, editor.PanY);
        Assert.Equal(1280, editor.ViewportWidth);
        Assert.Equal(720, editor.ViewportHeight);
    }

    [Fact]
    public void GraphEditorViewModel_ApplyKernelPendingConnection_UsesRebuiltNodeProjection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.projection.pending");
        var editor = CreateEditor(CreateDocument(
            "Pending Graph",
            "Pending document.",
            definitionId,
            sourceNodeId: "source-node",
            targetNodeId: "target-node",
            includeConnection: false));

        editor.ApplyKernelPendingConnection(GraphEditorPendingConnectionSnapshot.Create(true, "source-node", "out"));

        Assert.True(editor.HasPendingConnection);
        Assert.Equal("source-node", editor.PendingSourceNode!.Id);
        Assert.Equal("out", editor.PendingSourcePort!.Id);
    }

    [Fact]
    public void GraphEditorViewModel_SessionLoadWorkspace_BridgesKernelDocumentProjectionIntoFacade()
    {
        var initialDefinitionId = new NodeDefinitionId("tests.editor.projection.bridge.initial");
        var replacementDefinitionId = new NodeDefinitionId("tests.editor.projection.bridge.replacement");
        var workspaceService = new RecordingWorkspaceService(CreateDocument(
            "Loaded Graph",
            "Loaded from workspace service.",
            replacementDefinitionId,
            sourceNodeId: "loaded-source",
            targetNodeId: "loaded-target",
            includeConnection: true));
        var editor = new GraphEditorViewModel(
            CreateDocument(
                "Initial Graph",
                "Initial document.",
                initialDefinitionId,
                sourceNodeId: "initial-source",
                targetNodeId: "initial-target",
                includeConnection: false),
            CreateCatalog(initialDefinitionId, replacementDefinitionId),
            new DefaultPortCompatibilityService(),
            workspaceService: workspaceService);

        var loaded = editor.Session.Commands.LoadWorkspace();

        Assert.True(loaded);
        Assert.Equal("Loaded Graph", editor.Title);
        Assert.Equal("Loaded from workspace service.", editor.Description);
        Assert.Null(editor.FindNode("initial-source"));
        Assert.NotNull(editor.FindNode("loaded-source"));
        Assert.Single(editor.Connections);
        Assert.False(editor.IsDirty);
        Assert.Contains("Workspace loaded", editor.StatusMessage, StringComparison.Ordinal);
    }

    private static GraphEditorViewModel CreateEditor(GraphDocument document)
        => new(
            document,
            CreateCatalog(document.Nodes.Select(node => node.DefinitionId).OfType<NodeDefinitionId>().Distinct().ToArray()),
            new DefaultPortCompatibilityService());

    private static NodeCatalog CreateCatalog(params NodeDefinitionId[] definitionIds)
    {
        var catalog = new NodeCatalog();
        foreach (var definitionId in definitionIds.Distinct())
        {
            catalog.RegisterDefinition(new NodeDefinition(
                definitionId,
                $"{definitionId.Value} Node",
                "Tests",
                "Projection",
                [
                    new PortDefinition("in", "Input", new PortTypeId("float"), "#F3B36B"),
                ],
                [
                    new PortDefinition("out", "Output", new PortTypeId("float"), "#55D8C1"),
                ],
                parameters:
                [
                    new NodeParameterDefinition(
                        "threshold",
                        "Threshold",
                        new PortTypeId("float"),
                        ParameterEditorKind.Number,
                        defaultValue: 0.5),
                ]));
        }

        return catalog;
    }

    private static GraphDocument CreateDocument(
        string title,
        string description,
        NodeDefinitionId definitionId,
        string sourceNodeId,
        string targetNodeId,
        bool includeConnection)
    {
        return new GraphDocument(
            title,
            description,
            [
                CreateNode(sourceNodeId, definitionId, "Source Node", inputLabel: "Input", outputLabel: "Output").ToModel(),
                CreateNode(targetNodeId, definitionId, "Target Node", inputLabel: "Input", outputLabel: "Result").ToModel(),
            ],
            includeConnection
                ?
                [
                    new GraphConnection("connection-001", sourceNodeId, "out", targetNodeId, "in", "link", "#55D8C1"),
                ]
                : []);
    }

    private static NodeViewModel CreateNode(
        string nodeId,
        NodeDefinitionId definitionId,
        string title,
        string inputLabel,
        string outputLabel)
    {
        return new NodeViewModel(new GraphNode(
            nodeId,
            title,
            "Tests",
            "Projection",
            "Projection test node.",
            new GraphPoint(120, 80),
            new GraphSize(220, 160),
            [
                new GraphPort("in", inputLabel, PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
            ],
            [
                new GraphPort("out", outputLabel, PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
            ],
            "#55D8C1",
            definitionId,
            [
                new GraphParameterValue("threshold", new PortTypeId("float"), 0.5),
            ]));
    }

    private sealed class RecordingWorkspaceService(GraphDocument loadedDocument) : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://projection-tests";

        public void Save(GraphDocument document)
        {
        }

        public GraphDocument Load()
            => loadedDocument;

        public bool Exists()
            => true;
    }
}
