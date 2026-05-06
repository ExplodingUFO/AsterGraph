using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSelectionTransformContractsTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.transform.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.transform.target");

    [Fact]
    public void Queries_GetSelectionRectangleSnapshot_ReturnsNodesAndConnectionsInRectangle()
    {
        var session = CreateSession();

        var snapshot = session.Queries.GetSelectionRectangleSnapshot(
            new GraphPoint(100, 100),
            new GraphSize(600, 240));

        Assert.Equal(["source-001", "target-001"], snapshot.NodeIds);
        Assert.Equal(["connection-001"], snapshot.ConnectionIds);
    }

    [Fact]
    public void Queries_GetSelectionRectangleSnapshot_WithEmptyRectangle_ReturnsEmptyLists()
    {
        var session = CreateSession();

        var snapshot = session.Queries.GetSelectionRectangleSnapshot(
            new GraphPoint(0, 0),
            new GraphSize(10, 10));

        Assert.Empty(snapshot.NodeIds);
        Assert.Empty(snapshot.ConnectionIds);
    }

    [Fact]
    public void Queries_GetSelectionRectangleSnapshot_WithPartialOverlap_ReturnsIntersectingNodesOnly()
    {
        var session = CreateSession();

        var snapshot = session.Queries.GetSelectionRectangleSnapshot(
            new GraphPoint(100, 100),
            new GraphSize(200, 200));

        Assert.Equal(["source-001"], snapshot.NodeIds);
        Assert.Empty(snapshot.ConnectionIds);
    }

    [Fact]
    public void Queries_ProjectSelectionTransformBoundsPreviewAndRectangleHits()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001", "target-001"], "source-001", updateStatus: false);
        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Pipeline");

        var snapshot = session.Queries.GetSelectionTransformSnapshot(new GraphEditorSelectionTransformQuery(
            new GraphPoint(100, 100),
            new GraphSize(600, 240),
            new GraphPoint(24, 8),
            ConstrainPreviewToPrimaryAxis: true));

        Assert.True(snapshot.HasSelection);
        Assert.Equal(["source-001", "target-001"], snapshot.SelectedNodeIds);
        Assert.Equal(["connection-001"], snapshot.SelectedConnectionIds);
        Assert.Equal([groupId], snapshot.SelectedGroupIds);
        Assert.Equal(new GraphPoint(120, 120), snapshot.BoundsPosition);
        Assert.Equal(new GraphSize(520, 120), snapshot.BoundsSize);
        Assert.Equal(new GraphPoint(24, 0), snapshot.PreviewDelta);
        Assert.Contains(snapshot.Items, item => item.NodeId == "source-001" && item.PreviewPosition == new GraphPoint(144, 120));
        Assert.Equal(["source-001", "target-001"], snapshot.RectangleNodeIds);
        Assert.Equal(["connection-001"], snapshot.RectangleConnectionIds);
        Assert.Null(snapshot.EmptyReason);
    }

    [Fact]
    public void Commands_MoveSelectionByCommitsThroughUndoableNodePositionMutation()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001", "target-001"], "source-001", updateStatus: false);

        Assert.True(session.Commands.TryMoveSelectionBy(18, 30, constrainToPrimaryAxis: true, updateStatus: false));

        var moved = session.Queries.CreateDocumentSnapshot().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(120, 150), moved["source-001"].Position);
        Assert.Equal(new GraphPoint(420, 150), moved["target-001"].Position);
        Assert.True(session.Queries.GetCapabilitySnapshot().CanUndo);

        session.Commands.Undo();

        var restored = session.Queries.CreateDocumentSnapshot().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(120, 120), restored["source-001"].Position);
        Assert.Equal(new GraphPoint(420, 120), restored["target-001"].Position);
    }

    [Fact]
    public void Commands_RouteSelectionTransformMoveThroughCanonicalCommandInvocation()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001"], "source-001", updateStatus: false);

        Assert.True(session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
            "selection.transform.move",
            [
                new GraphEditorCommandArgumentSnapshot("deltaX", "12"),
                new GraphEditorCommandArgumentSnapshot("deltaY", "4"),
                new GraphEditorCommandArgumentSnapshot("constrainToPrimaryAxis", "true"),
                new GraphEditorCommandArgumentSnapshot("updateStatus", "false"),
            ])));

        var source = session.Queries.CreateDocumentSnapshot().Nodes.Single(node => node.Id == "source-001");
        Assert.Equal(new GraphPoint(132, 120), source.Position);
        Assert.Contains(
            session.Queries.GetCommandDescriptors(),
            descriptor => descriptor.Id == "selection.transform.move" && descriptor.IsEnabled);
    }

    [Fact]
    public void Commands_ComposeSelectionMoveSnapGroupRouteAndLayoutResetConstraints()
    {
        var session = CreateSession();
        session.Commands.SetSelection(["source-001", "target-001"], "source-001", updateStatus: false);
        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Coherent Pipeline");

        var preview = session.Queries.GetSelectionTransformSnapshot(new GraphEditorSelectionTransformQuery(
            PreviewDelta: new GraphPoint(13, 17),
            ConstrainPreviewToPrimaryAxis: true));

        Assert.Equal(new GraphPoint(0, 17), preview.PreviewDelta);
        Assert.Equal(["connection-001"], preview.SelectedConnectionIds);
        Assert.Equal([groupId], preview.SelectedGroupIds);

        Assert.True(session.Commands.TryMoveSelectionBy(13, 17, constrainToPrimaryAxis: true, updateStatus: false));

        var guides = session.Queries.GetSnapGuideSnapshot(new GraphEditorSnapGuideQuery(20));
        Assert.True(guides.IsAvailable);
        Assert.Equal(["source-001", "target-001"], guides.Items.Select(item => item.NodeId));
        Assert.Contains(guides.Items, item => item.NodeId == "source-001" && item.SnappedPosition == new GraphPoint(120, 140));
        Assert.Contains(guides.Items, item => item.NodeId == "target-001" && item.SnappedPosition == new GraphPoint(420, 140));

        Assert.True(session.Commands.TrySnapSelectedNodesToGrid(20, updateStatus: false));

        var snapped = session.Queries.CreateDocumentSnapshot();
        var snappedNodes = snapped.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(120, 140), snappedNodes["source-001"].Position);
        Assert.Equal(new GraphPoint(420, 140), snappedNodes["target-001"].Position);
        Assert.Equal(groupId, snappedNodes["source-001"].Surface?.GroupId);
        Assert.Equal(groupId, snappedNodes["target-001"].Surface?.GroupId);
        var snappedGroup = snapped.Groups!.Single(group => group.Id == groupId);
        Assert.Equal(["source-001", "target-001"], snappedGroup.NodeIds);
        Assert.Equal(new GraphConnectionRoute([new GraphPoint(300, 180)]), Assert.Single(snapped.Connections).Presentation?.Route);

        Assert.True(session.Commands.TryApplyLayoutPlan(new GraphLayoutPlan(
            true,
            new GraphLayoutRequest { Mode = GraphLayoutRequestMode.Selection, SelectedNodeIds = ["source-001", "target-001"] },
            [
                new GraphLayoutNodePosition("source-001", new GraphPoint(160, 100)),
                new GraphLayoutNodePosition("target-001", new GraphPoint(480, 100)),
            ],
            ResetManualRoutes: true),
            updateStatus: false));

        var laidOut = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(new GraphPoint(160, 100), laidOut.Nodes.Single(node => node.Id == "source-001").Position);
        Assert.Equal(new GraphPoint(480, 100), laidOut.Nodes.Single(node => node.Id == "target-001").Position);
        Assert.Equal(groupId, laidOut.Nodes.Single(node => node.Id == "source-001").Surface?.GroupId);
        Assert.Equal(groupId, laidOut.Nodes.Single(node => node.Id == "target-001").Surface?.GroupId);
        Assert.Null(Assert.Single(laidOut.Connections).Presentation?.Route);
    }

    [Fact]
    public void Queries_RejectEmptySelectionTransformWithoutProjectingStaleGroupOrRouteState()
    {
        var session = CreateSession();

        var snapshot = session.Queries.GetSelectionTransformSnapshot(new GraphEditorSelectionTransformQuery(
            new GraphPoint(96, 96),
            new GraphSize(584, 180),
            new GraphPoint(12, 8),
            ConstrainPreviewToPrimaryAxis: true));

        Assert.False(snapshot.HasSelection);
        Assert.Empty(snapshot.SelectedNodeIds);
        Assert.Empty(snapshot.SelectedConnectionIds);
        Assert.Empty(snapshot.SelectedGroupIds);
        Assert.Null(snapshot.BoundsPosition);
        Assert.Null(snapshot.BoundsSize);
        Assert.Equal(new GraphPoint(12, 0), snapshot.PreviewDelta);
        Assert.Equal(["source-001", "target-001"], snapshot.RectangleNodeIds);
        Assert.Equal(["connection-001"], snapshot.RectangleConnectionIds);
        Assert.Equal("Select one or more nodes before transforming the selection.", snapshot.EmptyReason);
    }

    private static IGraphEditorSession CreateSession()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Transform",
            [],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Transform",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Selection Transform",
                "Selection transform proof.",
                [
                    new GraphNode(
                        "source-001",
                        "Source",
                        "Tests",
                        "Transform",
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
                        "Transform",
                        "Target node.",
                        new GraphPoint(420, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        [],
                        "#8B7BFF",
                        TargetDefinitionId),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "source-001",
                        "out",
                        "target-001",
                        "in",
                        "Source To Target",
                        "#6AD5C4",
                        Presentation: new GraphEdgePresentation(Route: new GraphConnectionRoute([new GraphPoint(300, 180)]))),
                ],
                [
                    new GraphNodeGroup(
                        "group-001",
                        "Pipeline",
                        new GraphPoint(96, 96),
                        new GraphSize(584, 180),
                        ["source-001", "target-001"]),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }
}
