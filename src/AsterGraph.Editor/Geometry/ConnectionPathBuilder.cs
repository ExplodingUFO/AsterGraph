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

    public static IReadOnlyList<BezierConnection> BuildRoute(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphPoint end)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (route.IsEmpty)
        {
            return [Build(start, end)];
        }

        var segments = new List<BezierConnection>(route.Vertices.Count + 1);
        var previousPoint = start;
        foreach (var vertex in route.Vertices)
        {
            segments.Add(Build(previousPoint, vertex));
            previousPoint = vertex;
        }

        segments.Add(Build(previousPoint, end));
        return segments;
    }

    public static GraphPoint ResolveSegmentMidpoint(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphPoint end,
        int segmentIndex)
    {
        ArgumentNullException.ThrowIfNull(route);

        var points = new List<GraphPoint>(route.Vertices.Count + 2) { start };
        points.AddRange(route.Vertices);
        points.Add(end);

        if (segmentIndex < 0 || segmentIndex >= points.Count - 1)
        {
            throw new ArgumentOutOfRangeException(nameof(segmentIndex));
        }

        var segmentStart = points[segmentIndex];
        var segmentEnd = points[segmentIndex + 1];
        return new GraphPoint(
            (segmentStart.X + segmentEnd.X) / 2d,
            (segmentStart.Y + segmentEnd.Y) / 2d);
    }
}
