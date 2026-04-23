using AsterGraph.ScaleSmoke;
using Xunit;

namespace AsterGraph.ScaleSmoke.Tests;

public sealed class ScaleSmokeAuthoringBudgetTests
{
    [Fact]
    public void BaselineTier_EmitsMachineReadableAuthoringBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "baseline"]);

        var marker = tier.ToAuthoringBudgetMarker();

        Assert.Equal(
            "SCALE_AUTHORING_BUDGET:baseline:stencil<=100:command-surface<=250:quick-tool-projection<=100:quick-tool-execution<=150",
            marker);
    }

    [Fact]
    public void LargeTier_EmitsMachineReadableAuthoringBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);

        var marker = tier.ToAuthoringBudgetMarker();

        Assert.Equal(
            "SCALE_AUTHORING_BUDGET:large:stencil<=150:command-surface<=400:quick-tool-projection<=150:quick-tool-execution<=200",
            marker);
    }

    [Fact]
    public void LargeAuthoringBudget_AllowsObservedMetrics()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var metrics = new ScaleSmokeAuthoringMetrics(
            StencilFilterMs: 12,
            CommandSurfaceRefreshMs: 28,
            QuickToolProjectionMs: 16,
            QuickToolExecutionMs: 21);

        var result = tier.EvaluateAuthoring(metrics);

        Assert.True(result.Passed);
        Assert.Equal("none", result.FailureSummary);
    }

    [Fact]
    public void LargeAuthoringBudget_RejectsCommandSurfaceRegressionBeyondRedline()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);
        var metrics = new ScaleSmokeAuthoringMetrics(
            StencilFilterMs: 12,
            CommandSurfaceRefreshMs: 401,
            QuickToolProjectionMs: 16,
            QuickToolExecutionMs: 21);

        var result = tier.EvaluateAuthoring(metrics);

        Assert.False(result.Passed);
        Assert.Contains("command-surface>400", result.FailureSummary, StringComparison.Ordinal);
    }
}
