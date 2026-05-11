using System.ComponentModel;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

/// <summary>
/// Small geometry helpers for persisted 2D node rotation.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GraphNodeRotationGeometry
{
    public static NodeBounds GetAxisAlignedBounds(GraphPoint position, GraphSize size, double rotationDegrees)
    {
        var normalizedRotation = GraphNodeSurfaceState.NormalizeRotationDegrees(rotationDegrees);
        if (Math.Abs(normalizedRotation) < 0.0001d)
        {
            return new NodeBounds(position.X, position.Y, size.Width, size.Height);
        }

        var points = new[]
        {
            TransformLocalToWorld(position, size, normalizedRotation, new GraphPoint(0d, 0d)),
            TransformLocalToWorld(position, size, normalizedRotation, new GraphPoint(size.Width, 0d)),
            TransformLocalToWorld(position, size, normalizedRotation, new GraphPoint(size.Width, size.Height)),
            TransformLocalToWorld(position, size, normalizedRotation, new GraphPoint(0d, size.Height)),
        };
        var left = points.Min(point => point.X);
        var top = points.Min(point => point.Y);
        var right = points.Max(point => point.X);
        var bottom = points.Max(point => point.Y);
        return new NodeBounds(left, top, right - left, bottom - top);
    }

    public static GraphPoint TransformLocalToWorld(
        GraphPoint position,
        GraphSize size,
        double rotationDegrees,
        GraphPoint localPoint)
    {
        var normalizedRotation = GraphNodeSurfaceState.NormalizeRotationDegrees(rotationDegrees);
        if (Math.Abs(normalizedRotation) < 0.0001d)
        {
            return new GraphPoint(position.X + localPoint.X, position.Y + localPoint.Y);
        }

        var centerX = size.Width / 2d;
        var centerY = size.Height / 2d;
        var radians = normalizedRotation * Math.PI / 180d;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var dx = localPoint.X - centerX;
        var dy = localPoint.Y - centerY;
        return new GraphPoint(
            position.X + centerX + (dx * cos) - (dy * sin),
            position.Y + centerY + (dx * sin) + (dy * cos));
    }

    public static GraphPoint TransformWorldToLocal(
        GraphPoint position,
        GraphSize size,
        double rotationDegrees,
        GraphPoint worldPoint)
    {
        var normalizedRotation = GraphNodeSurfaceState.NormalizeRotationDegrees(rotationDegrees);
        if (Math.Abs(normalizedRotation) < 0.0001d)
        {
            return new GraphPoint(worldPoint.X - position.X, worldPoint.Y - position.Y);
        }

        var centerX = position.X + (size.Width / 2d);
        var centerY = position.Y + (size.Height / 2d);
        var radians = -normalizedRotation * Math.PI / 180d;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var dx = worldPoint.X - centerX;
        var dy = worldPoint.Y - centerY;
        return new GraphPoint(
            (size.Width / 2d) + (dx * cos) - (dy * sin),
            (size.Height / 2d) + (dx * sin) + (dy * cos));
    }
}
