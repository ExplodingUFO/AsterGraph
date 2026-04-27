using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorQuickAddConnectedNodeTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.quick-add.source");
    private static readonly NodeDefinitionId FlowTargetDefinitionId = new("tests.quick-add.flow-target");
    private static readonly NodeDefinitionId NumberTargetDefinitionId = new("tests.quick-add.number-target");

    [Fact]
    public void Queries_ReturnCompatibleNodeDefinitionsForPendingConnection()
    {
        var session = CreateSession();
        session.Commands.StartConnection("source-001", "out");

        var search = session.Queries.GetCompatibleNodeDefinitionsForPendingConnection();

        Assert.True(search.HasPendingConnection);
        Assert.Equal("source-001", search.SourceNodeId);
        Assert.Equal("out", search.SourcePortId);
        Assert.Null(search.EmptyReason);
        var target = Assert.Single(search.Results);
        Assert.Equal(FlowTargetDefinitionId, target.DefinitionId);
        Assert.Equal("in", target.TargetId);
        Assert.Equal(GraphConnectionTargetKind.Port, target.TargetKind);
        Assert.True(target.Compatibility.IsCompatible);
        Assert.DoesNotContain(search.Results, result => result.DefinitionId == NumberTargetDefinitionId);
    }

    [Fact]
    public void Queries_ReturnEmptyReasonWhenThereIsNoPendingConnection()
    {
        var session = CreateSession();

        var search = session.Queries.GetCompatibleNodeDefinitionsForPendingConnection();

        Assert.False(search.HasPendingConnection);
        Assert.Empty(search.Results);
        Assert.Equal("No pending connection.", search.EmptyReason);
    }

    [Fact]
    public void Commands_CreateCompatibleNodeAndCompletePendingConnection()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);
        session.Commands.StartConnection("source-001", "out");

        var created = session.Commands.TryCreateConnectedNodeFromPendingConnection(
            FlowTargetDefinitionId,
            "in",
            GraphConnectionTargetKind.Port,
            new GraphPoint(420, 120));

        Assert.True(created);
        Assert.False(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
        Assert.Contains("nodes.quick-add-connected", commandIds);
        var document = session.Queries.CreateDocumentSnapshot();
        var createdNode = Assert.Single(document.Nodes, node => node.DefinitionId == FlowTargetDefinitionId);
        Assert.True(createdNode.Position.X > 0);
        Assert.True(createdNode.Position.Y > 0);
        var connection = Assert.Single(document.Connections);
        Assert.Equal("source-001", connection.SourceNodeId);
        Assert.Equal("out", connection.SourcePortId);
        Assert.Equal(createdNode.Id, connection.TargetNodeId);
        Assert.Equal("in", connection.TargetPortId);
    }

    [Fact]
    public void Commands_RejectIncompatibleNodeWithoutMutatingGraph()
    {
        var session = CreateSession();
        session.Commands.StartConnection("source-001", "out");

        var created = session.Commands.TryCreateConnectedNodeFromPendingConnection(
            NumberTargetDefinitionId,
            "number",
            GraphConnectionTargetKind.Port,
            new GraphPoint(420, 120));

        Assert.False(created);
        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Single(document.Nodes);
        Assert.Empty(document.Connections);
        Assert.True(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
    }

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Quick Add",
                "Quick add connected node proof.",
                [
                    new GraphNode(
                        "source-001",
                        "Source",
                        "Tests",
                        "Source",
                        "Source node.",
                        new GraphPoint(120, 120),
                        new GraphSize(220, 120),
                        [],
                        [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        "#6AD5C4",
                        SourceDefinitionId),
                ],
                []),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Source",
            [],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            FlowTargetDefinitionId,
            "Flow Target",
            "Tests",
            "Target",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));
        catalog.RegisterDefinition(new NodeDefinition(
            NumberTargetDefinitionId,
            "Number Target",
            "Tests",
            "Target",
            [new PortDefinition("number", "Number", new PortTypeId("number"), "#F3B36B")],
            []));
        return catalog;
    }
}
