using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public string RuntimeSessionInterfaceName => nameof(IGraphEditorSession);

    public string RuntimeDiagnosticsSourceName => "Editor.Session.Diagnostics";

    public IReadOnlyList<string> RuntimeSignalLines =>
    [
        $"文档标题：{RuntimeDocumentTitle}",
        $"节点数量：{RuntimeNodeCount}",
        $"连线数量：{RuntimeConnectionCount}",
        $"当前选择：{RuntimeSelectedNodeCount}",
        $"视口缩放：{RuntimeViewportZoom:0.00}",
        $"待完成连线：{BoolText(RuntimeHasPendingConnection)}",
        $"当前状态：{CompatibilityStatusMessage}",
    ];

    /// <summary>
    /// 运行时指标摘要行。
    /// </summary>
    public IReadOnlyList<string> RuntimeMetricLines =>
    [
        .. RuntimeSignalLines.Take(5),
        $"可保存工作区：{BoolText(CurrentInspection.Capabilities.CanSaveWorkspace)}",
        $"可加载工作区：{BoolText(CurrentInspection.Capabilities.CanLoadWorkspace)}",
    ];

    /// <summary>
    /// 最近诊断的机器可读投影。
    /// </summary>
    public IReadOnlyList<RuntimeDiagnosticEntry> RecentDiagnostics =>
        Editor.Session.Diagnostics
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
                    "最近诊断：0 条",
                    $"当前状态消息：{CompatibilityStatusMessage}",
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
    public string RuntimeDiagnosticsSummary => RuntimeDiagnosticsHelper;

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
    public string MainEditorSummary => $"当前中心主编辑器绑定文档“{Editor.Title}”，宿主菜单与抽屉都作用于同一个 Editor.Session。";

    private GraphEditorInspectionSnapshot CurrentInspection
        => Editor.Session.Diagnostics.CaptureInspectionSnapshot();

    private void RefreshRuntimeProjection()
    {
        OnPropertyChanged(nameof(StandaloneSurfaceLines));
        OnPropertyChanged(nameof(PresentationLines));
        OnPropertyChanged(nameof(SelectedHostMenuGroupSummary));
        OnPropertyChanged(nameof(SelectedHostMenuGroupLines));
        OnPropertyChanged(nameof(IsShowcaseHostGroupSelected));
        OnPropertyChanged(nameof(IsViewHostGroupSelected));
        OnPropertyChanged(nameof(IsBehaviorHostGroupSelected));
        OnPropertyChanged(nameof(IsRuntimeHostGroupSelected));
        OnPropertyChanged(nameof(IsProofHostGroupSelected));
        OnPropertyChanged(nameof(ActiveHostGroupBadgeText));
        OnPropertyChanged(nameof(HostPaneStateCaption));
        OnPropertyChanged(nameof(HostSessionContinuityCaption));
        OnPropertyChanged(nameof(CurrentConfigurationLines));
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
            GraphEditorDiagnosticSeverity.Info => "信息",
            GraphEditorDiagnosticSeverity.Warning => "警告",
            GraphEditorDiagnosticSeverity.Error => "错误",
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
