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
    /// Stable document-order IDs of nodes intersecting the visible viewport budget.
    /// </summary>
    internal IReadOnlyList<string> VisibleNodeIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Stable document-order IDs of connections attached to visible nodes.
    /// </summary>
    internal IReadOnlyList<string> VisibleConnectionIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Stable document-order IDs of groups intersecting the visible viewport budget.
    /// </summary>
    internal IReadOnlyList<string> VisibleGroupIds { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Machine-readable marker used by scale and rendering proof tests.
    /// </summary>
    public string ToBudgetMarker(string tier)
        => "VISIBLE_SCENE_PROJECTION:"
           + $"{tier}:nodes={VisibleNodes}/{TotalNodes}:connections={VisibleConnections}/{TotalConnections}:"
           + $"groups={VisibleGroups}/{TotalGroups}:overscan={OverscanWorldUnits:0.##}";

    /// <summary>
    /// Computes a machine-readable invalidation marker for the diff to the next projection.
    /// </summary>
    public string ToInvalidationBudgetMarker(ViewportVisibleSceneProjection next)
        => Diff(next).ToBudgetMarker();

    /// <summary>
    /// Computes the bounded scene-item invalidation between two visible-scene projections.
    /// </summary>
    internal ViewportVisibleSceneInvalidation Diff(ViewportVisibleSceneProjection next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return new ViewportVisibleSceneInvalidation(
            Except(VisibleNodeIds, next.VisibleNodeIds),
            Except(next.VisibleNodeIds, VisibleNodeIds),
            Except(VisibleConnectionIds, next.VisibleConnectionIds),
            Except(next.VisibleConnectionIds, VisibleConnectionIds),
            Except(VisibleGroupIds, next.VisibleGroupIds),
            Except(next.VisibleGroupIds, VisibleGroupIds));
    }

    private static IReadOnlyList<string> Except(IReadOnlyList<string> source, IReadOnlyList<string> excluded)
    {
        var excludedSet = excluded.ToHashSet(StringComparer.Ordinal);
        return source.Where(item => !excludedSet.Contains(item)).ToArray();
    }
}

/// <summary>
/// Scene items that entered or left the visible viewport projection.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed record ViewportVisibleSceneInvalidation(
    IReadOnlyList<string> NodesLeavingViewport,
    IReadOnlyList<string> NodesEnteringViewport,
    IReadOnlyList<string> ConnectionsLeavingViewport,
    IReadOnlyList<string> ConnectionsEnteringViewport,
    IReadOnlyList<string> GroupsLeavingViewport,
    IReadOnlyList<string> GroupsEnteringViewport)
{
    /// <summary>
    /// Number of scene items whose visibility changed.
    /// </summary>
    public int InvalidatedSceneItemCount
        => NodesLeavingViewport.Count
           + NodesEnteringViewport.Count
           + ConnectionsLeavingViewport.Count
           + ConnectionsEnteringViewport.Count
           + GroupsLeavingViewport.Count
           + GroupsEnteringViewport.Count;

    /// <summary>
    /// Machine-readable marker used by viewport invalidation proof tests.
    /// </summary>
    public string ToBudgetMarker()
        => "VISIBLE_SCENE_INVALIDATION:"
           + $"nodes={NodesLeavingViewport.Count + NodesEnteringViewport.Count}:"
           + $"connections={ConnectionsLeavingViewport.Count + ConnectionsEnteringViewport.Count}:"
           + $"groups={GroupsLeavingViewport.Count + GroupsEnteringViewport.Count}:"
           + $"total={InvalidatedSceneItemCount}";
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
            .ToArray();
        var visibleNodeIdSet = visibleNodeIds.ToHashSet(StringComparer.Ordinal);

        var visibleGroupIds = document.Groups is null
            ? Array.Empty<string>()
            : document.Groups
                .Where(group => Intersects(
                    worldBounds,
                    group.Position.X,
                    group.Position.Y,
                    group.Size.Width,
                    group.Size.Height))
                .Select(group => group.Id)
                .ToArray();
        var visibleConnectionIds = document.Connections
            .Where(connection =>
                visibleNodeIdSet.Contains(connection.SourceNodeId)
                || visibleNodeIdSet.Contains(connection.TargetNodeId))
            .Select(connection => connection.Id)
            .ToArray();

        return new ViewportVisibleSceneProjection(
            document.Nodes.Count,
            visibleNodeIds.Length,
            document.Connections.Count,
            visibleConnectionIds.Length,
            document.Groups?.Count ?? 0,
            visibleGroupIds.Length,
            new GraphPoint(worldBounds.Left, worldBounds.Top),
            new GraphPoint(worldBounds.Right, worldBounds.Bottom),
            Math.Max(0d, overscanWorldUnits))
        {
            VisibleNodeIds = visibleNodeIds,
            VisibleConnectionIds = visibleConnectionIds,
            VisibleGroupIds = visibleGroupIds,
        };
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
