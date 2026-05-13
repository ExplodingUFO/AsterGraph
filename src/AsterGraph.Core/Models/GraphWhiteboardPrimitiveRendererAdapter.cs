namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral adapter for whiteboard primitive scene data.
/// </summary>
internal static class GraphWhiteboardPrimitiveRendererAdapter
{
    private const double MinimumFreehandHitTolerance = 4d;

    /// <summary>
    /// Projects whiteboard primitive model snapshots into renderer-neutral scene data.
    /// </summary>
    /// <param name="primitives">Whiteboard primitives to project.</param>
    /// <returns>Renderer-neutral whiteboard primitive scene snapshot.</returns>
    public static GraphWhiteboardPrimitiveSceneSnapshot Project(IReadOnlyList<GraphWhiteboardPrimitive> primitives)
    {
        ArgumentNullException.ThrowIfNull(primitives);

        return new GraphWhiteboardPrimitiveSceneSnapshot(
            primitives
                .Select(primitive => new GraphWhiteboardPrimitiveSceneItem(
                    primitive.Id,
                    primitive.Kind,
                    primitive.Geometry.Origin,
                    primitive.Geometry.Size,
                    primitive.Geometry.Points,
                    primitive.Style,
                    primitive.ZIndex,
                    primitive.EditLifecycle))
                .ToList());
    }

    /// <summary>
    /// Finds the topmost primitive scene item containing the graph-space point.
    /// </summary>
    /// <param name="scene">Whiteboard primitive scene snapshot.</param>
    /// <param name="worldPoint">Graph-space hit-test point.</param>
    /// <returns>The topmost hit result, or <see langword="null" /> when no primitive contains the point.</returns>
    public static GraphWhiteboardPrimitiveHitTestResult? HitTest(
        GraphWhiteboardPrimitiveSceneSnapshot scene,
        GraphPoint worldPoint)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var hit = scene.Primitives
            .Select((primitive, index) => new IndexedPrimitive(primitive, index))
            .Where(item => Contains(item.Primitive, worldPoint))
            .OrderByDescending(item => item.Primitive.ZIndex)
            .ThenByDescending(item => item.Index)
            .Select(item => item.Primitive)
            .FirstOrDefault();

        return hit is null
            ? null
            : new GraphWhiteboardPrimitiveHitTestResult(
                hit.Id,
                hit.Kind,
                hit.ZIndex,
                hit.EditLifecycle);
    }

    private static bool Contains(GraphWhiteboardPrimitiveSceneItem primitive, GraphPoint point)
        => primitive.Kind switch
        {
            GraphWhiteboardPrimitiveKind.Freehand => ContainsFreehandPrimitive(primitive, point),
            _ => ContainsBounds(primitive, point),
        };

    private static bool ContainsBounds(GraphWhiteboardPrimitiveSceneItem primitive, GraphPoint point)
    {
        var left = primitive.BoundsOrigin.X;
        var top = primitive.BoundsOrigin.Y;
        var right = left + Math.Max(0d, primitive.BoundsSize.Width);
        var bottom = top + Math.Max(0d, primitive.BoundsSize.Height);

        return point.X >= left
               && point.X <= right
               && point.Y >= top
               && point.Y <= bottom;
    }

    private static bool ContainsFreehandPrimitive(GraphWhiteboardPrimitiveSceneItem primitive, GraphPoint point)
    {
        if (!ContainsBounds(primitive, point) || primitive.Points.Count == 0)
        {
            return false;
        }

        var tolerance = Math.Max(MinimumFreehandHitTolerance, primitive.Style.StrokeThickness / 2d);
        if (primitive.Points.Count == 1)
        {
            return DistanceSquared(point, primitive.Points[0]) <= tolerance * tolerance;
        }

        for (var index = 1; index < primitive.Points.Count; index++)
        {
            if (DistanceToSegmentSquared(point, primitive.Points[index - 1], primitive.Points[index]) <= tolerance * tolerance)
            {
                return true;
            }
        }

        return false;
    }

    private static double DistanceToSegmentSquared(GraphPoint point, GraphPoint start, GraphPoint end)
    {
        var segmentX = end.X - start.X;
        var segmentY = end.Y - start.Y;
        var lengthSquared = (segmentX * segmentX) + (segmentY * segmentY);
        if (lengthSquared <= double.Epsilon)
        {
            return DistanceSquared(point, start);
        }

        var projected = (((point.X - start.X) * segmentX) + ((point.Y - start.Y) * segmentY)) / lengthSquared;
        var clamped = Math.Clamp(projected, 0d, 1d);
        var closest = new GraphPoint(
            start.X + (clamped * segmentX),
            start.Y + (clamped * segmentY));

        return DistanceSquared(point, closest);
    }

    private static double DistanceSquared(GraphPoint first, GraphPoint second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return (deltaX * deltaX) + (deltaY * deltaY);
    }

    private readonly record struct IndexedPrimitive(GraphWhiteboardPrimitiveSceneItem Primitive, int Index);
}
