using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

/// <summary>
/// Builds Bezier connection curves between two anchor points.
/// </summary>
public static class ConnectionPathBuilder
{
    /// <summary>
    /// Builds a smooth Bezier curve between two world-space anchors.
    /// </summary>
    /// <param name="start">Curve start point.</param>
    /// <param name="end">Curve end point.</param>
    /// <returns>The computed curve control points.</returns>
    public static BezierConnection Build(GraphPoint start, GraphPoint end)
    {
        var distanceX = Math.Abs(end.X - start.X);
        var tension = Math.Max(88, distanceX * 0.42);

        return new BezierConnection(
            start,
            new GraphPoint(start.X + tension, start.Y),
            new GraphPoint(end.X - tension, end.Y),
            end);
    }
}
