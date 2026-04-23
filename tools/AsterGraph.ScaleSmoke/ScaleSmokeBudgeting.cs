namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeMetrics(
    long SetupMs,
    long SelectionMs,
    long ConnectionMs,
    long HistoryMs,
    long ViewportMs,
    long SaveMs,
    long ReloadMs);

public sealed record ScaleSmokeAuthoringMetrics(
    long StencilFilterMs,
    long CommandSurfaceRefreshMs,
    long QuickToolProjectionMs,
    long QuickToolExecutionMs)
{
    public string ToMarker(string tierId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tierId);

        return string.Join(
            ':',
            [
                $"SCALE_AUTHORING_METRICS:{tierId}",
                $"stencil={StencilFilterMs}",
                $"command-surface={CommandSurfaceRefreshMs}",
                $"quick-tool-projection={QuickToolProjectionMs}",
                $"quick-tool-execution={QuickToolExecutionMs}",
            ]);
    }
}

public sealed record ScaleSmokeBudget(
    long SetupMs,
    long SelectionMs,
    long ConnectionMs,
    long HistoryMs,
    long ViewportMs,
    long SaveMs,
    long ReloadMs);

public sealed record ScaleSmokeAuthoringBudget(
    long StencilFilterMs,
    long CommandSurfaceRefreshMs,
    long QuickToolProjectionMs,
    long QuickToolExecutionMs);

public sealed record ScaleSmokeBudgetEvaluation(bool Passed, string FailureSummary);

public sealed record ScaleSmokeAuthoringBudgetEvaluation(bool Passed, string FailureSummary);

public sealed record ScaleSmokeTier(
    string Id,
    int NodeCount,
    int SelectionCount,
    int MoveCount,
    ScaleSmokeBudget? Budget,
    ScaleSmokeAuthoringBudget? AuthoringBudget)
{
    public bool EnforceBudgets => Budget is not null;

    public bool EnforceAuthoringBudgets => AuthoringBudget is not null;

    public string ToBudgetMarker()
    {
        if (Budget is null)
        {
            return $"SCALE_TIER_BUDGET:{Id}:nodes={NodeCount}:selection={SelectionCount}:moves={MoveCount}:budget=informational-only";
        }

        return string.Join(
            ':',
            [
                $"SCALE_TIER_BUDGET:{Id}",
                $"nodes={NodeCount}",
                $"selection={SelectionCount}",
                $"moves={MoveCount}",
                $"setup<={Budget.SetupMs}",
                $"selection<={Budget.SelectionMs}",
                $"connection<={Budget.ConnectionMs}",
                $"history<={Budget.HistoryMs}",
                $"viewport<={Budget.ViewportMs}",
                $"save<={Budget.SaveMs}",
                $"reload<={Budget.ReloadMs}",
            ]);
    }

    public string ToAuthoringBudgetMarker()
    {
        if (AuthoringBudget is null)
        {
            return $"SCALE_AUTHORING_BUDGET:{Id}:budget=informational-only";
        }

        return string.Join(
            ':',
            [
                $"SCALE_AUTHORING_BUDGET:{Id}",
                $"stencil<={AuthoringBudget.StencilFilterMs}",
                $"command-surface<={AuthoringBudget.CommandSurfaceRefreshMs}",
                $"quick-tool-projection<={AuthoringBudget.QuickToolProjectionMs}",
                $"quick-tool-execution<={AuthoringBudget.QuickToolExecutionMs}",
            ]);
    }

    public ScaleSmokeBudgetEvaluation Evaluate(ScaleSmokeMetrics metrics)
    {
        if (Budget is null)
        {
            return new ScaleSmokeBudgetEvaluation(true, "informational-only");
        }

        var failures = new List<string>();

        if (metrics.SetupMs > Budget.SetupMs)
        {
            failures.Add($"setup>{Budget.SetupMs}");
        }

        if (metrics.SelectionMs > Budget.SelectionMs)
        {
            failures.Add($"selection>{Budget.SelectionMs}");
        }

        if (metrics.ConnectionMs > Budget.ConnectionMs)
        {
            failures.Add($"connection>{Budget.ConnectionMs}");
        }

        if (metrics.HistoryMs > Budget.HistoryMs)
        {
            failures.Add($"history>{Budget.HistoryMs}");
        }

        if (metrics.ViewportMs > Budget.ViewportMs)
        {
            failures.Add($"viewport>{Budget.ViewportMs}");
        }

        if (metrics.SaveMs > Budget.SaveMs)
        {
            failures.Add($"save>{Budget.SaveMs}");
        }

        if (metrics.ReloadMs > Budget.ReloadMs)
        {
            failures.Add($"reload>{Budget.ReloadMs}");
        }

        return failures.Count == 0
            ? new ScaleSmokeBudgetEvaluation(true, "none")
            : new ScaleSmokeBudgetEvaluation(false, string.Join(',', failures));
    }

    public ScaleSmokeAuthoringBudgetEvaluation EvaluateAuthoring(ScaleSmokeAuthoringMetrics metrics)
    {
        if (AuthoringBudget is null)
        {
            return new ScaleSmokeAuthoringBudgetEvaluation(true, "informational-only");
        }

        var failures = new List<string>();

        if (metrics.StencilFilterMs > AuthoringBudget.StencilFilterMs)
        {
            failures.Add($"stencil>{AuthoringBudget.StencilFilterMs}");
        }

        if (metrics.CommandSurfaceRefreshMs > AuthoringBudget.CommandSurfaceRefreshMs)
        {
            failures.Add($"command-surface>{AuthoringBudget.CommandSurfaceRefreshMs}");
        }

        if (metrics.QuickToolProjectionMs > AuthoringBudget.QuickToolProjectionMs)
        {
            failures.Add($"quick-tool-projection>{AuthoringBudget.QuickToolProjectionMs}");
        }

        if (metrics.QuickToolExecutionMs > AuthoringBudget.QuickToolExecutionMs)
        {
            failures.Add($"quick-tool-execution>{AuthoringBudget.QuickToolExecutionMs}");
        }

        return failures.Count == 0
            ? new ScaleSmokeAuthoringBudgetEvaluation(true, "none")
            : new ScaleSmokeAuthoringBudgetEvaluation(false, string.Join(',', failures));
    }

    public static ScaleSmokeTier Parse(string[] args)
    {
        var requestedTier = "baseline";

        for (var index = 0; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--tier", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                requestedTier = args[index + 1].Trim();
                index++;
            }
        }

        return requestedTier.ToLowerInvariant() switch
        {
            "baseline" => new ScaleSmokeTier(
                "baseline",
                NodeCount: 180,
                SelectionCount: 48,
                MoveCount: 24,
                Budget: new ScaleSmokeBudget(
                    SetupMs: 1500,
                    SelectionMs: 500,
                    ConnectionMs: 150,
                    HistoryMs: 400,
                    ViewportMs: 150,
                    SaveMs: 150,
                    ReloadMs: 1200),
                AuthoringBudget: new ScaleSmokeAuthoringBudget(
                    StencilFilterMs: 100,
                    CommandSurfaceRefreshMs: 250,
                    QuickToolProjectionMs: 100,
                    QuickToolExecutionMs: 150)),
            "large" => new ScaleSmokeTier(
                "large",
                NodeCount: 1000,
                SelectionCount: 128,
                MoveCount: 64,
                Budget: new ScaleSmokeBudget(
                    SetupMs: 2500,
                    SelectionMs: 750,
                    ConnectionMs: 350,
                    HistoryMs: 800,
                    ViewportMs: 200,
                    SaveMs: 300,
                    ReloadMs: 1500),
                AuthoringBudget: new ScaleSmokeAuthoringBudget(
                    StencilFilterMs: 150,
                    CommandSurfaceRefreshMs: 400,
                    QuickToolProjectionMs: 150,
                    QuickToolExecutionMs: 200)),
            "stress" => new ScaleSmokeTier(
                "stress",
                NodeCount: 5000,
                SelectionCount: 256,
                MoveCount: 96,
                Budget: null,
                AuthoringBudget: null),
            _ => throw new ArgumentException($"Unsupported ScaleSmoke tier '{requestedTier}'. Supported tiers: baseline, large, stress.")
        };
    }
}
