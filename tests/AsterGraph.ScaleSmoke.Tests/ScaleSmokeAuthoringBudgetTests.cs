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
            "SCALE_AUTHORING_BUDGET:baseline:stencil<=100:command-surface<=250:quick-tool-projection<=100:quick-tool-execution<=150:inspector-open<=50:node-resize<=30:edge-create<=50",
            marker);
    }

    [Fact]
    public void LargeTier_EmitsMachineReadableAuthoringBudgetMarker()
    {
        var tier = ScaleSmokeTier.Parse(["--tier", "large"]);

        var marker = tier.ToAuthoringBudgetMarker();

        Assert.Equal(
            "SCALE_AUTHORING_BUDGET:large:stencil<=150:command-surface<=400:quick-tool-projection<=150:quick-tool-execution<=200:inspector-open<=100:node-resize<=60:edge-create<=100",
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
            QuickToolExecutionMs: 21,
            InspectorOpenMs: 0,
            NodeResizeMs: 0,
            EdgeCreateMs: 0);

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
            QuickToolExecutionMs: 21,
            InspectorOpenMs: 0,
            NodeResizeMs: 0,
            EdgeCreateMs: 0);

        var result = tier.EvaluateAuthoring(metrics);

        Assert.False(result.Passed);
        Assert.Contains("command-surface=401>400(defended)", result.FailureSummary, StringComparison.Ordinal);
        var failure = Assert.Single(result.Failures);
        Assert.Equal(
            "SCALE_BUDGET_FAILURE:large:area=authoring:metric=command-surface:actual=401:threshold=400:policy=defended",
            failure.ToMarker());
    }
}
