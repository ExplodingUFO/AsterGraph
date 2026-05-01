using CommunityToolkit.Mvvm.ComponentModel;

namespace AsterGraph.Demo.ViewModels;

public sealed record CookbookDetailMode(string Key, string DisplayName);

public partial class MainWindowViewModel
{
    private IReadOnlyList<CookbookDetailMode> _cookbookDetailModes = [];

    public IReadOnlyList<CookbookDetailMode> CookbookDetailModes => _cookbookDetailModes;

    public string CookbookGraphDemoSectionTitle => T("Code / Demo", "Code / Demo");

    public string CookbookWorkflowSectionTitle => T("Workflow Step", "Workflow Step");

    public string CookbookProofSupportSectionTitle => T("Proof / Support", "Proof / Support");

    public string CookbookDetailSectionTitle => T("Detail View", "Detail View");

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
                .. CookbookWorkspace.SelectedRecipe.ComponentShowcaseLines.Select(line => T("组件展示：", "Component showcase: ") + line),
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
                T("支持路线：", "Supported route: ") + selected.RouteClarity.SupportedRoute,
                T("包边界：", "Package boundary: ") + selected.RouteClarity.PackageBoundary,
                T("Demo 边界：", "Demo boundary: ") + selected.RouteClarity.DemoBoundary,
                T("路线说明：", "Route note: ") + selected.RouteStatusDescription,
                T("不可用操作：", "Unavailable action: ") + selected.UnavailableActionDescription,
                .. selected.DeferredGaps.Select(gap => T("延后缺口：", "Deferred gap: ") + gap),
            ];
        }
    }

    public IReadOnlyList<string> SelectedCookbookWorkspaceWorkflowStepLines
    {
        get
        {
            var scenario = SelectedCookbookScenarioPoint;
            var workflowSteps = CookbookWorkspace.SelectedRecipe.WorkflowSteps;

            return
            [
                T("Step：", "Step: ") + scenario.Label,
                T("Graph：", "Graph: ") + scenario.GraphCueLabel + " -> " + scenario.GraphCueTarget,
                T("Content：", "Content: ") + scenario.ContentCue,
                .. workflowSteps.Select(FormatCookbookWorkflowStep),
            ];
        }
    }

    public IReadOnlyList<string> SelectedCookbookWorkspaceProofSupportLines =>
    [
        .. SelectedCookbookRecipe.ProofMarkers.Select(marker => T("Proof：", "Proof: ") + marker),
        .. FormatCookbookAnchors(T("Docs：", "Docs: "), SelectedCookbookRecipe.DocumentationAnchors),
        T("Support：", "Support: ") + SelectedCookbookRecipe.SupportBoundary,
    ];

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
            "interaction" =>
            [
                .. FormatCookbookInteractionFacets(CookbookWorkspace.SelectedRecipe.InteractionFacets),
            ],
            "support" =>
            [
                T("支持边界：", "Support boundary: ") + SelectedCookbookRecipe.Title,
                SelectedCookbookRecipe.SupportBoundary,
            ],
            _ =>
            [
                .. FormatCookbookDetailAnchors(SelectedCookbookRecipe.CodeAnchors),
                .. FormatCookbookDetailAnchors(SelectedCookbookRecipe.DemoAnchors),
                .. CookbookWorkspace.SelectedRecipe.ComponentShowcaseLines.Select(line => T("组件展示：", "Component showcase: ") + line),
            ],
        };

    [ObservableProperty]
    private CookbookDetailMode selectedCookbookDetailMode = null!;

    private void RebuildCookbookDetailModes(string? selectedKey)
    {
        _cookbookDetailModes =
        [
            new CookbookDetailMode("code", T("Code / Demo", "Code / Demo")),
            new CookbookDetailMode("proof", T("证明", "Proof")),
            new CookbookDetailMode("docs", T("文档", "Docs")),
            new CookbookDetailMode("scenario", T("Workflow Step", "Workflow Step")),
            new CookbookDetailMode("interaction", T("交互", "Interaction")),
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

    private IEnumerable<string> FormatCookbookInteractionFacets(
        IReadOnlyList<Cookbook.DemoCookbookWorkspaceInteractionFacet> interactionFacets)
        => interactionFacets.Select(facet => FormatCookbookInteractionKind(facet.Kind)
                                             + facet.Label + Environment.NewLine
                                             + T("焦点：", "Focus: ") + facet.FocusLabel
                                             + Environment.NewLine
                                             + T("目标：", "Target: ") + facet.FocusTarget);

    private string FormatCookbookWorkflowStep(Cookbook.DemoCookbookWorkspaceWorkflowStep step)
        => T("Workflow：", "Workflow: ") + FormatCookbookWorkflowKind(step.Kind)
           + step.Title + Environment.NewLine
           + T("命令：", "Command: ") + step.CommandId + Environment.NewLine
           + T("代码目标：", "Code target: ") + step.CodeTarget + Environment.NewLine
           + T("Demo 目标：", "Demo target: ") + step.DemoTarget + Environment.NewLine
           + T("证明：", "Proof: ") + step.ProofMarker;

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

    private string FormatCookbookInteractionKind(Cookbook.DemoCookbookInteractionKind kind)
        => kind switch
        {
            Cookbook.DemoCookbookInteractionKind.Selection => T("选择：", "Selection: "),
            Cookbook.DemoCookbookInteractionKind.Connection => T("连接：", "Connection: "),
            Cookbook.DemoCookbookInteractionKind.LayoutReadability => T("布局/可读性：", "Layout/readability: "),
            Cookbook.DemoCookbookInteractionKind.Inspection => T("检查：", "Inspection: "),
            Cookbook.DemoCookbookInteractionKind.ValidationRuntimeFeedback => T("验证/运行反馈：", "Validation/runtime feedback: "),
            _ => kind + ": ",
        };

    private string FormatCookbookWorkflowKind(Cookbook.DemoCookbookWorkflowKind kind)
        => kind switch
        {
            Cookbook.DemoCookbookWorkflowKind.CommandRegistry => T("命令注册：", "Command registry: "),
            Cookbook.DemoCookbookWorkflowKind.SemanticEditing => T("语义编辑：", "Semantic editing: "),
            Cookbook.DemoCookbookWorkflowKind.TemplatePreset => T("模板预设：", "Template preset: "),
            Cookbook.DemoCookbookWorkflowKind.SelectionTransform => T("选择变换：", "Selection transform: "),
            Cookbook.DemoCookbookWorkflowKind.NavigationFocus => T("导航聚焦：", "Navigation focus: "),
            _ => kind + ": ",
        };
}
