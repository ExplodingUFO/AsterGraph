using AsterGraph.Core.Models;
using AsterGraph.Editor.Scene;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSceneInteractionSeamsTests
{
    [Fact]
    public void PointerInputRouter_RoutePressed_WithAltLeftDrag_ReturnsPanningRoute()
    {
        var route = GraphEditorPointerInputRouter.RoutePressed(new GraphEditorPointerInputContext(
            IsAlreadyHandled: false,
            IsLeftButtonPressed: true,
            IsMiddleButtonPressed: false,
            Modifiers: GraphEditorInputModifiers.Alt,
            EnableAltLeftDragPanning: true,
            HasPendingConnection: false));

        Assert.Equal(GraphEditorPointerPressRouteKind.BeginPanning, route.Kind);
        Assert.False(route.CancelPendingConnection);
    }

    [Fact]
    public void PointerInputRouter_RoutePressed_WithPendingConnection_ReturnsSelectionRouteAndCancelsPreview()
    {
        var route = GraphEditorPointerInputRouter.RoutePressed(new GraphEditorPointerInputContext(
            IsAlreadyHandled: false,
            IsLeftButtonPressed: true,
            IsMiddleButtonPressed: false,
            Modifiers: GraphEditorInputModifiers.None,
            EnableAltLeftDragPanning: true,
            HasPendingConnection: true));

        Assert.Equal(GraphEditorPointerPressRouteKind.BeginCanvasSelection, route.Kind);
        Assert.True(route.CancelPendingConnection);
    }

    [Fact]
    public void SceneResizeHitTester_TryHit_NodeProfile_ResolvesBottomRightCorner()
    {
        var hit = GraphEditorSceneResizeHitTester.TryHit(
            new GraphSize(240d, 160d),
            new GraphPoint(236d, 156d),
            GraphEditorSceneResizeHitTestProfile.Node);

        Assert.NotNull(hit);
        Assert.Equal(GraphEditorSceneSurfaceKind.Node, hit.Value.SurfaceKind);
        Assert.Equal(GraphEditorSceneResizeHandleKind.BottomRightCorner, hit.Value.Handle);
    }

    [Fact]
    public void SceneResizeHitTester_TryHit_GroupProfile_ResolvesLeftEdgeWithoutAvaloniaTypes()
    {
        var hit = GraphEditorSceneResizeHitTester.TryHit(
            new GraphSize(320d, 220d),
            new GraphPoint(3d, 80d),
            GraphEditorSceneResizeHitTestProfile.Group);

        Assert.NotNull(hit);
        Assert.Equal(GraphEditorSceneSurfaceKind.Group, hit.Value.SurfaceKind);
        Assert.Equal(GraphEditorSceneResizeHandleKind.LeftEdge, hit.Value.Handle);
    }
}
