using AsterGraph.Core.Compatibility;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Demo.Definitions;

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
            },
            Connection = GraphEditorStyleOptions.Default.Connection with
            {
                Thickness = 3.4,
            },
        };

        Editor = new GraphEditorViewModel(
            DemoGraphFactory.CreateDefault(catalog),
            catalog,
            new DefaultPortCompatibilityService(),
            new GraphWorkspaceService(),
            style);
    }

    public GraphEditorViewModel Editor { get; }
}
