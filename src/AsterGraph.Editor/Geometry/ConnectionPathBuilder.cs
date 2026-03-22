using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

public static class ConnectionPathBuilder
{
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
