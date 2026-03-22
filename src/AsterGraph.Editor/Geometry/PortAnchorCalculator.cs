using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

public static class PortAnchorCalculator
{
    private const double HeaderHeight = 60;
    private const double PortRowHeight = 30;
    private const double LeftMargin = 14;
    private const double RightMargin = 14;

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
