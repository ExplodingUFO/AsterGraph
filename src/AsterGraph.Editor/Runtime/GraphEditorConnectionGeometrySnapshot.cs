using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable adapter-neutral geometry snapshot for one committed connection.
/// </summary>
/// <param name="ConnectionId">Stable connection identifier.</param>
/// <param name="Source">Resolved source endpoint geometry.</param>
/// <param name="Target">Resolved target endpoint geometry.</param>
/// <param name="Route">Resolved persisted bend-point route between source and target anchors.</param>
/// <param name="RouteStyle">Resolved rendering style for this route.</param>
/// <param name="RoutingEvidence">Bounded routing evidence used by hosts for inspection and quality checks.</param>
public sealed record GraphEditorConnectionGeometrySnapshot(
    string ConnectionId,
    GraphEditorConnectionEndpointGeometrySnapshot Source,
    GraphEditorConnectionEndpointGeometrySnapshot Target,
    GraphConnectionRoute Route,
    GraphEditorConnectionRouteStyle RouteStyle = GraphEditorConnectionRouteStyle.Bezier,
    GraphEditorConnectionRouteEvidenceSnapshot? RoutingEvidence = null);

/// <summary>
/// Supported editor-side route rendering styles.
/// </summary>
public enum GraphEditorConnectionRouteStyle
{
    /// <summary>
    /// Smooth cubic Bezier routing between anchors and optional bend points.
    /// </summary>
    Bezier,

    /// <summary>
    /// Axis-aligned routing through deterministic dog-leg segments.
    /// </summary>
    Orthogonal,
}

/// <summary>
/// Bounded routing quality evidence for one projected connection.
/// </summary>
/// <param name="ObstacleNodeIds">Node ids whose bounds intersect the routed polyline, excluding endpoints.</param>
/// <param name="CrossingCount">Number of routed segment crossings with other projected connections.</param>
/// <param name="PathPoints">Polyline points used for bounded routing evidence.</param>
public sealed record GraphEditorConnectionRouteEvidenceSnapshot(
    IReadOnlyList<string> ObstacleNodeIds,
    int CrossingCount,
    IReadOnlyList<GraphPoint> PathPoints);
