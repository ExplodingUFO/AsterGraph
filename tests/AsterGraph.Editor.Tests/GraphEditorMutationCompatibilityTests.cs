using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorMutationCompatibilityTests
{
    private const string SourceNodeId = "tests.compatibility.source-001";
    private const string TargetNodeId = "tests.compatibility.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void GraphEditorMutationCompatibility_NodeAndConnectionSnapshots_RemainAlignedBetweenRetainedAndRuntime()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.node");
        var retained = CreateLegacyEditor(definitionId);
        var runtime = CreateRuntimeSession(definitionId);
        var addedNodePosition = new GraphPoint(340, 240);
        var movedSourceNodePosition = new GraphPoint(180, 210);

        retained.AddNode(Assert.Single(retained.NodeTemplates), addedNodePosition);
        runtime.Commands.AddNode(definitionId, addedNodePosition);

        var positionUpdate = new NodePositionSnapshot(SourceNodeId, movedSourceNodePosition);
        retained.SetNodePositions([positionUpdate], updateStatus: false);
        runtime.Commands.SetNodePositions([positionUpdate], updateStatus: false);

        retained.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
        runtime.Commands.StartConnection(SourceNodeId, SourcePortId);
        runtime.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        AssertParity(retained, runtime);
    }

    [Fact]
    public void GraphEditorMutationCompatibility_DeleteConnection_KeepSelectionAndSnapshotConsistent()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.delete-connection");
        var retained = CreateLegacyEditor(definitionId);
        var runtime = CreateRuntimeSession(definitionId);

        ConnectDefaultPair(retained, runtime);
        SelectSourceNode(retained, runtime);

        var retainedConnectionId = Assert.Single(retained.CreateDocumentSnapshot().Connections).Id;
        var runtimeConnectionId = Assert.Single(runtime.Queries.CreateDocumentSnapshot().Connections).Id;

        retained.DeleteConnection(retainedConnectionId);
        runtime.Commands.DeleteConnection(runtimeConnectionId);

        Assert.Empty(retained.CreateDocumentSnapshot().Connections);
        Assert.Empty(runtime.Queries.CreateDocumentSnapshot().Connections);
        AssertParity(retained, runtime);
    }

    [Fact]
    public void GraphEditorMutationCompatibility_BreakConnectionsForPort_KeepSelectionAndSnapshotConsistent()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.break-port");
        var retained = CreateLegacyEditor(definitionId);
        var runtime = CreateRuntimeSession(definitionId);

        ConnectDefaultPair(retained, runtime);
        SelectSourceNode(retained, runtime);

        retained.BreakConnectionsForPort(SourceNodeId, SourcePortId);
        runtime.Commands.BreakConnectionsForPort(SourceNodeId, SourcePortId);

        Assert.Empty(retained.CreateDocumentSnapshot().Connections);
        Assert.Empty(runtime.Queries.CreateDocumentSnapshot().Connections);
        AssertParity(retained, runtime);
    }

    [Fact]
    public void GraphEditorMutationCompatibility_DisconnectAll_KeepSelectionAndSnapshotConsistent()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.disconnect-all");
        var retained = CreateLegacyEditor(definitionId);
        var runtime = CreateRuntimeSession(definitionId);

        ConnectDefaultPair(retained, runtime);
        SelectSourceNode(retained, runtime);

        retained.DisconnectAll(TargetNodeId);
        runtime.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                "connections.disconnect-all",
                [
                    new GraphEditorCommandArgumentSnapshot("nodeId", TargetNodeId),
                ]));

        Assert.Empty(retained.CreateDocumentSnapshot().Connections);
        Assert.Empty(runtime.Queries.CreateDocumentSnapshot().Connections);
        AssertParity(retained, runtime);
    }

    [Fact]
    public void GraphEditorMutationCompatibility_DirtyStateAndUndoCapability_TrackNodeMutationAcrossFacadeAndRuntime()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.dirty-undo");
        var retainedWorkspace = new InMemoryWorkspaceService("workspace://retained");
        var runtimeWorkspace = new InMemoryWorkspaceService("workspace://runtime");
        var retained = CreateLegacyEditor(definitionId, retainedWorkspace);
        var runtime = CreateRuntimeSession(definitionId, runtimeWorkspace);
        var movedSourceNodePosition = new GraphPoint(212, 244);
        var positionUpdate = new NodePositionSnapshot(SourceNodeId, movedSourceNodePosition);

        retained.SaveWorkspace();
        runtime.Commands.SaveWorkspace();

        Assert.False(retained.IsDirty);
        Assert.False(retained.CanUndo);
        Assert.False(runtime.Queries.GetCapabilitySnapshot().CanUndo);

        retained.SetNodePositions([positionUpdate], updateStatus: true);
        runtime.Commands.SetNodePositions([positionUpdate], updateStatus: true);

        Assert.True(retained.IsDirty);
        Assert.True(retained.CanUndo);
        Assert.True(runtime.Queries.GetCapabilitySnapshot().CanUndo);
        AssertParity(retained, runtime);

        retained.Undo();
        runtime.Commands.Undo();

        Assert.False(retained.IsDirty);
        Assert.False(retained.CanUndo);
        Assert.False(runtime.Queries.GetCapabilitySnapshot().CanUndo);
        AssertParity(retained, runtime);
    }

    private static void ConnectDefaultPair(GraphEditorViewModel retained, IGraphEditorSession runtime)
    {
        retained.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
        runtime.Commands.StartConnection(SourceNodeId, SourcePortId);
        runtime.Commands.CompleteConnection(TargetNodeId, TargetPortId);
    }

    private static void SelectSourceNode(GraphEditorViewModel retained, IGraphEditorSession runtime)
    {
        retained.SelectSingleNode(Assert.Single(retained.Nodes, node => node.Id == SourceNodeId), updateStatus: false);
        runtime.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
    }

    private static void AssertParity(GraphEditorViewModel retained, IGraphEditorSession runtime)
    {
        var retainedDocument = retained.CreateDocumentSnapshot();
        var runtimeDocument = runtime.Queries.CreateDocumentSnapshot();

        Assert.Equal(CreateSignature(retainedDocument), CreateSignature(runtimeDocument));
        Assert.Equal(CaptureSelectionSignature(retained), CaptureSelectionSignature(runtime));
    }

    private static string CaptureSelectionSignature(GraphEditorViewModel editor)
        => string.Join(
            "|",
            editor.SelectedNodes.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal))
        + $"::{editor.SelectedNode?.Id ?? "<none>"}";

    private static string CaptureSelectionSignature(IGraphEditorSession session)
    {
        var snapshot = session.Queries.GetSelectionSnapshot();
        return string.Join(
            "|",
            snapshot.SelectedNodeIds.OrderBy(id => id, StringComparer.Ordinal))
        + $"::{snapshot.PrimarySelectedNodeId ?? "<none>"}";
    }

    private static string CreateSignature(GraphDocument document)
        => GraphDocumentSerializer.Serialize(document);

    private static GraphEditorViewModel CreateLegacyEditor(
        NodeDefinitionId definitionId,
        IGraphWorkspaceService? workspaceService = null)
        => new(
            CreateDocument(definitionId),
            CreateCatalog(definitionId),
            new DefaultPortCompatibilityService(),
            workspaceService: workspaceService);

    private static IGraphEditorSession CreateRuntimeSession(
        NodeDefinitionId definitionId,
        IGraphWorkspaceService? workspaceService = null)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
            WorkspaceService = workspaceService,
        });

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Compatibility Graph",
            "Retained facade and runtime parity coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Compatibility",
                    "Parity source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Compatibility",
                    "Parity target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    definitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Compatibility Node",
            "Tests",
            "Compatibility",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private sealed class InMemoryWorkspaceService : IGraphWorkspaceService
    {
        private GraphDocument? _document;

        public InMemoryWorkspaceService(string workspacePath)
        {
            WorkspacePath = workspacePath;
        }

        public string WorkspacePath { get; }

        public void Save(GraphDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            _document = document;
        }

        public GraphDocument Load()
            => _document ?? throw new InvalidOperationException("No saved graph document is available.");

        public bool Exists()
            => _document is not null;
    }
}
