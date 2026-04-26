using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeExportBudgetTests
{
    [Fact]
    public void BaselineTier_EmitsMachineReadableExportBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);

        var marker = tier.ToExportBudgetMarker();

        Assert.Equal(
            "SCALE_EXPORT_BUDGET:baseline:svg<=300:png<=2500:jpeg<=1800:reload<=250",
            marker);
    }

    [Fact]
    public void LargeTier_EmitsMachineReadableExportBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);

        var marker = tier.ToExportBudgetMarker();

        Assert.Equal(
            "SCALE_EXPORT_BUDGET:large:svg<=300:png<=16000:jpeg<=12000:reload<=400",
            marker);
    }

    [Fact]
    public void LargeExportBudget_AllowsObservedMetrics()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var metrics = new ScaleSmokeExportMetrics(
            SvgExportMs: 220,
            PngExportMs: 640,
            JpegExportMs: 610,
            ReloadMs: 44);

        var result = tier.EvaluateExport(metrics);

        Assert.True(result.Passed);
        Assert.Equal("none", result.FailureSummary);
    }

    [Fact]
    public void LargeExportBudget_RejectsPngRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var metrics = new ScaleSmokeExportMetrics(
            SvgExportMs: 220,
            PngExportMs: 16001,
            JpegExportMs: 610,
            ReloadMs: 44);

        var result = tier.EvaluateExport(metrics);

        Assert.False(result.Passed);
        Assert.Contains("png>16000", result.FailureSummary, StringComparison.Ordinal);
    }
}
