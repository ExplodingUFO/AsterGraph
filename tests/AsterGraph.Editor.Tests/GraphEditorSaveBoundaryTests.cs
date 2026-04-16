using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSaveBoundaryTests
{
    [Fact]
    public void GraphEditorViewModel_SaveBoundary_PreservesUndoRedoDirtySemantics()
    {
        var definitionId = new NodeDefinitionId("tests.history.save-boundary");
        var workspace = new GraphEditorHistoryTestSupport.RecordingWorkspaceService();
        var editor = GraphEditorHistoryTestSupport.CreateEditor(definitionId, workspace);
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        var origin = new GraphPoint(sourceNode.X, sourceNode.Y);

        editor.BeginHistoryInteraction();
        editor.ApplyDragOffset(
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [GraphEditorHistoryTestSupport.SourceNodeId] = origin,
            },
            64,
            24);
        editor.CompleteHistoryInteraction("Moved before save.");

        editor.SaveWorkspace();
        Assert.False(editor.IsDirty);

        editor.Undo();
        var undoneNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        Assert.True(editor.IsDirty);
        Assert.Equal(origin.X, undoneNode.X);
        Assert.Equal(origin.Y, undoneNode.Y);

        editor.Redo();
        var redoneNode = Assert.Single(editor.Nodes, node => node.Id == GraphEditorHistoryTestSupport.SourceNodeId);
        Assert.False(editor.IsDirty);
        Assert.Equal(origin.X + 64, redoneNode.X);
        Assert.Equal(origin.Y + 24, redoneNode.Y);
        Assert.True(workspace.Exists());
    }
}
