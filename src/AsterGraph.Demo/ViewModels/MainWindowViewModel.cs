using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Compatibility;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.Menus;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const double DemoSnapTolerance = 18;
    private const string RuntimeDiagnosticsHelper = "以下信息直接来自 Editor.Session.Diagnostics，而不是状态栏文案。";
    private const string StandaloneSurfaceHelper = "这些预览与主编辑器共享同一运行时会话。";
    private const string PresentationHelper = "可替换的是视觉呈现，不是编辑行为。";
    private const string ChromeModeHelper = "关闭后可体验完整编辑流程；开启后仅保留只读浏览。";

    public MainWindowViewModel()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new DemoNodeDefinitionProvider());
        var style = GraphEditorStyleOptions.Default with
        {
            Shell = GraphEditorStyleOptions.Default.Shell with
            {
                HighlightHex = "#9EF6C9",
                LibraryPanelWidth = 296,
                InspectorPanelWidth = 356,
            },
            Connection = GraphEditorStyleOptions.Default.Connection with
            {
                Thickness = 3.4,
            },
            Canvas = GraphEditorStyleOptions.Default.Canvas with
            {
                EnableGridSnapping = true,
                EnableAlignmentGuides = true,
                SnapTolerance = DemoSnapTolerance,
            },
            ContextMenu = GraphEditorStyleOptions.Default.ContextMenu with
            {
                BackgroundHex = "#0E1824",
                HoverHex = "#21425C",
                BorderHex = "#34536B",
                SeparatorHex = "#42637C",
            },
        };
        var behavior = GraphEditorBehaviorOptions.Default with
        {
            DragAssist = GraphEditorBehaviorOptions.Default.DragAssist with
            {
                EnableGridSnapping = true,
                EnableAlignmentGuides = true,
                SnapTolerance = DemoSnapTolerance,
            },
            View = GraphEditorBehaviorOptions.Default.View with
            {
                ShowMiniMap = true,
            },
        };

        GraphEditorViewModel? editor = null;
        var contextMenuAugmentor = new DemoNodeResultsMenuContributor(message =>
        {
            if (editor is not null)
            {
                editor.StatusMessage = message;
                RefreshRuntimeProjection();
            }
        });

        editor = new GraphEditorViewModel(
            DemoGraphFactory.CreateDefault(catalog),
            catalog,
            new DefaultPortCompatibilityService(),
            new GraphWorkspaceService(),
            null,
            style,
            behavior,
            contextMenuAugmentor: contextMenuAugmentor,
            localizationProvider: new DemoGraphLocalizationProvider());

        Editor = editor;
        Editor.DocumentChanged += (_, _) => RefreshRuntimeProjection();
        Editor.SelectionChanged += (_, _) => RefreshRuntimeProjection();
        Editor.ViewportChanged += (_, _) => RefreshRuntimeProjection();
        Editor.FragmentExported += (_, _) => RefreshRuntimeProjection();
        Editor.FragmentImported += (_, _) => RefreshRuntimeProjection();

        Capabilities =
        [
            new CapabilityShowcaseItem(
                "full-shell",
                "完整壳层",
                "以默认 GraphEditorView 展示开箱即用的完整编辑壳层。",
                "完整壳层把节点库、画布、检查器与状态区组合成一个宿主可直接嵌入的默认体验。",
                [
                    "所属层：AsterGraph.Avalonia 完整壳层。",
                    "宿主入口：GraphEditorView 或 AsterGraphAvaloniaViewFactory。",
                    "可替换点：保留默认交互，同时通过 Presentation/菜单/本地化等 seam 做增量替换。",
                ],
                [
                    "主区只有一个实时 GraphEditorView。",
                    $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.Default)} 与 {nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.CanvasOnly)} 共用同一 Editor.Session。",
                ]),
            new CapabilityShowcaseItem(
                "standalone-surfaces",
                "独立表面",
                "把画布、检查器、缩略图拆成独立宿主表面而不复制运行时。",
                "Standalone Canvas / Inspector / Mini Map 让宿主按需嵌入局部 UI，同时继续绑定同一个编辑器实例。",
                [
                    "所属层：AsterGraph.Avalonia 独立宿主表面。",
                    "宿主入口：AsterGraphCanvasViewFactory、AsterGraphInspectorViewFactory、AsterGraphMiniMapViewFactory。",
                    "可替换点：宿主只取需要的表面，不必强制采用完整壳层。",
                ],
                [
                    StandaloneSurfaceHelper,
                    $"当前主编辑器文档：{Editor.Title}。",
                ]),
            new CapabilityShowcaseItem(
                "replaceable-presentation",
                "可替换呈现",
                "用公开 presenter seam 替换节点、菜单、检查器与缩略图的视觉表达。",
                "AsterGraph 保持编辑命令与运行时不变，把视觉层替换收敛到 presenter contract，而不是复制一套行为系统。",
                [
                    "所属层：AsterGraph.Avalonia 呈现适配层。",
                    "宿主入口：AsterGraphPresentationOptions。",
                    "可替换点：NodeVisualPresenter、ContextMenuPresenter、InspectorPresenter、MiniMapPresenter。",
                ],
                [
                    PresentationHelper,
                    "同一 Editor 继续负责命令、选择、视口和诊断。",
                ]),
            new CapabilityShowcaseItem(
                "runtime-diagnostics",
                "运行时与诊断",
                "通过 Editor.Session 查询、检查快照与最近诊断显式观察当前运行时。",
                "诊断区直接读取 Editor.Session.Diagnostics 与 Queries 快照，帮助宿主验证会话状态而不是依赖状态栏文案。",
                [
                    "所属层：AsterGraph.Editor 运行时与诊断契约。",
                    "宿主入口：Editor.Session.Queries 与 Editor.Session.Diagnostics。",
                    "可替换点：宿主可以把这些快照接入自己的日志、支持面板或调试工作流。",
                ],
                [
                    RuntimeDiagnosticsHelper,
                    $"最近诊断数量：{Editor.Session.Diagnostics.GetRecentDiagnostics(10).Count}。",
                ]),
        ];

        SelectedCapability = Capabilities[0];
        ApplyHostOptions(status: null);
        RefreshRuntimeProjection();
    }

    public GraphEditorViewModel Editor { get; }

    public IReadOnlyList<CapabilityShowcaseItem> Capabilities { get; }

    public IReadOnlyList<string> DemoEntries { get; } =
    [
        "LIVE：中心主编辑器始终绑定同一个 Editor。",
        "STOCK：默认完整壳层来自 GraphEditorView。",
        "CUSTOM：视觉替换通过 presenter seam 接入。",
    ];

    [ObservableProperty]
    private CapabilityShowcaseItem selectedCapability = null!;

    [ObservableProperty]
    private bool isGridSnappingEnabled = true;

    [ObservableProperty]
    private bool isAlignmentGuidesEnabled = true;

    [ObservableProperty]
    private bool isReadOnlyEnabled;

    [ObservableProperty]
    private bool areWorkspaceCommandsEnabled = true;

    [ObservableProperty]
    private bool areFragmentCommandsEnabled = true;

    [ObservableProperty]
    private bool areHostMenuExtensionsEnabled = true;

    public string SelectedCapabilityTitle => SelectedCapability.Title;

    public string SelectedCapabilitySummary => SelectedCapability.Summary;

    public string SelectedCapabilityArchitecture => SelectedCapability.Architecture;

    public IReadOnlyList<string> SelectedCapabilityBullets => SelectedCapability.Bullets;

    public IReadOnlyList<string> SelectedCapabilityProofLines => SelectedCapability.ProofLines;

    public IReadOnlyList<string> ChromeModeProofLines =>
    [
        $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.Default)}：保留头部、节点库、检查器与状态栏。",
        $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.CanvasOnly)}：收敛为只读浏览导向的画布视图。",
        ChromeModeHelper,
        "切换 chrome 只影响壳层可见性，不会重建底层运行时。",
    ];

    public IReadOnlyList<string> StandaloneSurfaceLines =>
    [
        "AsterGraphCanvasViewFactory — 只嵌入节点画布。",
        "AsterGraphInspectorViewFactory — 只嵌入检查器。",
        "AsterGraphMiniMapViewFactory — 只嵌入缩略图。",
        StandaloneSurfaceHelper,
        $"共享证据：当前文档 {CurrentInspection.Document.Title}，节点 {CurrentInspection.Document.Nodes.Count}。",
    ];

    public IReadOnlyList<string> PresentationLines =>
    [
        "AsterGraphPresentationOptions",
        "NodeVisualPresenter / ContextMenuPresenter / InspectorPresenter / MiniMapPresenter",
        PresentationHelper,
        $"当前选择仍由 Editor 维护：{CurrentInspection.Selection.SelectedNodeIds.Count} 个节点。",
    ];

    public IReadOnlyList<string> RuntimeMetricLines =>
    [
        $"文档标题：{RuntimeDocumentTitle}",
        $"节点数量：{RuntimeNodeCount}",
        $"当前选择：{RuntimeSelectedNodeCount}",
        $"视口缩放：{RuntimeViewportZoom:0.00}",
        $"可保存工作区：{BoolText(CurrentInspection.Capabilities.CanSaveWorkspace)}",
        $"可加载工作区：{BoolText(CurrentInspection.Capabilities.CanLoadWorkspace)}",
    ];

    public IReadOnlyList<RuntimeDiagnosticEntry> RecentDiagnostics =>
        Editor.Session.Diagnostics
            .GetRecentDiagnostics(10)
            .Select(diagnostic => new RuntimeDiagnosticEntry(
                diagnostic.Code,
                diagnostic.Operation,
                diagnostic.Message,
                diagnostic.Severity))
            .ToArray();

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

    public string RuntimeDiagnosticsSummary => RuntimeDiagnosticsHelper;

    public string RuntimeSessionInterfaceName => nameof(IGraphEditorSession);

    public string RuntimeDiagnosticsSourceName => "Editor.Session.Diagnostics";

    public string RuntimeDocumentTitle => CurrentInspection.Document.Title;

    public int RuntimeNodeCount => CurrentInspection.Document.Nodes.Count;

    public int RuntimeConnectionCount => CurrentInspection.Document.Connections.Count;

    public int RuntimeSelectedNodeCount => CurrentInspection.Selection.SelectedNodeIds.Count;

    public IReadOnlyList<string> RuntimeSelectedNodeIds => CurrentInspection.Selection.SelectedNodeIds;

    public double RuntimeViewportZoom => CurrentInspection.Viewport.Zoom;

    public double RuntimeViewportPanX => CurrentInspection.Viewport.PanX;

    public double RuntimeViewportPanY => CurrentInspection.Viewport.PanY;

    public bool RuntimeHasPendingConnection => CurrentInspection.PendingConnection.HasPendingConnection;

    public string CompatibilityStatusMessage => CurrentInspection.Status.Message;

    public string MainEditorSummary => $"当前中心主编辑器绑定文档“{Editor.Title}”，并保留完整运行时会话。";

    partial void OnSelectedCapabilityChanged(CapabilityShowcaseItem value)
    {
        OnPropertyChanged(nameof(SelectedCapabilityTitle));
        OnPropertyChanged(nameof(SelectedCapabilitySummary));
        OnPropertyChanged(nameof(SelectedCapabilityArchitecture));
        OnPropertyChanged(nameof(SelectedCapabilityBullets));
        OnPropertyChanged(nameof(SelectedCapabilityProofLines));
    }

    partial void OnIsGridSnappingEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnIsAlignmentGuidesEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnIsReadOnlyEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnAreWorkspaceCommandsEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnAreFragmentCommandsEnabledChanged(bool value)
        => ApplyHostOptions();

    partial void OnAreHostMenuExtensionsEnabledChanged(bool value)
        => ApplyHostOptions();

    [RelayCommand]
    public void SelectCapability(CapabilityShowcaseItem capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        SelectedCapability = capability;
    }

    private GraphEditorInspectionSnapshot CurrentInspection
        => Editor.Session.Diagnostics.CaptureInspectionSnapshot();

    private void ApplyHostOptions(string? status = "Host behavior updated.")
    {
        Editor.UpdateBehaviorOptions(
            Editor.BehaviorOptions with
            {
                DragAssist = Editor.BehaviorOptions.DragAssist with
                {
                    EnableGridSnapping = IsGridSnappingEnabled,
                    EnableAlignmentGuides = IsAlignmentGuidesEnabled,
                    SnapTolerance = DemoSnapTolerance,
                },
                Commands = BuildCommandPermissions(),
            },
            status);

        RefreshRuntimeProjection();
    }

    private GraphEditorCommandPermissions BuildCommandPermissions()
    {
        return GraphEditorCommandPermissions.Default with
        {
            Workspace = new WorkspaceCommandPermissions
            {
                AllowSave = AreWorkspaceCommandsEnabled && !IsReadOnlyEnabled,
                AllowLoad = AreWorkspaceCommandsEnabled && !IsReadOnlyEnabled,
            },
            History = new HistoryCommandPermissions
            {
                AllowUndo = !IsReadOnlyEnabled,
                AllowRedo = !IsReadOnlyEnabled,
            },
            Nodes = new NodeCommandPermissions
            {
                AllowCreate = !IsReadOnlyEnabled,
                AllowDelete = !IsReadOnlyEnabled,
                AllowMove = !IsReadOnlyEnabled,
                AllowDuplicate = !IsReadOnlyEnabled,
                AllowEditParameters = !IsReadOnlyEnabled,
            },
            Connections = new ConnectionCommandPermissions
            {
                AllowCreate = !IsReadOnlyEnabled,
                AllowDelete = !IsReadOnlyEnabled,
                AllowDisconnect = !IsReadOnlyEnabled,
            },
            Clipboard = new ClipboardCommandPermissions
            {
                AllowCopy = !IsReadOnlyEnabled,
                AllowPaste = !IsReadOnlyEnabled,
            },
            Layout = new LayoutCommandPermissions
            {
                AllowAlign = !IsReadOnlyEnabled,
                AllowDistribute = !IsReadOnlyEnabled,
            },
            Fragments = new FragmentCommandPermissions
            {
                AllowImport = AreFragmentCommandsEnabled && !IsReadOnlyEnabled,
                AllowExport = AreFragmentCommandsEnabled,
                AllowClearWorkspaceFragment = AreFragmentCommandsEnabled && !IsReadOnlyEnabled,
                AllowTemplateManagement = AreFragmentCommandsEnabled && !IsReadOnlyEnabled,
            },
            Host = new HostCommandPermissions
            {
                AllowContextMenuExtensions = AreHostMenuExtensionsEnabled,
            },
        };
    }

    private void RefreshRuntimeProjection()
    {
        OnPropertyChanged(nameof(StandaloneSurfaceLines));
        OnPropertyChanged(nameof(PresentationLines));
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

    private static string BoolText(bool value)
        => value ? "是" : "否";

    private static string SeverityText(GraphEditorDiagnosticSeverity severity)
        => severity switch
        {
            GraphEditorDiagnosticSeverity.Info => "信息",
            GraphEditorDiagnosticSeverity.Warning => "警告",
            GraphEditorDiagnosticSeverity.Error => "错误",
            _ => severity.ToString(),
        };

    public sealed record RuntimeDiagnosticEntry(
        string Code,
        string Operation,
        string Message,
        GraphEditorDiagnosticSeverity Severity);

    public sealed record CapabilityShowcaseItem(
        string Key,
        string Title,
        string Summary,
        string Architecture,
        IReadOnlyList<string> Bullets,
        IReadOnlyList<string> ProofLines);

    private sealed class DemoGraphLocalizationProvider : IGraphLocalizationProvider
    {
        private static readonly IReadOnlyDictionary<string, string> Values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["editor.menu.canvas.addNode"] = "添加节点",
            ["editor.inspector.title.none"] = "请选择一个节点",
        };

        public string GetString(string key, string fallback)
            => Values.TryGetValue(key, out var localized) ? localized : fallback;
    }
}
