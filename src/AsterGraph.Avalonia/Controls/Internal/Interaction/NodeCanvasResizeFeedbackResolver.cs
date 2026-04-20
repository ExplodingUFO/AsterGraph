using Avalonia;
using Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;

namespace AsterGraph.Avalonia.Controls.Internal;

internal static class NodeCanvasResizeFeedbackResolver
{
    private const double NodeEdgeResizeHitThickness = 10d;
    private const double NodeCornerResizeHitThickness = 16d;

    public static bool TryResolveNode(Control surface, Point point, out NodeCanvasResizeFeedbackHit hit)
        => TryResolve(
            surface,
            point,
            edgeThickness: NodeEdgeResizeHitThickness,
            cornerThickness: NodeCornerResizeHitThickness,
            includeLeftEdge: false,
            includeTopEdge: false,
            includeRightEdge: true,
            includeBottomEdge: true,
            includeBottomRightCorner: true,
            GraphResizeFeedbackSurfaceKind.Node,
            out hit);

    public static bool TryResolveGroup(Control surface, Point point, out NodeCanvasResizeFeedbackHit hit)
        => TryResolve(
            surface,
            point,
            edgeThickness: NodeCanvasGroupChromeMetrics.ResizeHandleThickness,
            cornerThickness: NodeCanvasGroupChromeMetrics.ResizeHandleThickness,
            includeLeftEdge: true,
            includeTopEdge: true,
            includeRightEdge: true,
            includeBottomEdge: true,
            includeBottomRightCorner: false,
            GraphResizeFeedbackSurfaceKind.Group,
            out hit);

    private static bool TryResolve(
        Control surface,
        Point point,
        double edgeThickness,
        double cornerThickness,
        bool includeLeftEdge,
        bool includeTopEdge,
        bool includeRightEdge,
        bool includeBottomEdge,
        bool includeBottomRightCorner,
        GraphResizeFeedbackSurfaceKind surfaceKind,
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

        if (includeBottomRightCorner
            && point.X >= width - cornerThickness
            && point.Y >= height - cornerThickness)
        {
            hit = CreateHit(surface, surfaceKind, GraphResizeFeedbackHandle.BottomRightCorner);
            return true;
        }

        if (includeLeftEdge && point.X <= edgeThickness)
        {
            hit = CreateHit(surface, surfaceKind, GraphResizeFeedbackHandle.LeftEdge);
            return true;
        }

        if (includeTopEdge && point.Y <= edgeThickness)
        {
            hit = CreateHit(surface, surfaceKind, GraphResizeFeedbackHandle.TopEdge);
            return true;
        }

        if (includeRightEdge && point.X >= width - edgeThickness)
        {
            hit = CreateHit(surface, surfaceKind, GraphResizeFeedbackHandle.RightEdge);
            return true;
        }

        if (includeBottomEdge && point.Y >= height - edgeThickness)
        {
            hit = CreateHit(surface, surfaceKind, GraphResizeFeedbackHandle.BottomEdge);
            return true;
        }

        return false;
    }

    private static NodeCanvasResizeFeedbackHit CreateHit(
        Control surface,
        GraphResizeFeedbackSurfaceKind surfaceKind,
        GraphResizeFeedbackHandle handle)
    {
        return surfaceKind switch
        {
            GraphResizeFeedbackSurfaceKind.Node => new NodeCanvasResizeFeedbackHit(
                surface,
                new GraphResizeFeedbackContext(surfaceKind, handle),
                handle switch
                {
                    GraphResizeFeedbackHandle.RightEdge => GraphNodeResizeHandleKind.Right,
                    GraphResizeFeedbackHandle.BottomEdge => GraphNodeResizeHandleKind.Bottom,
                    GraphResizeFeedbackHandle.BottomRightCorner => GraphNodeResizeHandleKind.BottomRight,
                    _ => null,
                },
                null),
            GraphResizeFeedbackSurfaceKind.Group => new NodeCanvasResizeFeedbackHit(
                surface,
                new GraphResizeFeedbackContext(surfaceKind, handle),
                null,
                handle switch
                {
                    GraphResizeFeedbackHandle.LeftEdge => NodeCanvasGroupResizeEdge.Left,
                    GraphResizeFeedbackHandle.TopEdge => NodeCanvasGroupResizeEdge.Top,
                    GraphResizeFeedbackHandle.RightEdge => NodeCanvasGroupResizeEdge.Right,
                    GraphResizeFeedbackHandle.BottomEdge => NodeCanvasGroupResizeEdge.Bottom,
                    _ => null,
                }),
            _ => default,
        };
    }
}
