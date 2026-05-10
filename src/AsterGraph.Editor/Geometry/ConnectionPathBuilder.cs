using System.ComponentModel;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Geometry;

/// <summary>
/// Builds Bezier connection curves between two anchor points.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
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
        => BuildRoute(start, route, end, GraphEditorConnectionRouteStyle.Bezier);

    public static IReadOnlyList<BezierConnection> BuildRoute(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphPoint end,
        GraphEditorConnectionRouteStyle routeStyle)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (routeStyle == GraphEditorConnectionRouteStyle.Bezier && route.IsEmpty)
        {
            return [Build(start, end)];
        }

        var points = BuildRoutePoints(start, route, end, routeStyle);
        if (routeStyle == GraphEditorConnectionRouteStyle.SmoothStep)
        {
            return BuildSmoothStepSegments(points);
        }

        var segments = new List<BezierConnection>(points.Count - 1);
        for (var index = 0; index < points.Count - 1; index++)
        {
            segments.Add(routeStyle == GraphEditorConnectionRouteStyle.Bezier
                ? Build(points[index], points[index + 1])
                : BuildStraightSegment(points[index], points[index + 1]));
        }

        return segments;
    }

    public static IReadOnlyList<GraphPoint> BuildRoutePoints(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphPoint end,
        GraphEditorConnectionRouteStyle routeStyle)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (routeStyle is GraphEditorConnectionRouteStyle.Bezier or GraphEditorConnectionRouteStyle.Straight)
        {
            var bezierPoints = new List<GraphPoint>(route.Vertices.Count + 2) { start };
            bezierPoints.AddRange(route.Vertices);
            bezierPoints.Add(end);
            return bezierPoints;
        }

        var anchors = new List<GraphPoint>(route.Vertices.Count + 2) { start };
        anchors.AddRange(route.Vertices);
        anchors.Add(end);

        var points = new List<GraphPoint>(anchors.Count * 3) { anchors[0] };
        for (var index = 0; index < anchors.Count - 1; index++)
        {
            AppendOrthogonalLeg(points, anchors[index], anchors[index + 1]);
        }

        return points;
    }

    public static GraphPoint ResolveSegmentMidpoint(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphPoint end,
        int segmentIndex)
        => ResolveSegmentMidpoint(start, route, end, segmentIndex, GraphEditorConnectionRouteStyle.Bezier);

    public static GraphPoint ResolveSegmentMidpoint(
        GraphPoint start,
        GraphConnectionRoute route,
        GraphPoint end,
        int segmentIndex,
        GraphEditorConnectionRouteStyle routeStyle)
    {
        ArgumentNullException.ThrowIfNull(route);

        var points = BuildRoutePoints(start, route, end, routeStyle);

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

    private static BezierConnection BuildStraightSegment(GraphPoint start, GraphPoint end)
        => new(
            start,
            new GraphPoint(start.X + ((end.X - start.X) / 3d), start.Y + ((end.Y - start.Y) / 3d)),
            new GraphPoint(start.X + (((end.X - start.X) * 2d) / 3d), start.Y + (((end.Y - start.Y) * 2d) / 3d)),
            end);

    private static IReadOnlyList<BezierConnection> BuildSmoothStepSegments(IReadOnlyList<GraphPoint> points)
    {
        if (points.Count < 2)
        {
            return [];
        }

        const double cornerRadius = 18d;
        var segments = new List<BezierConnection>();
        var current = points[0];
        for (var index = 1; index < points.Count; index++)
        {
            var corner = points[index];
            if (index == points.Count - 1)
            {
                AddStraightIfDistinct(segments, current, corner);
                continue;
            }

            var next = points[index + 1];
            var beforeCorner = MoveToward(corner, current, cornerRadius);
            var afterCorner = MoveToward(corner, next, cornerRadius);
            AddStraightIfDistinct(segments, current, beforeCorner);
            if (!IsSamePoint(beforeCorner, afterCorner))
            {
                segments.Add(new BezierConnection(beforeCorner, corner, corner, afterCorner));
            }

            current = afterCorner;
        }

        return segments;
    }

    private static void AddStraightIfDistinct(List<BezierConnection> segments, GraphPoint start, GraphPoint end)
    {
        if (!IsSamePoint(start, end))
        {
            segments.Add(BuildStraightSegment(start, end));
        }
    }

    private static GraphPoint MoveToward(GraphPoint from, GraphPoint to, double distance)
    {
        var deltaX = to.X - from.X;
        var deltaY = to.Y - from.Y;
        var length = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        if (length < 0.001d)
        {
            return from;
        }

        var resolved = Math.Min(distance, length / 2d);
        return new GraphPoint(
            from.X + (deltaX / length * resolved),
            from.Y + (deltaY / length * resolved));
    }

    private static void AppendOrthogonalLeg(List<GraphPoint> points, GraphPoint start, GraphPoint end)
    {
        if (IsSamePoint(start, end))
        {
            return;
        }

        if (NearlyEqual(start.X, end.X) || NearlyEqual(start.Y, end.Y))
        {
            AddDistinct(points, end);
            return;
        }

        var midX = (start.X + end.X) / 2d;
        AddDistinct(points, new GraphPoint(midX, start.Y));
        AddDistinct(points, new GraphPoint(midX, end.Y));
        AddDistinct(points, end);
    }

    private static void AddDistinct(List<GraphPoint> points, GraphPoint point)
    {
        if (points.Count == 0 || !IsSamePoint(points[^1], point))
        {
            points.Add(point);
        }
    }

    private static bool IsSamePoint(GraphPoint left, GraphPoint right)
        => NearlyEqual(left.X, right.X) && NearlyEqual(left.Y, right.Y);

    private static bool NearlyEqual(double left, double right)
        => Math.Abs(left - right) < 0.001d;
}
