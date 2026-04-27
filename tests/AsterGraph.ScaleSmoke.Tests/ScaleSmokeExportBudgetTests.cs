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
            "SCALE_EXPORT_BUDGET:baseline:svg<=300:png<=2500:jpeg<=3500:reload<=250",
            marker);
    }

    [Fact]
    public void BaselineExportBudget_AllowsObservedGithubRunnerMetrics()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);
        var metrics = new ScaleSmokeExportMetrics(
            SvgExportMs: 9,
            PngExportMs: 2395,
            JpegExportMs: 2911,
            ReloadMs: 39);

        var result = tier.EvaluateExport(metrics);

        Assert.True(result.Passed);
        Assert.Equal("none", result.FailureSummary);
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
    public void StressTier_EmitsDefendedRasterExportBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "stress"]);

        var marker = tier.ToExportBudgetMarker();

        Assert.Equal(
            "SCALE_EXPORT_BUDGET:stress:svg<=300:png<=120000:jpeg<=100000:reload<=800",
            marker);
        Assert.True(tier.HasDefendedRasterExportBudget);
    }

    [Fact]
    public void XLargeTier_EmitsTelemetryOnlyExportBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "xlarge"]);

        var marker = tier.ToExportBudgetMarker();
        var result = tier.EvaluateExport(new ScaleSmokeExportMetrics(1, 1, 1, 1));

        Assert.Equal(
            "SCALE_EXPORT_BUDGET:xlarge:budget=informational-only",
            marker);
        Assert.True(result.Passed);
        Assert.Equal("informational-only", result.FailureSummary);
        Assert.False(tier.HasDefendedRasterExportBudget);
        Assert.Null(tier.ToRasterExportStressMarker(result));
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
        Assert.Contains("png=16001>16000(defended)", result.FailureSummary, StringComparison.Ordinal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(
            "SCALE_BUDGET_FAILURE:large:area=export:metric=png:actual=16001:threshold=16000:policy=defended",
            failure.ToMarker());
    }

    [Fact]
    public void StressExportBudget_AllowsInitialDefendedRasterRedlines()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "stress"]);
        var metrics = new ScaleSmokeExportMetrics(
            SvgExportMs: 120,
            PngExportMs: 120_000,
            JpegExportMs: 100_000,
            ReloadMs: 400);

        var result = tier.EvaluateExport(metrics);

        Assert.True(result.Passed);
        Assert.Empty(result.Failures);
        Assert.Equal("SCALE_RASTER_EXPORT_STRESS_OK:True", tier.ToRasterExportStressMarker(result));
    }

    [Fact]
    public void StressExportBudget_RejectsRasterRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "stress"]);
        var metrics = new ScaleSmokeExportMetrics(
            SvgExportMs: 120,
            PngExportMs: 120_001,
            JpegExportMs: 100_001,
            ReloadMs: 400);

        var result = tier.EvaluateExport(metrics);

        Assert.False(result.Passed);
        Assert.Contains("png=120001>120000(defended)", result.FailureSummary, StringComparison.Ordinal);
        Assert.Contains("jpeg=100001>100000(defended)", result.FailureSummary, StringComparison.Ordinal);
        Assert.Equal("SCALE_RASTER_EXPORT_STRESS_OK:False", tier.ToRasterExportStressMarker(result));
        Assert.Collection(
            result.Failures,
            failure => Assert.Equal(
                "SCALE_BUDGET_FAILURE:stress:area=export:metric=png:actual=120001:threshold=120000:policy=defended",
                failure.ToMarker()),
            failure => Assert.Equal(
                "SCALE_BUDGET_FAILURE:stress:area=export:metric=jpeg:actual=100001:threshold=100000:policy=defended",
                failure.ToMarker()));
    }

    [Fact]
    public void StressExportBudget_RejectsPromotedSvgRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "stress"]);
        var metrics = new ScaleSmokeExportMetrics(
            SvgExportMs: 301,
            PngExportMs: 32_000,
            JpegExportMs: 24_000,
            ReloadMs: 400);

        var result = tier.EvaluateExport(metrics);

        Assert.False(result.Passed);
        Assert.Contains("svg=301>300(defended)", result.FailureSummary, StringComparison.Ordinal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(
            "SCALE_BUDGET_FAILURE:stress:area=export:metric=svg:actual=301:threshold=300:policy=defended",
            failure.ToMarker());
    }
}
