using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorNodeSurfaceContractsTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.node-surface.contracts");
    private const string NodeId = "tests.node-surface.node-001";
    private const string SiblingNodeId = "tests.node-surface.node-002";

    [Fact]
    public void RuntimeContracts_ExposeNodeSurfaceQueriesAndCommands()
    {
        var queriesType = typeof(IGraphEditorQueries);
        var commandsType = typeof(IGraphEditorCommands);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodeSurfaceSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorNodeSurfaceSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodeSurfaceSnapshots))!.ReturnType);

        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.NodeId)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.Size)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.ExpansionState)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.GroupId)));

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeWidth), typeof(string), typeof(double), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeWidth), [typeof(string), typeof(double), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeExpansionState), typeof(string), typeof(GraphNodeExpansionState));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeExpansionState), [typeof(string), typeof(GraphNodeExpansionState)])!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodeGroups));
        Assert.Equal(
            typeof(IReadOnlyList<GraphNodeGroup>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodeGroups))!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryCreateNodeGroupFromSelection), typeof(string));
        Assert.Equal(
            typeof(string),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryCreateNodeGroupFromSelection), [typeof(string)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeGroupCollapsed), typeof(string), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeGroupCollapsed), [typeof(string), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeGroupPosition), typeof(string), typeof(GraphPoint), typeof(bool), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeGroupPosition), [typeof(string), typeof(GraphPoint), typeof(bool), typeof(bool)])!.ReturnType);
    }

    [Fact]
    public void SessionCommands_TrySetNodeWidth_PersistsUndoableSurfaceMutation()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var changed = session.Commands.TrySetNodeWidth(NodeId, 360d, updateStatus: false);

        Assert.True(changed);
        Assert.Contains("nodes.resize-width", commandIds);

        var node = Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, node => node.Id == NodeId);
        Assert.Equal(360d, node.Size.Width);

        var surface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == NodeId);
        Assert.Equal(360d, surface.Size.Width);

        session.Commands.Undo();
        Assert.Equal(240d, Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, node => node.Id == NodeId).Size.Width);

        session.Commands.Redo();
        Assert.Equal(360d, Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, node => node.Id == NodeId).Size.Width);
    }

    [Fact]
    public void SessionCommands_TrySetNodeExpansionState_PersistsAcrossSerialization()
    {
        var session = CreateSession();

        var changed = session.Commands.TrySetNodeExpansionState(NodeId, GraphNodeExpansionState.Expanded);

        Assert.True(changed);

        var surface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == NodeId);
        Assert.Equal(GraphNodeExpansionState.Expanded, surface.ExpansionState);

        var document = session.Queries.CreateDocumentSnapshot();
        var json = GraphDocumentSerializer.Serialize(document);
        var restored = GraphDocumentSerializer.Deserialize(json);
        var restoredNode = Assert.Single(restored.Nodes, node => node.Id == NodeId);

        Assert.NotNull(restoredNode.Surface);
        Assert.Equal(GraphNodeExpansionState.Expanded, restoredNode.Surface!.ExpansionState);
    }

    [Fact]
    public void SessionCommands_NodeGroups_PersistMembershipCollapseAndMovement()
    {
        var session = CreateSession();

        session.Commands.SetSelection([NodeId, SiblingNodeId], SiblingNodeId, updateStatus: false);
        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Surface Cluster");

        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var createdGroup = Assert.Single(session.Queries.GetNodeGroups());
        Assert.Equal(groupId, createdGroup.Id);
        Assert.Equal("Surface Cluster", createdGroup.Title);
        Assert.Equal(
            [NodeId, SiblingNodeId],
            createdGroup.NodeIds.OrderBy(id => id, StringComparer.Ordinal));

        var initialPositions = session.Queries.CreateDocumentSnapshot().Nodes
            .ToDictionary(node => node.Id, node => node.Position, StringComparer.Ordinal);

        Assert.True(session.Commands.TrySetNodeGroupCollapsed(groupId!, isCollapsed: true));
        Assert.True(session.Commands.TrySetNodeGroupPosition(groupId, new GraphPoint(200, 120), moveMemberNodes: true, updateStatus: false));

        var movedGroup = Assert.Single(session.Queries.GetNodeGroups());
        Assert.True(movedGroup.IsCollapsed);
        Assert.Equal(new GraphPoint(200, 120), movedGroup.Position);

        var movedDocument = session.Queries.CreateDocumentSnapshot();
        foreach (var node in movedDocument.Nodes)
        {
            Assert.Equal(groupId, node.Surface?.GroupId);
            Assert.NotEqual(initialPositions[node.Id], node.Position);
        }

        var json = GraphDocumentSerializer.Serialize(movedDocument);
        var restored = GraphDocumentSerializer.Deserialize(json);
        var restoredGroup = Assert.Single(restored.Groups!);
        Assert.Equal(groupId, restoredGroup.Id);
        Assert.True(restoredGroup.IsCollapsed);
        Assert.Equal(new GraphPoint(200, 120), restoredGroup.Position);
        Assert.All(restored.Nodes, node => Assert.Equal(groupId, node.Surface?.GroupId));
    }

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Node Surface Graph",
                "Covers runtime node surface contracts.",
                [
                    new GraphNode(
                        NodeId,
                        "Node Surface",
                        "Tests",
                        "Contracts",
                        "Covers width and expanded-state mutations.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        DefinitionId,
                        []),
                    new GraphNode(
                        SiblingNodeId,
                        "Node Surface Sibling",
                        "Tests",
                        "Contracts",
                        "Covers grouped node-surface mutations.",
                        new GraphPoint(420, 160),
                        new GraphSize(220, 150),
                        [],
                        [],
                        "#F3B36B",
                        DefinitionId,
                        []),
                ],
                []),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                DefinitionId,
                "Node Surface",
                "Tests",
                "Contracts",
                [],
                [],
                description: "Node surface regression definition.",
                defaultWidth: 240d,
                defaultHeight: 160d));
        return catalog;
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(methodName, parameterTypes);
        Assert.NotNull(method);
    }
}
