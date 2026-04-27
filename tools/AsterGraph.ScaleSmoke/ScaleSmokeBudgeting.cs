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
    long QuickToolExecutionMs,
    long InspectorOpenMs,
    long NodeResizeMs,
    long EdgeCreateMs);

public sealed record ScaleSmokeExportBudget(
    long SvgExportMs,
    long PngExportMs,
    long JpegExportMs,
    long ReloadMs);

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
                $"inspector-open<={AuthoringBudget.InspectorOpenMs}",
                $"node-resize<={AuthoringBudget.NodeResizeMs}",
                $"edge-create<={AuthoringBudget.EdgeCreateMs}",
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
                $"svg<={ExportBudget.SvgExportMs}",
                $"png<={ExportBudget.PngExportMs}",
                $"jpeg<={ExportBudget.JpegExportMs}",
                $"reload<={ExportBudget.ReloadMs}",
            ]);
    }

    public ScaleSmokeBudgetEvaluation Evaluate(ScaleSmokeMetrics metrics)
    {
        if (Budget is null)
        {
            return new ScaleSmokeBudgetEvaluation(true, "informational-only");
        }

        var failures = new List<ScaleSmokeBudgetFailure>();

        if (metrics.SetupMs > Budget.SetupMs)
        {
            AddFailure(failures, "performance", "setup", metrics.SetupMs, Budget.SetupMs);
        }

        if (metrics.SelectionMs > Budget.SelectionMs)
        {
            AddFailure(failures, "performance", "selection", metrics.SelectionMs, Budget.SelectionMs);
        }

        if (metrics.ConnectionMs > Budget.ConnectionMs)
        {
            AddFailure(failures, "performance", "connection", metrics.ConnectionMs, Budget.ConnectionMs);
        }

        if (metrics.HistoryMs > Budget.HistoryMs)
        {
            AddFailure(failures, "performance", "history", metrics.HistoryMs, Budget.HistoryMs);
        }

        if (metrics.ViewportMs > Budget.ViewportMs)
        {
            AddFailure(failures, "performance", "viewport", metrics.ViewportMs, Budget.ViewportMs);
        }

        if (metrics.SaveMs > Budget.SaveMs)
        {
            AddFailure(failures, "performance", "save", metrics.SaveMs, Budget.SaveMs);
        }

        if (metrics.ReloadMs > Budget.ReloadMs)
        {
            AddFailure(failures, "performance", "reload", metrics.ReloadMs, Budget.ReloadMs);
        }

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

        if (metrics.StencilFilterMs > AuthoringBudget.StencilFilterMs)
        {
            AddFailure(failures, "authoring", "stencil", metrics.StencilFilterMs, AuthoringBudget.StencilFilterMs);
        }

        if (metrics.CommandSurfaceRefreshMs > AuthoringBudget.CommandSurfaceRefreshMs)
        {
            AddFailure(failures, "authoring", "command-surface", metrics.CommandSurfaceRefreshMs, AuthoringBudget.CommandSurfaceRefreshMs);
        }

        if (metrics.QuickToolProjectionMs > AuthoringBudget.QuickToolProjectionMs)
        {
            AddFailure(failures, "authoring", "quick-tool-projection", metrics.QuickToolProjectionMs, AuthoringBudget.QuickToolProjectionMs);
        }

        if (metrics.QuickToolExecutionMs > AuthoringBudget.QuickToolExecutionMs)
        {
            AddFailure(failures, "authoring", "quick-tool-execution", metrics.QuickToolExecutionMs, AuthoringBudget.QuickToolExecutionMs);
        }

        if (metrics.InspectorOpenMs > AuthoringBudget.InspectorOpenMs)
        {
            AddFailure(failures, "authoring", "inspector-open", metrics.InspectorOpenMs, AuthoringBudget.InspectorOpenMs);
        }

        if (metrics.NodeResizeMs > AuthoringBudget.NodeResizeMs)
        {
            AddFailure(failures, "authoring", "node-resize", metrics.NodeResizeMs, AuthoringBudget.NodeResizeMs);
        }

        if (metrics.EdgeCreateMs > AuthoringBudget.EdgeCreateMs)
        {
            AddFailure(failures, "authoring", "edge-create", metrics.EdgeCreateMs, AuthoringBudget.EdgeCreateMs);
        }

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

        if (metrics.SvgExportMs > ExportBudget.SvgExportMs)
        {
            AddFailure(failures, "export", "svg", metrics.SvgExportMs, ExportBudget.SvgExportMs);
        }

        if (metrics.PngExportMs > ExportBudget.PngExportMs)
        {
            AddFailure(failures, "export", "png", metrics.PngExportMs, ExportBudget.PngExportMs);
        }

        if (metrics.JpegExportMs > ExportBudget.JpegExportMs)
        {
            AddFailure(failures, "export", "jpeg", metrics.JpegExportMs, ExportBudget.JpegExportMs);
        }

        if (metrics.ReloadMs > ExportBudget.ReloadMs)
        {
            AddFailure(failures, "export", "reload", metrics.ReloadMs, ExportBudget.ReloadMs);
        }

        return failures.Count == 0
            ? new ScaleSmokeExportBudgetEvaluation(true, "none")
            : new ScaleSmokeExportBudgetEvaluation(false, string.Join(',', failures.Select(failure => failure.ToSummary())), failures);
    }

    private void AddFailure(
        List<ScaleSmokeBudgetFailure> failures,
        string area,
        string metric,
        long actual,
        long threshold)
    {
        failures.Add(new ScaleSmokeBudgetFailure(Id, area, metric, actual, threshold, Defended: true));
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
                    QuickToolProjectionMs: 150,
                    QuickToolExecutionMs: 200,
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
                Budget: null,
                AuthoringBudget: null,
                ExportBudget: null),
            _ => throw new ArgumentException($"Unsupported ScaleSmoke tier '{requestedTier}'. Supported tiers: baseline, large, stress.")
        };
    }
}
