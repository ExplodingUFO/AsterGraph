using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Compatibility;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.Integration;
using AsterGraph.Demo.Menus;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Demo.Shell;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const double DemoSnapTolerance = 18;
    private const string RuntimeDiagnosticsHelper = "以下诊断直接来自 Session.Diagnostics，用于确认共享运行时状态。";
    private const string StandaloneSurfaceHelper = "这些预览与主编辑器共享同一运行时会话。";
    private const string PresentationHelper = "可替换的是视觉呈现，不是编辑行为。";
    private const string ChromeModeHelper = "关闭后可体验完整编辑流程；开启后仅保留只读浏览。";
    private const string ChromeControlsHelper = "这些开关只控制壳层显示，不会重建当前 Editor 会话。";
    private const string DemoStorageFolderName = "AsterGraph.Demo";
    private IReadOnlyList<CapabilityShowcaseItem> _capabilities = [];
    private readonly DemoAiPipelineMockRuntimeProvider _aiPipelineMockRuntimeProvider = new();

    public MainWindowViewModel()
        : this(null)
    {
    }

    public MainWindowViewModel(MainWindowShellOptions? shellOptions)
    {
        _shellOptions = shellOptions ?? new MainWindowShellOptions();
        var storageRootPath = _shellOptions.ResolveStorageRootPath();
        _shellStateStore = new DemoShellStateStore(storageRootPath);
        _workspaceService = new DemoMutableWorkspaceService(_shellOptions.ResolveDefaultWorkspacePath(storageRootPath));

        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new DemoNodeDefinitionProvider());
        _pluginShowcase = DemoPluginShowcase.Create(storageRootPath);
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

        editor = AsterGraphEditorFactory.Create(new AsterGraphEditorOptions
        {
            Document = DemoGraphFactory.CreateStartupDocument(catalog, _shellOptions.InitialScenario),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = storageRootPath,
            WorkspaceService = _workspaceService,
            StyleOptions = style,
            BehaviorOptions = behavior,
            PluginRegistrations = _pluginShowcase.Registrations,
            PluginTrustPolicy = _pluginShowcase.TrustPolicy,
            ContextMenuAugmentor = contextMenuAugmentor,
            LocalizationProvider = CreateGraphLocalizationProvider(SelectedLanguage.Code),
            RuntimeOverlayProvider = _aiPipelineMockRuntimeProvider,
        });

        Editor = editor;
        RefreshPluginCandidates();
        Editor.DocumentChanged += (_, _) => RefreshRuntimeProjection();
        Editor.SelectionChanged += (_, _) => RefreshRuntimeProjection();
        Editor.ViewportChanged += (_, _) => RefreshRuntimeProjection();
        Editor.FragmentExported += (_, _) => RefreshRuntimeProjection();
        Editor.FragmentImported += (_, _) => RefreshRuntimeProjection();
        Session.Events.DocumentChanged += (_, _) => PersistAutosaveDraft();
        Session.Events.CommandExecuted += (_, args) =>
        {
            TrackCommandExecuted(args);
            PersistAutosaveDraft();
            RefreshRuntimeProjection();
        };
        Session.Events.AutomationStarted += (_, args) => OnAutomationStarted(args);
        Session.Events.AutomationProgress += (_, args) => OnAutomationProgress(args);
        Session.Events.AutomationCompleted += (_, args) => OnAutomationCompleted(args);
        Editor.DocumentChanged += (_, _) => PersistAutosaveDraft();
        Editor.ViewportChanged += (_, _) => PersistShellState();

        UpdateCapabilities();
        UpdateScenarioTour();
        SelectedCapability = Capabilities[0];
        InitializeShellState();
        ApplyHostOptions(status: null);
        RefreshRuntimeProjection();
    }

    public GraphEditorViewModel Editor { get; }

    public IGraphEditorSession Session => Editor.Session;

    public IReadOnlyList<CapabilityShowcaseItem> Capabilities => _capabilities;

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
}
