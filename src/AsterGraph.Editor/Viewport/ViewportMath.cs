using System.ComponentModel;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Viewport;

/// <summary>
/// Coordinate conversion helpers for the editor viewport.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ViewportMath
{
    /// <summary>
    /// Converts a screen-space point into world-space coordinates.
    /// </summary>
    /// <param name="viewport">Current viewport state.</param>
    /// <param name="screen">Screen-space point.</param>
    /// <returns>World-space point.</returns>
    public static GraphPoint ScreenToWorld(ViewportState viewport, GraphPoint screen)
        => new(
            (screen.X - viewport.PanX) / viewport.Zoom,
            (screen.Y - viewport.PanY) / viewport.Zoom);

    /// <summary>
    /// Converts a world-space point into screen-space coordinates.
    /// </summary>
    /// <param name="viewport">Current viewport state.</param>
    /// <param name="world">World-space point.</param>
    /// <returns>Screen-space point.</returns>
    public static GraphPoint WorldToScreen(ViewportState viewport, GraphPoint world)
        => new(
            (world.X * viewport.Zoom) + viewport.PanX,
            (world.Y * viewport.Zoom) + viewport.PanY);

    /// <summary>
    /// Applies zoom around a screen-space anchor while preserving the anchored world point.
    /// </summary>
    /// <param name="viewport">Current viewport state.</param>
    /// <param name="zoomFactor">Zoom multiplier to apply.</param>
    /// <param name="screenAnchor">Screen-space anchor point.</param>
    /// <param name="minimumZoom">Minimum allowed zoom level.</param>
    /// <param name="maximumZoom">Maximum allowed zoom level.</param>
    /// <returns>The updated viewport state.</returns>
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
