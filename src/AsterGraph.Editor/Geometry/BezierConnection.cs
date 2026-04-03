using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

/// <summary>
/// Bezier curve control points used to render a connection path.
/// </summary>
/// <param name="Start">Curve start point.</param>
/// <param name="Control1">First control point.</param>
/// <param name="Control2">Second control point.</param>
/// <param name="End">Curve end point.</param>
public readonly record struct BezierConnection(
    GraphPoint Start,
    GraphPoint Control1,
    GraphPoint Control2,
    GraphPoint End);
