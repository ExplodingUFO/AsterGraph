using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public string RuntimeSessionInterfaceName => nameof(IGraphEditorSession);

    public string RuntimeDiagnosticsSourceName => "Session.Diagnostics";

    public IReadOnlyList<string> RuntimeSignalLines =>
    [
        T("文档标题：", "Document title: ") + RuntimeDocumentTitle,
        T("节点数量：", "Node count: ") + RuntimeNodeCount,
        T("连线数量：", "Connection count: ") + RuntimeConnectionCount,
        T("当前选择：", "Current selection: ") + RuntimeSelectedNodeCount,
        T("视口缩放：", "Viewport zoom: ") + RuntimeViewportZoom.ToString("0.00"),
        T("待完成连线：", "Pending connection: ") + BoolText(RuntimeHasPendingConnection),
        T("当前状态：", "Current status: ") + CompatibilityStatusMessage,
    ];

    /// <summary>
    /// 运行时指标摘要行。
    /// </summary>
    public IReadOnlyList<string> RuntimeMetricLines =>
    [
        .. RuntimeSignalLines.Take(5),
        T("可保存工作区：", "Can save workspace: ") + BoolText(CurrentInspection.Capabilities.CanSaveWorkspace),
        T("可加载工作区：", "Can load workspace: ") + BoolText(CurrentInspection.Capabilities.CanLoadWorkspace),
    ];

    /// <summary>
    /// 最近诊断的机器可读投影。
    /// </summary>
    public IReadOnlyList<RuntimeDiagnosticEntry> RecentDiagnostics =>
        Session.Diagnostics
            .GetRecentDiagnostics(10)
            .Select(diagnostic => new RuntimeDiagnosticEntry(
                diagnostic.Code,
                diagnostic.Operation,
                diagnostic.Message,
                diagnostic.Severity,
                SeverityText(diagnostic.Severity),
                SeverityHex(diagnostic.Severity)))
            .ToArray();

    /// <summary>
    /// 最近诊断的紧凑展示文本。
    /// </summary>
    public IReadOnlyList<string> RecentDiagnosticLines
    {
        get
        {
            if (RecentDiagnostics.Count == 0)
            {
                return
                [
                    T("最近诊断：0 条", "Recent diagnostics: 0"),
                    T("当前状态消息：", "Current status message: ") + CompatibilityStatusMessage,
                ];
            }

            return RecentDiagnostics
                .Select(diagnostic => $"{SeverityText(diagnostic.Severity)} · {diagnostic.Code} · {diagnostic.Message}")
                .ToArray();
        }
    }

    /// <summary>
    /// 运行时诊断帮助文案。
    /// </summary>
    public string RuntimeDiagnosticsSummary
        => T(RuntimeDiagnosticsHelper, "These diagnostics come directly from Session.Diagnostics so the shared runtime state stays visible.");

    /// <summary>
    /// 当前运行时文档标题。
    /// </summary>
    public string RuntimeDocumentTitle => CurrentInspection.Document.Title;

    /// <summary>
    /// 当前运行时节点数。
    /// </summary>
    public int RuntimeNodeCount => CurrentInspection.Document.Nodes.Count;

    /// <summary>
    /// 当前运行时连线数。
    /// </summary>
    public int RuntimeConnectionCount => CurrentInspection.Document.Connections.Count;

    /// <summary>
    /// 当前运行时选择节点数。
    /// </summary>
    public int RuntimeSelectedNodeCount => CurrentInspection.Selection.SelectedNodeIds.Count;

    /// <summary>
    /// 当前运行时选择节点标识。
    /// </summary>
    public IReadOnlyList<string> RuntimeSelectedNodeIds => CurrentInspection.Selection.SelectedNodeIds;

    /// <summary>
    /// 当前运行时视口缩放。
    /// </summary>
    public double RuntimeViewportZoom => CurrentInspection.Viewport.Zoom;

    /// <summary>
    /// 当前运行时视口横向平移。
    /// </summary>
    public double RuntimeViewportPanX => CurrentInspection.Viewport.PanX;

    /// <summary>
    /// 当前运行时视口纵向平移。
    /// </summary>
    public double RuntimeViewportPanY => CurrentInspection.Viewport.PanY;

    /// <summary>
    /// 当前是否存在待完成连线。
    /// </summary>
    public bool RuntimeHasPendingConnection => CurrentInspection.PendingConnection.HasPendingConnection;

    /// <summary>
    /// 兼容状态栏文案。
    /// </summary>
    public string CompatibilityStatusMessage => CurrentInspection.Status.Message;

    /// <summary>
    /// 主编辑器摘要文案。
    /// </summary>
    public string MainEditorSummary
        => T("当前中心主编辑器绑定文档“", "The center editor is currently bound to document “")
        + Editor.Title
        + T("”，并通过 Create(...) + AsterGraphAvaloniaViewFactory.Create(...) 组合，而共享运行时所有权由 Session 持有。", "” and is composed through Create(...) + AsterGraphAvaloniaViewFactory.Create(...), while Session remains the shared runtime owner.");

    private GraphEditorInspectionSnapshot CurrentInspection
        => Session.Diagnostics.CaptureInspectionSnapshot();

    private void RefreshRuntimeProjection()
    {
        OnPropertyChanged(nameof(UiText));
        OnPropertyChanged(nameof(CurrentLanguageBadgeText));
        OnPropertyChanged(nameof(SelectedHostMenuGroupTitle));
        OnPropertyChanged(nameof(HostDrawerCaption));
        OnPropertyChanged(nameof(LiveSessionTitle));
        OnPropertyChanged(nameof(HostOwnershipBadgeText));
        OnPropertyChanged(nameof(RuntimeOwnershipBadgeText));
        OnPropertyChanged(nameof(StandaloneSurfaceLines));
        OnPropertyChanged(nameof(PresentationLines));
        OnPropertyChanged(nameof(LocalizationProofLines));
        OnPropertyChanged(nameof(SelectedHostMenuGroupSummary));
        OnPropertyChanged(nameof(SelectedHostMenuGroupLines));
        OnPropertyChanged(nameof(IsShowcaseHostGroupSelected));
        OnPropertyChanged(nameof(IsViewHostGroupSelected));
        OnPropertyChanged(nameof(IsBehaviorHostGroupSelected));
        OnPropertyChanged(nameof(IsRuntimeHostGroupSelected));
        OnPropertyChanged(nameof(IsExtensionsHostGroupSelected));
        OnPropertyChanged(nameof(IsAutomationHostGroupSelected));
        OnPropertyChanged(nameof(IsIntegrationHostGroupSelected));
        OnPropertyChanged(nameof(IsProofHostGroupSelected));
        OnPropertyChanged(nameof(ActiveHostGroupBadgeText));
        OnPropertyChanged(nameof(HostPaneStateCaption));
        OnPropertyChanged(nameof(HostSessionContinuityCaption));
        OnPropertyChanged(nameof(CurrentConfigurationLines));
        OnPropertyChanged(nameof(ActiveWorkspacePath));
        OnPropertyChanged(nameof(RecentWorkspacePaths));
        OnPropertyChanged(nameof(RecentWorkspaceEntries));
        OnPropertyChanged(nameof(HasAutosaveDraft));
        OnPropertyChanged(nameof(AutosaveDraftPath));
        OnPropertyChanged(nameof(DirtyExitPromptPending));
        OnPropertyChanged(nameof(DirtyExitPromptMessage));
        OnPropertyChanged(nameof(PreferredWindowWidth));
        OnPropertyChanged(nameof(PreferredWindowHeight));
        OnPropertyChanged(nameof(ThemeVariantCaption));
        OnPropertyChanged(nameof(ShellWorkflowLines));
        OnPropertyChanged(nameof(PluginCandidates));
        OnPropertyChanged(nameof(PluginCandidateEntries));
        OnPropertyChanged(nameof(PluginLoadSnapshots));
        OnPropertyChanged(nameof(PluginCandidateLines));
        OnPropertyChanged(nameof(PluginAllowlistLines));
        OnPropertyChanged(nameof(PluginLoadLines));
        OnPropertyChanged(nameof(ConsumerPathLines));
        OnPropertyChanged(nameof(LastAutomationRequest));
        OnPropertyChanged(nameof(LastAutomationResult));
        OnPropertyChanged(nameof(AutomationProgressSteps));
        OnPropertyChanged(nameof(AutomationRequestLines));
        OnPropertyChanged(nameof(AutomationProgressLines));
        OnPropertyChanged(nameof(AutomationResultLines));
        OnPropertyChanged(nameof(RuntimeSignalLines));
        OnPropertyChanged(nameof(OwnershipProofLines));
        OnPropertyChanged(nameof(RuntimeMetricLines));
        OnPropertyChanged(nameof(RecentDiagnostics));
        OnPropertyChanged(nameof(RecentDiagnosticLines));
        OnPropertyChanged(nameof(RuntimeDiagnosticsSummary));
        OnPropertyChanged(nameof(RuntimeSessionInterfaceName));
        OnPropertyChanged(nameof(RuntimeDiagnosticsSourceName));
        OnPropertyChanged(nameof(RuntimeDocumentTitle));
        OnPropertyChanged(nameof(RuntimeNodeCount));
        OnPropertyChanged(nameof(RuntimeConnectionCount));
        OnPropertyChanged(nameof(RuntimeSelectedNodeCount));
        OnPropertyChanged(nameof(RuntimeSelectedNodeIds));
        OnPropertyChanged(nameof(RuntimeViewportZoom));
        OnPropertyChanged(nameof(RuntimeViewportPanX));
        OnPropertyChanged(nameof(RuntimeViewportPanY));
        OnPropertyChanged(nameof(RuntimeHasPendingConnection));
        OnPropertyChanged(nameof(CompatibilityStatusMessage));
        OnPropertyChanged(nameof(MainEditorSummary));
    }

    private static string SeverityText(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => "Info",
            GraphEditorDiagnosticSeverity.Warning => "Warning",
            GraphEditorDiagnosticSeverity.Error => "Error",
            _ => severity.ToString(),
        };

    private static string SeverityHex(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => "#7FE7D7",
            GraphEditorDiagnosticSeverity.Warning => "#F3C97A",
            GraphEditorDiagnosticSeverity.Error => "#FF9B9B",
            _ => "#7FE7D7",
        };

    /// <summary>
    /// 最近诊断项的界面投影。
    /// </summary>
    public sealed record RuntimeDiagnosticEntry(
        string Code,
        string Operation,
        string Message,
        GraphEditorDiagnosticSeverity Severity,
        string SeverityLabel,
        string SeverityHex);
}
