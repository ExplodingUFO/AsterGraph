using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorToolProviderContractTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.tool-provider.node");
    private const string SourceNodeId = "tests.tool-provider.source-001";
    private const string TargetNodeId = "tests.tool-provider.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void CreateSession_WithToolProvider_ExposesStockAndHostToolDescriptorsAcrossContexts()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions() with
        {
            ToolProvider = new TestToolProvider(),
        });

        var selectionTools = session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForSelection([SourceNodeId, TargetNodeId], SourceNodeId));
        var hostSelectionTool = Assert.Single(selectionTools, tool => tool.Id == "tests.tool-provider.selection.group");

        Assert.Contains(selectionTools, tool => tool.Id == "selection-create-group");
        Assert.Contains(selectionTools, tool => tool.Id == "selection-wrap-composite");
        Assert.Equal("Host Group Tool", hostSelectionTool.Title);
        Assert.Equal("host.tools", hostSelectionTool.Group);
        Assert.Equal("groups.create", hostSelectionTool.Invocation.CommandId);
        Assert.Equal(GraphEditorCommandSourceKind.Host, hostSelectionTool.Source);

        var nodeTools = session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForNode(SourceNodeId, [SourceNodeId], SourceNodeId));
        var hostNodeTool = Assert.Single(nodeTools, tool => tool.Id == "tests.tool-provider.node.expand");

        Assert.Contains(nodeTools, tool => tool.Id == "node-inspect");
        Assert.Contains(nodeTools, tool => tool.Id == "node-center");
        Assert.Contains(nodeTools, tool => tool.Id == "node-toggle-surface-expansion");
        Assert.Contains(nodeTools, tool => tool.Id == "node-delete");
        Assert.Contains(nodeTools, tool => tool.Id == "node-duplicate");
        Assert.Contains(nodeTools, tool => tool.Id == "node-disconnect-incoming");
        Assert.Contains(nodeTools, tool => tool.Id == "node-disconnect-outgoing");
        Assert.Contains(nodeTools, tool => tool.Id == "node-disconnect-all");
        Assert.Equal("Host Expand Tool", hostNodeTool.Title);
        Assert.Equal("nodes.surface.expand", hostNodeTool.Invocation.CommandId);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;
        Assert.True(session.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                "connections.note.set",
                [
                    new GraphEditorCommandArgumentSnapshot("connectionId", connectionId),
                    new GraphEditorCommandArgumentSnapshot("text", "Tool provider note"),
                    new GraphEditorCommandArgumentSnapshot("updateStatus", "false"),
                ])));

        var connectionTools = session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForConnection(connectionId, [SourceNodeId], SourceNodeId));
        var hostConnectionTool = Assert.Single(connectionTools, tool => tool.Id == "tests.tool-provider.connection.disconnect");

        Assert.Contains(connectionTools, tool => tool.Id == "connection-reconnect");
        Assert.Contains(connectionTools, tool => tool.Id == "connection-disconnect");
        Assert.Contains(connectionTools, tool => tool.Id == "connection-clear-note");
        Assert.Equal("Host Disconnect Tool", hostConnectionTool.Title);
        Assert.Equal("connections.disconnect", hostConnectionTool.Invocation.CommandId);

        var features = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.True(features["query.tool-descriptors"].IsAvailable);
        Assert.True(features["integration.tool-provider"].IsAvailable);
    }

    [Fact]
    public void HostedActionFactory_CreateToolActions_ExecutesInvocationBackedTools()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions() with
        {
            ToolProvider = new TestToolProvider(),
        });

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;

        var tools = session.Queries.GetToolDescriptors(
            GraphEditorToolContextSnapshot.ForConnection(connectionId, [SourceNodeId], SourceNodeId));
        var actions = AsterGraphHostedActionFactory.CreateToolActions(session, tools);
        var disconnect = Assert.Single(actions, action => action.Id == "tests.tool-provider.connection.disconnect");

        Assert.True(disconnect.CanExecute);
        Assert.True(disconnect.TryExecute());
        Assert.Empty(session.Queries.CreateDocumentSnapshot().Connections);
    }

    private static AsterGraphEditorOptions CreateOptions()
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };

    private static GraphDocument CreateDocument()
        => new(
            "Tool Provider Graph",
            "Tool provider contract coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Tool Provider",
                    "Source node for contextual tool tests.",
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
                    "Tool Provider",
                    "Target node for contextual tool tests.",
                    new GraphPoint(520, 180),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefinitionId),
            ],
            []);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Tool Provider Node",
            "Tests",
            "Tool Provider",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private sealed class TestToolProvider : IGraphEditorToolProvider
    {
        public IReadOnlyList<GraphEditorToolDescriptorSnapshot> GetToolDescriptors(GraphEditorToolProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return context.Context.Kind switch
            {
                GraphEditorToolContextKind.Selection => CreateSelectionTools(),
                GraphEditorToolContextKind.Node => CreateNodeTools(context.Context.NodeId!),
                GraphEditorToolContextKind.Connection => CreateConnectionTools(context.Context.ConnectionId!),
                _ => [],
            };
        }

        private static IReadOnlyList<GraphEditorToolDescriptorSnapshot> CreateSelectionTools()
            => [
                new GraphEditorToolDescriptorSnapshot(
                    "tests.tool-provider.selection.group",
                    GraphEditorToolContextKind.Selection,
                    new GraphEditorCommandDescriptorSnapshot(
                        "tests.tool-provider.selection.group",
                        "Host Group Tool",
                        "host.tools",
                        "group-create",
                        null,
                        GraphEditorCommandSourceKind.Host,
                        isEnabled: true),
                    new GraphEditorCommandInvocationSnapshot(
                        "groups.create",
                        [new GraphEditorCommandArgumentSnapshot("title", "Host Group")]))
            ];

        private static IReadOnlyList<GraphEditorToolDescriptorSnapshot> CreateNodeTools(string nodeId)
            => [
                new GraphEditorToolDescriptorSnapshot(
                    "tests.tool-provider.node.expand",
                    GraphEditorToolContextKind.Node,
                    new GraphEditorCommandDescriptorSnapshot(
                        "tests.tool-provider.node.expand",
                        "Host Expand Tool",
                        "host.tools",
                        "expand",
                        null,
                        GraphEditorCommandSourceKind.Host,
                        isEnabled: true),
                    new GraphEditorCommandInvocationSnapshot(
                        "nodes.surface.expand",
                        [
                            new GraphEditorCommandArgumentSnapshot("nodeId", nodeId),
                            new GraphEditorCommandArgumentSnapshot("expansionState", GraphNodeExpansionState.Expanded.ToString()),
                        ]))
            ];

        private static IReadOnlyList<GraphEditorToolDescriptorSnapshot> CreateConnectionTools(string connectionId)
            => [
                new GraphEditorToolDescriptorSnapshot(
                    "tests.tool-provider.connection.disconnect",
                    GraphEditorToolContextKind.Connection,
                    new GraphEditorCommandDescriptorSnapshot(
                        "tests.tool-provider.connection.disconnect",
                        "Host Disconnect Tool",
                        "host.tools",
                        "disconnect",
                        null,
                        GraphEditorCommandSourceKind.Host,
                        isEnabled: true),
                    new GraphEditorCommandInvocationSnapshot(
                        "connections.disconnect",
                        [new GraphEditorCommandArgumentSnapshot("connectionId", connectionId)]))
            ];
    }
}
