using AsterGraph.Core.Compatibility;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.Menus;

namespace AsterGraph.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
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
                SnapTolerance = 18,
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
                SnapTolerance = 18,
            },
            View = GraphEditorBehaviorOptions.Default.View with
            {
                ShowMiniMap = true,
            },
        };

        GraphEditorViewModel? editor = null;
        var contextMenuContributors = new IGraphContextMenuContributor[]
        {
            new DemoNodeResultsMenuContributor(message =>
            {
                if (editor is not null)
                {
                    editor.StatusMessage = message;
                }
            }),
        };

        editor = new GraphEditorViewModel(
            DemoGraphFactory.CreateDefault(catalog),
            catalog,
            new DefaultPortCompatibilityService(),
            new GraphWorkspaceService(),
            null,
            style,
            behavior,
            contextMenuContributors: contextMenuContributors);

        Editor = editor;
    }

    public GraphEditorViewModel Editor { get; }
}
