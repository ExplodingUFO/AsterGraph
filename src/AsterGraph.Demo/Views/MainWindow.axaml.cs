using Avalonia.Controls;
using Avalonia.Data;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Demo.Presentation;
using AsterGraph.Demo.ViewModels;

namespace AsterGraph.Demo.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel? _currentViewModel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not MainWindowViewModel viewModel || ReferenceEquals(viewModel, _currentViewModel))
        {
            return;
        }

        _currentViewModel = viewModel;
        ComposeSurfaces(viewModel);
    }

    private void ComposeSurfaces(MainWindowViewModel viewModel)
    {
        var replacementPresentation = DemoShowcasePresenters.CreateReplacementPreviewOptions();

        var mainView = AsterGraphAvaloniaViewFactory.Create(new AsterGraphAvaloniaViewOptions
        {
            Editor = viewModel.Editor,
        });
        mainView.Name = "MainGraphEditorView";
        BindChrome(mainView, viewModel);
        SetHostContent("PART_MainGraphEditorHost", mainView);

        var standaloneCanvas = AsterGraphCanvasViewFactory.Create(new AsterGraphCanvasViewOptions
        {
            Editor = viewModel.Editor,
            EnableDefaultContextMenu = false,
            EnableDefaultCommandShortcuts = false,
        });
        standaloneCanvas.Name = "StandaloneCanvasPreview";
        SetHostContent("PART_StandaloneCanvasHost", standaloneCanvas);

        var standaloneInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
        {
            Editor = viewModel.Editor,
        });
        standaloneInspector.Name = "StandaloneInspectorPreview";
        SetHostContent("PART_StandaloneInspectorHost", standaloneInspector);

        var standaloneMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Editor = viewModel.Editor,
        });
        standaloneMiniMap.Name = "StandaloneMiniMapPreview";
        SetHostContent("PART_StandaloneMiniMapHost", standaloneMiniMap);

        var customInspector = AsterGraphInspectorViewFactory.Create(new AsterGraphInspectorViewOptions
        {
            Editor = viewModel.Editor,
            Presentation = replacementPresentation,
        });
        customInspector.Name = "CustomInspectorPreview";
        SetHostContent("PART_CustomInspectorHost", customInspector);

        var customMiniMap = AsterGraphMiniMapViewFactory.Create(new AsterGraphMiniMapViewOptions
        {
            Editor = viewModel.Editor,
            Presentation = replacementPresentation,
        });
        customMiniMap.Name = "CustomMiniMapPreview";
        SetHostContent("PART_CustomMiniMapHost", customMiniMap);
    }

    private void BindChrome(GraphEditorView view, MainWindowViewModel viewModel)
    {
        view.Bind(GraphEditorView.IsHeaderChromeVisibleProperty, new Binding(nameof(MainWindowViewModel.IsHeaderChromeVisible)) { Source = viewModel });
        view.Bind(GraphEditorView.IsLibraryChromeVisibleProperty, new Binding(nameof(MainWindowViewModel.IsLibraryChromeVisible)) { Source = viewModel });
        view.Bind(GraphEditorView.IsInspectorChromeVisibleProperty, new Binding(nameof(MainWindowViewModel.IsInspectorChromeVisible)) { Source = viewModel });
        view.Bind(GraphEditorView.IsStatusChromeVisibleProperty, new Binding(nameof(MainWindowViewModel.IsStatusChromeVisible)) { Source = viewModel });
    }

    private void SetHostContent(string hostName, Control control)
    {
        if (this.FindControl<ContentControl>(hostName) is { } host)
        {
            host.Content = control;
        }
    }
}
