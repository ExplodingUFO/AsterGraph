using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
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

    partial void OnSelectedCapabilityChanged(CapabilityShowcaseItem value)
    {
        OnPropertyChanged(nameof(SelectedCapabilityTitle));
        OnPropertyChanged(nameof(SelectedCapabilitySummary));
        OnPropertyChanged(nameof(SelectedCapabilityArchitecture));
        OnPropertyChanged(nameof(SelectedCapabilityBullets));
        OnPropertyChanged(nameof(SelectedCapabilityProofLines));
    }

    private IReadOnlyList<CapabilityShowcaseItem> CreateCapabilityShowcaseItems()
    {
        return
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
    }

    public sealed record CapabilityShowcaseItem(
        string Key,
        string Title,
        string Summary,
        string Architecture,
        IReadOnlyList<string> Bullets,
        IReadOnlyList<string> ProofLines);
}
