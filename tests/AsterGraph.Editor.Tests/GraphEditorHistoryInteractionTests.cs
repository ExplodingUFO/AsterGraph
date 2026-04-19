using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorHistoryInteractionTests
{
    [Fact]
    public void GraphEditorViewModel_HistoryInteraction_PreservesUndoAndDirtySemantics()
    {
        var definitionId = new NodeDefinitionId("tests.history.interaction");
        var workspace = new GraphEditorHistoryTestSupport.RecordingWorkspaceService();
        var editor = GraphEditorHistoryTestSupport.CreateEditor(definitionId, workspace);
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var origin = new GraphPoint(sourceNode.X, sourceNode.Y);

        editor.SaveWorkspace();
        Assert.False(editor.IsDirty);

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [GraphEditorHistoryTestSupport.SourceNodeId] = origin,
            },
            80,
            40);
        editor.CompleteHistoryInteraction("Drag complete.");

        Assert.True(editor.IsDirty);
        Assert.True(editor.CanUndo);
        Assert.Equal(origin.X + 80, sourceNode.X);
        Assert.Equal(origin.Y + 40, sourceNode.Y);

        editor.Undo();
        var restoredNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);

        Assert.False(editor.IsDirty);
        Assert.Equal(origin.X, restoredNode.X);
        Assert.Equal(origin.Y, restoredNode.Y);
        Assert.False(editor.CanUndo);
        Assert.True(workspace.Exists());
    }

    [Fact]
    public void GraphEditorViewModel_HistoryInteraction_NoOpDrag_DoesNotLeaveDirtyStateLatched()
    {
        var definitionId = new NodeDefinitionId("tests.history.noop");
        var editor = GraphEditorHistoryTestSupport.CreateEditor(definitionId);
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var origin = new GraphPoint(sourceNode.X, sourceNode.Y);

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [GraphEditorHistoryTestSupport.SourceNodeId] = origin,
            },
            40,
            20);
        editor.ApplyDragOffset(
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [GraphEditorHistoryTestSupport.SourceNodeId] = origin,
            },
            0,
            0);
        editor.CompleteHistoryInteraction("No-op drag.");

        var currentNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        Assert.False(editor.IsDirty);
        Assert.False(editor.CanUndo);
        Assert.Equal(origin.X, currentNode.X);
        Assert.Equal(origin.Y, currentNode.Y);
    }

    [Fact]
    public void GraphEditorViewModel_HistoryInteraction_RestoresSelectionMembershipAndPrimaryNodeAcrossUndoRedo()
    {
        var definitionId = new NodeDefinitionId("tests.history.selection-roundtrip");
        var workspace = new GraphEditorHistoryTestSupport.RecordingWorkspaceService();
        var editor = GraphEditorHistoryTestSupport.CreateEditor(definitionId, workspace);
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var targetNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.TargetNodeId);
        var initialPositions = new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
        {
            [GraphEditorHistoryTestSupport.SourceNodeId] = new GraphPoint(sourceNode.X, sourceNode.Y),
            [GraphEditorHistoryTestSupport.TargetNodeId] = new GraphPoint(targetNode.X, targetNode.Y),
        };

        editor.SaveWorkspace();
        editor.SetSelection([sourceNode, targetNode], targetNode, status: null);

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(initialPositions, 48, 18);
        editor.CompleteHistoryInteraction("Moved with multi-selection.");

        editor.SelectSingleNode(sourceNode, updateStatus: false);
        var intermediatePositions = new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
        {
            [GraphEditorHistoryTestSupport.SourceNodeId] = new GraphPoint(sourceNode.X, sourceNode.Y),
            [GraphEditorHistoryTestSupport.TargetNodeId] = new GraphPoint(targetNode.X, targetNode.Y),
        };

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(intermediatePositions, -12, 30);
        editor.CompleteHistoryInteraction("Moved with single selection.");

        editor.Undo();

        Assert.Equal(
            [GraphEditorHistoryTestSupport.SourceNodeId, GraphEditorHistoryTestSupport.TargetNodeId],
            editor.SelectedNodes.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(GraphEditorHistoryTestSupport.TargetNodeId, editor.SelectedNode?.Id);
        Assert.True(editor.FindNode(GraphEditorHistoryTestSupport.SourceNodeId)!.IsSelected);
        Assert.True(editor.FindNode(GraphEditorHistoryTestSupport.TargetNodeId)!.IsSelected);
        var undoneSelection = editor.Session.Queries.GetSelectionSnapshot();
        Assert.Equal(
            [GraphEditorHistoryTestSupport.SourceNodeId, GraphEditorHistoryTestSupport.TargetNodeId],
            undoneSelection.SelectedNodeIds.OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(GraphEditorHistoryTestSupport.TargetNodeId, undoneSelection.PrimarySelectedNodeId);
        Assert.True(editor.IsDirty);
        Assert.True(editor.CanRedo);

        editor.Redo();

        Assert.Equal([GraphEditorHistoryTestSupport.SourceNodeId], editor.SelectedNodes.Select(node => node.Id));
        Assert.Equal(GraphEditorHistoryTestSupport.SourceNodeId, editor.SelectedNode?.Id);
        Assert.True(editor.FindNode(GraphEditorHistoryTestSupport.SourceNodeId)!.IsSelected);
        Assert.False(editor.FindNode(GraphEditorHistoryTestSupport.TargetNodeId)!.IsSelected);
        var redoneSelection = editor.Session.Queries.GetSelectionSnapshot();
        Assert.Equal([GraphEditorHistoryTestSupport.SourceNodeId], redoneSelection.SelectedNodeIds);
        Assert.Equal(GraphEditorHistoryTestSupport.SourceNodeId, redoneSelection.PrimarySelectedNodeId);
        Assert.True(editor.IsDirty);
        Assert.False(editor.CanRedo);
        Assert.True(workspace.Exists());
    }

    [Fact]
    public void GraphEditorViewModel_NodeGroupMutations_PreserveUndoRedoAndDirtySemantics()
    {
        var definitionId = new NodeDefinitionId("tests.history.node-groups");
        var workspace = new GraphEditorHistoryTestSupport.RecordingWorkspaceService();
        var editor = GraphEditorHistoryTestSupport.CreateEditor(definitionId, workspace);
        var originalPositions = editor.Nodes.ToDictionary(
            node => node.Id,
            node => new GraphPoint(node.X, node.Y),
            StringComparer.Ordinal);

        editor.SaveWorkspace();
        Assert.False(editor.IsDirty);

        editor.SetSelection(
            [
                editor.FindNode(GraphEditorHistoryTestSupport.SourceNodeId)!,
                editor.FindNode(GraphEditorHistoryTestSupport.TargetNodeId)!,
            ],
            editor.FindNode(GraphEditorHistoryTestSupport.TargetNodeId),
            status: null);

        var groupId = editor.TryCreateNodeGroupFromSelection("History Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(editor.TrySetNodeGroupCollapsed(groupId, isCollapsed: true));
        Assert.True(editor.TrySetNodeGroupPosition(groupId, new GraphPoint(160, 120), moveMemberNodes: true, updateStatus: false));

        var movedGroup = Assert.Single(editor.GetNodeGroups());
        Assert.Equal(groupId, movedGroup.Id);
        Assert.True(movedGroup.IsCollapsed);
        Assert.Equal(new GraphPoint(160, 120), movedGroup.Position);
        Assert.True(editor.IsDirty);

        foreach (var node in editor.Nodes)
        {
            Assert.Equal(groupId, node.GroupId);
            Assert.NotEqual(originalPositions[node.Id], new GraphPoint(node.X, node.Y));
        }

        editor.Undo();
        var undoneMove = Assert.Single(editor.GetNodeGroups());
        Assert.Equal(
            originalPositions[GraphEditorHistoryTestSupport.SourceNodeId].X,
            editor.FindNode(GraphEditorHistoryTestSupport.SourceNodeId)!.X);
        Assert.Equal(
            originalPositions[GraphEditorHistoryTestSupport.TargetNodeId].X,
            editor.FindNode(GraphEditorHistoryTestSupport.TargetNodeId)!.X);
        Assert.NotEqual(new GraphPoint(160, 120), undoneMove.Position);
        Assert.True(undoneMove.IsCollapsed);
        Assert.True(editor.IsDirty);

        editor.Undo();
        var undoneCollapse = Assert.Single(editor.GetNodeGroups());
        Assert.False(undoneCollapse.IsCollapsed);
        Assert.True(editor.IsDirty);

        editor.Undo();
        Assert.Empty(editor.GetNodeGroups());
        Assert.All(editor.Nodes, node => Assert.Null(node.GroupId));
        Assert.False(editor.IsDirty);

        editor.Redo();
        editor.Redo();
        editor.Redo();

        var redoneGroup = Assert.Single(editor.GetNodeGroups());
        Assert.Equal(groupId, redoneGroup.Id);
        Assert.True(redoneGroup.IsCollapsed);
        Assert.Equal(new GraphPoint(160, 120), redoneGroup.Position);
        Assert.All(editor.Nodes, node => Assert.Equal(groupId, node.GroupId));
        Assert.True(editor.IsDirty);
        Assert.False(editor.CanRedo);
        Assert.True(workspace.Exists());
    }
}
