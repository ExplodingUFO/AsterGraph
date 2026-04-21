using Avalonia;
using Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Scene;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class NodeCanvasResizeFeedbackResolver
{
    public static bool TryResolveNode(Control surface, Point point, out NodeCanvasResizeFeedbackHit hit)
        => TryResolve(surface, point, GraphEditorSceneResizeHitTestProfile.Node, out hit);

    public static bool TryResolveGroup(Control surface, Point point, out NodeCanvasResizeFeedbackHit hit)
        => TryResolve(surface, point, GraphEditorSceneResizeHitTestProfile.Group, out hit);

    private static bool TryResolve(
        Control surface,
        Point point,
        GraphEditorSceneResizeHitTestProfile profile,
        out NodeCanvasResizeFeedbackHit hit)
    {
        ArgumentNullException.ThrowIfNull(surface);

        hit = default;
        var width = double.IsNaN(surface.Width) ? surface.Bounds.Width : surface.Width;
        var height = double.IsNaN(surface.Height) ? surface.Bounds.Height : surface.Height;
        if (width <= 0d || height <= 0d)
        {
            return false;
        }

        var sceneHit = GraphEditorSceneResizeHitTester.TryHit(
            new GraphSize(width, height),
            new GraphPoint(point.X, point.Y),
            profile);
        if (sceneHit is null)
        {
            return false;
        }

        hit = CreateHit(surface, sceneHit.Value);
        return true;
    }

    private static NodeCanvasResizeFeedbackHit CreateHit(
        Control surface,
        GraphEditorSceneResizeHit hit)
    {
        return hit.SurfaceKind switch
        {
            GraphEditorSceneSurfaceKind.Node => new NodeCanvasResizeFeedbackHit(
                surface,
                new GraphResizeFeedbackContext(GraphResizeFeedbackSurfaceKind.Node, MapHandle(hit.Handle)),
                hit.Handle switch
                {
                    GraphEditorSceneResizeHandleKind.RightEdge => GraphNodeResizeHandleKind.Right,
                    GraphEditorSceneResizeHandleKind.BottomEdge => GraphNodeResizeHandleKind.Bottom,
                    GraphEditorSceneResizeHandleKind.BottomRightCorner => GraphNodeResizeHandleKind.BottomRight,
                    _ => null,
                },
                null),
            GraphEditorSceneSurfaceKind.Group => new NodeCanvasResizeFeedbackHit(
                surface,
                new GraphResizeFeedbackContext(GraphResizeFeedbackSurfaceKind.Group, MapHandle(hit.Handle)),
                null,
                hit.Handle switch
                {
                    GraphEditorSceneResizeHandleKind.LeftEdge => NodeCanvasGroupResizeEdge.Left,
                    GraphEditorSceneResizeHandleKind.TopEdge => NodeCanvasGroupResizeEdge.Top,
                    GraphEditorSceneResizeHandleKind.RightEdge => NodeCanvasGroupResizeEdge.Right,
                    GraphEditorSceneResizeHandleKind.BottomEdge => NodeCanvasGroupResizeEdge.Bottom,
                    _ => null,
                }),
            _ => default,
        };
    }

    private static GraphResizeFeedbackHandle MapHandle(GraphEditorSceneResizeHandleKind handle)
        => handle switch
        {
            GraphEditorSceneResizeHandleKind.LeftEdge => GraphResizeFeedbackHandle.LeftEdge,
            GraphEditorSceneResizeHandleKind.TopEdge => GraphResizeFeedbackHandle.TopEdge,
            GraphEditorSceneResizeHandleKind.RightEdge => GraphResizeFeedbackHandle.RightEdge,
            GraphEditorSceneResizeHandleKind.BottomEdge => GraphResizeFeedbackHandle.BottomEdge,
            GraphEditorSceneResizeHandleKind.BottomRightCorner => GraphResizeFeedbackHandle.BottomRightCorner,
            _ => GraphResizeFeedbackHandle.BottomRightCorner,
        };
}
