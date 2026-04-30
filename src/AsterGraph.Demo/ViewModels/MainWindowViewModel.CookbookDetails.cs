using CommunityToolkit.Mvvm.ComponentModel;

namespace AsterGraph.Demo.ViewModels;

public sealed record CookbookDetailMode(string Key, string DisplayName);

public partial class MainWindowViewModel
{
    private IReadOnlyList<CookbookDetailMode> _cookbookDetailModes = [];

    public IReadOnlyList<CookbookDetailMode> CookbookDetailModes => _cookbookDetailModes;

    public IReadOnlyList<string> SelectedCookbookWorkspaceGraphLines
        => FormatCookbookAnchors(T("图示上下文：", "Graph context: "), SelectedCookbookRecipe.DemoAnchors).ToArray();

    public IReadOnlyList<string> SelectedCookbookWorkspaceCoverageLines
    {
        get
        {
            var selected = CookbookWorkspace.SelectedRecipe;

            return
            [
                T("路线状态：", "Route status: ") + selected.RouteStatus,
                T("路线说明：", "Route note: ") + selected.RouteStatusDescription,
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
}
