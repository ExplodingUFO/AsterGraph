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

public sealed class GraphEditorDeleteReconnectDetachTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.reconnect.source");
    private static readonly NodeDefinitionId TransformDefinitionId = new("tests.reconnect.transform");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.reconnect.target");
    private static readonly NodeDefinitionId IncompatibleTargetDefinitionId = new("tests.reconnect.incompatible-target");

    [Fact]
    public void Commands_DeleteSelectionAndReconnect_RemovesMiddleNodeAndRestoresWire()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["transform-001"], "transform-001");

        var deleted = session.Commands.TryDeleteSelectionAndReconnect();

        Assert.True(deleted);
        var document = session.Queries.CreateDocumentSnapshot();
        Assert.DoesNotContain(document.Nodes, node => node.Id == "transform-001");
        var connection = Assert.Single(document.Connections);
        Assert.Equal("source-001", connection.SourceNodeId);
        Assert.Equal("out", connection.SourcePortId);
        Assert.Equal("target-001", connection.TargetNodeId);
        Assert.Equal("in", connection.TargetPortId);
    }

    [Fact]
    public void Commands_DetachSelectionFromConnections_KeepsMiddleNodeAndRestoresWire()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["transform-001"], "transform-001");

        var detached = session.Commands.TryDetachSelectionFromConnections();

        Assert.True(detached);
        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Contains(document.Nodes, node => node.Id == "transform-001");
        var connection = Assert.Single(document.Connections);
        Assert.Equal("source-001", connection.SourceNodeId);
        Assert.Equal("target-001", connection.TargetNodeId);
        Assert.Equal(["transform-001"], session.Queries.GetSelectionSnapshot().SelectedNodeIds);
    }

    [Fact]
    public void Commands_DeleteSelectionAndReconnect_ReportsConflictWithoutMutation()
    {
        var session = CreateSession(targetTypeId: new PortTypeId("number"), targetDefinitionId: IncompatibleTargetDefinitionId);
        session.Commands.SetSelection(["transform-001"], "transform-001");

        var deleted = session.Commands.TryDeleteSelectionAndReconnect();

        Assert.False(deleted);
        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(3, document.Nodes.Count);
        Assert.Equal(2, document.Connections.Count);
        Assert.Contains("Reconnect conflict", session.Diagnostics.CaptureInspectionSnapshot().Status.Message);
    }

    [Fact]
    public void Commands_ReconnectWorkflows_AreUndoableAsSingleActions()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["transform-001"], "transform-001");

        Assert.True(session.Commands.TryDeleteSelectionAndReconnect());
        session.Commands.Undo();

        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(3, document.Nodes.Count);
        Assert.Contains(document.Connections, connection => connection.Id == "connection-001");
        Assert.Contains(document.Connections, connection => connection.Id == "connection-002");
        Assert.DoesNotContain(document.Connections, connection =>
            connection.SourceNodeId == "source-001"
            && connection.TargetNodeId == "target-001");
    }

    private static IGraphEditorSession CreateSession(
        PortTypeId? targetTypeId = null,
        NodeDefinitionId? targetDefinitionId = null)
    {
        var resolvedTargetTypeId = targetTypeId ?? new PortTypeId("flow");
        var resolvedTargetDefinitionId = targetDefinitionId ?? TargetDefinitionId;
        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Delete Reconnect",
                "Delete reconnect proof.",
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
                        new GraphPoint(520, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, resolvedTargetTypeId.Value, "#6AD5C4", resolvedTargetTypeId)],
                        [],
                        "#6AD5C4",
                        resolvedTargetDefinitionId),
                ],
                [
                    new GraphConnection("connection-001", "source-001", "out", "transform-001", "in", "Source to transform", "#6AD5C4"),
                    new GraphConnection("connection-002", "transform-001", "out", "target-001", "in", "Transform to target", "#6AD5C4"),
                ]),
            NodeCatalog = CreateCatalog(resolvedTargetTypeId, resolvedTargetDefinitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }

    private static INodeCatalog CreateCatalog(PortTypeId targetTypeId, NodeDefinitionId targetDefinitionId)
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
            targetDefinitionId,
            "Target",
            "Tests",
            "Target",
            [new PortDefinition("in", "In", targetTypeId, "#6AD5C4")],
            []));
        return catalog;
    }
}
