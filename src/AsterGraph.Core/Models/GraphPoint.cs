namespace AsterGraph.Core.Models;

public readonly record struct GraphPoint(double X, double Y)
{
    public static GraphPoint operator +(GraphPoint left, GraphPoint right)
        => new(left.X + right.X, left.Y + right.Y);

    public static GraphPoint operator -(GraphPoint left, GraphPoint right)
        => new(left.X - right.X, left.Y - right.Y);
}
