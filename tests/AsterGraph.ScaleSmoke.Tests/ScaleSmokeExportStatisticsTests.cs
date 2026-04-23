using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeExportStatisticsTests
{
    [Fact]
    public void ExportSummaryMarker_EmitsNearestRankPercentiles()
    {
        var summary = ScaleSmokeExportMetricSummary.FromSamples(
            "large",
            [
                new ScaleSmokeExportMetrics(100, 300, 280, 20),
                new ScaleSmokeExportMetrics(120, 340, 310, 24),
                new ScaleSmokeExportMetrics(140, 380, 340, 28),
                new ScaleSmokeExportMetrics(160, 420, 370, 32),
                new ScaleSmokeExportMetrics(180, 460, 400, 36),
            ]);

        Assert.Equal(
            "SCALE_EXPORT_SUMMARY:large:samples=5:svg-p50=140:svg-p95=180:png-p50=380:png-p95=460:jpeg-p50=340:jpeg-p95=400:reload-p50=28:reload-p95=36",
            summary.ToMarker());
    }
}
