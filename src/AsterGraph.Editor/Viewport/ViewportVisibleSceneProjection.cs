using System.ComponentModel;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Viewport;

/// <summary>
/// Visible-scene projection counts for a viewport snapshot.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record ViewportVisibleSceneProjection(
    int TotalNodes,
    int VisibleNodes,
    int TotalConnections,
    int VisibleConnections,
    int TotalGroups,
    int VisibleGroups,
    GraphPoint WorldTopLeft,
    GraphPoint WorldBottomRight,
    double OverscanWorldUnits)
{
    /// <summary>
    /// Machine-readable marker used by scale and rendering proof tests.
    /// </summary>
    public string ToBudgetMarker(string tier)
        => "VISIBLE_SCENE_PROJECTION:"
           + $"{tier}:nodes={VisibleNodes}/{TotalNodes}:connections={VisibleConnections}/{TotalConnections}:"
           + $"groups={VisibleGroups}/{TotalGroups}:overscan={OverscanWorldUnits:0.##}";
}

/// <summary>
/// Computes world-space visibility counts from the retained viewport state.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ViewportVisibleSceneProjector
{
    /// <summary>
    /// Default world-space overscan included around the viewport to avoid edge churn.
    /// </summary>
    public const double DefaultOverscanWorldUnits = 96d;

    /// <summary>
    /// Creates a visible-scene projection for the root document.
    /// </summary>
    public static ViewportVisibleSceneProjection Project(
        GraphDocument document,
        GraphEditorViewportSnapshot viewport,
        double overscanWorldUnits = DefaultOverscanWorldUnits)
    {
        ArgumentNullException.ThrowIfNull(document);

        var worldBounds = ResolveVisibleWorldBounds(viewport, Math.Max(0d, overscanWorldUnits));
        var visibleNodeIds = document.Nodes
            .Where(node => Intersects(
                worldBounds,
                node.Position.X,
                node.Position.Y,
                node.Size.Width,
                node.Size.Height))
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);

        var visibleGroups = document.Groups is null
            ? 0
            : document.Groups.Count(group => Intersects(
                worldBounds,
                group.Position.X,
                group.Position.Y,
                group.Size.Width,
                group.Size.Height));
        var visibleConnections = document.Connections.Count(connection =>
            visibleNodeIds.Contains(connection.SourceNodeId)
            || visibleNodeIds.Contains(connection.TargetNodeId));

        return new ViewportVisibleSceneProjection(
            document.Nodes.Count,
            visibleNodeIds.Count,
            document.Connections.Count,
            visibleConnections,
            document.Groups?.Count ?? 0,
            visibleGroups,
            new GraphPoint(worldBounds.Left, worldBounds.Top),
            new GraphPoint(worldBounds.Right, worldBounds.Bottom),
            Math.Max(0d, overscanWorldUnits));
    }

    private static VisibleWorldBounds ResolveVisibleWorldBounds(GraphEditorViewportSnapshot viewport, double overscanWorldUnits)
    {
        if (viewport.ViewportWidth <= 0 || viewport.ViewportHeight <= 0 || viewport.Zoom <= 0)
        {
            return new VisibleWorldBounds(
                -overscanWorldUnits,
                -overscanWorldUnits,
                overscanWorldUnits,
                overscanWorldUnits);
        }

        var viewportState = new ViewportState(viewport.Zoom, viewport.PanX, viewport.PanY);
        var topLeft = ViewportMath.ScreenToWorld(viewportState, new GraphPoint(0, 0));
        var bottomRight = ViewportMath.ScreenToWorld(
            viewportState,
            new GraphPoint(viewport.ViewportWidth, viewport.ViewportHeight));

        return new VisibleWorldBounds(
            Math.Min(topLeft.X, bottomRight.X) - overscanWorldUnits,
            Math.Min(topLeft.Y, bottomRight.Y) - overscanWorldUnits,
            Math.Max(topLeft.X, bottomRight.X) + overscanWorldUnits,
            Math.Max(topLeft.Y, bottomRight.Y) + overscanWorldUnits);
    }

    private static bool Intersects(
        VisibleWorldBounds bounds,
        double x,
        double y,
        double width,
        double height)
    {
        var right = x + Math.Max(0d, width);
        var bottom = y + Math.Max(0d, height);
        return x <= bounds.Right
               && right >= bounds.Left
               && y <= bounds.Bottom
               && bottom >= bounds.Top;
    }

    private readonly record struct VisibleWorldBounds(double Left, double Top, double Right, double Bottom);
}
