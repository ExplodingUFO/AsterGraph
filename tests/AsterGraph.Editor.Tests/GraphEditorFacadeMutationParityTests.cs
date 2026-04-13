using AsterGraph.Abstractions.Identifiers;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorFacadeMutationParityTests
{
    [Fact]
    public void DuplicateNode_KeepsRetainedFacadeAlignedWithKernelSnapshots()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.facade-duplicate");
        var editor = GraphEditorMutationCompatibilityTests.CreateLegacyEditor(definitionId);
        var original = Assert.Single(editor.Nodes, node => node.Id == GraphEditorMutationCompatibilityTests.SourceNodeId);

        editor.DuplicateNode(original.Id);

        var duplicate = Assert.Single(editor.Nodes, node => node.Id != original.Id && node.Title == original.Title);
        Assert.True(editor.IsDirty);
        Assert.Equal(duplicate.Id, editor.SelectedNode?.Id);
        Assert.Equal(
            GraphEditorMutationCompatibilityTests.CaptureSelectionSignature(editor),
            GraphEditorMutationCompatibilityTests.CaptureSelectionSignature(editor.Session));
        GraphEditorMutationCompatibilityTests.AssertRetainedFacadeMatchesKernelSnapshots(editor);
    }

    [Fact]
    public void DeleteNodeById_KeepsRetainedFacadeAlignedWithKernelSnapshots()
    {
        var definitionId = new NodeDefinitionId("tests.compatibility.facade-delete");
        var editor = GraphEditorMutationCompatibilityTests.CreateLegacyEditor(definitionId);

        editor.ConnectPorts(
            GraphEditorMutationCompatibilityTests.SourceNodeId,
            GraphEditorMutationCompatibilityTests.SourcePortId,
            GraphEditorMutationCompatibilityTests.TargetNodeId,
            GraphEditorMutationCompatibilityTests.TargetPortId);

        var source = Assert.Single(editor.Nodes, node => node.Id == GraphEditorMutationCompatibilityTests.SourceNodeId);
        var target = Assert.Single(editor.Nodes, node => node.Id == GraphEditorMutationCompatibilityTests.TargetNodeId);
        editor.SetSelection([source, target], target);

        editor.DeleteNodeById(source.Id);

        Assert.True(editor.IsDirty);
        Assert.DoesNotContain(editor.Nodes, node => node.Id == source.Id);
        Assert.Empty(editor.Connections);
        Assert.Collection(
            editor.SelectedNodes,
            node => Assert.Equal(target.Id, node.Id));
        Assert.Equal(target.Id, editor.SelectedNode?.Id);
        GraphEditorMutationCompatibilityTests.AssertRetainedFacadeMatchesKernelSnapshots(editor);
    }
}
