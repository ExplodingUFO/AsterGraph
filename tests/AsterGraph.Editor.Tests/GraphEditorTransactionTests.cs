using System.Reflection;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorTransactionTests
{
    [Fact]
    public void IGraphEditorSession_ExposesLightweightBeginMutationEntryPoint()
    {
        var method = typeof(IGraphEditorSession).GetMethod(
            nameof(IGraphEditorSession.BeginMutation),
            BindingFlags.Public | BindingFlags.Instance,
            [typeof(string)]);

        Assert.NotNull(method);
        Assert.Equal(typeof(IGraphEditorMutationScope), method!.ReturnType);

        var label = Assert.Single(method.GetParameters());
        Assert.True(label.IsOptional);
        Assert.Equal(typeof(string), label.ParameterType);
    }

    [Fact]
    public void RuntimeSnapshots_CaptureSelectionViewportAndCapabilities()
    {
        var selection = new GraphEditorSelectionSnapshot(["node-001", "node-002"], "node-002");
        Assert.Equal(new[] { "node-001", "node-002" }, selection.SelectedNodeIds);
        Assert.Equal("node-002", selection.PrimarySelectedNodeId);

        var viewport = new GraphEditorViewportSnapshot(0.88, 110, 96, 1280, 720);
        Assert.Equal(0.88, viewport.Zoom);
        Assert.Equal(110, viewport.PanX);
        Assert.Equal(96, viewport.PanY);
        Assert.Equal(1280, viewport.ViewportWidth);
        Assert.Equal(720, viewport.ViewportHeight);

        var capabilities = new GraphEditorCapabilitySnapshot(
            CanUndo: true,
            CanRedo: false,
            CanCopySelection: true,
            CanPaste: true,
            CanSaveWorkspace: true,
            CanLoadWorkspace: true);
        Assert.True(capabilities.CanUndo);
        Assert.False(capabilities.CanRedo);
        Assert.True(capabilities.CanCopySelection);
        Assert.True(capabilities.CanPaste);
        Assert.True(capabilities.CanSaveWorkspace);
        Assert.True(capabilities.CanLoadWorkspace);
    }

    [Fact]
    public void RuntimeEventPayloads_CarryStableCommandAndFailureMetadata()
    {
        var command = new GraphEditorCommandExecutedEventArgs(
            "nodes.add",
            "batch-add",
            isInMutationScope: true,
            statusMessage: "Added node.");
        Assert.Equal("nodes.add", command.CommandId);
        Assert.Equal("batch-add", command.MutationLabel);
        Assert.True(command.IsInMutationScope);
        Assert.Equal("Added node.", command.StatusMessage);

        var exception = new InvalidOperationException("Load failed.");
        var failure = new GraphEditorRecoverableFailureEventArgs(
            "workspace.load.failed",
            "workspace.load",
            "Load failed: disk offline.",
            exception);
        Assert.Equal("workspace.load.failed", failure.Code);
        Assert.Equal("workspace.load", failure.Operation);
        Assert.Equal("Load failed: disk offline.", failure.Message);
        Assert.Same(exception, failure.Exception);
    }

    [Fact(Skip = "Phase 2 Plan 02-02 implements mutation batching behavior.")]
    public void RuntimeSession_MutationScope_WillCoalesceNotificationsUntilDisposed()
    {
    }
}
