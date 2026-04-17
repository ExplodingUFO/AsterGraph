using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeBudgetTests
{
    [Fact]
    public void BaselineBudget_AllowsObservedGithubRunnerMetrics()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);
        var metrics = new ScaleSmokeMetrics(
            SetupMs: 135,
            SelectionMs: 347,
            ConnectionMs: 35,
            HistoryMs: 190,
            ViewportMs: 4,
            SaveMs: 9,
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
