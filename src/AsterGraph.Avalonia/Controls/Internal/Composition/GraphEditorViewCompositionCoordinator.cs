using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface IGraphEditorViewCompositionHost
{
    GraphEditorViewModel? Editor { get; }

    GraphEditorViewChromeMode ChromeMode { get; }

    bool IsHeaderChromeVisible { get; }

    bool IsLibraryChromeVisible { get; }

    bool IsInspectorChromeVisible { get; }

    bool IsStatusChromeVisible { get; }

    bool EnableDefaultContextMenu { get; }

    bool EnableDefaultCommandShortcuts { get; }

    bool EnableDefaultWheelViewportGestures { get; }

    bool EnableAltLeftDragPanning { get; }

    NodeCanvas? NodeCanvas { get; }

    GraphInspectorView? InspectorSurface { get; }

    GraphMiniMap? MiniMapSurface { get; }

    Grid? ShellGrid { get; }

    Border? HeaderChrome { get; }

    Border? LibraryChrome { get; }

    Border? InspectorChrome { get; }

    Border? StatusChrome { get; }

    double DefaultShellRowSpacing { get; }

    double DefaultShellColumnSpacing { get; }

    IResourceDictionary Resources { get; }
}

internal sealed class GraphEditorViewCompositionCoordinator
{
    private readonly IGraphEditorViewCompositionHost _host;

    public GraphEditorViewCompositionCoordinator(IGraphEditorViewCompositionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void ApplyStyleOptions(GraphEditorViewModel? editor)
    {
        if (editor?.StyleOptions is null)
        {
            return;
        }

        var adapter = new GraphEditorStyleAdapter(editor.StyleOptions);
        adapter.ApplyResources(_host.Resources);
    }

    public void ApplyChromeMode()
    {
        var showChrome = _host.ChromeMode == GraphEditorViewChromeMode.Default;
        var showHeader = showChrome && _host.IsHeaderChromeVisible;
        var showLibrary = showChrome && _host.IsLibraryChromeVisible;
        var showInspector = showChrome && _host.IsInspectorChromeVisible;
        var showStatus = showChrome && _host.IsStatusChromeVisible;

        if (_host.HeaderChrome is not null)
        {
            _host.HeaderChrome.IsVisible = showHeader;
        }

        if (_host.LibraryChrome is not null)
        {
            _host.LibraryChrome.IsVisible = showLibrary;
        }

        if (_host.InspectorChrome is not null)
        {
            _host.InspectorChrome.IsVisible = showInspector;
        }

        if (_host.StatusChrome is not null)
        {
            _host.StatusChrome.IsVisible = showStatus;
        }

        if (_host.ShellGrid is not null)
        {
            _host.ShellGrid.RowSpacing = showHeader || showStatus ? _host.DefaultShellRowSpacing : 0;
            _host.ShellGrid.ColumnSpacing = showLibrary || showInspector ? _host.DefaultShellColumnSpacing : 0;
        }
    }

    public void ApplyCanvasBehaviorOptions()
    {
        if (_host.NodeCanvas is null)
        {
            return;
        }

        _host.NodeCanvas.EnableDefaultContextMenu = _host.EnableDefaultContextMenu;
        _host.NodeCanvas.EnableDefaultCommandShortcuts = _host.EnableDefaultCommandShortcuts;
        _host.NodeCanvas.EnableDefaultWheelViewportGestures = _host.EnableDefaultWheelViewportGestures;
        _host.NodeCanvas.EnableAltLeftDragPanning = _host.EnableAltLeftDragPanning;
    }

    public void ApplyPresentationOptions(AsterGraphPresentationOptions? presentation)
    {
        if (_host.NodeCanvas is not null)
        {
            _host.NodeCanvas.NodeVisualPresenter = presentation?.NodeVisualPresenter;
            _host.NodeCanvas.ContextMenuPresenter = presentation?.ContextMenuPresenter;
            _host.NodeCanvas.NodeParameterEditorRegistry = presentation?.NodeParameterEditorRegistry;
        }

        if (_host.InspectorSurface is not null)
        {
            _host.InspectorSurface.InspectorPresenter = presentation?.InspectorPresenter;
            _host.InspectorSurface.NodeParameterEditorRegistry = presentation?.NodeParameterEditorRegistry;
        }

        if (_host.MiniMapSurface is not null)
        {
            _host.MiniMapSurface.MiniMapPresenter = presentation?.MiniMapPresenter;
        }
    }
}
