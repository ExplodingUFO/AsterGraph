namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral adapter for whiteboard primitive scene data.
/// </summary>
internal static class GraphWhiteboardPrimitiveRendererAdapter
{
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

    private readonly record struct IndexedPrimitive(GraphWhiteboardPrimitiveSceneItem Primitive, int Index);
}
