namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable two-dimensional point in graph world space.
/// </summary>
/// <param name="X">Horizontal coordinate.</param>
/// <param name="Y">Vertical coordinate.</param>
public readonly record struct GraphPoint(double X, double Y)
{
    /// <summary>
    /// Adds two graph points component-wise.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>The summed point.</returns>
    public static GraphPoint operator +(GraphPoint left, GraphPoint right)
        => new(left.X + right.X, left.Y + right.Y);

    /// <summary>
    /// Subtracts two graph points component-wise.
    /// </summary>
    /// <param name="left">Left operand.</param>
    /// <param name="right">Right operand.</param>
    /// <returns>The resulting delta point.</returns>
    public static GraphPoint operator -(GraphPoint left, GraphPoint right)
        => new(left.X - right.X, left.Y - right.Y);
}
