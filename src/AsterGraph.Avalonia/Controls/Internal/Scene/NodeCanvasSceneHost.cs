using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Automation;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

    INodeParameterEditorRegistry? NodeParameterEditorRegistry { get; }

    NodeCanvasInteractionSession InteractionSession { get; }

    NodeCanvasContextMenuCoordinator ContextMenuCoordinator { get; }

    void FocusCanvas();

    void BeginNodeDrag(NodeViewModel node, PointerPressedEventArgs args);

    void BeginNodeResize(NodeViewModel node, GraphNodeResizeHandleKind handleKind, PointerPressedEventArgs args);

    void BeginGroupDrag(GraphEditorNodeGroupSnapshot group, PointerPressedEventArgs args);

    void BeginGroupResize(string groupId, string groupTitle, NodeCanvasGroupResizeEdge edge, PointerPressedEventArgs args);

    void ActivatePort(NodeViewModel node, PortViewModel port);
}

internal sealed record NodeCanvasRenderedGroupVisual(
    Border Root,
    Border HeaderControl,
    Border BodyBorder,
    TextBlock TitleText,
    Thumb LeftResizeThumb,
    Thumb TopResizeThumb,
    Thumb RightResizeThumb,
    Thumb BottomResizeThumb);

internal enum NodeCanvasGroupResizeEdge
{
    Left,
    Top,
    Right,
    Bottom,
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
            ClipToBounds = true,
        };
        AutomationProperties.SetName(root, $"{group.Title} group");
        root.PointerPressed += (_, args) =>
        {
            if (TryBeginGroupResize(root, group, args))
            {
                return;
            }
        };

        var layout = new Grid
        {
            RowDefinitions = new RowDefinitions($"{NodeCanvasGroupChromeMetrics.HeaderHeight},*"),
        };
        var chrome = new Grid();
        chrome.Children.Add(layout);

        var titleText = new TextBlock
        {
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var header = new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(
                NodeCanvasGroupChromeMetrics.HeaderHorizontalPadding,
                NodeCanvasGroupChromeMetrics.HeaderVerticalPadding),
            CornerRadius = new CornerRadius(16, 16, 0, 0),
            Child = titleText,
        };
        AutomationProperties.SetName(header, $"{group.Title} group header");
        header.PointerPressed += (_, args) =>
        {
            if (TryBeginGroupResize(root, group, args))
            {
                return;
            }

            if (args.Source is Thumb)
            {
                return;
            }

            _host.BeginGroupDrag(group, args);
        };
        header.Tapped += (_, _) =>
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

            args.Handled = _host.ViewModel.Session.Commands.TrySetNodeGroupCollapsed(group.Id, !currentGroup.IsCollapsed);
        };
        layout.Children.Add(header);

        var body = new Border
        {
            Margin = new Thickness(
                NodeCanvasGroupChromeMetrics.HeaderHorizontalPadding,
                0,
                NodeCanvasGroupChromeMetrics.HeaderHorizontalPadding,
                NodeCanvasGroupChromeMetrics.BottomInset),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
        };
        Grid.SetRow(body, 1);
        layout.Children.Add(body);

        var leftThumb = CreateGroupResizeThumb(
            group.Id,
            group.Title,
            NodeCanvasGroupResizeEdge.Left,
            $"{group.Title} group left resize handle",
            HorizontalAlignment.Left,
            VerticalAlignment.Stretch,
            width: NodeCanvasGroupChromeMetrics.ResizeHandleThickness,
            height: double.NaN);
        var topThumb = CreateGroupResizeThumb(
            group.Id,
            group.Title,
            NodeCanvasGroupResizeEdge.Top,
            $"{group.Title} group top resize handle",
            HorizontalAlignment.Stretch,
            VerticalAlignment.Top,
            width: double.NaN,
            height: NodeCanvasGroupChromeMetrics.ResizeHandleThickness);
        var rightThumb = CreateGroupResizeThumb(
            group.Id,
            group.Title,
            NodeCanvasGroupResizeEdge.Right,
            $"{group.Title} group right resize handle",
            HorizontalAlignment.Right,
            VerticalAlignment.Stretch,
            width: NodeCanvasGroupChromeMetrics.ResizeHandleThickness,
            height: double.NaN);
        var bottomThumb = CreateGroupResizeThumb(
            group.Id,
            group.Title,
            NodeCanvasGroupResizeEdge.Bottom,
            $"{group.Title} group bottom resize handle",
            HorizontalAlignment.Stretch,
            VerticalAlignment.Bottom,
            width: double.NaN,
            height: NodeCanvasGroupChromeMetrics.ResizeHandleThickness);

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

        var renderedGroup = ResolveRenderedGroupSnapshot(group);
        var selectedNodeIds = _host.ViewModel.SelectedNodes
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);
        var isSelected = group.NodeIds.Count > 0 && group.NodeIds.All(selectedNodeIds.Contains);
        var isDropTarget = string.Equals(_host.InteractionSession.HoveredDropGroupId, group.Id, StringComparison.Ordinal);
        var renderedHeight = NodeCanvasGroupChromeMetrics.ResolveRenderedHeight(renderedGroup);

        visual.Root.Width = NodeCanvasGroupChromeMetrics.ResolveRenderedWidth(renderedGroup);
        visual.Root.Height = renderedHeight;
        visual.Root.BorderBrush = BrushFactory.Solid(
            isDropTarget ? "#F8CF6A" : isSelected ? "#7FE7D7" : "#365063",
            isDropTarget ? 0.98 : isSelected ? 0.95 : 0.72);
        visual.Root.Background = BrushFactory.Solid(
            isDropTarget ? "#2C2412" : "#0E1824",
            isDropTarget ? 0.38 : renderedGroup.IsCollapsed ? 0.72 : 0.18);
        var renderedPosition = renderedGroup.Position;
        Canvas.SetLeft(visual.Root, renderedPosition.X);
        Canvas.SetTop(visual.Root, renderedPosition.Y);

        visual.HeaderControl.Background = BrushFactory.Solid(
            isDropTarget ? "#4A3917" : isSelected ? "#173241" : "#132131",
            0.96);
        visual.TitleText.Text = FormatGroupHeaderTitle(group);
        visual.TitleText.Foreground = BrushFactory.Solid(isSelected ? "#F4FFFC" : "#D8EEF3", 0.98);
        visual.BodyBorder.IsVisible = !renderedGroup.IsCollapsed;
        visual.BodyBorder.Height = Math.Max(
            0d,
            renderedHeight - NodeCanvasGroupChromeMetrics.HeaderHeight - NodeCanvasGroupChromeMetrics.BottomInset);
        visual.BodyBorder.BorderBrush = BrushFactory.Solid("#365063", 0.6);
        visual.BodyBorder.Background = BrushFactory.Solid("#122130", 0.32);
    }

    private Thumb CreateGroupResizeThumb(
        string groupId,
        string groupTitle,
        NodeCanvasGroupResizeEdge edge,
        string automationName,
        HorizontalAlignment horizontalAlignment,
        VerticalAlignment verticalAlignment,
        double width,
        double height)
    {
        var thumb = new Thumb
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            Width = width,
            Height = height,
        };
        AutomationProperties.SetName(thumb, automationName);
        thumb.AddHandler(
            InputElement.PointerPressedEvent,
            (_, args) =>
            {
                if (_host.ViewModel is null)
                {
                    return;
                }

                _host.BeginGroupResize(groupId, groupTitle, edge, args);
            },
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);

        return thumb;
    }

    private bool TryBeginGroupResize(
        Border surface,
        GraphEditorNodeGroupSnapshot group,
        PointerPressedEventArgs args)
    {
        if (args.Source is Thumb)
        {
            return false;
        }

        var point = ResolveSurfacePoint(surface, group, args);
        if (!NodeCanvasResizeFeedbackResolver.TryResolveGroup(surface, point, out var resizeHit)
            || resizeHit.GroupEdge is not NodeCanvasGroupResizeEdge edge)
        {
            return false;
        }

        _host.BeginGroupResize(group.Id, group.Title, edge, args);
        return args.Handled;
    }

    private static string FormatGroupHeaderTitle(GraphEditorNodeGroupSnapshot group)
        => group.Title;

    private static Point ResolveSurfacePoint(
        Border surface,
        GraphEditorNodeGroupSnapshot group,
        PointerPressedEventArgs args)
    {
        if (surface.Parent is Visual parent)
        {
            var parentPoint = args.GetPosition(parent);
            return new Point(
                parentPoint.X - group.Position.X,
                parentPoint.Y - group.Position.Y);
        }

        return args.GetPosition(surface);
    }

    private GraphEditorNodeGroupSnapshot? ResolveCurrentGroupSnapshot(string groupId)
        => _host.ViewModel?.GetNodeGroupSnapshots()
            .FirstOrDefault(group => string.Equals(group.Id, groupId, StringComparison.Ordinal));

    private GraphSize? ResolveNodePreviewSize(NodeViewModel node)
        => _host.InteractionSession.NodeResizePreview is NodeCanvasNodeResizePreview preview
           && string.Equals(preview.NodeId, node.Id, StringComparison.Ordinal)
            ? preview.Size
            : null;

    private GraphEditorNodeGroupSnapshot ResolveRenderedGroupSnapshot(GraphEditorNodeGroupSnapshot group)
    {
        if (_host.InteractionSession.GroupResizePreview is NodeCanvasGroupResizePreview resizePreview
            && string.Equals(resizePreview.GroupId, group.Id, StringComparison.Ordinal))
        {
            return group with
            {
                Position = resizePreview.Position,
                Size = resizePreview.Size,
            };
        }

        if (string.Equals(_host.InteractionSession.DragGroupId, group.Id, StringComparison.Ordinal)
            && _host.InteractionSession.DragGroupPreviewPosition is GraphPoint dragPreviewPosition)
        {
            return group with
            {
                Position = dragPreviewPosition,
            };
        }

        return group;
    }

    private GraphNodeVisualContext CreateNodeVisualContext(NodeViewModel node)
    {
        var editor = _host.ViewModel ?? throw new InvalidOperationException("Node visuals require a bound editor view model.");
        return new GraphNodeVisualContext(
            editor,
            node,
            editor.StyleOptions,
            _host.NodeParameterEditorRegistry,
            _host.FocusCanvas,
            _host.BeginNodeDrag,
            _host.BeginNodeResize,
            editor.BeginHistoryInteraction,
            editor.CompleteHistoryInteraction,
            (targetNode, size, updateStatus) => editor.TrySetNodeSize(targetNode, size, updateStatus),
            (targetNode, width, updateStatus) => editor.TrySetNodeWidth(targetNode, width, updateStatus),
            (targetNode, expansionState) => editor.Session.Commands.TrySetNodeExpansionState(targetNode.Id, expansionState),
            (targetNode, port) => editor.HasIncomingConnection(targetNode, port),
            _host.ActivatePort,
            (targetNode, target) => editor.ActivateConnectionTarget(targetNode, target),
            _host.ContextMenuCoordinator.OpenNodeContextMenu,
            _host.ContextMenuCoordinator.OpenPortContextMenu,
            surfacePreviewSize: ResolveNodePreviewSize(node));
    }

    private NodeCanvasConnectionSceneContext CreateConnectionSceneContext()
        => new(
            _host.ViewModel,
            _host.ConnectionLayer,
            _host.NodeLayer,
            _host.CoordinateRoot,
            _host.NodeVisuals,
            _host.ViewModel?.Session.Queries.GetConnectionGeometrySnapshots()
                .ToDictionary(snapshot => snapshot.ConnectionId, StringComparer.Ordinal)
                ?? new Dictionary<string, GraphEditorConnectionGeometrySnapshot>(StringComparer.Ordinal),
            _host.InteractionSession.PointerScreenPosition,
            GetConnectionStyle,
            _host.ContextMenuCoordinator.CreateContextMenuSnapshot,
            _host.ContextMenuCoordinator.ResolveWorldPosition,
            _host.ContextMenuCoordinator.OpenContextMenu,
            ResolveNodePreviewSize);

    private ConnectionStyleOptions GetConnectionStyle(ConnectionViewModel connection)
        => connection.ConversionId is not null
            ? _host.ViewModel?.StyleOptions.ConnectionOverrides.FirstOrDefault(overrideStyle => overrideStyle.ConversionId == connection.ConversionId)?.Style
              ?? _host.ViewModel?.StyleOptions.Connection
              ?? GraphEditorStyleOptions.Default.Connection
            : _host.ViewModel?.StyleOptions.Connection
              ?? GraphEditorStyleOptions.Default.Connection;

}
