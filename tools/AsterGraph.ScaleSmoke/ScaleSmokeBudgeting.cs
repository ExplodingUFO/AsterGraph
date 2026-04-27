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
    long QuickToolExecutionMs,
    long InspectorOpenMs,
    long NodeResizeMs,
    long EdgeCreateMs)
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
                $"inspector-open={InspectorOpenMs}",
                $"node-resize={NodeResizeMs}",
                $"edge-create={EdgeCreateMs}",
            ]);
    }
}

public sealed record ScaleSmokeExportMetrics(
    long SvgExportMs,
    long PngExportMs,
    long JpegExportMs,
    long ReloadMs)
{
    public string ToMarker(string tierId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tierId);

        return string.Join(
            ':',
            [
                $"SCALE_EXPORT_METRICS:{tierId}",
                $"svg={SvgExportMs}",
                $"png={PngExportMs}",
                $"jpeg={JpegExportMs}",
                $"reload={ReloadMs}",
            ]);
    }
}

public sealed record ScaleSmokeBudget(
    long? SetupMs,
    long? SelectionMs,
    long? ConnectionMs,
    long? HistoryMs,
    long? ViewportMs,
    long? SaveMs,
    long? ReloadMs);

public sealed record ScaleSmokeAuthoringBudget(
    long? StencilFilterMs,
    long? CommandSurfaceRefreshMs,
    long? QuickToolProjectionMs,
    long? QuickToolExecutionMs,
    long? InspectorOpenMs,
    long? NodeResizeMs,
    long? EdgeCreateMs);

public sealed record ScaleSmokeExportBudget(
    long? SvgExportMs,
    long? PngExportMs,
    long? JpegExportMs,
    long? ReloadMs);

public sealed record ScaleSmokeBudgetFailure(
    string TierId,
    string Area,
    string Metric,
    long Actual,
    long Threshold,
    bool Defended)
{
    public string ToMarker()
        => string.Join(
            ':',
            [
                $"SCALE_BUDGET_FAILURE:{TierId}",
                $"area={Area}",
                $"metric={Metric}",
                $"actual={Actual}",
                $"threshold={Threshold}",
                $"policy={(Defended ? "defended" : "informational")}",
            ]);

    public string ToSummary()
        => $"{Metric}={Actual}>{Threshold}({(Defended ? "defended" : "informational")})";
}

public sealed record ScaleSmokeBudgetEvaluation(
    bool Passed,
    string FailureSummary,
    IReadOnlyList<ScaleSmokeBudgetFailure> Failures)
{
    public ScaleSmokeBudgetEvaluation(bool passed, string failureSummary)
        : this(passed, failureSummary, [])
    {
    }
}

public sealed record ScaleSmokeAuthoringBudgetEvaluation(
    bool Passed,
    string FailureSummary,
    IReadOnlyList<ScaleSmokeBudgetFailure> Failures)
{
    public ScaleSmokeAuthoringBudgetEvaluation(bool passed, string failureSummary)
        : this(passed, failureSummary, [])
    {
    }
}

public sealed record ScaleSmokeExportBudgetEvaluation(
    bool Passed,
    string FailureSummary,
    IReadOnlyList<ScaleSmokeBudgetFailure> Failures)
{
    public ScaleSmokeExportBudgetEvaluation(bool passed, string failureSummary)
        : this(passed, failureSummary, [])
    {
    }
}

public sealed record ScaleSmokeTier(
    string Id,
    int NodeCount,
    int SelectionCount,
    int MoveCount,
    ScaleSmokeBudget? Budget,
    ScaleSmokeAuthoringBudget? AuthoringBudget,
    ScaleSmokeExportBudget? ExportBudget)
{
    public bool EnforceBudgets => Budget is not null;

    public bool EnforceAuthoringBudgets => AuthoringBudget is not null;

    public bool EnforceExportBudgets => ExportBudget is not null;

    public bool HasDefendedRasterExportBudget
        => ExportBudget?.PngExportMs is not null
        && ExportBudget.JpegExportMs is not null;

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
                FormatBudget("setup", Budget.SetupMs),
                FormatBudget("selection", Budget.SelectionMs),
                FormatBudget("connection", Budget.ConnectionMs),
                FormatBudget("history", Budget.HistoryMs),
                FormatBudget("viewport", Budget.ViewportMs),
                FormatBudget("save", Budget.SaveMs),
                FormatBudget("reload", Budget.ReloadMs),
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
                FormatBudget("stencil", AuthoringBudget.StencilFilterMs),
                FormatBudget("command-surface", AuthoringBudget.CommandSurfaceRefreshMs),
                FormatBudget("quick-tool-projection", AuthoringBudget.QuickToolProjectionMs),
                FormatBudget("quick-tool-execution", AuthoringBudget.QuickToolExecutionMs),
                FormatBudget("inspector-open", AuthoringBudget.InspectorOpenMs),
                FormatBudget("node-resize", AuthoringBudget.NodeResizeMs),
                FormatBudget("edge-create", AuthoringBudget.EdgeCreateMs),
            ]);
    }

    public string ToExportBudgetMarker()
    {
        if (ExportBudget is null)
        {
            return $"SCALE_EXPORT_BUDGET:{Id}:budget=informational-only";
        }

        return string.Join(
            ':',
            [
                $"SCALE_EXPORT_BUDGET:{Id}",
                FormatBudget("svg", ExportBudget.SvgExportMs),
                FormatBudget("png", ExportBudget.PngExportMs),
                FormatBudget("jpeg", ExportBudget.JpegExportMs),
                FormatBudget("reload", ExportBudget.ReloadMs),
            ]);
    }

    public string? ToRasterExportStressMarker(ScaleSmokeExportBudgetEvaluation evaluation)
        => string.Equals(Id, "stress", StringComparison.OrdinalIgnoreCase)
            ? $"SCALE_RASTER_EXPORT_STRESS_OK:{HasDefendedRasterExportBudget && evaluation.Passed}"
            : null;

    public ScaleSmokeBudgetEvaluation Evaluate(ScaleSmokeMetrics metrics)
    {
        if (Budget is null)
        {
            return new ScaleSmokeBudgetEvaluation(true, "informational-only");
        }

        var failures = new List<ScaleSmokeBudgetFailure>();

        AddFailureIfExceeded(failures, "performance", "setup", metrics.SetupMs, Budget.SetupMs);
        AddFailureIfExceeded(failures, "performance", "selection", metrics.SelectionMs, Budget.SelectionMs);
        AddFailureIfExceeded(failures, "performance", "connection", metrics.ConnectionMs, Budget.ConnectionMs);
        AddFailureIfExceeded(failures, "performance", "history", metrics.HistoryMs, Budget.HistoryMs);
        AddFailureIfExceeded(failures, "performance", "viewport", metrics.ViewportMs, Budget.ViewportMs);
        AddFailureIfExceeded(failures, "performance", "save", metrics.SaveMs, Budget.SaveMs);
        AddFailureIfExceeded(failures, "performance", "reload", metrics.ReloadMs, Budget.ReloadMs);

        return failures.Count == 0
            ? new ScaleSmokeBudgetEvaluation(true, "none")
            : new ScaleSmokeBudgetEvaluation(false, string.Join(',', failures.Select(failure => failure.ToSummary())), failures);
    }

    public ScaleSmokeAuthoringBudgetEvaluation EvaluateAuthoring(ScaleSmokeAuthoringMetrics metrics)
    {
        if (AuthoringBudget is null)
        {
            return new ScaleSmokeAuthoringBudgetEvaluation(true, "informational-only");
        }

        var failures = new List<ScaleSmokeBudgetFailure>();

        AddFailureIfExceeded(failures, "authoring", "stencil", metrics.StencilFilterMs, AuthoringBudget.StencilFilterMs);
        AddFailureIfExceeded(failures, "authoring", "command-surface", metrics.CommandSurfaceRefreshMs, AuthoringBudget.CommandSurfaceRefreshMs);
        AddFailureIfExceeded(failures, "authoring", "quick-tool-projection", metrics.QuickToolProjectionMs, AuthoringBudget.QuickToolProjectionMs);
        AddFailureIfExceeded(failures, "authoring", "quick-tool-execution", metrics.QuickToolExecutionMs, AuthoringBudget.QuickToolExecutionMs);
        AddFailureIfExceeded(failures, "authoring", "inspector-open", metrics.InspectorOpenMs, AuthoringBudget.InspectorOpenMs);
        AddFailureIfExceeded(failures, "authoring", "node-resize", metrics.NodeResizeMs, AuthoringBudget.NodeResizeMs);
        AddFailureIfExceeded(failures, "authoring", "edge-create", metrics.EdgeCreateMs, AuthoringBudget.EdgeCreateMs);

        return failures.Count == 0
            ? new ScaleSmokeAuthoringBudgetEvaluation(true, "none")
            : new ScaleSmokeAuthoringBudgetEvaluation(false, string.Join(',', failures.Select(failure => failure.ToSummary())), failures);
    }

    public ScaleSmokeExportBudgetEvaluation EvaluateExport(ScaleSmokeExportMetrics metrics)
    {
        if (ExportBudget is null)
        {
            return new ScaleSmokeExportBudgetEvaluation(true, "informational-only");
        }

        var failures = new List<ScaleSmokeBudgetFailure>();

        AddFailureIfExceeded(failures, "export", "svg", metrics.SvgExportMs, ExportBudget.SvgExportMs);
        AddFailureIfExceeded(failures, "export", "png", metrics.PngExportMs, ExportBudget.PngExportMs);
        AddFailureIfExceeded(failures, "export", "jpeg", metrics.JpegExportMs, ExportBudget.JpegExportMs);
        AddFailureIfExceeded(failures, "export", "reload", metrics.ReloadMs, ExportBudget.ReloadMs);

        return failures.Count == 0
            ? new ScaleSmokeExportBudgetEvaluation(true, "none")
            : new ScaleSmokeExportBudgetEvaluation(false, string.Join(',', failures.Select(failure => failure.ToSummary())), failures);
    }

    private static string FormatBudget(string metric, long? threshold)
        => threshold.HasValue
            ? $"{metric}<={threshold.Value}"
            : $"{metric}=informational";

    private void AddFailureIfExceeded(
        List<ScaleSmokeBudgetFailure> failures,
        string area,
        string metric,
        long actual,
        long? threshold)
    {
        if (threshold.HasValue && actual > threshold.Value)
        {
            failures.Add(new ScaleSmokeBudgetFailure(Id, area, metric, actual, threshold.Value, Defended: true));
        }
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
                    HistoryMs: 1500,
                    ViewportMs: 150,
                    SaveMs: 1300,
                    ReloadMs: 1200),
                AuthoringBudget: new ScaleSmokeAuthoringBudget(
                    StencilFilterMs: 100,
                    CommandSurfaceRefreshMs: 250,
                    QuickToolProjectionMs: 100,
                    QuickToolExecutionMs: 150,
                    InspectorOpenMs: 50,
                    NodeResizeMs: 30,
                    EdgeCreateMs: 50),
                ExportBudget: new ScaleSmokeExportBudget(
                    SvgExportMs: 300,
                    PngExportMs: 2500,
                    JpegExportMs: 3500,
                    ReloadMs: 250)),
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
                    QuickToolProjectionMs: 250,
                    QuickToolExecutionMs: 300,
                    InspectorOpenMs: 100,
                    NodeResizeMs: 60,
                    EdgeCreateMs: 100),
                ExportBudget: new ScaleSmokeExportBudget(
                    SvgExportMs: 300,
                    PngExportMs: 16000,
                    JpegExportMs: 12000,
                    ReloadMs: 400)),
            "stress" => new ScaleSmokeTier(
                "stress",
                NodeCount: 5000,
                SelectionCount: 256,
                MoveCount: 96,
                Budget: new ScaleSmokeBudget(
                    SetupMs: 1500,
                    SelectionMs: 200,
                    ConnectionMs: 2500,
                    HistoryMs: 2500,
                    ViewportMs: 100,
                    SaveMs: 700,
                    ReloadMs: 500),
                AuthoringBudget: new ScaleSmokeAuthoringBudget(
                    StencilFilterMs: 150,
                    CommandSurfaceRefreshMs: 800,
                    QuickToolProjectionMs: 800,
                    QuickToolExecutionMs: 1000,
                    InspectorOpenMs: 100,
                    NodeResizeMs: 150,
                    EdgeCreateMs: 250),
                ExportBudget: new ScaleSmokeExportBudget(
                    SvgExportMs: 300,
                    PngExportMs: 120_000,
                    JpegExportMs: 100_000,
                    ReloadMs: 800)),
            "xlarge" => new ScaleSmokeTier(
                "xlarge",
                NodeCount: 10_000,
                SelectionCount: 512,
                MoveCount: 128,
                Budget: null,
                AuthoringBudget: null,
                ExportBudget: null),
            _ => throw new ArgumentException($"Unsupported ScaleSmoke tier '{requestedTier}'. Supported tiers: baseline, large, stress, xlarge.")
        };
    }
}
