using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Hosting;
using AsterGraph.Demo.Presentation;
using AsterGraph.Demo.ViewModels;

namespace AsterGraph.Demo.Views;

public partial class MainWindow : Window
{
    private static readonly IReadOnlyList<string> HostCommandRailIds =
    [
        "workspace.save",
        "workspace.load",
        "history.undo",
        "history.redo",
        "viewport.fit",
        "viewport.reset",
    ];

    private MainWindowViewModel? _currentViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DragOverEvent, HandleWorkspaceDragOver);
        AddHandler(DragDrop.DropEvent, HandleWorkspaceDrop);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (_currentViewModel is not null)
        {
            DetachCommandRailSubscriptions(_currentViewModel);
        }

        if (DataContext is not MainWindowViewModel viewModel || ReferenceEquals(viewModel, _currentViewModel))
        {
            return;
        }

        _currentViewModel = viewModel;
        ComposeSurfaces(viewModel);
        AttachCommandRailSubscriptions(viewModel);
        RebuildCommandRail(viewModel);
        ApplyWindowShellState(viewModel);
        UpdateThemeProjection(viewModel);
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

    private void AttachCommandRailSubscriptions(MainWindowViewModel viewModel)
    {
        viewModel.Editor.Session.Events.DocumentChanged -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.SelectionChanged -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.ViewportChanged -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.CommandExecuted -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.PendingConnectionChanged -= HandleCommandRailChanged;

        viewModel.Editor.Session.Events.DocumentChanged += HandleCommandRailChanged;
        viewModel.Editor.Session.Events.SelectionChanged += HandleCommandRailChanged;
        viewModel.Editor.Session.Events.ViewportChanged += HandleCommandRailChanged;
        viewModel.Editor.Session.Events.CommandExecuted += HandleCommandRailChanged;
        viewModel.Editor.Session.Events.PendingConnectionChanged += HandleCommandRailChanged;
    }

    private void DetachCommandRailSubscriptions(MainWindowViewModel viewModel)
    {
        viewModel.Editor.Session.Events.DocumentChanged -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.SelectionChanged -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.ViewportChanged -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.CommandExecuted -= HandleCommandRailChanged;
        viewModel.Editor.Session.Events.PendingConnectionChanged -= HandleCommandRailChanged;
    }

    private void HandleCommandRailChanged(object? sender, EventArgs e)
    {
        if (_currentViewModel is not null)
        {
            RebuildCommandRail(_currentViewModel);
        }
    }

    private void RebuildCommandRail(MainWindowViewModel viewModel)
    {
        var rail = this.FindControl<WrapPanel>("PART_CommandRail");
        if (rail is null)
        {
            return;
        }

        rail.Children.Clear();
        var projection = AsterGraphHostedActionFactory.CreateProjection(
            AsterGraphHostedActionFactory.CreateCommandActions(viewModel.Editor.Session));
        foreach (var action in projection.Select(HostCommandRailIds))
        {
            var button = new Button
            {
                Name = $"PART_HostCommand_{action.Id}",
                Content = action.Title,
                IsEnabled = action.CanExecute,
            };
            if (!string.IsNullOrWhiteSpace(action.DisabledReason))
            {
                ToolTip.SetTip(button, action.DisabledReason);
            }

            button.Click += (_, _) =>
            {
                action.TryExecute();
                RebuildCommandRail(viewModel);
            };
            rail.Children.Add(button);
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (_currentViewModel is null)
        {
            return;
        }

        ApplyWindowShellState(_currentViewModel);
        _currentViewModel.RestorePendingViewport();
        UpdateThemeProjection(_currentViewModel);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_currentViewModel?.HandleWindowClosingRequest() == true)
        {
            e.Cancel = true;
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _currentViewModel?.RecordWindowSize(e.NewSize.Width, e.NewSize.Height);
    }

    public bool TryOpenWorkspaceFiles(IReadOnlyList<string> paths)
    {
        if (_currentViewModel is null)
        {
            return false;
        }

        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            if (_currentViewModel.TryOpenWorkspacePath(path))
            {
                _currentViewModel.RestorePendingViewport();
                return true;
            }
        }

        return false;
    }

    private void ApplyWindowShellState(MainWindowViewModel viewModel)
    {
        if (viewModel.PreferredWindowWidth > 0)
        {
            Width = Math.Max(MinWidth, viewModel.PreferredWindowWidth);
        }

        if (viewModel.PreferredWindowHeight > 0)
        {
            Height = Math.Max(MinHeight, viewModel.PreferredWindowHeight);
        }
    }

    private void UpdateThemeProjection(MainWindowViewModel viewModel)
    {
        RequestedThemeVariant = ThemeVariant.Default;
        viewModel.UpdateThemeVariant(ActualThemeVariant?.ToString() ?? RequestedThemeVariant?.ToString());
    }

    private void HandleWorkspaceDragOver(object? sender, DragEventArgs e)
    {
        if (ResolveDroppedWorkspacePaths(e).Count == 0)
        {
            return;
        }

        e.DragEffects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private void HandleWorkspaceDrop(object? sender, DragEventArgs e)
    {
        var paths = ResolveDroppedWorkspacePaths(e);
        if (paths.Count == 0)
        {
            return;
        }

        TryOpenWorkspaceFiles(paths);
        e.Handled = true;
    }

    private static List<string> ResolveDroppedWorkspacePaths(DragEventArgs args)
        => ResolveDroppedWorkspacePaths(args.DataTransfer);

    private static List<string> ResolveDroppedWorkspacePaths(IDataTransfer? dataTransfer)
        => (dataTransfer is null ? null : dataTransfer.TryGetFiles())?
            .Select(item => item.TryGetLocalPath())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .Where(path => path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToList()
        ?? [];
}
