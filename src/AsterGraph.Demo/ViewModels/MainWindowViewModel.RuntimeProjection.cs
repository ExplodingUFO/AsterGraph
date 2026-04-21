using System.Globalization;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    public RuntimeInspectionSurfaceProjection RuntimeInspectionSurface
        => runtimeInspectionSurface ??= BuildRuntimeInspectionSurface();

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
        CurrentInspection.RecentDiagnostics
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

    private RuntimeInspectionSurfaceProjection? runtimeInspectionSurface;

    private GraphEditorInspectionSnapshot CurrentInspection
        => Session.Diagnostics.CaptureInspectionSnapshot();

    private void RefreshRuntimeProjection()
    {
        runtimeInspectionSurface = BuildRuntimeInspectionSurface();
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
        OnPropertyChanged(nameof(RuntimeInspectionSurface));
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

    private RuntimeInspectionSurfaceProjection BuildRuntimeInspectionSurface()
    {
        var inspection = CurrentInspection;

        return new RuntimeInspectionSurfaceProjection(
            T("运行时检查", "Runtime inspection"),
            new RuntimeInspectionSurfaceProjection.Section(
                T("文档", "Document"),
                [
                    T("文档标题：", "Document title: ") + inspection.Document.Title,
                    T("节点数量：", "Node count: ") + inspection.Document.Nodes.Count,
                    T("连线数量：", "Connection count: ") + inspection.Document.Connections.Count,
                ]),
            new RuntimeInspectionSurfaceProjection.Section(
                T("选择", "Selection"),
                [
                    T("当前选择数量：", "Selected node count: ") + inspection.Selection.SelectedNodeIds.Count,
                    T("当前选择标识：", "Selected node ids: ") + FormatListOrNone(inspection.Selection.SelectedNodeIds),
                ]),
            new RuntimeInspectionSurfaceProjection.Section(
                T("视口", "Viewport"),
                [
                    T("视口缩放：", "Viewport zoom: ") + inspection.Viewport.Zoom.ToString("0.00", CultureInfo.InvariantCulture),
                    T("视口横向平移：", "Viewport pan X: ") + inspection.Viewport.PanX.ToString("0.00", CultureInfo.InvariantCulture),
                    T("视口纵向平移：", "Viewport pan Y: ") + inspection.Viewport.PanY.ToString("0.00", CultureInfo.InvariantCulture),
                ]),
            new RuntimeInspectionSurfaceProjection.Section(
                T("能力", "Capabilities"),
                BuildCapabilityLines(inspection.Capabilities)),
            new RuntimeInspectionSurfaceProjection.Section(
                T("待完成连线", "Pending connection"),
                BuildPendingConnectionLines(inspection.PendingConnection)),
            new RuntimeInspectionSurfaceProjection.Section(
                T("特性描述", "Feature descriptors"),
                BuildFeatureDescriptorLines(inspection.FeatureDescriptors)),
            new RuntimeInspectionSurfaceProjection.Section(
                T("命令时间线", "Command timeline"),
                BuildCommandTimelineLines()),
            new RuntimeInspectionSurfaceProjection.Section(
                T("最近诊断", "Recent diagnostics"),
                BuildDiagnosticLines(inspection.RecentDiagnostics)),
            new RuntimeInspectionSurfaceProjection.Section(
                T("插件信任与加载", "Plugin trust and load"),
                BuildPluginLoadLines(inspection.PluginLoadSnapshots)));
    }

    private IReadOnlyList<string> BuildCommandTimelineLines()
    {
        if (RuntimeCommandTimelineEntries.Count == 0)
        {
            return
            [
                T("当前没有命令事件。", "No command events yet."),
            ];
        }

        return RuntimeCommandTimelineEntries
            .Select(entry =>
                $"{T("命令", "Command")} · {entry.CommandId} · {T("批量标签：", "Mutation: ")}{FormatValueOrNone(entry.MutationLabel)} · {T("作用域：", "Scope: ")}{MutationScopeText(entry.IsInMutationScope)} · {T("状态：", "Status: ")}{FormatValueOrNone(entry.StatusMessage)}")
            .ToArray();
    }

    private IReadOnlyList<string> BuildCapabilityLines(GraphEditorCapabilitySnapshot capabilities)
    {
        return
        [
            T("可撤销：", "Can undo: ") + BoolText(capabilities.CanUndo),
            T("可重做：", "Can redo: ") + BoolText(capabilities.CanRedo),
            T("可复制选择：", "Can copy selection: ") + BoolText(capabilities.CanCopySelection),
            T("可粘贴：", "Can paste: ") + BoolText(capabilities.CanPaste),
            T("可保存工作区：", "Can save workspace: ") + BoolText(capabilities.CanSaveWorkspace),
            T("可加载工作区：", "Can load workspace: ") + BoolText(capabilities.CanLoadWorkspace),
            T("可设置选择：", "Can set selection: ") + BoolText(capabilities.CanSetSelection),
            T("可移动节点：", "Can move nodes: ") + BoolText(capabilities.CanMoveNodes),
            T("可对齐选择：", "Can align selection: ") + BoolText(capabilities.CanAlignSelection),
            T("可分布选择：", "Can distribute selection: ") + BoolText(capabilities.CanDistributeSelection),
            T("可编辑节点参数：", "Can edit node parameters: ") + BoolText(capabilities.CanEditNodeParameters),
            T("可创建连线：", "Can create connections: ") + BoolText(capabilities.CanCreateConnections),
            T("可删除连线：", "Can delete connections: ") + BoolText(capabilities.CanDeleteConnections),
            T("可断开连线：", "Can break connections: ") + BoolText(capabilities.CanBreakConnections),
            T("可更新视口：", "Can update viewport: ") + BoolText(capabilities.CanUpdateViewport),
            T("可适配内容：", "Can fit to viewport: ") + BoolText(capabilities.CanFitToViewport),
            T("可居中视口：", "Can center viewport: ") + BoolText(capabilities.CanCenterViewport),
        ];
    }

    private IReadOnlyList<string> BuildPendingConnectionLines(GraphEditorPendingConnectionSnapshot pendingConnection)
    {
        return
        [
            T("待完成连线：", "Pending connection: ") + BoolText(pendingConnection.HasPendingConnection),
            T("源节点：", "Source node: ") + FormatValueOrNone(pendingConnection.SourceNodeId),
            T("源端口：", "Source port: ") + FormatValueOrNone(pendingConnection.SourcePortId),
        ];
    }

    private IReadOnlyList<string> BuildFeatureDescriptorLines(IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> descriptors)
    {
        if (descriptors.Count == 0)
        {
            return
            [
                T("当前没有特性描述。", "No feature descriptors exposed."),
            ];
        }

        return descriptors
            .Select(descriptor =>
                $"{descriptor.Category} · {descriptor.Id} · {T("可用", "Available")}: {BoolText(descriptor.IsAvailable)}")
            .ToArray();
    }

    private IReadOnlyList<string> BuildDiagnosticLines(IReadOnlyList<GraphEditorDiagnostic> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return
            [
                T("当前没有最近诊断。", "No recent diagnostics."),
            ];
        }

        return diagnostics
            .Select(diagnostic =>
                $"{DiagnosticSeverityText(diagnostic.Severity)} · {diagnostic.Code} · {diagnostic.Operation} · {diagnostic.Message}")
            .ToArray();
    }

    private IReadOnlyList<string> BuildPluginLoadLines(IReadOnlyList<GraphEditorPluginLoadSnapshot> pluginLoads)
    {
        if (pluginLoads.Count == 0)
        {
            return
            [
                T("当前没有插件信任或加载记录。", "No plugin trust or load entries."),
            ];
        }

        return pluginLoads
            .Select(snapshot =>
                $"{T("插件", "Plugin")} · {snapshot.Manifest.DisplayName} · {T("状态：", "State: ")}{LoadStatusText(snapshot.Status)} · {T("信任：", "Trust: ")}{PluginTrustDecisionText(snapshot.TrustEvaluation.Decision)} · {T("来源：", "Source: ")}{LoadSourceKindText(snapshot.SourceKind)} · {T("来源证据：", "Provenance: ")}{FormatPluginProvenance(snapshot.ProvenanceEvidence)} · {T("原因：", "Reason: ")}{ResolvePluginTimelineReason(snapshot)}")
            .ToArray();
    }

    private string DiagnosticSeverityText(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => T("信息", "Info"),
            GraphEditorDiagnosticSeverity.Warning => T("警告", "Warning"),
            GraphEditorDiagnosticSeverity.Error => T("错误", "Error"),
            _ => severity.ToString(),
        };

    private string LoadStatusText(GraphEditorPluginLoadStatus status)
        => status switch
        {
            GraphEditorPluginLoadStatus.Loaded => T("已加载", "Loaded"),
            GraphEditorPluginLoadStatus.Blocked => T("已阻止", "Blocked"),
            GraphEditorPluginLoadStatus.Failed => T("加载失败", "Failed"),
            _ => status.ToString(),
        };

    private string LoadSourceKindText(GraphEditorPluginLoadSourceKind sourceKind)
        => sourceKind switch
        {
            GraphEditorPluginLoadSourceKind.Direct => T("直接", "Direct"),
            GraphEditorPluginLoadSourceKind.Assembly => T("程序集", "Assembly"),
            GraphEditorPluginLoadSourceKind.Package => T("包", "Package"),
            _ => sourceKind.ToString(),
        };

    private string MutationScopeText(bool isInMutationScope)
        => isInMutationScope
            ? T("批量变更内", "In mutation scope")
            : T("直接执行", "Direct");

    private string PluginTrustDecisionText(GraphEditorPluginTrustDecision decision)
        => decision switch
        {
            GraphEditorPluginTrustDecision.Allowed => T("允许", "Allowed"),
            GraphEditorPluginTrustDecision.Blocked => T("阻止", "Blocked"),
            _ => decision.ToString(),
        };

    private string FormatPluginProvenance(GraphEditorPluginProvenanceEvidence provenance)
    {
        if (provenance.PackageIdentity is { } packageIdentity)
        {
            return string.IsNullOrWhiteSpace(packageIdentity.Version)
                ? packageIdentity.Id
                : $"{packageIdentity.Id}@{packageIdentity.Version}";
        }

        return provenance.Signature.Status switch
        {
            GraphEditorPluginSignatureStatus.Valid => T("签名有效", "Signature valid"),
            GraphEditorPluginSignatureStatus.Invalid => T("签名无效", "Signature invalid"),
            GraphEditorPluginSignatureStatus.Unsigned => T("未签名", "Unsigned"),
            GraphEditorPluginSignatureStatus.Unknown => T("签名未知", "Signature unknown"),
            _ => T("未提供", "Not provided"),
        };
    }

    private string ResolvePluginTimelineReason(GraphEditorPluginLoadSnapshot snapshot)
        => FirstNonEmpty(
            snapshot.FailureMessage,
            snapshot.Stage?.ReasonMessage,
            snapshot.TrustEvaluation.ReasonMessage,
            snapshot.Compatibility.ReasonMessage,
            snapshot.ProvenanceEvidence.Signature.ReasonMessage,
            snapshot.FailureMessage,
            T("无", "None"));

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private string FormatValueOrNone(string? value)
        => string.IsNullOrWhiteSpace(value) ? T("无", "None") : value;

    private string FormatListOrNone(IReadOnlyList<string> values)
        => values.Count == 0 ? T("无", "None") : string.Join(", ", values);

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

    /// <summary>
    /// 运行时检查状态的稳定投影。
    /// </summary>
    public sealed record RuntimeInspectionSurfaceProjection(
        string Heading,
        RuntimeInspectionSurfaceProjection.Section Document,
        RuntimeInspectionSurfaceProjection.Section Selection,
        RuntimeInspectionSurfaceProjection.Section Viewport,
        RuntimeInspectionSurfaceProjection.Section Capabilities,
        RuntimeInspectionSurfaceProjection.Section PendingConnection,
        RuntimeInspectionSurfaceProjection.Section FeatureDescriptors,
        RuntimeInspectionSurfaceProjection.Section CommandTimeline,
        RuntimeInspectionSurfaceProjection.Section RecentDiagnostics,
        RuntimeInspectionSurfaceProjection.Section PluginLoads)
    {
        /// <summary>
        /// 可渲染的检查分组。
        /// </summary>
        public sealed record Section(
            string Heading,
            IReadOnlyList<string> Lines);
    }
}
