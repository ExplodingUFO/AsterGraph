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

public sealed class GraphEditorDropNodeOnEdgeTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.edge-drop.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.edge-drop.target");
    private static readonly NodeDefinitionId TransformDefinitionId = new("tests.edge-drop.transform");
    private static readonly NodeDefinitionId IncompatibleDefinitionId = new("tests.edge-drop.incompatible");

    [Fact]
    public void Commands_InsertCompatibleNodeIntoConnection()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var inserted = session.Commands.TryInsertNodeIntoConnection(
            "connection-001",
            TransformDefinitionId,
            "in",
            GraphConnectionTargetKind.Port,
            "out",
            new GraphPoint(280, 120));

        Assert.True(inserted);
        Assert.Contains("nodes.insert-into-connection", commandIds);
        var document = session.Queries.CreateDocumentSnapshot();
        var transform = Assert.Single(document.Nodes, node => node.DefinitionId == TransformDefinitionId);
        Assert.True(transform.Position.X > 0);
        Assert.True(transform.Position.Y > 0);
        Assert.DoesNotContain(document.Connections, connection => connection.Id == "connection-001");
        Assert.Contains(document.Connections, connection =>
            connection.SourceNodeId == "source-001"
            && connection.SourcePortId == "out"
            && connection.TargetNodeId == transform.Id
            && connection.TargetPortId == "in");
        Assert.Contains(document.Connections, connection =>
            connection.SourceNodeId == transform.Id
            && connection.SourcePortId == "out"
            && connection.TargetNodeId == "target-001"
            && connection.TargetPortId == "in");
    }

    [Fact]
    public void Commands_ValidateBothSplitHalvesBeforeMutatingGraph()
    {
        var session = CreateSession();

        var inserted = session.Commands.TryInsertNodeIntoConnection(
            "connection-001",
            IncompatibleDefinitionId,
            "in",
            GraphConnectionTargetKind.Port,
            "out",
            new GraphPoint(280, 120));

        Assert.False(inserted);
        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(2, document.Nodes.Count);
        var connection = Assert.Single(document.Connections);
        Assert.Equal("connection-001", connection.Id);
        Assert.Equal("source-001", connection.SourceNodeId);
        Assert.Equal("target-001", connection.TargetNodeId);
    }

    [Fact]
    public void Commands_InsertNodeIntoConnection_IsOneUndoableTransaction()
    {
        var session = CreateSession();

        Assert.True(session.Commands.TryInsertNodeIntoConnection(
            "connection-001",
            TransformDefinitionId,
            "in",
            GraphConnectionTargetKind.Port,
            "out",
            new GraphPoint(280, 120)));

        session.Commands.Undo();

        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(2, document.Nodes.Count);
        var connection = Assert.Single(document.Connections);
        Assert.Equal("connection-001", connection.Id);
        Assert.Equal("source-001", connection.SourceNodeId);
        Assert.Equal("target-001", connection.TargetNodeId);
    }

    [Fact]
    public void Commands_InsertNodeIntoConnection_MigratesNoteAndResetsRoute()
    {
        var session = CreateSession();

        Assert.True(session.Commands.TryInsertNodeIntoConnection(
            "connection-001",
            TransformDefinitionId,
            "in",
            GraphConnectionTargetKind.Port,
            "out",
            new GraphPoint(280, 120)));

        var document = session.Queries.CreateDocumentSnapshot();
        var transform = Assert.Single(document.Nodes, node => node.DefinitionId == TransformDefinitionId);
        var upstream = Assert.Single(document.Connections, connection => connection.TargetNodeId == transform.Id);
        var downstream = Assert.Single(document.Connections, connection => connection.SourceNodeId == transform.Id);
        Assert.Equal("Original edge note", upstream.Presentation?.NoteText);
        Assert.Null(upstream.Presentation?.Route);
        Assert.Null(downstream.Presentation);
    }

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Drop Node On Edge",
                "Drop compatible node on edge proof.",
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
                    new GraphNode(
                        "target-001",
                        "Target",
                        "Tests",
                        "Target",
                        "Target node.",
                        new GraphPoint(520, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        [],
                        "#6AD5C4",
                        TargetDefinitionId),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "source-001",
                        "out",
                        "target-001",
                        "in",
                        "Source to target",
                        "#6AD5C4",
                        Presentation: new GraphEdgePresentation(
                            "Original edge note",
                            new GraphConnectionRoute([new GraphPoint(280, 160)]))),
                ]),
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
            TargetDefinitionId,
            "Target",
            "Tests",
            "Target",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));
        catalog.RegisterDefinition(new NodeDefinition(
            TransformDefinitionId,
            "Transform",
            "Tests",
            "Transform",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            IncompatibleDefinitionId,
            "Incompatible",
            "Tests",
            "Incompatible",
            [new PortDefinition("in", "In", new PortTypeId("number"), "#F3B36B")],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        return catalog;
    }
}
