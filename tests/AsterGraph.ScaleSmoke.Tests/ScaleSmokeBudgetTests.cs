using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeBudgetTests
{
    [Fact]
    public void BaselineTier_EmitsMachineReadableBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);

        var marker = tier.ToBudgetMarker();

        Assert.Equal(
            "SCALE_TIER_BUDGET:baseline:nodes=180:selection=48:moves=24:setup<=1500:selection<=500:connection<=150:history<=1500:viewport<=150:save<=1300:reload<=1200",
            marker);
    }

    [Fact]
    public void LargeTier_EmitsMachineReadableBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);

        var marker = tier.ToBudgetMarker();

        Assert.Equal(
            "SCALE_TIER_BUDGET:large:nodes=1000:selection=128:moves=64:setup<=2500:selection<=750:connection<=350:history<=800:viewport<=200:save<=300:reload<=1500",
            marker);
    }

    [Fact]
    public void StressTier_EmitsDefendedPerformanceBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "stress"]);

        var marker = tier.ToBudgetMarker();

        Assert.Equal(
            "SCALE_TIER_BUDGET:stress:nodes=5000:selection=256:moves=96:setup<=1500:selection<=200:connection<=1500:history<=2500:viewport<=100:save<=700:reload<=500",
            marker);
    }

    [Fact]
    public void LargeBudget_AllowsObservedRepeatedLargeTierMetrics()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var metrics = new ScaleSmokeMetrics(
            SetupMs: 202,
            SelectionMs: 11,
            ConnectionMs: 163,
            HistoryMs: 271,
            ViewportMs: 2,
            SaveMs: 55,
            ReloadMs: 26);

        var result = tier.Evaluate(metrics);

        Assert.True(result.Passed);
        Assert.Equal("none", result.FailureSummary);
    }

    [Fact]
    public void LargeBudget_RejectsConnectionRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var metrics = new ScaleSmokeMetrics(
            SetupMs: 202,
            SelectionMs: 11,
            ConnectionMs: 351,
            HistoryMs: 271,
            ViewportMs: 2,
            SaveMs: 55,
            ReloadMs: 26);

        var result = tier.Evaluate(metrics);

        Assert.False(result.Passed);
        Assert.Contains("connection=351>350(defended)", result.FailureSummary, StringComparison.Ordinal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(
            "SCALE_BUDGET_FAILURE:large:area=performance:metric=connection:actual=351:threshold=350:policy=defended",
            failure.ToMarker());
    }

    [Fact]
    public void StressBudget_RejectsPromotedConnectionRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "stress"]);
        var metrics = new ScaleSmokeMetrics(
            SetupMs: 600,
            SelectionMs: 60,
            ConnectionMs: 1501,
            HistoryMs: 1400,
            ViewportMs: 5,
            SaveMs: 300,
            ReloadMs: 150);

        var result = tier.Evaluate(metrics);

        Assert.False(result.Passed);
        Assert.Contains("connection=1501>1500(defended)", result.FailureSummary, StringComparison.Ordinal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(
            "SCALE_BUDGET_FAILURE:stress:area=performance:metric=connection:actual=1501:threshold=1500:policy=defended",
            failure.ToMarker());
    }

    [Fact]
    public void BaselineBudget_AllowsObservedGithubRunnerMetrics()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);
        var metrics = new ScaleSmokeMetrics(
            SetupMs: 135,
            SelectionMs: 347,
            ConnectionMs: 35,
            HistoryMs: 1016,
            ViewportMs: 4,
            SaveMs: 941,
            ReloadMs: 5);

        var result = tier.Evaluate(metrics);

        Assert.True(result.Passed);
        Assert.Equal("none", result.FailureSummary);
    }

    [Fact]
    public void BaselineBudget_StillRejectsSelectionRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);
        var metrics = new ScaleSmokeMetrics(
            SetupMs: 135,
            SelectionMs: 501,
            ConnectionMs: 35,
            HistoryMs: 190,
            ViewportMs: 4,
            SaveMs: 9,
            ReloadMs: 5);

        var result = tier.Evaluate(metrics);

        Assert.False(result.Passed);
        Assert.Contains("selection=501>500(defended)", result.FailureSummary, StringComparison.Ordinal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(
            "SCALE_BUDGET_FAILURE:baseline:area=performance:metric=selection:actual=501:threshold=500:policy=defended",
            failure.ToMarker());
    }
}
