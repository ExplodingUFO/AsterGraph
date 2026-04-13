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
    private const string RuntimeDiagnosticsHelper = "以下诊断直接来自 Editor.Session.Diagnostics，用于确认共享运行时状态。";
    private const string StandaloneSurfaceHelper = "这些预览与主编辑器共享同一运行时会话。";
    private const string PresentationHelper = "可替换的是视觉呈现，不是编辑行为。";
    private const string ChromeModeHelper = "关闭后可体验完整编辑流程；开启后仅保留只读浏览。";
    private const string ChromeControlsHelper = "这些开关只控制壳层显示，不会重建当前 Editor 会话。";

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

    [ObservableProperty]
    private bool isHeaderChromeVisible;

    [ObservableProperty]
    private bool isLibraryChromeVisible;

    [ObservableProperty]
    private bool isInspectorChromeVisible;

    [ObservableProperty]
    private bool isStatusChromeVisible;

    [ObservableProperty]
    private bool isHostPaneOpen;

    [ObservableProperty]
    private string selectedHostMenuGroupTitle = "展示";

    public string SelectedCapabilityTitle => SelectedCapability.Title;

    public string SelectedCapabilitySummary => SelectedCapability.Summary;

    public string SelectedCapabilityArchitecture => SelectedCapability.Architecture;

    public IReadOnlyList<string> SelectedCapabilityBullets => SelectedCapability.Bullets;

    public IReadOnlyList<string> SelectedCapabilityProofLines => SelectedCapability.ProofLines;

    public bool IsShowcaseHostGroupSelected => SelectedHostMenuGroupTitle == "展示";

    public bool IsViewHostGroupSelected => SelectedHostMenuGroupTitle == "视图";

    public bool IsBehaviorHostGroupSelected => SelectedHostMenuGroupTitle == "行为";

    public bool IsRuntimeHostGroupSelected => SelectedHostMenuGroupTitle == "运行时";

    public bool IsProofHostGroupSelected => SelectedHostMenuGroupTitle == "证明";

    public string HostDrawerCaption => "宿主控制抽屉";

    public string LiveSessionTitle => "实时 SDK 会话";

    public string HostOwnershipBadgeText => "宿主控制";

    public string RuntimeOwnershipBadgeText => "共享运行时";

    public string ActiveHostGroupBadgeText => $"当前分组 · {SelectedHostMenuGroupTitle}";

    public IReadOnlyList<string> CurrentConfigurationLines =>
    [
        $"当前分组：{SelectedHostMenuGroupTitle}",
        $"显示顶栏：{BoolText(IsHeaderChromeVisible)}",
        $"显示节点库：{BoolText(IsLibraryChromeVisible)}",
        $"显示检查器：{BoolText(IsInspectorChromeVisible)}",
        $"显示状态栏：{BoolText(IsStatusChromeVisible)}",
        $"只读模式：{BoolText(IsReadOnlyEnabled)}",
        $"网格吸附：{BoolText(IsGridSnappingEnabled)}",
        $"对齐辅助线：{BoolText(IsAlignmentGuidesEnabled)}",
        $"工作区命令：{BoolText(AreWorkspaceCommandsEnabled)}",
        $"片段命令：{BoolText(AreFragmentCommandsEnabled)}",
        $"宿主菜单扩展：{BoolText(AreHostMenuExtensionsEnabled)}",
    ];

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

    public IReadOnlyList<string> OwnershipProofLines =>
    [
        "宿主壳层开关由 MainWindowViewModel 持有，只控制菜单与抽屉，不会生成第二个编辑器。",
        $"共享运行时证据来自 {RuntimeDiagnosticsSourceName} 的检查快照与最近诊断。",
        "中心画布、宿主菜单和抽屉始终指向同一个 Editor.Session。",
        $"当前展示能力：{SelectedCapabilityTitle}",
    ];

    public IReadOnlyList<string> ChromeModeProofLines =>
    [
        $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.Default)}：保留头部、节点库、检查器与状态栏。",
        $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.CanvasOnly)}：收敛为只读浏览导向的画布视图。",
        ChromeModeHelper,
        ChromeControlsHelper,
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

    public string SelectedHostMenuGroupSummary => SelectedHostMenuGroupTitle switch
    {
        "展示" => "当前壳层把宿主菜单放在第一层，让用户先看到菜单和节点图，再按需展开展示信息。",
        "视图" => "壳层与视图开关仍作用于同一个 GraphEditorView，只是默认不再把大块说明区常驻在首屏。",
        "行为" => "编辑行为仍由同一个 Editor 负责；宿主菜单只是集中暴露能力入口，不替代运行时本身。",
        "运行时" => "运行时面板直接读取共享运行时的文档、选择、视口和诊断，方便操作时核对同一个 Editor.Session 的当前状态。",
        "证明" => "证明面板把宿主壳层控制与共享运行时证据并排展示，用来确认当前窗口没有第二个编辑器实例，只有同一个 Editor.Session。",
        _ => "通过宿主级菜单控制同一张实时节点图。"
    };

    public IReadOnlyList<string> SelectedHostMenuGroupLines => SelectedHostMenuGroupTitle switch
    {
        "展示" =>
        [
            "第一眼先看到宿主菜单和实时节点图，而不是三栏说明墙。",
            "主区只有一个 GraphEditorView，宿主面板只补充解释和控制入口。",
            .. DemoEntries,
        ],
        "视图" =>
        [
            $"显示顶栏：{BoolText(IsHeaderChromeVisible)}",
            $"显示节点库：{BoolText(IsLibraryChromeVisible)}",
            $"显示检查器：{BoolText(IsInspectorChromeVisible)}",
            $"显示状态栏：{BoolText(IsStatusChromeVisible)}",
            ChromeControlsHelper,
        ],
        "行为" =>
        [
            $"只读模式：{BoolText(IsReadOnlyEnabled)}",
            $"网格吸附：{BoolText(IsGridSnappingEnabled)}",
            $"对齐辅助线：{BoolText(IsAlignmentGuidesEnabled)}",
            $"工作区命令：{BoolText(AreWorkspaceCommandsEnabled)}",
            $"片段命令：{BoolText(AreFragmentCommandsEnabled)}",
            $"宿主菜单扩展：{BoolText(AreHostMenuExtensionsEnabled)}",
        ],
        "运行时" =>
        [
            .. RuntimeMetricLines,
            $"共享运行时入口：{RuntimeDiagnosticsSourceName}",
            $"最近诊断：{RecentDiagnostics.Count} 条",
            RuntimeDiagnosticsHelper,
        ],
        "证明" =>
        [
            "宿主壳层控制来自 MainWindowViewModel，负责菜单、抽屉与壳层开关。",
            $"共享运行时证据：{RuntimeDiagnosticsSourceName} 提供当前文档、选择、视口和最近诊断。",
            "当前窗口没有第二个编辑器实例；中心画布与宿主控制都作用于同一个 Editor.Session。",
            HostPaneStateCaption,
        ],
        _ =>
        [
            HostSessionContinuityCaption,
        ]
    };

    public string HostPaneStateCaption
        => IsHostPaneOpen
            ? $"面板已展开 · {SelectedHostMenuGroupTitle}"
            : "面板已收起";

    public string HostSessionContinuityCaption
        => "宿主菜单和抽屉始终作用于同一实时会话";

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
    /// 规范运行时会话接口名。
    /// </summary>
    public string RuntimeSessionInterfaceName => nameof(IGraphEditorSession);

    /// <summary>
    /// 规范诊断入口名。
    /// </summary>
    public string RuntimeDiagnosticsSourceName => "Editor.Session.Diagnostics";

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

    partial void OnIsHeaderChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsLibraryChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsInspectorChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsStatusChromeVisibleChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnIsHostPaneOpenChanged(bool value)
        => RefreshRuntimeProjection();

    partial void OnSelectedHostMenuGroupTitleChanged(string value)
        => RefreshRuntimeProjection();

    [RelayCommand]
    public void SelectCapability(CapabilityShowcaseItem capability)
    {
        ArgumentNullException.ThrowIfNull(capability);
        SelectedCapability = capability;
    }

    [RelayCommand]
    public void OpenHostMenuGroup(string groupTitle)
    {
        if (string.IsNullOrWhiteSpace(groupTitle))
        {
            return;
        }

        SelectedHostMenuGroupTitle = groupTitle;
        IsHostPaneOpen = true;
    }

    [RelayCommand]
    public void CloseHostPane()
        => IsHostPaneOpen = false;

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
