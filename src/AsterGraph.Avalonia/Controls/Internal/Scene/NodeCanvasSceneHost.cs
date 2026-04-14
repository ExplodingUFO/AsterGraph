using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasSceneHost
{
    GraphEditorViewModel? ViewModel { get; }

    Grid? SceneRoot { get; }

    Canvas? ConnectionLayer { get; }

    Canvas? NodeLayer { get; }

    Control CoordinateRoot { get; }

    GridBackground? BackgroundGrid { get; }

    Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals { get; }

    IGraphNodeVisualPresenter? NodeVisualPresenter { get; }

    IGraphNodeVisualPresenter StockNodeVisualPresenter { get; }

    NodeCanvasInteractionSession InteractionSession { get; }

    NodeCanvasContextMenuCoordinator ContextMenuCoordinator { get; }

    void FocusCanvas();

    void BeginNodeDrag(NodeViewModel node, PointerPressedEventArgs args);

    void ActivatePort(NodeViewModel node, PortViewModel port);
}

internal sealed class NodeCanvasSceneHost
{
    private readonly INodeCanvasSceneHost _host;
    private readonly NodeCanvasConnectionSceneRenderer _connectionSceneRenderer = new();

    public NodeCanvasSceneHost(INodeCanvasSceneHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void RebuildScene()
    {
        if (_host.NodeLayer is null || _host.ConnectionLayer is null)
        {
            return;
        }

        _host.NodeLayer.Children.Clear();
        _host.ConnectionLayer.Children.Clear();
        _host.NodeVisuals.Clear();

        if (_host.ViewModel is null)
        {
            return;
        }

        foreach (var node in _host.ViewModel.Nodes)
        {
            var visual = CreateNodeVisual(node);
            _host.NodeVisuals[node] = visual;
            _host.NodeLayer.Children.Add(visual.Root);
            UpdateNodeVisual(node);
        }

        UpdateViewportTransform();
        RenderConnections();
        Dispatcher.UIThread.Post(RenderConnections, DispatcherPriority.Loaded);
    }

    public void UpdateViewportTransform()
    {
        if (_host.SceneRoot is null || _host.ViewModel is null)
        {
            return;
        }

        var transforms = new TransformGroup();
        transforms.Children.Add(new ScaleTransform(_host.ViewModel.Zoom, _host.ViewModel.Zoom));
        transforms.Children.Add(new TranslateTransform(_host.ViewModel.PanX, _host.ViewModel.PanY));
        _host.SceneRoot.RenderTransform = transforms;
        _host.BackgroundGrid?.InvalidateVisual();
    }

    public void RenderConnections()
        => _connectionSceneRenderer.RenderConnections(CreateConnectionSceneContext());

    public void UpdateSelectionState()
    {
        foreach (var node in _host.ViewModel?.Nodes ?? [])
        {
            UpdateNodeVisual(node);
        }
    }

    public void UpdateNodePosition(NodeViewModel node)
    {
        if (_host.NodeVisuals.TryGetValue(node, out var visual))
        {
            Canvas.SetLeft(visual.Root, node.X);
            Canvas.SetTop(visual.Root, node.Y);
        }
    }

    public void UpdateNodeVisual(NodeViewModel node)
    {
        if (!_host.NodeVisuals.TryGetValue(node, out var visual))
        {
            return;
        }

        visual.Presenter.Update(visual.Visual, CreateNodeVisualContext(node));
        Canvas.SetLeft(visual.Root, node.X);
        Canvas.SetTop(visual.Root, node.Y);
    }

    public GraphPoint GetPortAnchor(NodeViewModel node, PortViewModel port)
        => _connectionSceneRenderer.GetPortAnchor(CreateConnectionSceneContext(), node, port);

    private NodeCanvasRenderedNodeVisual CreateNodeVisual(NodeViewModel node)
    {
        var presenter = _host.NodeVisualPresenter ?? _host.StockNodeVisualPresenter;
        var visual = presenter.Create(CreateNodeVisualContext(node));
        return new NodeCanvasRenderedNodeVisual(visual.Root, presenter, visual);
    }

    private GraphNodeVisualContext CreateNodeVisualContext(NodeViewModel node)
    {
        var editor = _host.ViewModel ?? throw new InvalidOperationException("Node visuals require a bound editor view model.");
        return new GraphNodeVisualContext(
            editor,
            node,
            editor.StyleOptions,
            _host.FocusCanvas,
            _host.BeginNodeDrag,
            _host.ActivatePort,
            _host.ContextMenuCoordinator.OpenNodeContextMenu,
            _host.ContextMenuCoordinator.OpenPortContextMenu);
    }

    private NodeCanvasConnectionSceneContext CreateConnectionSceneContext()
        => new(
            _host.ViewModel,
            _host.ConnectionLayer,
            _host.NodeLayer,
            _host.CoordinateRoot,
            _host.NodeVisuals,
            _host.InteractionSession.PointerScreenPosition,
            GetConnectionStyle,
            _host.ContextMenuCoordinator.CreateContextMenuSnapshot,
            _host.ContextMenuCoordinator.ResolveWorldPosition,
            _host.ContextMenuCoordinator.OpenContextMenu);

    private ConnectionStyleOptions GetConnectionStyle(ConnectionViewModel connection)
        => connection.ConversionId is not null
            ? _host.ViewModel?.StyleOptions.ConnectionOverrides.FirstOrDefault(overrideStyle => overrideStyle.ConversionId == connection.ConversionId)?.Style
              ?? _host.ViewModel?.StyleOptions.Connection
              ?? GraphEditorStyleOptions.Default.Connection
            : _host.ViewModel?.StyleOptions.Connection
              ?? GraphEditorStyleOptions.Default.Connection;
}
