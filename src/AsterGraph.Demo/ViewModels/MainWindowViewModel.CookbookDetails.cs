using CommunityToolkit.Mvvm.ComponentModel;

namespace AsterGraph.Demo.ViewModels;

public sealed record CookbookDetailMode(string Key, string DisplayName);

public partial class MainWindowViewModel
{
    private IReadOnlyList<CookbookDetailMode> _cookbookDetailModes = [];

    public IReadOnlyList<CookbookDetailMode> CookbookDetailModes => _cookbookDetailModes;

    public IReadOnlyList<string> SelectedCookbookWorkspaceGraphLines
    {
        get
        {
            var scenario = SelectedCookbookScenarioPoint;

            return
            [
                .. FormatCookbookAnchors(T("图示上下文：", "Graph context: "), SelectedCookbookRecipe.DemoAnchors),
                T("当前场景：", "Selected scenario: ") + scenario.Label,
                T("图线索：", "Graph cue: ") + scenario.GraphCueLabel + " -> " + scenario.GraphCueTarget,
            ];
        }
    }

    public IReadOnlyList<string> SelectedCookbookWorkspaceCoverageLines
    {
        get
        {
            var selected = CookbookWorkspace.SelectedRecipe;

            return
            [
                T("路线状态：", "Route status: ") + selected.RouteStatus,
                T("内容线索：", "Content cue: ") + SelectedCookbookScenarioPoint.ContentCue,
                T("路线说明：", "Route note: ") + selected.RouteStatusDescription,
                T("不可用操作：", "Unavailable action: ") + selected.UnavailableActionDescription,
                .. selected.DeferredGaps.Select(gap => T("延后缺口：", "Deferred gap: ") + gap),
            ];
        }
    }

    public IReadOnlyList<string> SelectedCookbookWorkspaceDetailLines
        => SelectedCookbookDetailMode?.Key switch
        {
            "proof" => SelectedCookbookRecipe.ProofMarkers
                .Select(marker => T("证明标记：", "Proof marker: ") + marker)
                .ToArray(),
            "docs" => FormatCookbookDetailAnchors(SelectedCookbookRecipe.DocumentationAnchors).ToArray(),
            "scenario" =>
            [
                .. FormatSelectedCookbookScenarioPoint(SelectedCookbookScenarioPoint),
                .. FormatCookbookScenarioPoints(CookbookWorkspace.SelectedRecipe.ScenarioPoints),
            ],
            "support" =>
            [
                T("支持边界：", "Support boundary: ") + SelectedCookbookRecipe.Title,
                SelectedCookbookRecipe.SupportBoundary,
            ],
            _ => FormatCookbookDetailAnchors(SelectedCookbookRecipe.CodeAnchors).ToArray(),
        };

    [ObservableProperty]
    private CookbookDetailMode selectedCookbookDetailMode = null!;

    private void RebuildCookbookDetailModes(string? selectedKey)
    {
        _cookbookDetailModes =
        [
            new CookbookDetailMode("code", T("代码", "Code")),
            new CookbookDetailMode("proof", T("证明", "Proof")),
            new CookbookDetailMode("docs", T("文档", "Docs")),
            new CookbookDetailMode("scenario", T("场景", "Scenario")),
            new CookbookDetailMode("support", T("边界", "Support")),
        ];

        SelectedCookbookDetailMode = _cookbookDetailModes.Single(mode =>
            string.Equals(mode.Key, selectedKey ?? "code", StringComparison.Ordinal));
    }

    partial void OnSelectedCookbookDetailModeChanged(CookbookDetailMode value)
        => RefreshCookbookProjection();

    private IEnumerable<string> FormatCookbookDetailAnchors(IReadOnlyList<Cookbook.DemoCookbookAnchor> anchors)
        => anchors.Select(anchor => T("路径：", "Path: ") + anchor.Path + Environment.NewLine
                                    + T("证据：", "Evidence: ") + anchor.Evidence);

    private IEnumerable<string> FormatCookbookScenarioPoints(
        IReadOnlyList<Cookbook.DemoCookbookWorkspaceScenarioPoint> scenarioPoints)
        => scenarioPoints.Select(point => FormatCookbookScenarioKind(point.Kind)
                                           + point.Label + Environment.NewLine
                                           + T("证据：", "Evidence: ") + point.Evidence);

    private IEnumerable<string> FormatSelectedCookbookScenarioPoint(
        Cookbook.DemoCookbookWorkspaceScenarioPoint point)
    {
        yield return T("当前场景：", "Selected scenario: ") + point.Label;
        yield return T("图线索：", "Graph cue: ") + point.GraphCueLabel + " -> " + point.GraphCueTarget;
        yield return T("内容线索：", "Content cue: ") + point.ContentCue;
    }

    private string FormatCookbookScenarioKind(Cookbook.DemoCookbookScenarioKind kind)
        => kind switch
        {
            Cookbook.DemoCookbookScenarioKind.GraphOperations => T("图操作：", "Graph operations: "),
            Cookbook.DemoCookbookScenarioKind.NodeMetadata => T("节点元数据：", "Node metadata: "),
            Cookbook.DemoCookbookScenarioKind.ValidationRuntimeOverlay => T("验证/运行时覆盖：", "Validation/runtime overlay: "),
            Cookbook.DemoCookbookScenarioKind.SupportEvidence => T("支持证据：", "Support evidence: "),
            Cookbook.DemoCookbookScenarioKind.HostCodeExample => T("宿主代码示例：", "Host code example: "),
            _ => kind + ": ",
        };
}
