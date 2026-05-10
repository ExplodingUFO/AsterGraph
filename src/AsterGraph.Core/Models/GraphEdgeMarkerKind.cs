namespace AsterGraph.Core.Models;

/// <summary>
/// Optional marker rendered at an edge endpoint.
/// </summary>
public enum GraphEdgeMarkerKind
{
    /// <summary>
    /// No endpoint marker.
    /// </summary>
    None,

    /// <summary>
    /// Closed arrow marker aligned to the edge tangent.
    /// </summary>
    ArrowClosed,
}
