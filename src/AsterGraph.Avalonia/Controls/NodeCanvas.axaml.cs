using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 图编辑器画布控件，负责节点渲染、连线渲染和画布级交互。
/// </summary>
public partial class NodeCanvas : UserControl
{
    private const double SelectionDragThreshold = 6;
    /// <summary>
    /// 编辑器视图模型依赖属性。
    /// </summary>
    public static readonly StyledProperty<GraphEditorViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<NodeCanvas, GraphEditorViewModel?>(nameof(ViewModel));

    /// <summary>
    /// 控制是否启用默认内置上下文菜单。
    /// </summary>
    public static readonly StyledProperty<bool> EnableDefaultContextMenuProperty =
        AvaloniaProperty.Register<NodeCanvas, bool>(nameof(EnableDefaultContextMenu), true);

    /// <summary>
    /// 控制是否启用默认内置命令快捷键。
    /// </summary>
    public static readonly StyledProperty<bool> EnableDefaultCommandShortcutsProperty =
        AvaloniaProperty.Register<NodeCanvas, bool>(nameof(EnableDefaultCommandShortcuts), true);

    /// <summary>
    /// 控制是否启用默认滚轮缩放/平移手势。
    /// </summary>
    public static readonly StyledProperty<bool> EnableDefaultWheelViewportGesturesProperty =
        AvaloniaProperty.Register<NodeCanvas, bool>(nameof(EnableDefaultWheelViewportGestures), true);

    /// <summary>
    /// 控制是否启用 Alt+左键拖拽平移。
    /// </summary>
    public static readonly StyledProperty<bool> EnableAltLeftDragPanningProperty =
        AvaloniaProperty.Register<NodeCanvas, bool>(nameof(EnableAltLeftDragPanning), true);

    /// <summary>
    /// 控制节点可视树替换的展示器。
    /// </summary>
    public static readonly StyledProperty<IGraphNodeVisualPresenter?> NodeVisualPresenterProperty =
        AvaloniaProperty.Register<NodeCanvas, IGraphNodeVisualPresenter?>(nameof(NodeVisualPresenter));

    /// <summary>
    /// 控制上下文菜单展示层替换的展示器。
    /// </summary>
    public static readonly StyledProperty<IGraphContextMenuPresenter?> ContextMenuPresenterProperty =
        AvaloniaProperty.Register<NodeCanvas, IGraphContextMenuPresenter?>(nameof(ContextMenuPresenter));

    private readonly Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> _nodeVisuals = new();
    private Grid? _sceneRoot;
    private Canvas? _connectionLayer;
    private Canvas? _nodeLayer;
    private Canvas? _overlayLayer;
    private GridBackground? _backgroundGrid;
    private Border? _selectionAdorner;
    private Border? _verticalGuideAdorner;
    private Border? _horizontalGuideAdorner;
    private readonly GraphContextMenuPresenter _stockContextMenuPresenter = new();
    private readonly IGraphNodeVisualPresenter _stockNodeVisualPresenter = new DefaultGraphNodeVisualPresenter();
    private readonly NodeCanvasInteractionSession _interactionSession = new();
    private readonly NodeCanvasContextMenuCoordinator _contextMenuCoordinator;
    private readonly NodeCanvasSceneHost _sceneHost;
    private readonly NodeCanvasViewModelObserver _viewModelObserver;
    private readonly NodeCanvasOverlayCoordinator _overlayCoordinator;
    private readonly NodeCanvasWheelInteractionCoordinator _wheelInteractionCoordinator;
    private bool _isAttachedToVisualTree;

    /// <summary>
    /// 初始化节点画布。
    /// </summary>
    public NodeCanvas()
    {
        InitializeComponent();
        Focusable = true;
        _contextMenuCoordinator = new NodeCanvasContextMenuCoordinator(new NodeCanvasContextMenuHost(this), this);
        _sceneHost = new NodeCanvasSceneHost(new NodeCanvasSceneHostAdapter(this));
        _viewModelObserver = new NodeCanvasViewModelObserver(new NodeCanvasViewModelObserverHost(this));
        _overlayCoordinator = new NodeCanvasOverlayCoordinator(new NodeCanvasOverlayHost(this));
        _wheelInteractionCoordinator = new NodeCanvasWheelInteractionCoordinator(new NodeCanvasWheelInteractionHost(this));

        ContextRequested += HandleCanvasContextRequested;
        KeyDown += HandleCanvasKeyDown;
        PointerPressed += HandlePointerPressed;
        PointerMoved += HandlePointerMoved;
        PointerReleased += HandlePointerReleased;
        PointerWheelChanged += HandlePointerWheelChanged;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        ViewModel?.UpdateViewportSize(finalSize.Width, finalSize.Height);
        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// 当前绑定的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <summary>
    /// 是否启用默认内置上下文菜单。
    /// </summary>
    public bool EnableDefaultContextMenu
    {
        get => GetValue(EnableDefaultContextMenuProperty);
        set => SetValue(EnableDefaultContextMenuProperty, value);
    }

    /// <summary>
    /// 是否启用默认内置命令快捷键。
    /// </summary>
    public bool EnableDefaultCommandShortcuts
    {
        get => GetValue(EnableDefaultCommandShortcutsProperty);
        set => SetValue(EnableDefaultCommandShortcutsProperty, value);
    }

    /// <summary>
    /// 是否启用默认滚轮缩放/平移手势。
    /// </summary>
    public bool EnableDefaultWheelViewportGestures
    {
        get => GetValue(EnableDefaultWheelViewportGesturesProperty);
        set => SetValue(EnableDefaultWheelViewportGesturesProperty, value);
    }

    /// <summary>
    /// 是否启用 Alt+左键拖拽平移。
    /// </summary>
    public bool EnableAltLeftDragPanning
    {
        get => GetValue(EnableAltLeftDragPanningProperty);
        set => SetValue(EnableAltLeftDragPanningProperty, value);
    }

    internal bool AttachPlatformSeams { get; set; } = true;

    /// <summary>
    /// 当前节点可视树展示器。
    /// </summary>
    public IGraphNodeVisualPresenter? NodeVisualPresenter
    {
        get => GetValue(NodeVisualPresenterProperty);
        set => SetValue(NodeVisualPresenterProperty, value);
    }

    /// <summary>
    /// 当前上下文菜单展示器。
    /// </summary>
    public IGraphContextMenuPresenter? ContextMenuPresenter
    {
        get => GetValue(ContextMenuPresenterProperty);
        set => SetValue(ContextMenuPresenterProperty, value);
    }

    /// <summary>
    /// 将当前图内容缩放到可视区域。
    /// </summary>
    public void FitToScene(bool updateStatus = true)
        => ViewModel?.FitToViewport(Bounds.Width, Bounds.Height, updateStatus);

    /// <summary>
    /// 重置当前视口缩放与平移。
    /// </summary>
    public void ResetViewport()
        => ViewModel?.ResetView();

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewModelProperty)
        {
            var previous = change.GetOldValue<GraphEditorViewModel?>();
            var current = change.GetNewValue<GraphEditorViewModel?>();
            if (AttachPlatformSeams && _isAttachedToVisualTree)
            {
                GraphEditorPlatformSeamBinder.Replace(previous, current, this);
            }

            AttachViewModel(previous, current);
        }
        else if (change.Property == NodeVisualPresenterProperty)
        {
            RebuildScene();
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;

        if (AttachPlatformSeams)
        {
            GraphEditorPlatformSeamBinder.Apply(ViewModel, this);
        }
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (AttachPlatformSeams)
        {
            GraphEditorPlatformSeamBinder.Clear(ViewModel);
        }

        _isAttachedToVisualTree = false;
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _sceneRoot = this.FindControl<Grid>("SceneRoot");
        _connectionLayer = this.FindControl<Canvas>("ConnectionLayer");
        _nodeLayer = this.FindControl<Canvas>("NodeLayer");
        _overlayLayer = this.FindControl<Canvas>("OverlayLayer");
        _backgroundGrid = this.FindControl<GridBackground>("BackgroundGrid");
        _verticalGuideAdorner = this.FindControl<Border>("VerticalGuideAdorner");
        _horizontalGuideAdorner = this.FindControl<Border>("HorizontalGuideAdorner");
        _selectionAdorner = this.FindControl<Border>("SelectionAdorner");
    }

    private void AttachViewModel(GraphEditorViewModel? previous, GraphEditorViewModel? current)
        => _viewModelObserver.AttachViewModel(previous, current);

    private void RebuildScene()
        => _sceneHost.RebuildScene();

    private void ActivatePortFromVisual(NodeViewModel node, PortViewModel port)
    {
        Focus();
        ViewModel?.ActivatePort(node, port);
        RenderConnections();
    }

    private void UpdateViewportTransform()
        => _sceneHost.UpdateViewportTransform();

    private void RenderConnections()
        => _sceneHost.RenderConnections();

    private void BeginNodeDrag(NodeViewModel node, PointerPressedEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        var props = args.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed)
        {
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            Focus();
            ViewModel.ToggleNodeSelection(node);
            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            Focus();
            ViewModel.AddNodeToSelection(node);
            args.Handled = true;
            return;
        }

        Focus();
        if (node.IsSelected && ViewModel.HasMultipleSelection)
        {
            ViewModel.SetSelection(ViewModel.SelectedNodes.ToList(), node);
        }
        else
        {
            ViewModel.SelectSingleNode(node);
        }

        if (_nodeLayer is not null && _nodeVisuals.TryGetValue(node, out var visual))
        {
            _nodeLayer.Children.Remove(visual.Root);
            _nodeLayer.Children.Add(visual.Root);
        }

        HideSelectionAdorner();
        HideGuideAdorners();
        var dragStart = args.GetPosition(this);
        var dragNodes = node.IsSelected && ViewModel.HasMultipleSelection
            ? ViewModel.SelectedNodes.ToList()
            : [node];
        _interactionSession.BeginNodeDrag(node, dragStart, CreateDragSession(dragNodes));
        ViewModel.BeginHistoryInteraction();
        args.Pointer.Capture(this);
        args.Handled = true;
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (ViewModel is null || args.Handled)
        {
            return;
        }

        Focus();

        var props = args.GetCurrentPoint(this).Properties;
        var current = args.GetPosition(this);
        _interactionSession.UpdateLastPointerPosition(current);
        _interactionSession.UpdatePointerPosition(current);

        // 如果按下鼠标中键，或者按住 Alt 键的同时点击鼠标左键，触发平移交互。
        // 此改动允许触控板用户通过按住键盘 Alt 键 + 单指拖拽的方式平移视图。
        if (props.IsMiddleButtonPressed
            || (EnableAltLeftDragPanning && props.IsLeftButtonPressed && args.KeyModifiers.HasFlag(KeyModifiers.Alt)))
        {
            _interactionSession.BeginPanning(current);
            HideSelectionAdorner();
            HideGuideAdorners();
            args.Pointer.Capture(this);
            args.Handled = true;
            return;
        }

        if (props.IsLeftButtonPressed)
        {
            if (ViewModel.HasPendingConnection)
            {
                ViewModel.CancelPendingConnection("Connection preview cancelled.");
                RenderConnections();
            }

            _interactionSession.BeginCanvasSelection(current, args.KeyModifiers, ViewModel.SelectedNodes.ToList());
            HideSelectionAdorner();
            HideGuideAdorners();
            args.Pointer.Capture(this);
            args.Handled = true;
        }
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (ViewModel is null)
        {
            return;
        }

        var current = args.GetPosition(this);
        _interactionSession.UpdatePointerPosition(current);

        if (_interactionSession.SelectionStartScreenPosition is not null
            && !_interactionSession.IsPanning
            && _interactionSession.DragNode is null)
        {
            if (_interactionSession.TryBeginMarqueeSelection(current, SelectionDragThreshold))
            {
                UpdateMarqueeSelection(current, finalize: false);
                args.Handled = true;
                return;
            }
        }

        if (_interactionSession.IsPanning || _interactionSession.DragNode is not null)
        {
            if (_interactionSession.DragNode is not null)
            {
                if (_interactionSession.DragSession is not null && _interactionSession.DragStartScreenPosition is not null)
                {
                    var rawDelta = current - _interactionSession.DragStartScreenPosition.Value;
                    var adjustedDelta = ApplyDragAssist(
                        _interactionSession.DragSession.Value,
                        rawDelta.X / ViewModel.Zoom,
                        rawDelta.Y / ViewModel.Zoom);
                    ViewModel.ApplyDragOffset(_interactionSession.DragSession.Value.OriginPositions, adjustedDelta.X, adjustedDelta.Y);
                }
            }
            else if (_interactionSession.IsPanning)
            {
                var delta = current - _interactionSession.LastPointerPosition;
                _interactionSession.UpdateLastPointerPosition(current);
                ViewModel.PanBy(delta.X, delta.Y);
            }

            args.Handled = true;
        }

        if (ViewModel.HasPendingConnection)
        {
            RenderConnections();
        }
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        if (_interactionSession.SelectionStartScreenPosition is not null)
        {
            if (_interactionSession.IsMarqueeSelecting)
            {
                UpdateMarqueeSelection(args.GetPosition(this), finalize: true);
            }
            else
            {
                ViewModel?.ClearSelection();
            }

            HideSelectionAdorner();
        }

        if (_interactionSession.DragNode is not null)
        {
            ViewModel?.CompleteHistoryInteraction(
                ViewModel.HasMultipleSelection
                    ? "Moved selection."
                    : $"Moved {_interactionSession.DragNode.Title}.");
        }

        _interactionSession.ResetAfterPointerRelease();
        HideGuideAdorners();
        args.Pointer.Capture(null);
    }

    private void HandlePointerWheelChanged(object? sender, PointerWheelEventArgs args)
    {
        if (_wheelInteractionCoordinator.HandleWheel(args.GetPosition(this), args.Delta, args.KeyModifiers))
        {
            args.Handled = true;
        }
    }

    private void HandleCanvasKeyDown(object? sender, KeyEventArgs args)
    {
        if (!EnableDefaultCommandShortcuts)
        {
            return;
        }

        if (GraphEditorDefaultCommandShortcutRouter.TryHandle(
            ViewModel,
            args.Source,
            args,
            includePendingConnectionCancel: true))
        {
            args.Handled = true;
        }
    }

    private void HandleCanvasContextRequested(object? sender, ContextRequestedEventArgs args)
        => args.Handled = _contextMenuCoordinator.HandleCanvasContextRequested(this, args);

    private void UpdateSelectionState()
        => _sceneHost.UpdateSelectionState();

    private void UpdateNodePosition(NodeViewModel node)
        => _sceneHost.UpdateNodePosition(node);

    private void UpdateNodeVisual(NodeViewModel node)
        => _sceneHost.UpdateNodeVisual(node);

    private GraphPoint GetPortAnchor(NodeViewModel node, PortViewModel port)
        => _sceneHost.GetPortAnchor(node, port);

    private GraphPoint ResolveWorldPosition(ContextRequestedEventArgs args, Control relativeTo)
    {
        if (ViewModel is not null && args.TryGetPosition(relativeTo, out var point))
        {
            return ViewModel.ScreenToWorld(new GraphPoint(point.X, point.Y));
        }

        return ViewModel is null ? new GraphPoint(0, 0) : ViewModel.ScreenToWorld(new GraphPoint(0, 0));
    }

    private NodeCanvasContextMenuSnapshot CreateContextMenuSnapshot()
        => ViewModel is null
            ? new NodeCanvasContextMenuSnapshot(null, [], [])
            : new NodeCanvasContextMenuSnapshot(
                ViewModel.SelectedNode?.Id,
                ViewModel.SelectedNodes.Select(selected => selected.Id).ToList(),
                ViewModel.NodeTemplates.Select(template => template.Definition).ToList());

    private void ApplySelectionAdornerStyle()
        => _overlayCoordinator.ApplySelectionAdornerStyle();

    private void ApplyGuideAdornerStyle()
        => _overlayCoordinator.ApplyGuideAdornerStyle();

    private void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize)
        => _overlayCoordinator.UpdateMarqueeSelection(currentScreenPosition, finalize);

    private void HideSelectionAdorner()
        => _overlayCoordinator.HideSelectionAdorner();

    private GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY)
        => _overlayCoordinator.ApplyDragAssist(dragSession, deltaX, deltaY);

    private NodeCanvasDragSession CreateDragSession(IReadOnlyList<NodeViewModel> nodes)
        => _overlayCoordinator.CreateDragSession(nodes);

    private void ShowGuideAdorners(double? worldX, double? worldY)
        => _overlayCoordinator.ShowGuideAdorners(worldX, worldY);

    private void HideGuideAdorners()
        => _overlayCoordinator.HideGuideAdorners();

    private GraphPoint WorldToScreen(double x, double y)
        => ViewportMath.WorldToScreen(
            new ViewportState(ViewModel?.Zoom ?? 1, ViewModel?.PanX ?? 0, ViewModel?.PanY ?? 0),
            new GraphPoint(x, y));

}
