using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Viewport;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class ViewportVisibleSceneProjectionTests
{
    [Fact]
    public void Project_CountsOnlyNodesGroupsAndConnectionsInsideViewportBudget()
    {
        var document = CreateDocument();
        var viewport = new GraphEditorViewportSnapshot(
            Zoom: 1d,
            PanX: 0d,
            PanY: 0d,
            ViewportWidth: 640d,
            ViewportHeight: 420d);

        var projection = ViewportVisibleSceneProjector.Project(document, viewport, overscanWorldUnits: 0d);

        Assert.Equal(3, projection.TotalNodes);
        Assert.Equal(2, projection.VisibleNodes);
        Assert.Equal(["node-a", "node-b"], projection.VisibleNodeIds);
        Assert.Equal(2, projection.TotalConnections);
        Assert.Equal(1, projection.VisibleConnections);
        Assert.Equal(["connection-visible"], projection.VisibleConnectionIds);
        Assert.Equal(2, projection.TotalGroups);
        Assert.Equal(1, projection.VisibleGroups);
        Assert.Equal(["group-visible"], projection.VisibleGroupIds);
        Assert.Equal(
            "VISIBLE_SCENE_PROJECTION:baseline:nodes=2/3:connections=1/2:groups=1/2:overscan=0",
            projection.ToBudgetMarker("baseline"));
    }

    [Fact]
    public void Project_UsesPanZoomAndOverscanForWorldBounds()
    {
        var document = CreateDocument();
        var viewport = new GraphEditorViewportSnapshot(
            Zoom: 2d,
            PanX: -200d,
            PanY: -100d,
            ViewportWidth: 400d,
            ViewportHeight: 300d);

        var projection = ViewportVisibleSceneProjector.Project(document, viewport, overscanWorldUnits: 25d);

        Assert.Equal(new GraphPoint(75d, 25d), projection.WorldTopLeft);
        Assert.Equal(new GraphPoint(325d, 225d), projection.WorldBottomRight);
        Assert.Equal(1, projection.VisibleNodes);
        Assert.Equal(1, projection.VisibleConnections);
        Assert.Equal(25d, projection.OverscanWorldUnits);
    }

    [Fact]
    public void Diff_BoundsInvalidationToSceneItemsEnteringOrLeavingTheVisibleSet()
    {
        var document = CreateDocument();
        var first = ViewportVisibleSceneProjector.Project(
            document,
            new GraphEditorViewportSnapshot(
                Zoom: 1d,
                PanX: 0d,
                PanY: 0d,
                ViewportWidth: 640d,
                ViewportHeight: 420d),
            overscanWorldUnits: 0d);
        var second = ViewportVisibleSceneProjector.Project(
            document,
            new GraphEditorViewportSnapshot(
                Zoom: 1d,
                PanX: -720d,
                PanY: -560d,
                ViewportWidth: 640d,
                ViewportHeight: 420d),
            overscanWorldUnits: 0d);

        var diff = first.Diff(second);

        Assert.Equal(["node-a", "node-b"], diff.NodesLeavingViewport);
        Assert.Equal(["node-c"], diff.NodesEnteringViewport);
        Assert.Equal(["connection-visible"], diff.ConnectionsLeavingViewport);
        Assert.Equal(["connection-hidden"], diff.ConnectionsEnteringViewport);
        Assert.Equal(["group-visible"], diff.GroupsLeavingViewport);
        Assert.Equal(["group-hidden"], diff.GroupsEnteringViewport);
        Assert.Equal(7, diff.InvalidatedSceneItemCount);
        Assert.Equal("VISIBLE_SCENE_INVALIDATION:nodes=3:connections=2:groups=2:total=7", diff.ToBudgetMarker());
        Assert.Equal(diff.ToBudgetMarker(), first.Diff(second).ToBudgetMarker());
        Assert.Equal(diff.ToBudgetMarker(), first.ToInvalidationBudgetMarker(second));
    }

    private static GraphDocument CreateDocument()
        => new(
            "Viewport Projection",
            "Visible scene budget proof.",
            [
                CreateNode("node-a", 40d, 40d),
                CreateNode("node-b", 360d, 180d),
                CreateNode("node-c", 980d, 760d),
            ],
            [
                CreateConnection("connection-visible", "node-a", "node-b"),
                CreateConnection("connection-hidden", "node-c", "node-missing"),
            ],
            [
                new GraphNodeGroup("group-visible", "Visible", new GraphPoint(20d, 20d), new GraphSize(580d, 340d), ["node-a", "node-b"]),
                new GraphNodeGroup("group-hidden", "Hidden", new GraphPoint(900d, 700d), new GraphSize(180d, 140d), ["node-c"]),
            ]);

    private static GraphNode CreateNode(string id, double x, double y)
        => new(
            id,
            id,
            "Tests",
            "Projection",
            "Viewport projection node.",
            new GraphPoint(x, y),
            new GraphSize(180d, 120d),
            [],
            [],
            "#6AD5C4",
            new NodeDefinitionId($"tests.viewport.{id}"));

    private static GraphConnection CreateConnection(string id, string sourceNodeId, string targetNodeId)
        => new(
            id,
            sourceNodeId,
            "out",
            targetNodeId,
            "in",
            id,
            "#6AD5C4");
}
