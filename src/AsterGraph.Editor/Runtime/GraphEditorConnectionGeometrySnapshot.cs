using AsterGraph.Editor.Geometry;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable adapter-neutral geometry snapshot for one committed connection.
/// </summary>
/// <param name="ConnectionId">Stable connection identifier.</param>
/// <param name="Source">Resolved source endpoint geometry.</param>
/// <param name="Target">Resolved target endpoint geometry.</param>
/// <param name="Curve">Resolved Bezier curve between source and target anchors.</param>
public sealed record GraphEditorConnectionGeometrySnapshot(
    string ConnectionId,
    GraphEditorConnectionEndpointGeometrySnapshot Source,
    GraphEditorConnectionEndpointGeometrySnapshot Target,
    BezierConnection Curve);
