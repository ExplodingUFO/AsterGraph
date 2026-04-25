using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeAuthoringStatisticsTests
{
    [Fact]
    public void AuthoringSummaryMarker_EmitsNearestRankPercentiles()
    {
        var summary = ScaleSmokeAuthoringMetricSummary.FromSamples(
            "large",
            [
                new ScaleSmokeAuthoringMetrics(10, 20, 30, 40, 0, 0, 0),
                new ScaleSmokeAuthoringMetrics(12, 24, 32, 42, 0, 0, 0),
                new ScaleSmokeAuthoringMetrics(14, 28, 34, 44, 0, 0, 0),
                new ScaleSmokeAuthoringMetrics(16, 32, 36, 46, 0, 0, 0),
                new ScaleSmokeAuthoringMetrics(18, 36, 38, 48, 0, 0, 0),
            ]);

        Assert.Equal(
            "SCALE_AUTHORING_SUMMARY:large:samples=5:stencil-p50=14:stencil-p95=18:command-surface-p50=28:command-surface-p95=36:quick-tool-projection-p50=34:quick-tool-projection-p95=38:quick-tool-execution-p50=44:quick-tool-execution-p95=48:inspector-open-p50=0:inspector-open-p95=0:node-resize-p50=0:node-resize-p95=0:edge-create-p50=0:edge-create-p95=0",
            summary.ToMarker());
    }
}
