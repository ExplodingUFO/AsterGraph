using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel
{
    private string? _lastSelectedCapabilityKey = "full-shell";

    public string SelectedCapabilityTitle => ResolveSelectedCapability().Title;

    public string SelectedCapabilitySummary => ResolveSelectedCapability().Summary;

    public string SelectedCapabilityArchitecture => ResolveSelectedCapability().Architecture;

    public IReadOnlyList<string> SelectedCapabilityBullets => ResolveSelectedCapability().Bullets;

    public IReadOnlyList<string> SelectedCapabilityProofLines => ResolveSelectedCapability().ProofLines;

    public bool IsShowcaseHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Showcase;

    public bool IsViewHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.View;

    public bool IsBehaviorHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Behavior;

    public bool IsRuntimeHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Runtime;

    public bool IsExtensionsHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Extensions;

    public bool IsAutomationHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Automation;

    public bool IsIntegrationHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Integration;

    public bool IsProofHostGroupSelected => SelectedHostMenuGroup == DemoHostMenuGroups.Proof;

    public string HostDrawerCaption => Text("drawer.caption");

    public string LiveSessionTitle => Text("intro.title");

    public string HostOwnershipBadgeText => Text("badge.hostOwnership");

    public string RuntimeOwnershipBadgeText => Text("badge.runtimeOwnership");

    public string ActiveHostGroupBadgeText => string.Format(
        System.Globalization.CultureInfo.InvariantCulture,
        Text("badge.activeGroup"),
        SelectedHostMenuGroupTitle);

    public IReadOnlyList<string> DemoEntries =>
    [
        T("LIVE：中心主编辑器始终绑定同一个 Editor。", "LIVE: the center editor always binds to the same Editor."),
        T("STOCK：默认完整壳层来自 GraphEditorView。", "STOCK: the default full shell comes from GraphEditorView."),
        T("CUSTOM：视觉替换通过 presenter seam 接入。", "CUSTOM: visual replacement enters through the presenter seam."),
    ];

    public IReadOnlyList<string> CurrentConfigurationLines =>
    [
        T("当前分组：", "Current group: ") + SelectedHostMenuGroupTitle,
        T("显示顶栏：", "Show header: ") + BoolText(IsHeaderChromeVisible),
        T("显示节点库：", "Show library: ") + BoolText(IsLibraryChromeVisible),
        T("显示检查器：", "Show inspector: ") + BoolText(IsInspectorChromeVisible),
        T("显示状态栏：", "Show status bar: ") + BoolText(IsStatusChromeVisible),
        T("显示迷你地图：", "Show mini map: ") + BoolText(IsMiniMapVisible),
        T("只读模式：", "Read-only mode: ") + BoolText(IsReadOnlyEnabled),
        T("网格吸附：", "Grid snapping: ") + BoolText(IsGridSnappingEnabled),
        T("对齐辅助线：", "Alignment guides: ") + BoolText(IsAlignmentGuidesEnabled),
        T("工作区命令：", "Workspace commands: ") + BoolText(AreWorkspaceCommandsEnabled),
        T("片段命令：", "Fragment commands: ") + BoolText(AreFragmentCommandsEnabled),
        T("宿主菜单扩展：", "Host menu extensions: ") + BoolText(AreHostMenuExtensionsEnabled),
    ];

    public IReadOnlyList<string> OwnershipProofLines =>
    [
        T(
            "宿主壳层开关由 MainWindowViewModel 持有，只控制菜单与抽屉，不会生成第二个编辑器。",
            "MainWindowViewModel owns the host shell toggles and only controls menus and drawers; it does not create a second editor."),
        T(
            "共享运行时证据来自 ",
            "Shared runtime proof comes from ") + RuntimeDiagnosticsSourceName + T(" 的检查快照与最近诊断。", " inspection snapshots and recent diagnostics."),
        T(
            "中心画布、宿主菜单和抽屉始终指向同一个 Editor.Session。",
            "The center canvas, host menu, and drawer always point to the same Editor.Session."),
        T(
            "主编辑器通过 AsterGraphEditorFactory.Create(...) 和 AsterGraphAvaloniaViewFactory.Create(...) 组合。",
            "The main editor is composed through AsterGraphEditorFactory.Create(...) and AsterGraphAvaloniaViewFactory.Create(...)."),
        T("当前展示能力：", "Current showcased capability: ") + SelectedCapabilityTitle,
    ];

    public IReadOnlyList<string> ChromeModeProofLines =>
    [
        $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.Default)}：{T("保留头部、节点库、检查器与状态栏。", "keeps the header, library, inspector, and status bar.")}",
        $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.CanvasOnly)}：{T("收敛为只读浏览导向的画布视图。", "collapses the shell into a canvas-forward browsing view.")}",
        T(ChromeModeHelper, "Turning it off restores the full editing flow; turning it on keeps only read-only browsing chrome."),
        T(ChromeControlsHelper, "These toggles only control shell chrome and never rebuild the current Editor session."),
    ];

    public IReadOnlyList<string> StandaloneSurfaceLines =>
    [
        "AsterGraphCanvasViewFactory — " + T("只嵌入节点画布。", "embeds only the node canvas."),
        "AsterGraphInspectorViewFactory — " + T("只嵌入检查器。", "embeds only the inspector."),
        "AsterGraphMiniMapViewFactory — " + T("只嵌入缩略图。", "embeds only the minimap."),
        T(StandaloneSurfaceHelper, "These previews share the same runtime session as the main editor."),
        T("共享证据：当前文档 ", "Shared proof: current document ") + CurrentInspection.Document.Title + T("，节点 ", ", nodes ") + CurrentInspection.Document.Nodes.Count + "。",
    ];

    public IReadOnlyList<string> PresentationLines =>
    [
        "AsterGraphPresentationOptions",
        "NodeVisualPresenter / ContextMenuPresenter / InspectorPresenter / MiniMapPresenter",
        T(PresentationHelper, "Presentation is replaceable; editing behavior is not."),
        T("当前选择仍由 Editor 维护：", "Current selection is still owned by the Editor: ") + CurrentInspection.Selection.SelectedNodeIds.Count + T(" 个节点。", " nodes."),
    ];

    public string SelectedHostMenuGroupSummary => SelectedHostMenuGroup switch
    {
        DemoHostMenuGroups.Showcase => T(
            "当前壳层把宿主菜单放在第一层，让用户先看到菜单和节点图，再按需展开展示信息。",
            "The shell keeps the host menu in the first layer so users see the menu and graph first, then open showcase details on demand."),
        DemoHostMenuGroups.View => T(
            "壳层与视图开关仍作用于同一个 GraphEditorView，只是默认不再把大块说明区常驻在首屏。",
            "Shell and view toggles still act on the same GraphEditorView; the shell just no longer keeps the explanation pane always open."),
        DemoHostMenuGroups.Behavior => T(
            "编辑行为仍由同一个 Editor 负责；宿主菜单只是集中暴露能力入口，不替代运行时本身。",
            "Editing behavior is still owned by the same Editor; the host menu only exposes capability entry points and does not replace runtime authority."),
        DemoHostMenuGroups.Runtime => T(
            "运行时面板直接读取共享运行时的文档、选择、视口和诊断，方便操作时核对同一个 Editor.Session 的当前状态。",
            "The runtime panel reads document, selection, viewport, and diagnostics directly from the shared runtime so the current Editor.Session state stays visible during interaction."),
        DemoHostMenuGroups.Extensions => T(
            "扩展面板把 plugin discovery、trust decision、load snapshot 和 contribution shape 放在同一个位置，证明这些能力不是 README-only 叙事。",
            "The extensions panel keeps plugin discovery, trust decisions, load snapshots, and contribution shape in one place so the feature is visible instead of README-only."),
        DemoHostMenuGroups.Automation => T(
            "自动化面板提供可直接执行的 canned run，并把 request、step progress 和 result snapshot 连到同一会话上。",
            "The automation panel provides runnable canned flows and projects request, step progress, and result snapshots back onto the same session."),
        DemoHostMenuGroups.Integration => T(
            "集成面板同时说明 HostSample 的最小 consumer path，并展示独立表面、presenter replacement 和本地化证明。",
            "The integration panel explains HostSample as the minimal consumer path and shows standalone surfaces, presenter replacement, and localization proof together."),
        DemoHostMenuGroups.Proof => T(
            "证明面板把宿主壳层控制与共享运行时证据并排展示，用来确认当前窗口没有第二个编辑器实例，只有同一个 Editor.Session。",
            "The proof panel shows host-shell controls next to shared runtime evidence to confirm the window owns no second editor instance, only the same Editor.Session."),
        _ => T("通过宿主级菜单控制同一张实时节点图。", "Control the same live graph through the host-owned menu.")
    };

    public IReadOnlyList<string> SelectedHostMenuGroupLines => SelectedHostMenuGroup switch
    {
        DemoHostMenuGroups.Showcase =>
        [
            T("第一眼先看到宿主菜单和实时节点图，而不是三栏说明墙。", "Users see the host menu and live graph first instead of a three-column explanation wall."),
            T("主区只有一个 GraphEditorView，宿主面板只补充解释和控制入口。", "The main area holds a single GraphEditorView, while the host pane adds explanation and control entry points."),
            .. DemoEntries,
        ],
        DemoHostMenuGroups.View =>
        [
            T("显示顶栏：", "Show header: ") + BoolText(IsHeaderChromeVisible),
            T("显示节点库：", "Show library: ") + BoolText(IsLibraryChromeVisible),
            T("显示检查器：", "Show inspector: ") + BoolText(IsInspectorChromeVisible),
            T("显示状态栏：", "Show status bar: ") + BoolText(IsStatusChromeVisible),
            T(ChromeControlsHelper, "These toggles only control shell chrome and never rebuild the current Editor session."),
        ],
        DemoHostMenuGroups.Behavior =>
        [
            T("只读模式：", "Read-only mode: ") + BoolText(IsReadOnlyEnabled),
            T("网格吸附：", "Grid snapping: ") + BoolText(IsGridSnappingEnabled),
            T("对齐辅助线：", "Alignment guides: ") + BoolText(IsAlignmentGuidesEnabled),
            T("工作区命令：", "Workspace commands: ") + BoolText(AreWorkspaceCommandsEnabled),
            T("片段命令：", "Fragment commands: ") + BoolText(AreFragmentCommandsEnabled),
            T("宿主菜单扩展：", "Host menu extensions: ") + BoolText(AreHostMenuExtensionsEnabled),
        ],
        DemoHostMenuGroups.Runtime =>
        [
            .. RuntimeMetricLines,
            T("共享运行时入口：", "Shared runtime surface: ") + RuntimeDiagnosticsSourceName,
            T("最近诊断：", "Recent diagnostics: ") + RecentDiagnostics.Count + T(" 条", ""),
            T(RuntimeDiagnosticsHelper, "These diagnostics come directly from Editor.Session.Diagnostics so the shared runtime state stays visible."),
        ],
        DemoHostMenuGroups.Extensions =>
        [
            T("发现候选项：", "Discovered candidates: ") + PluginCandidates.Count + T(" 个", ""),
            T("Allowlist 决策：", "Allowlist decisions: ") + _pluginShowcase.TrustPolicy.Entries.Count,
            T("加载快照：", "Load snapshots: ") + PluginLoadSnapshots.Count + T(" 条", ""),
            .. PluginCandidateLines,
            .. PluginAllowlistLines,
            .. PluginLoadLines,
        ],
        DemoHostMenuGroups.Automation =>
        [
            .. AutomationRequestLines,
            .. AutomationResultLines,
        ],
        DemoHostMenuGroups.Integration =>
        [
            .. ConsumerPathLines,
            .. LocalizationProofLines,
            T(StandaloneSurfaceHelper, "These previews share the same runtime session as the main editor."),
            T(PresentationHelper, "Presentation is replaceable; editing behavior is not."),
        ],
        DemoHostMenuGroups.Proof =>
        [
            T("宿主壳层控制来自 MainWindowViewModel，负责菜单、抽屉与壳层开关。", "Host shell control comes from MainWindowViewModel and owns menus, drawers, and shell toggles."),
            T("共享运行时证据：", "Shared runtime proof: ") + RuntimeDiagnosticsSourceName + T(" 提供当前文档、选择、视口和最近诊断。", " provides the current document, selection, viewport, and recent diagnostics."),
            T("当前窗口没有第二个编辑器实例；中心画布与宿主控制都作用于同一个 Editor.Session。", "The current window does not host a second editor instance; the center canvas and host controls act on the same Editor.Session."),
            HostPaneStateCaption,
        ],
        _ =>
        [
            HostSessionContinuityCaption,
        ]
    };

    public string HostPaneStateCaption
        => IsHostPaneOpen
            ? T("面板已展开 · ", "Panel open · ") + SelectedHostMenuGroupTitle
            : T("面板已收起", "Panel closed");

    public string HostSessionContinuityCaption
        => T("宿主菜单和抽屉始终作用于同一实时会话", "The host menu and drawer always act on the same live session");

    partial void OnSelectedCapabilityChanged(CapabilityShowcaseItem value)
    {
        if (value is not null)
        {
            _lastSelectedCapabilityKey = value.Key;
        }

        OnPropertyChanged(nameof(SelectedCapabilityTitle));
        OnPropertyChanged(nameof(SelectedCapabilitySummary));
        OnPropertyChanged(nameof(SelectedCapabilityArchitecture));
        OnPropertyChanged(nameof(SelectedCapabilityBullets));
        OnPropertyChanged(nameof(SelectedCapabilityProofLines));
    }

    private void UpdateCapabilities()
    {
        var selectedKey = SelectedCapability?.Key ?? _lastSelectedCapabilityKey;
        _capabilities = CreateCapabilityShowcaseItems();
        OnPropertyChanged(nameof(Capabilities));

        var selected = selectedKey is null
            ? _capabilities[0]
            : _capabilities.SingleOrDefault(item => item.Key == selectedKey) ?? _capabilities[0];

        SelectedCapability = selected;
    }

    private CapabilityShowcaseItem ResolveSelectedCapability()
    {
        if (SelectedCapability is not null)
        {
            return SelectedCapability;
        }

        if (!string.IsNullOrWhiteSpace(_lastSelectedCapabilityKey))
        {
            var retained = _capabilities.SingleOrDefault(item => item.Key == _lastSelectedCapabilityKey);
            if (retained is not null)
            {
                return retained;
            }
        }

        return _capabilities[0];
    }

    private IReadOnlyList<CapabilityShowcaseItem> CreateCapabilityShowcaseItems()
    {
        return
        [
            new CapabilityShowcaseItem(
                "full-shell",
                T("完整壳层", "Full Shell"),
                T("以默认 GraphEditorView 展示开箱即用的完整编辑壳层。", "Show the ready-to-use full editing shell through the default GraphEditorView."),
                T(
                    "完整壳层把节点库、画布、检查器与状态区组合成一个宿主可直接嵌入的默认体验。",
                    "The full shell combines library, canvas, inspector, and status areas into a default experience the host can embed directly."),
                [
                    T("所属层：AsterGraph.Avalonia 完整壳层。", "Layer: AsterGraph.Avalonia full shell."),
                    T("宿主入口：GraphEditorView 或 AsterGraphAvaloniaViewFactory。", "Host entry: GraphEditorView or AsterGraphAvaloniaViewFactory."),
                    T("可替换点：保留默认交互，同时通过 Presentation/菜单/本地化等 seam 做增量替换。", "Seams: keep default interaction while replacing presentation, menus, or localization incrementally."),
                ],
                [
                    T("主区只有一个实时 GraphEditorView。", "The main area hosts exactly one live GraphEditorView."),
                    $"{nameof(GraphEditorViewChromeMode)}.{nameof(GraphEditorViewChromeMode.Default)} / {nameof(GraphEditorViewChromeMode.CanvasOnly)} " +
                    T("共用同一 Editor.Session。", "share the same Editor.Session."),
                ]),
            new CapabilityShowcaseItem(
                "standalone-surfaces",
                T("独立表面", "Standalone Surfaces"),
                T("把画布、检查器、缩略图拆成独立宿主表面而不复制运行时。", "Break canvas, inspector, and minimap into standalone host surfaces without copying runtime state."),
                T(
                    "Standalone Canvas / Inspector / Mini Map 让宿主按需嵌入局部 UI，同时继续绑定同一个编辑器实例。",
                    "Standalone Canvas / Inspector / Mini Map let the host embed partial UI while still binding to the same editor instance."),
                [
                    T("所属层：AsterGraph.Avalonia 独立宿主表面。", "Layer: AsterGraph.Avalonia standalone host surfaces."),
                    T("宿主入口：AsterGraphCanvasViewFactory、AsterGraphInspectorViewFactory、AsterGraphMiniMapViewFactory。", "Host entry: AsterGraphCanvasViewFactory, AsterGraphInspectorViewFactory, and AsterGraphMiniMapViewFactory."),
                    T("可替换点：宿主只取需要的表面，不必强制采用完整壳层。", "Seams: the host can take only the surfaces it needs instead of adopting the whole shell."),
                ],
                [
                    T(StandaloneSurfaceHelper, "These previews share the same runtime session as the main editor."),
                    T("当前主编辑器文档：", "Current main editor document: ") + Editor.Title + "。",
                ]),
            new CapabilityShowcaseItem(
                "replaceable-presentation",
                T("可替换呈现", "Replaceable Presentation"),
                T("用公开 presenter seam 替换节点、菜单、检查器与缩略图的视觉表达。", "Replace the visual expression of nodes, menus, inspectors, and minimaps through public presenter seams."),
                T(
                    "AsterGraph 保持编辑命令与运行时不变，把视觉层替换收敛到 presenter contract，而不是复制一套行为系统。",
                    "AsterGraph keeps editing commands and runtime stable while collapsing visual replacement into presenter contracts instead of duplicating a behavior stack."),
                [
                    T("所属层：AsterGraph.Avalonia 呈现适配层。", "Layer: AsterGraph.Avalonia presentation adapters."),
                    T("宿主入口：AsterGraphPresentationOptions。", "Host entry: AsterGraphPresentationOptions."),
                    T("可替换点：NodeVisualPresenter、ContextMenuPresenter、InspectorPresenter、MiniMapPresenter。", "Seams: NodeVisualPresenter, ContextMenuPresenter, InspectorPresenter, and MiniMapPresenter."),
                ],
                [
                    T(PresentationHelper, "Presentation is replaceable; editing behavior is not."),
                    T("同一 Editor 继续负责命令、选择、视口和诊断。", "The same Editor continues to own commands, selection, viewport, and diagnostics."),
                ]),
            new CapabilityShowcaseItem(
                "runtime-diagnostics",
                T("运行时与诊断", "Runtime And Diagnostics"),
                T("通过 Editor.Session 查询、检查快照与最近诊断显式观察当前运行时。", "Observe the current runtime explicitly through Editor.Session queries, inspection snapshots, and recent diagnostics."),
                T(
                    "诊断区直接读取 Editor.Session.Diagnostics 与 Queries 快照，帮助宿主验证会话状态而不是依赖状态栏文案。",
                    "The diagnostics area reads Editor.Session.Diagnostics and query snapshots directly so hosts can verify session state instead of relying on status-bar prose."),
                [
                    T("所属层：AsterGraph.Editor 运行时与诊断契约。", "Layer: AsterGraph.Editor runtime and diagnostics contracts."),
                    T("宿主入口：Editor.Session.Queries 与 Editor.Session.Diagnostics。", "Host entry: Editor.Session.Queries and Editor.Session.Diagnostics."),
                    T("可替换点：宿主可以把这些快照接入自己的日志、支持面板或调试工作流。", "Seams: hosts can route the same snapshots into their own logging, support panes, or debugging flows."),
                ],
                [
                    T(RuntimeDiagnosticsHelper, "These diagnostics come directly from Editor.Session.Diagnostics so the shared runtime state stays visible."),
                    T("最近诊断数量：", "Recent diagnostic count: ") + Editor.Session.Diagnostics.GetRecentDiagnostics(10).Count + "。",
                ]),
            new CapabilityShowcaseItem(
                "plugin-trust-and-loading",
                T("插件信任与加载", "Plugin Trust And Loading"),
                T("把 candidate discovery、trust policy 和 load snapshot 变成真正可见的产品面。", "Turn candidate discovery, trust policy, and load snapshots into a visible product surface."),
                T(
                    "Demo 同时声明 plugin registration、trust policy 和 manifest-source discovery，再把候选项与加载结果直接投影到宿主抽屉。",
                    "The demo declares plugin registration, trust policy, and manifest-source discovery together, then projects candidate and load results directly into the host drawer."),
                [
                    T("所属层：AsterGraph.Editor plugin contracts。", "Layer: AsterGraph.Editor plugin contracts."),
                    T("宿主入口：DiscoverPluginCandidates(...)、PluginTrustPolicy、GetPluginLoadSnapshots()。", "Host entry: DiscoverPluginCandidates(...), PluginTrustPolicy, and GetPluginLoadSnapshots()."),
                    T("可替换点：宿主可替换 trust policy、manifest source、allowlist persistence 和 future distribution policy。", "Seams: hosts can replace the trust policy, manifest source, allowlist persistence, and future distribution policy."),
                ],
                [
                    T("发现候选项：", "Discovered candidates: ") + PluginCandidates.Count + "。",
                    T("Allowlist 决策：", "Allowlist decisions: ") + _pluginShowcase.TrustPolicy.Entries.Count + "。",
                    T("加载快照：", "Load snapshots: ") + PluginLoadSnapshots.Count + "。",
                ]),
            new CapabilityShowcaseItem(
                "automation-execution",
                T("自动化执行", "Automation Execution"),
                T("把 IGraphEditorSession.Automation 的 request、progress 和 result 做成可运行的 demo surface。", "Make IGraphEditorSession.Automation request, progress, and result visible as a runnable demo surface."),
                T(
                    "宿主通过稳定 command id 组合 canned automation run，运行结果回到 typed snapshot 与共享诊断里。",
                    "The host composes canned automation runs from stable command ids and gets typed snapshots plus shared diagnostics back."),
                [
                    T("所属层：AsterGraph.Editor automation contract。", "Layer: AsterGraph.Editor automation contract."),
                    T("宿主入口：IGraphEditorSession.Automation.Execute(...).", "Host entry: IGraphEditorSession.Automation.Execute(...)."),
                    T("可替换点：宿主可以继续叠加审批、日志、编排或脚本层。", "Seams: hosts can layer approval, logging, orchestration, or scripting on top."),
                ],
                [
                    T("三组 canned automation 可直接运行。", "Three canned automation runs can be executed directly."),
                    T("progress 与 result snapshot 会持续投影到抽屉。", "Progress and result snapshots keep projecting into the drawer."),
                ]),
            new CapabilityShowcaseItem(
                "consumer-host-path",
                T("Consumer Path", "Consumer Path"),
                T("把 HostSample 和 Demo 的职责分开，让最小接入路径与 showcase host 各司其职。", "Separate HostSample and Demo so the minimal adoption path and the showcase host keep distinct responsibilities."),
                T(
                    "HostSample 证明最小 consumer path；Demo 负责展示完整能力和宿主边界，不再兼任最小样例。",
                    "HostSample proves the minimal consumer path; Demo shows the full capability surface and host boundary instead of acting as the minimal sample."),
                [
                    T("所属层：consumer onboarding。", "Layer: consumer onboarding."),
                    T("宿主入口：CreateSession(...)、Create(...)、AsterGraphAvaloniaViewFactory、HostSample。", "Host entry: CreateSession(...), Create(...), AsterGraphAvaloniaViewFactory, and HostSample."),
                    T("可替换点：宿主可只采用 runtime、完整壳层或独立表面。", "Seams: hosts can adopt only runtime, the full shell, or standalone surfaces."),
                ],
                [
                    T("HostSample 明确作为最小 consumer path。", "HostSample is explicitly the minimal consumer path."),
                    T("Demo 明确作为 showcase host。", "Demo is explicitly the showcase host."),
                ]),
            new CapabilityShowcaseItem(
                "history-save-contract",
                T("History / Save Contract", "History / Save Contract"),
                T("把 save/dirty/history 语义继续留在公开产品面，而不是只藏在 regression lane 里。", "Keep save/dirty/history semantics in the public product surface instead of only inside the regression lane."),
                T(
                    "Demo 的 automation、diagnostics 和 consumer docs 围绕同一份 state contract 组织，避免运行时语义只存在于内部经验里。",
                    "The demo's automation, diagnostics, and consumer docs now organize around the same state contract so runtime semantics do not live only in internal memory."),
                [
                    T("所属层：state contract / proof ring。", "Layer: state contract / proof ring."),
                    T("宿主入口：state-contracts.md、contract lane、ScaleSmoke markers。", "Host entry: state-contracts.md, the contract lane, and ScaleSmoke markers."),
                    T("可替换点：宿主可以依赖 contract，而不是猜 retained facade 的行为。", "Seams: hosts can rely on the contract instead of guessing retained-facade behavior."),
                ],
                [
                    T("automation run 会触达同一个 save/history baseline。", "Automation runs touch the same save/history baseline."),
                    T("proof ring 继续对这条线做机器化保护。", "The proof ring keeps this line machine-protected."),
                ]),
            new CapabilityShowcaseItem(
                "progressive-node-surface",
                T("渐进式节点表面", "Progressive Node Surface"),
                T("把宽度、展开态、分组和输入端内联值编辑变成同一条 node surface 路径。", "Turn width, expansion, grouping, and inline input literals into one shared node-surface path."),
                T(
                    "Demo 现在默认展示 expanded node card、editor-only group boundary，以及“未连接时可内联、连接后以上游值为准”的单一值来源规则。",
                    "The demo now shows an expanded node card, an editor-only group boundary, and the single-source rule where unconnected inputs edit inline while connected inputs defer to upstream values."),
                [
                    T("所属层：AsterGraph.Editor node-surface contract + AsterGraph.Avalonia stock canvas。", "Layer: AsterGraph.Editor node-surface contract plus the stock AsterGraph.Avalonia canvas."),
                    T("宿主入口：GetNodeSurfaceSnapshots()、GetNodeGroupSnapshots()、TrySetNodeWidth(...)、TrySetNodeExpansionState(...)、TrySetNodeGroupExtraPadding(...)。", "Host entry: GetNodeSurfaceSnapshots(), GetNodeGroupSnapshots(), TrySetNodeWidth(...), TrySetNodeExpansionState(...), and TrySetNodeGroupExtraPadding(...)."),
                    T("可替换点：宿主可复用同一份 surface state，再决定自定义卡片、分组样式、resolved bounds 策略或后续 composite node 路径。", "Seams: hosts can reuse the same surface state while customizing cards, group visuals, resolved-bounds policy, or a later composite-node path."),
                ],
                [
                    T("默认图包含会随成员节点自动贴合的 editor-only 分组边界。", "The default graph includes an editor-only group boundary that auto-fits its member nodes."),
                    T("Lighting Mix 节点默认展开，并显示 inline input authoring。", "The Lighting Mix node starts expanded and exposes inline input authoring."),
                    T("连接中的 Pulse 输入会覆盖本地 literal，未连接的 Rim Mask 继续内联编辑。", "The connected Pulse input overrides its local literal while the unconnected Rim Mask stays inline-editable."),
                    T("组框支持按边拉伸并保留额外留白，同时继续跟随成员节点移动与扩展。", "Group frames support per-edge resize with persistent whitespace while continuing to follow member-node movement and expansion."),
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
