using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeCanvasContextMenuContextFactoryTests
{
    [Fact]
    public void CreateCanvasContext_UsesSelectionTargetWhenRequested()
    {
        var snapshot = CreateSnapshot();

        var context = NodeCanvasContextMenuContextFactory.CreateCanvasContext(
            snapshot,
            new GraphPoint(10, 20),
            useSelectionTools: true);

        Assert.Equal(ContextMenuTargetKind.Selection, context.TargetKind);
        Assert.Equal("node-001", context.SelectedNodeId);
        Assert.Equal(["node-001", "node-002"], context.SelectedNodeIds);
    }

    [Fact]
    public void CreateNodeContext_SetsClickedNodeAndPreservesSelection()
    {
        var snapshot = CreateSnapshot();

        var context = NodeCanvasContextMenuContextFactory.CreateNodeContext(
            snapshot,
            new GraphPoint(30, 40),
            "node-002",
            useSelectionTools: false);

        Assert.Equal(ContextMenuTargetKind.Node, context.TargetKind);
        Assert.Equal("node-002", context.ClickedNodeId);
        Assert.Equal("node-001", context.SelectedNodeId);
        Assert.Equal(["node-001", "node-002"], context.SelectedNodeIds);
    }

    [Fact]
    public void CreatePortContext_SetsClickedPortIdentifiers()
    {
        var snapshot = CreateSnapshot();

        var context = NodeCanvasContextMenuContextFactory.CreatePortContext(
            snapshot,
            new GraphPoint(50, 60),
            "node-003",
            "out-color");

        Assert.Equal(ContextMenuTargetKind.Port, context.TargetKind);
        Assert.Equal("node-003", context.ClickedNodeId);
        Assert.Equal("node-003", context.ClickedPortNodeId);
        Assert.Equal("out-color", context.ClickedPortId);
    }

    [Fact]
    public void CreateConnectionContext_SetsConnectionSelectionAndClick()
    {
        var snapshot = CreateSnapshot();

        var context = NodeCanvasContextMenuContextFactory.CreateConnectionContext(
            snapshot,
            new GraphPoint(70, 80),
            "connection-001");

        Assert.Equal(ContextMenuTargetKind.Connection, context.TargetKind);
        Assert.Equal("connection-001", context.SelectedConnectionId);
        Assert.Equal(["connection-001"], context.SelectedConnectionIds);
        Assert.Equal("connection-001", context.ClickedConnectionId);
    }

    [Fact]
    public void CreateAllContexts_PreserveHostContext()
    {
        var snapshot = CreateSnapshot();
        var hostContext = new TestGraphHostContext();

        var canvasContext = NodeCanvasContextMenuContextFactory.CreateCanvasContext(
            snapshot,
            new GraphPoint(10, 20),
            useSelectionTools: false,
            hostContext: hostContext);
        var nodeContext = NodeCanvasContextMenuContextFactory.CreateNodeContext(
            snapshot,
            new GraphPoint(20, 30),
            "node-002",
            useSelectionTools: false,
            hostContext: hostContext);
        var portContext = NodeCanvasContextMenuContextFactory.CreatePortContext(
            snapshot,
            new GraphPoint(30, 40),
            "node-002",
            "out-color",
            hostContext: hostContext);
        var connectionContext = NodeCanvasContextMenuContextFactory.CreateConnectionContext(
            snapshot,
            new GraphPoint(40, 50),
            "connection-001",
            hostContext: hostContext);

        Assert.Same(hostContext, canvasContext.HostContext);
        Assert.Same(hostContext, nodeContext.HostContext);
        Assert.Same(hostContext, portContext.HostContext);
        Assert.Same(hostContext, connectionContext.HostContext);
    }

    private static NodeCanvasContextMenuSnapshot CreateSnapshot()
        => new(
            SelectedNodeId: "node-001",
            SelectedNodeIds: ["node-001", "node-002"],
            AvailableNodeDefinitions:
            [
                new NodeDefinition(
                    new NodeDefinitionId("tests.context.menu"),
                    "Sample",
                    "Tests",
                    "Context",
                    [],
                    []),
            ]);

    private sealed class TestGraphHostContext : IGraphHostContext
    {
        public object Owner { get; } = new();

        public object TopLevel { get; } = new();

        public IServiceProvider? Services => null;
    }
}
