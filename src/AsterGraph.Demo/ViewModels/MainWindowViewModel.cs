using CommunityToolkit.Mvvm.ComponentModel;
using AsterGraph.Core.Compatibility;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.Menus;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const double DemoSnapTolerance = 18;

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
        ApplyHostOptions(status: null);
    }

    public GraphEditorViewModel Editor { get; }

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
