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
        Assert.Contains("connection>350", result.FailureSummary, StringComparison.Ordinal);
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
        Assert.Contains("selection>500", result.FailureSummary, StringComparison.Ordinal);
    }
}
