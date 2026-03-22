using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Viewport;

public static class ViewportMath
{
    public static GraphPoint ScreenToWorld(ViewportState viewport, GraphPoint screen)
        => new(
            (screen.X - viewport.PanX) / viewport.Zoom,
            (screen.Y - viewport.PanY) / viewport.Zoom);

    public static GraphPoint WorldToScreen(ViewportState viewport, GraphPoint world)
        => new(
            (world.X * viewport.Zoom) + viewport.PanX,
            (world.Y * viewport.Zoom) + viewport.PanY);

    public static ViewportState ZoomAround(
        ViewportState viewport,
        double zoomFactor,
        GraphPoint screenAnchor,
        double minimumZoom = 0.55,
        double maximumZoom = 1.85)
    {
        var currentWorldAnchor = ScreenToWorld(viewport, screenAnchor);
        var nextZoom = Math.Clamp(viewport.Zoom * zoomFactor, minimumZoom, maximumZoom);

        return new ViewportState(
            nextZoom,
            screenAnchor.X - (currentWorldAnchor.X * nextZoom),
            screenAnchor.Y - (currentWorldAnchor.Y * nextZoom));
    }
}
