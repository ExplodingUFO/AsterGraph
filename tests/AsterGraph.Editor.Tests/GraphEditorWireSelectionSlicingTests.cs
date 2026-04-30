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

public sealed class GraphEditorWireSelectionSlicingTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.wire.source");
    private static readonly NodeDefinitionId TransformDefinitionId = new("tests.wire.transform");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.wire.target");

    [Fact]
    public void Commands_SetConnectionSelection_StoresMultipleConnectionIdsAndClearsNodeSelection()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001"], "source-001", updateStatus: false);

        session.Commands.SetConnectionSelection(["connection-001", "missing", "connection-002"], "connection-001", updateStatus: false);

        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Empty(selection.SelectedNodeIds);
        Assert.Null(selection.PrimarySelectedNodeId);
        Assert.Equal(["connection-001", "connection-002"], selection.SelectedConnectionIds);
        Assert.Equal("connection-001", selection.PrimarySelectedConnectionId);
    }

    [Fact]
    public void Commands_TryExecuteCommand_RoutesCanonicalConnectionSelection()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001"], "source-001", updateStatus: false);

        var routed = session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
            "selection.connections.set",
            [
                new GraphEditorCommandArgumentSnapshot("connectionId", "connection-002"),
                new GraphEditorCommandArgumentSnapshot("primaryConnectionId", "connection-002"),
                new GraphEditorCommandArgumentSnapshot("updateStatus", bool.FalseString),
            ]));

        var selection = session.Queries.GetSelectionSnapshot();
        Assert.True(routed);
        Assert.Empty(selection.SelectedNodeIds);
        Assert.Equal(["connection-002"], selection.SelectedConnectionIds);
        Assert.Equal("connection-002", selection.PrimarySelectedConnectionId);
    }

    [Fact]
    public void Commands_DeleteSelectedConnections_RemovesSelectedWiresAndClearsConnectionSelection()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);
        session.Commands.SetConnectionSelection(["connection-001", "connection-002"], "connection-002", updateStatus: false);

        var deleted = session.Commands.TryDeleteSelectedConnections();

        Assert.True(deleted);
        Assert.Contains("connections.delete-selected", commandIds);
        Assert.Empty(session.Queries.CreateDocumentSnapshot().Connections);
        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Empty(selection.SelectedConnectionIds);
        Assert.Null(selection.PrimarySelectedConnectionId);
    }

    [Fact]
    public void Commands_SliceConnections_RemovesIntersectedWires()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var sliced = session.Commands.TrySliceConnections(new GraphPoint(580, 40), new GraphPoint(580, 320));

        Assert.True(sliced);
        Assert.Contains("connections.slice", commandIds);
        var document = session.Queries.CreateDocumentSnapshot();
        var connection = Assert.Single(document.Connections);
        Assert.Equal("connection-001", connection.Id);
    }

    [Fact]
    public void Queries_GetSelectedNodeConnectionIds_ReturnsEdgesBetweenSelectedNodes()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001", "transform-001"], "source-001", updateStatus: false);

        var connectionIds = session.Queries.GetSelectedNodeConnectionIds();

        Assert.Equal(["connection-001"], connectionIds);
    }

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Wire Selection",
                "Wire selection and slicing proof.",
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
                        "transform-001",
                        "Transform",
                        "Tests",
                        "Transform",
                        "Transform node.",
                        new GraphPoint(320, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        "#6AD5C4",
                        TransformDefinitionId),
                    new GraphNode(
                        "target-001",
                        "Target",
                        "Tests",
                        "Target",
                        "Target node.",
                        new GraphPoint(620, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        [],
                        "#6AD5C4",
                        TargetDefinitionId),
                ],
                [
                    new GraphConnection("connection-001", "source-001", "out", "transform-001", "in", "Source to transform", "#6AD5C4"),
                    new GraphConnection("connection-002", "transform-001", "out", "target-001", "in", "Transform to target", "#6AD5C4"),
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
            TransformDefinitionId,
            "Transform",
            "Tests",
            "Transform",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Target",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));
        return catalog;
    }
}
