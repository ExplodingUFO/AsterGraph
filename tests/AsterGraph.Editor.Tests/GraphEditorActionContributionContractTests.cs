using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorActionContributionContractTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.action-contract.node");
    private const string SourceNodeId = "tests.action-contract.source";
    private const string TargetNodeId = "tests.action-contract.target";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const string ConnectionId = "tests.action-contract.connection";

    [Fact]
    public void CommandRegistry_NodeAndEdgeActionsExposeDescriptorPlacementSurfaces()
    {
        var session = CreateSession();
        var registry = session.Queries.GetCommandRegistry()
            .ToDictionary(entry => entry.CommandId, StringComparer.Ordinal);

        AssertPlacement(registry["nodes.inspect"], GraphEditorCommandSurfaceKind.CommandRoute, "runtime.session.commands", "nodes.inspect", null);
        AssertPlacement(registry["nodes.inspect"], GraphEditorCommandSurfaceKind.ContextMenu, "context-menu.node", "node-inspect", "node");
        AssertPlacement(registry["nodes.inspect"], GraphEditorCommandSurfaceKind.Tool, "tool.node", "node-inspect", "node");
        AssertPlacement(registry["nodes.delete-by-id"], GraphEditorCommandSurfaceKind.ContextMenu, "context-menu.node", "node-delete", "node");
        AssertPlacement(registry["nodes.delete-by-id"], GraphEditorCommandSurfaceKind.Tool, "tool.node", "node-delete", "node");

        AssertPlacement(registry["connections.reconnect"], GraphEditorCommandSurfaceKind.CommandRoute, "runtime.session.commands", "connections.reconnect", null);
        AssertPlacement(registry["connections.reconnect"], GraphEditorCommandSurfaceKind.ContextMenu, "context-menu.connection", "connection-reconnect", "connection");
        AssertPlacement(registry["connections.reconnect"], GraphEditorCommandSurfaceKind.Tool, "tool.connection", "connection-reconnect", "connection");
        AssertPlacement(registry["connections.disconnect"], GraphEditorCommandSurfaceKind.ContextMenu, "context-menu.connection", "connection-disconnect", "connection");
        AssertPlacement(registry["connections.disconnect"], GraphEditorCommandSurfaceKind.Tool, "tool.connection", "connection-disconnect", "connection");
    }

    [Fact]
    public void ContextMenus_NodeAndEdgeActionsUseCanonicalMenuDescriptors()
    {
        var session = CreateSession();

        var nodeMenu = Flatten(session.Queries.BuildContextMenuDescriptors(
                new ContextMenuContext(
                    ContextMenuTargetKind.Node,
                    new GraphPoint(120, 160),
                    clickedNodeId: SourceNodeId)))
            .ToDictionary(item => item.Id, StringComparer.Ordinal);
        var edgeMenu = Flatten(session.Queries.BuildContextMenuDescriptors(
                new ContextMenuContext(
                    ContextMenuTargetKind.Connection,
                    new GraphPoint(260, 180),
                    clickedConnectionId: ConnectionId)))
            .ToDictionary(item => item.Id, StringComparer.Ordinal);

        AssertCommand(nodeMenu["node-inspect"], "nodes.inspect", ("nodeId", SourceNodeId));
        AssertCommand(nodeMenu["node-delete"], "nodes.delete-by-id", ("nodeId", SourceNodeId));
        AssertCommand(
            nodeMenu[$"compatible-port-{TargetNodeId}-{TargetPortId}"],
            "connections.connect",
            ("sourceNodeId", SourceNodeId),
            ("sourcePortId", SourcePortId),
            ("targetNodeId", TargetNodeId),
            ("targetPortId", TargetPortId));

        AssertCommand(edgeMenu["connection-reconnect"], "connections.reconnect", ("connectionId", ConnectionId));
        AssertCommand(edgeMenu["connection-disconnect"], "connections.disconnect", ("connectionId", ConnectionId));
        AssertCommand(edgeMenu["connection-clear-note"], "connections.note.set", ("connectionId", ConnectionId), ("text", string.Empty));
    }

    [Fact]
    public void HostedActions_NodeAndEdgeActionsProjectRuntimeDescriptorsToCanonicalCommandRoute()
    {
        var session = CreateSession();
        var executedCommandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => executedCommandIds.Add(args.CommandId);

        var nodeActions = AsterGraphAuthoringToolActionFactory.CreateNodeActions(
            session,
            SourceNodeId,
            [SourceNodeId],
            SourceNodeId);
        var edgeActions = AsterGraphAuthoringToolActionFactory.CreateConnectionActions(
            session,
            ConnectionId,
            [SourceNodeId],
            SourceNodeId);

        var duplicateNode = Assert.Single(nodeActions, action => action.Id == "node-duplicate");
        var disconnectEdge = Assert.Single(edgeActions, action => action.Id == "connection-disconnect");

        Assert.Equal("nodes.duplicate", duplicateNode.CommandId);
        Assert.Equal("Duplicate Node", duplicateNode.Title);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, duplicateNode.CommandSource);
        Assert.Equal("connections.disconnect", disconnectEdge.CommandId);
        Assert.Equal("Disconnect Connection", disconnectEdge.Title);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, disconnectEdge.CommandSource);

        Assert.True(duplicateNode.TryExecute());
        Assert.True(disconnectEdge.TryExecute());
        Assert.Contains(executedCommandIds, commandId => commandId == "nodes.duplicate");
        Assert.Contains(executedCommandIds, commandId => commandId == "connections.disconnect");
        Assert.DoesNotContain(
            session.Queries.CreateDocumentSnapshot().Connections,
            connection => connection.Id == ConnectionId);
    }

    private static void AssertPlacement(
        GraphEditorCommandRegistryEntrySnapshot entry,
        GraphEditorCommandSurfaceKind surfaceKind,
        string surfaceId,
        string placementId,
        string? contextKind)
        => Assert.Contains(
            entry.Placements,
            placement =>
                placement.SurfaceKind == surfaceKind
                && placement.SurfaceId == surfaceId
                && placement.PlacementId == placementId
                && placement.ContextKind == contextKind);

    private static void AssertCommand(
        GraphEditorMenuItemDescriptorSnapshot descriptor,
        string commandId,
        params (string Name, string Value)[] arguments)
    {
        Assert.NotNull(descriptor.Command);
        Assert.Equal(commandId, descriptor.Command!.CommandId);
        foreach (var (name, value) in arguments)
        {
            Assert.Contains(
                descriptor.Command.Arguments,
                argument => argument.Name == name && argument.Value == value);
        }
    }

    private static IEnumerable<GraphEditorMenuItemDescriptorSnapshot> Flatten(
        IEnumerable<GraphEditorMenuItemDescriptorSnapshot> items)
    {
        foreach (var item in items)
        {
            yield return item;
            foreach (var child in Flatten(item.Children))
            {
                yield return child;
            }
        }
    }

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateDocument()
        => new(
            "Action Contribution Graph",
            "Node and edge action contribution contract coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Action Contract",
                    "Source node for action contribution tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Action Contract",
                    "Target node for action contribution tests.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefinitionId),
            ],
            [
                new GraphConnection(
                    ConnectionId,
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "Source To Target",
                    "#6AD5C4",
                    Presentation: new GraphEdgePresentation(NoteText: "Runtime note")),
            ]);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Action Contract Node",
            "Tests",
            "Action Contract",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }
}
