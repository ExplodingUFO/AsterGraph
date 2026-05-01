using AsterGraph.Avalonia.Controls;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GridBackgroundTests
{
    [Fact]
    public void CalculateVisibleLineMetrics_WithExtremeZoomSpacing_KeepsLineDensityBounded()
    {
        var metrics = GridBackground.CalculateVisibleLineMetrics(
            length: 1440d,
            rawSpacing: 20d * 0.000001d,
            offset: -100000d);

        Assert.InRange(metrics.Spacing, 22d, 44d);
        Assert.InRange(metrics.Offset, 0d, metrics.Spacing);
        Assert.InRange(metrics.LineCount, 1, 67);
    }
}
