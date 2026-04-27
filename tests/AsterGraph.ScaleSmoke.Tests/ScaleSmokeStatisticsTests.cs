using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeStatisticsTests
{
    [Fact]
    public void RunConfiguration_DefaultsToSingleSample()
    {
        var configuration = ScaleSmokeRunConfiguration.Parse(["--tier", "large"]);

        Assert.Equal("large", configuration.Tier.Id);
        Assert.Equal(1, configuration.Samples);
    }

    [Fact]
    public void RunConfiguration_ParsesExplicitSampleCount()
    {
        var configuration = ScaleSmokeRunConfiguration.Parse(["--tier", "stress", "--samples", "5"]);

        Assert.Equal("stress", configuration.Tier.Id);
        Assert.Equal(5, configuration.Samples);
    }

    [Fact]
    public void RunConfiguration_ParsesXLargeTelemetryOnlyTier()
    {
        var configuration = ScaleSmokeRunConfiguration.Parse(["--tier", "xlarge", "--samples", "1"]);

        Assert.Equal("xlarge", configuration.Tier.Id);
        Assert.Equal(10_000, configuration.Tier.NodeCount);
        Assert.False(configuration.Tier.EnforceBudgets);
        Assert.False(configuration.Tier.EnforceAuthoringBudgets);
        Assert.False(configuration.Tier.EnforceExportBudgets);
    }

    [Fact]
    public void SummaryMarker_EmitsNearestRankPercentiles()
    {
        var summary = ScaleSmokeMetricSummary.FromSamples(
            "large",
            [
                new ScaleSmokeMetrics(100, 300, 40, 200, 10, 15, 20),
                new ScaleSmokeMetrics(120, 330, 45, 230, 12, 16, 22),
                new ScaleSmokeMetrics(140, 360, 50, 260, 14, 17, 24),
                new ScaleSmokeMetrics(160, 390, 55, 290, 16, 18, 26),
                new ScaleSmokeMetrics(180, 420, 60, 320, 18, 19, 28),
            ]);

        Assert.Equal(
            "SCALE_PERF_SUMMARY:large:samples=5:setup-p50=140:setup-p95=180:selection-p50=360:selection-p95=420:connection-p50=50:connection-p95=60:history-p50=260:history-p95=320:viewport-p50=14:viewport-p95=18:save-p50=17:save-p95=19:reload-p50=24:reload-p95=28",
            summary.ToMarker());
    }
}
