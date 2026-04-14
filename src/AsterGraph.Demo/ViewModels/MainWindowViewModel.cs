using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Compatibility;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.Menus;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Localization;
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

        Capabilities = CreateCapabilityShowcaseItems();
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
