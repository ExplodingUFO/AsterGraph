using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.Kernel.Internal;

internal sealed class GraphEditorKernelViewportCoordinator
{
    private readonly double _defaultZoom;
    private readonly double _defaultPanX;
    private readonly double _defaultPanY;

    public GraphEditorKernelViewportCoordinator(double defaultZoom, double defaultPanX, double defaultPanY)
    {
        _defaultZoom = defaultZoom;
        _defaultPanX = defaultPanX;
        _defaultPanY = defaultPanY;
    }

    public GraphEditorViewportSnapshot PanBy(GraphEditorViewportSnapshot snapshot, double deltaX, double deltaY)
        => snapshot with
        {
            PanX = snapshot.PanX + deltaX,
            PanY = snapshot.PanY + deltaY,
        };

    public GraphEditorViewportSnapshot ZoomAt(GraphEditorViewportSnapshot snapshot, double factor, GraphPoint screenAnchor)
    {
        var updated = ViewportMath.ZoomAround(
            new ViewportState(snapshot.Zoom, snapshot.PanX, snapshot.PanY),
            factor,
            screenAnchor,
            minimumZoom: 0.35,
            maximumZoom: 1.9);

        return snapshot with
        {
            Zoom = updated.Zoom,
            PanX = updated.PanX,
            PanY = updated.PanY,
        };
    }

    public GraphEditorViewportSnapshot UpdateViewportSize(GraphEditorViewportSnapshot snapshot, double width, double height)
        => snapshot with
        {
            ViewportWidth = width,
            ViewportHeight = height,
        };

    public GraphEditorViewportSnapshot ResetView(GraphEditorViewportSnapshot snapshot)
        => snapshot with
        {
            Zoom = _defaultZoom,
            PanX = _defaultPanX,
            PanY = _defaultPanY,
        };

    public bool TryFitToViewport(
        GraphEditorViewportSnapshot snapshot,
        IReadOnlyList<GraphNode> nodes,
        out GraphEditorViewportSnapshot updated)
    {
        if (nodes.Count == 0 || snapshot.ViewportWidth <= 0 || snapshot.ViewportHeight <= 0)
        {
            updated = snapshot;
            return false;
        }

        var minX = nodes.Min(node => node.Position.X);
        var minY = nodes.Min(node => node.Position.Y);
        var maxX = nodes.Max(node => node.Position.X + node.Size.Width);
        var maxY = nodes.Max(node => node.Position.Y + node.Size.Height);
        var graphWidth = Math.Max(maxX - minX, 1);
        var graphHeight = Math.Max(maxY - minY, 1);
        const double padding = 120;

        var zoomX = snapshot.ViewportWidth / (graphWidth + (padding * 2));
        var zoomY = snapshot.ViewportHeight / (graphHeight + (padding * 2));

        updated = snapshot with
        {
            Zoom = Math.Clamp(Math.Min(zoomX, zoomY), 0.32, 1.4),
        };
        updated = updated with
        {
            PanX = ((updated.ViewportWidth - (graphWidth * updated.Zoom)) / 2) - (minX * updated.Zoom),
            PanY = ((updated.ViewportHeight - (graphHeight * updated.Zoom)) / 2) - (minY * updated.Zoom),
        };
        return true;
    }

    public bool TryCenterViewOnNode(
        GraphEditorViewportSnapshot snapshot,
        GraphNode? node,
        out GraphEditorViewportSnapshot updated)
    {
        if (node is null || snapshot.ViewportWidth <= 0 || snapshot.ViewportHeight <= 0)
        {
            updated = snapshot;
            return false;
        }

        updated = snapshot with
        {
            PanX = (snapshot.ViewportWidth / 2) - ((node.Position.X + (node.Size.Width / 2)) * snapshot.Zoom),
            PanY = (snapshot.ViewportHeight / 2) - ((node.Position.Y + (node.Size.Height / 2)) * snapshot.Zoom),
        };
        return true;
    }

    public bool TryCenterViewAt(
        GraphEditorViewportSnapshot snapshot,
        GraphPoint worldPoint,
        out GraphEditorViewportSnapshot updated)
    {
        if (snapshot.ViewportWidth <= 0 || snapshot.ViewportHeight <= 0)
        {
            updated = snapshot;
            return false;
        }

        updated = snapshot with
        {
            PanX = (snapshot.ViewportWidth / 2) - (worldPoint.X * snapshot.Zoom),
            PanY = (snapshot.ViewportHeight / 2) - (worldPoint.Y * snapshot.Zoom),
        };
        return true;
    }

    public GraphPoint GetViewportCenter(GraphEditorViewportSnapshot snapshot)
    {
        if (snapshot.ViewportWidth <= 0 || snapshot.ViewportHeight <= 0)
        {
            return ViewportMath.ScreenToWorld(
                new ViewportState(snapshot.Zoom, snapshot.PanX, snapshot.PanY),
                new GraphPoint(820, 440));
        }

        return ViewportMath.ScreenToWorld(
            new ViewportState(snapshot.Zoom, snapshot.PanX, snapshot.PanY),
            new GraphPoint(snapshot.ViewportWidth / 2, snapshot.ViewportHeight / 2));
    }
}
