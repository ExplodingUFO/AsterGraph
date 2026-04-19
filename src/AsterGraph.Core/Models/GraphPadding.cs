namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable per-edge padding in graph world space.
/// </summary>
/// <param name="Left">Padding applied to the left edge.</param>
/// <param name="Top">Padding applied to the top edge.</param>
/// <param name="Right">Padding applied to the right edge.</param>
/// <param name="Bottom">Padding applied to the bottom edge.</param>
public readonly record struct GraphPadding(double Left, double Top, double Right, double Bottom)
{
    /// <summary>
    /// Clamps every edge to a non-negative value.
    /// </summary>
    /// <returns>A normalized padding value.</returns>
    public GraphPadding ClampNonNegative()
        => new(
            Math.Max(0d, Left),
            Math.Max(0d, Top),
            Math.Max(0d, Right),
            Math.Max(0d, Bottom));
}
