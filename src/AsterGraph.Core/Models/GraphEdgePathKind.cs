namespace AsterGraph.Core.Models;

/// <summary>
/// Persisted edge path shape requested by hosts.
/// </summary>
public enum GraphEdgePathKind
{
    /// <summary>
    /// Preserve the default editor behavior: Bezier for direct edges and step routing when bend points exist.
    /// </summary>
    Auto,

    /// <summary>
    /// Smooth cubic Bezier path.
    /// </summary>
    Bezier,

    /// <summary>
    /// Orthogonal path with rounded step corners.
    /// </summary>
    SmoothStep,

    /// <summary>
    /// Orthogonal step path.
    /// </summary>
    Step,

    /// <summary>
    /// Straight line path through the resolved anchors and optional route vertices.
    /// </summary>
    Straight,
}
