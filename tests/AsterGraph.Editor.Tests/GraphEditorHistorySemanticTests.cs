using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorHistorySemanticTests
{
    [Fact]
    public void MixedKernelAndRetainedHistory_SaveBoundaryRoundTripsLatestRetainedMutation()
    {
        var definitionId = new NodeDefinitionId("tests.history.semantic.mixed");
        var workspace = new GraphEditorHistoryTestSupport.RecordingWorkspaceService();
        var editor = GraphEditorHistoryTestSupport.CreateEditor(definitionId, workspace);
        var session = editor.Session;
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var targetNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.TargetNodeId);
        var sourceOrigin = new GraphPoint(sourceNode.X, sourceNode.Y);
        var targetOrigin = new GraphPoint(targetNode.X, targetNode.Y);
        var existingConnection = Assert.Single(editor.Connections);

        session.Commands.DeleteConnection(existingConnection.Id);
        session.Commands.StartConnection(GraphEditorHistoryTestSupport.SourceNodeId, GraphEditorHistoryTestSupport.SourcePortId);
        session.Commands.CompleteConnection(GraphEditorHistoryTestSupport.TargetNodeId, GraphEditorHistoryTestSupport.TargetPortId);

        Assert.True(session.Queries.GetCapabilitySnapshot().CanUndo);
        Assert.Single(editor.Connections);

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [GraphEditorHistoryTestSupport.SourceNodeId] = sourceOrigin,
                [GraphEditorHistoryTestSupport.TargetNodeId] = targetOrigin,
            },
            42,
            18);
        editor.CompleteHistoryInteraction("Scale move complete.");

        Assert.True(editor.CanUndo);

        editor.SaveWorkspace();
        Assert.False(editor.IsDirty);

        editor.Undo();

        var undoneSource = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var undoneTarget = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.TargetNodeId);
        Assert.Single(editor.Connections);
        Assert.Equal(sourceOrigin.X, undoneSource.X);
        Assert.Equal(sourceOrigin.Y, undoneSource.Y);
        Assert.Equal(targetOrigin.X, undoneTarget.X);
        Assert.Equal(targetOrigin.Y, undoneTarget.Y);
        Assert.True(editor.IsDirty);

        editor.Redo();

        var redoneSource = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var redoneTarget = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.TargetNodeId);
        Assert.Single(editor.Connections);
        Assert.Equal(sourceOrigin.X + 42, redoneSource.X);
        Assert.Equal(sourceOrigin.Y + 18, redoneSource.Y);
        Assert.Equal(targetOrigin.X + 42, redoneTarget.X);
        Assert.Equal(targetOrigin.Y + 18, redoneTarget.Y);
        Assert.False(editor.IsDirty);
        Assert.True(workspace.Exists());
    }
}
