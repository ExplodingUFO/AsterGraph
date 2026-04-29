using System.ComponentModel;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

/// <summary>
/// Calculates node port anchor positions from node bounds and port ordering.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class PortAnchorCalculator
{
    private const double HeaderHeight = 60;
    private const double PortRowHeight = 30;
    private const double LeftMargin = 14;
    private const double RightMargin = 14;

    /// <summary>
    /// Gets the anchor point for a port within the given node bounds.
    /// </summary>
    /// <param name="bounds">Node bounds in world space.</param>
    /// <param name="direction">Port direction.</param>
    /// <param name="index">Zero-based port index within its side.</param>
    /// <param name="total">Total number of ports on that side.</param>
    /// <returns>The computed anchor point.</returns>
    public static GraphPoint GetAnchor(
        NodeBounds bounds,
        PortDirection direction,
        int index,
        int total)
    {
        var safeTotal = Math.Max(total, 1);
        var contentHeight = Math.Max(bounds.Height - HeaderHeight - 22, PortRowHeight);
        var step = contentHeight / safeTotal;
        var y = bounds.Y + HeaderHeight + (step * index) + (step / 2);
        var x = direction == PortDirection.Input
            ? bounds.X + LeftMargin
            : bounds.X + bounds.Width - RightMargin;

        return new GraphPoint(x, y);
    }
}
