namespace AsterGraph.ScaleSmoke;

public sealed record ScaleSmokeMetrics(
    long SetupMs,
    long SelectionMs,
    long ConnectionMs,
    long HistoryMs,
    long ViewportMs,
    long SaveMs,
    long ReloadMs);

public sealed record ScaleSmokeBudget(
    long SetupMs,
    long SelectionMs,
    long ConnectionMs,
    long HistoryMs,
    long ViewportMs,
    long SaveMs,
    long ReloadMs);

public sealed record ScaleSmokeBudgetEvaluation(bool Passed, string FailureSummary);

public sealed record ScaleSmokeTier(
    string Id,
    int NodeCount,
    int SelectionCount,
    int MoveCount,
    ScaleSmokeBudget? Budget)
{
    public bool EnforceBudgets => Budget is not null;

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
                    ReloadMs: 1200)),
            "large" => new ScaleSmokeTier(
                "large",
                NodeCount: 1000,
                SelectionCount: 128,
                MoveCount: 64,
                Budget: null),
            "stress" => new ScaleSmokeTier(
                "stress",
                NodeCount: 5000,
                SelectionCount: 256,
                MoveCount: 96,
                Budget: null),
            _ => throw new ArgumentException($"Unsupported ScaleSmoke tier '{requestedTier}'. Supported tiers: baseline, large, stress.")
        };
    }
}
