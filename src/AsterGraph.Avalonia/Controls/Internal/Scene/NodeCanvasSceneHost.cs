using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Automation;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasSceneHost
{
    GraphEditorViewModel? ViewModel { get; }

    Grid? SceneRoot { get; }

    Canvas? GroupLayer { get; }

    Canvas? ConnectionLayer { get; }

    Canvas? NodeLayer { get; }

    Control CoordinateRoot { get; }

    GridBackground? BackgroundGrid { get; }

    Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals { get; }

    Dictionary<string, NodeCanvasRenderedGroupVisual> GroupVisuals { get; }

    IGraphNodeVisualPresenter? NodeVisualPresenter { get; }

    IGraphNodeVisualPresenter StockNodeVisualPresenter { get; }

    NodeCanvasInteractionSession InteractionSession { get; }

    NodeCanvasContextMenuCoordinator ContextMenuCoordinator { get; }

    void FocusCanvas();

    void BeginNodeDrag(NodeViewModel node, PointerPressedEventArgs args);

    void BeginGroupDrag(GraphEditorNodeGroupSnapshot group, PointerPressedEventArgs args);

    void ActivatePort(NodeViewModel node, PortViewModel port);
}

internal sealed record NodeCanvasRenderedGroupVisual(
    Border Root,
    Button HeaderButton,
    Border BodyBorder,
    TextBlock TitleText,
    Thumb LeftResizeThumb,
    Thumb TopResizeThumb,
    Thumb RightResizeThumb,
    Thumb BottomResizeThumb);

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
        if (_host.NodeLayer is null || _host.ConnectionLayer is null || _host.GroupLayer is null)
        {
            return;
        }

        _host.GroupLayer.Children.Clear();
        _host.NodeLayer.Children.Clear();
        _host.ConnectionLayer.Children.Clear();
        _host.GroupVisuals.Clear();
        _host.NodeVisuals.Clear();

        if (_host.ViewModel is null)
        {
            return;
        }

        foreach (var group in _host.ViewModel.GetNodeGroupSnapshots())
        {
            var visual = CreateGroupVisual(group);
            _host.GroupVisuals[group.Id] = visual;
            _host.GroupLayer.Children.Add(visual.Root);
            UpdateGroupVisual(group);
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

        UpdateGroupVisuals();
    }

    public void UpdateGroupVisuals()
    {
        foreach (var group in _host.ViewModel?.GetNodeGroupSnapshots() ?? [])
        {
            UpdateGroupVisual(group);
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

    private NodeCanvasRenderedGroupVisual CreateGroupVisual(GraphEditorNodeGroupSnapshot group)
    {
        var root = new Border
        {
            CornerRadius = new CornerRadius(16),
            BorderThickness = new Thickness(1),
        };
        AutomationProperties.SetName(root, $"{group.Title} group");
        root.PointerPressed += (_, args) =>
        {
            if (args.Source is Thumb)
            {
                return;
            }

            _host.BeginGroupDrag(group, args);
        };

        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions("40,*"),
        };
        var chrome = new Grid();
        chrome.Children.Add(layout);

        var titleText = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var header = new Button
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(12, 8),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Content = titleText,
        };
        AutomationProperties.SetName(header, $"{group.Title} group header");
        header.Click += (_, _) =>
        {
            var currentGroup = ResolveCurrentGroupSnapshot(group.Id);
            if (_host.ViewModel is null || currentGroup is null)
            {
                return;
            }

            var nodes = currentGroup.NodeIds
                .Select(_host.ViewModel.FindNode)
                .Where(node => node is not null)
                .Select(node => node!)
                .ToList();
            if (nodes.Count == 0)
            {
                return;
            }

            _host.FocusCanvas();
            _host.ViewModel.SetSelection(nodes, nodes.LastOrDefault(), status: null);
        };
        header.DoubleTapped += (_, args) =>
        {
            var currentGroup = ResolveCurrentGroupSnapshot(group.Id);
            if (_host.ViewModel is null || currentGroup is null)
            {
                return;
            }

            args.Handled = _host.ViewModel.TrySetNodeGroupCollapsed(group.Id, !currentGroup.IsCollapsed);
        };
        layout.Children.Add(header);

        var body = new Border
        {
            Margin = new Thickness(12, 0, 12, 12),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
        };
        Grid.SetRow(body, 1);
        layout.Children.Add(body);

        var leftThumb = CreateGroupResizeThumb(
            group.Id,
            $"{group.Title} group left resize handle",
            new Cursor(StandardCursorType.SizeWestEast),
            HorizontalAlignment.Left,
            VerticalAlignment.Stretch,
            width: 12d,
            height: double.NaN,
            vector => vector.X,
            (padding, delta) => padding with { Left = padding.Left - delta });
        var topThumb = CreateGroupResizeThumb(
            group.Id,
            $"{group.Title} group top resize handle",
            new Cursor(StandardCursorType.SizeNorthSouth),
            HorizontalAlignment.Stretch,
            VerticalAlignment.Top,
            width: double.NaN,
            height: 12d,
            vector => vector.Y,
            (padding, delta) => padding with { Top = padding.Top - delta });
        var rightThumb = CreateGroupResizeThumb(
            group.Id,
            $"{group.Title} group right resize handle",
            new Cursor(StandardCursorType.SizeWestEast),
            HorizontalAlignment.Right,
            VerticalAlignment.Stretch,
            width: 12d,
            height: double.NaN,
            vector => vector.X,
            (padding, delta) => padding with { Right = padding.Right + delta });
        var bottomThumb = CreateGroupResizeThumb(
            group.Id,
            $"{group.Title} group bottom resize handle",
            new Cursor(StandardCursorType.SizeNorthSouth),
            HorizontalAlignment.Stretch,
            VerticalAlignment.Bottom,
            width: double.NaN,
            height: 12d,
            vector => vector.Y,
            (padding, delta) => padding with { Bottom = padding.Bottom + delta });

        chrome.Children.Add(leftThumb);
        chrome.Children.Add(topThumb);
        chrome.Children.Add(rightThumb);
        chrome.Children.Add(bottomThumb);

        root.Child = chrome;
        return new NodeCanvasRenderedGroupVisual(root, header, body, titleText, leftThumb, topThumb, rightThumb, bottomThumb);
    }

    private void UpdateGroupVisual(GraphEditorNodeGroupSnapshot group)
    {
        if (_host.ViewModel is null || !_host.GroupVisuals.TryGetValue(group.Id, out var visual))
        {
            return;
        }

        var selectedNodeIds = _host.ViewModel.SelectedNodes
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);
        var isSelected = group.NodeIds.Count > 0 && group.NodeIds.All(selectedNodeIds.Contains);
        var isDropTarget = string.Equals(_host.InteractionSession.HoveredDropGroupId, group.Id, StringComparison.Ordinal);
        var headerHeight = 40d;
        var renderedHeight = group.IsCollapsed ? headerHeight : Math.Max(headerHeight + 16d, group.Size.Height);

        visual.Root.Width = Math.Max(168d, group.Size.Width);
        visual.Root.Height = renderedHeight;
        visual.Root.BorderBrush = BrushFactory.Solid(
            isDropTarget ? "#F8CF6A" : isSelected ? "#7FE7D7" : "#365063",
            isDropTarget ? 0.98 : isSelected ? 0.95 : 0.72);
        visual.Root.Background = BrushFactory.Solid(
            isDropTarget ? "#2C2412" : "#0E1824",
            isDropTarget ? 0.38 : group.IsCollapsed ? 0.72 : 0.18);
        Canvas.SetLeft(visual.Root, group.Position.X);
        Canvas.SetTop(visual.Root, group.Position.Y);

        visual.HeaderButton.Background = BrushFactory.Solid(
            isDropTarget ? "#4A3917" : isSelected ? "#173241" : "#132131",
            0.96);
        visual.TitleText.Text = $"{group.Title} ({group.NodeIds.Count})";
        visual.TitleText.Foreground = BrushFactory.Solid(isSelected ? "#F4FFFC" : "#D8EEF3", 0.98);
        visual.BodyBorder.IsVisible = !group.IsCollapsed;
        visual.BodyBorder.Height = Math.Max(0d, renderedHeight - headerHeight - 12d);
        visual.BodyBorder.BorderBrush = BrushFactory.Solid("#365063", 0.6);
        visual.BodyBorder.Background = BrushFactory.Solid("#122130", 0.32);
    }

    private Thumb CreateGroupResizeThumb(
        string groupId,
        string automationName,
        Cursor cursor,
        HorizontalAlignment horizontalAlignment,
        VerticalAlignment verticalAlignment,
        double width,
        double height,
        Func<Vector, double> deltaSelector,
        Func<GraphPadding, double, GraphPadding> updatePadding)
    {
        var thumb = new Thumb
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Width = width,
            Height = height,
            Cursor = cursor,
        };
        AutomationProperties.SetName(thumb, automationName);
        thumb.DragDelta += (_, args) =>
        {
            var editor = _host.ViewModel;
            var currentGroup = ResolveCurrentGroupSnapshot(groupId);
            if (editor is null || currentGroup is null)
            {
                return;
            }

            var nextPadding = updatePadding(currentGroup.ExtraPadding, deltaSelector(args.Vector)).ClampNonNegative();
            args.Handled = editor.TrySetNodeGroupExtraPadding(groupId, nextPadding, updateStatus: false);
        };

        return thumb;
    }

    private GraphEditorNodeGroupSnapshot? ResolveCurrentGroupSnapshot(string groupId)
        => _host.ViewModel?.GetNodeGroupSnapshots()
            .FirstOrDefault(group => string.Equals(group.Id, groupId, StringComparison.Ordinal));

    private GraphNodeVisualContext CreateNodeVisualContext(NodeViewModel node)
    {
        var editor = _host.ViewModel ?? throw new InvalidOperationException("Node visuals require a bound editor view model.");
        return new GraphNodeVisualContext(
            editor,
            node,
            editor.StyleOptions,
            _host.FocusCanvas,
            _host.BeginNodeDrag,
            (targetNode, width, updateStatus) => editor.TrySetNodeWidth(targetNode, width, updateStatus),
            (targetNode, expansionState) => editor.TrySetNodeExpansionState(targetNode, expansionState),
            (targetNode, port) => editor.HasIncomingConnection(targetNode, port),
            (targetNode, port) => editor.ResolveInlineParameter(targetNode, port),
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
