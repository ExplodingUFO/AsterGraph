using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
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

    /// <summary>
    /// Controls the registry used by shipped inline parameter-editor hosts.
    /// </summary>
    public static readonly StyledProperty<INodeParameterEditorRegistry?> NodeParameterEditorRegistryProperty =
        AvaloniaProperty.Register<NodeCanvas, INodeParameterEditorRegistry?>(nameof(NodeParameterEditorRegistry));

    /// <summary>
    /// Controls stock resize-hover feedback resolution for node and group surfaces.
    /// </summary>
    public static readonly StyledProperty<IGraphResizeFeedbackPolicy?> ResizeFeedbackPolicyProperty =
        AvaloniaProperty.Register<NodeCanvas, IGraphResizeFeedbackPolicy?>(nameof(ResizeFeedbackPolicy));

    private readonly Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> _nodeVisuals = new();
    private readonly Dictionary<string, NodeCanvasRenderedGroupVisual> _groupVisuals = new(StringComparer.Ordinal);
    private readonly Dictionary<Control, NodeViewModel> _resizeFeedbackNodesBySurface = new();
    private readonly Dictionary<Border, string> _resizeFeedbackGroupsBySurface = new();
    private readonly Dictionary<string, GraphEditorNodeGroupSnapshot> _resizeFeedbackGroupSnapshots = new(StringComparer.Ordinal);
    private Grid? _sceneRoot;
    private Canvas? _groupLayer;
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
    private readonly NodeCanvasLifecycleCoordinator _lifecycleCoordinator;
    private readonly NodeCanvasOverlayCoordinator _overlayCoordinator;
    private readonly NodeCanvasNodeDragCoordinator _nodeDragCoordinator;
    private readonly NodeCanvasPointerInteractionCoordinator _pointerInteractionCoordinator;
    private readonly NodeCanvasResizeFeedbackCoordinator _resizeFeedbackCoordinator;
    private readonly NodeCanvasWheelInteractionCoordinator _wheelInteractionCoordinator;
    private bool _isAttachedToVisualTree;

    /// <summary>
    /// 初始化节点画布。
    /// </summary>
    public NodeCanvas()
    {
        _lifecycleCoordinator = new NodeCanvasLifecycleCoordinator(new NodeCanvasLifecycleHost(this));
        InitializeComponent();
        Focusable = true;
        _contextMenuCoordinator = new NodeCanvasContextMenuCoordinator(new NodeCanvasContextMenuHost(this), this);
        _sceneHost = new NodeCanvasSceneHost(new NodeCanvasSceneHostAdapter(this));
        _viewModelObserver = new NodeCanvasViewModelObserver(new NodeCanvasViewModelObserverHost(this));
        _overlayCoordinator = new NodeCanvasOverlayCoordinator(new NodeCanvasOverlayHost(this));
        _nodeDragCoordinator = new NodeCanvasNodeDragCoordinator(new NodeCanvasNodeDragHost(this));
        _pointerInteractionCoordinator = new NodeCanvasPointerInteractionCoordinator(new NodeCanvasPointerInteractionHost(this));
        _resizeFeedbackCoordinator = new NodeCanvasResizeFeedbackCoordinator(new NodeCanvasResizeFeedbackHost(this));
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
    /// Registry used by shipped node-side parameter editors.
    /// </summary>
    public INodeParameterEditorRegistry? NodeParameterEditorRegistry
    {
        get => GetValue(NodeParameterEditorRegistryProperty);
        set => SetValue(NodeParameterEditorRegistryProperty, value);
    }

    /// <summary>
    /// Host-owned policy used to override or extend stock resize-hover feedback.
    /// </summary>
    public IGraphResizeFeedbackPolicy? ResizeFeedbackPolicy
    {
        get => GetValue(ResizeFeedbackPolicyProperty);
        set => SetValue(ResizeFeedbackPolicyProperty, value);
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
        _lifecycleCoordinator.HandlePropertyChanged(change);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _lifecycleCoordinator.HandleAttachedToVisualTree();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClearResizeFeedback();
        _interactionSession.ResetAfterPointerRelease();
        _lifecycleCoordinator.HandleDetachedFromVisualTree();
        base.OnDetachedFromVisualTree(e);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _sceneRoot = this.FindControl<Grid>("SceneRoot");
        _groupLayer = this.FindControl<Canvas>("GroupLayer");
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
    {
        ClearResizeFeedback();
        _sceneHost.RebuildScene();
        RefreshResizeFeedbackNodeSurfaceIndex();
        RefreshResizeFeedbackGroupSurfaceIndex();
    }

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
        var props = args.GetCurrentPoint(this).Properties;
        var result = _nodeDragCoordinator.BeginNodeDrag(
            node,
            args.GetPosition(this),
            props.IsLeftButtonPressed,
            args.KeyModifiers);

        if (!result.Handled)
        {
            return;
        }

        if (result.CapturePointer)
        {
            args.Pointer.Capture(this);
        }

        args.Handled = true;
    }

    private void BeginNodeResize(NodeViewModel node, GraphNodeResizeHandleKind handleKind, PointerPressedEventArgs args)
    {
        if (ViewModel is null || !args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        Focus();
        _resizeFeedbackCoordinator.BeginResizeSession(new GraphResizeFeedbackContext(
            GraphResizeFeedbackSurfaceKind.Node,
            handleKind switch
            {
                GraphNodeResizeHandleKind.Right => GraphResizeFeedbackHandle.RightEdge,
                GraphNodeResizeHandleKind.Bottom => GraphResizeFeedbackHandle.BottomEdge,
                _ => GraphResizeFeedbackHandle.BottomRightCorner,
            }));
        ViewModel.SelectSingleNode(node, updateStatus: false);
        _interactionSession.BeginNodeResize(
            ViewModel.FindNode(node.Id) ?? node,
            handleKind,
            args.GetPosition(this));
        ViewModel.BeginHistoryInteraction();
        args.Pointer.Capture(this);
        args.Handled = true;
    }

    private void BeginGroupDrag(GraphEditorNodeGroupSnapshot group, PointerPressedEventArgs args)
    {
        var props = args.GetCurrentPoint(this).Properties;
        var result = _nodeDragCoordinator.BeginGroupDrag(
            group,
            args.GetPosition(this),
            props.IsLeftButtonPressed,
            args.KeyModifiers);

        if (!result.Handled)
        {
            return;
        }

        if (result.CapturePointer)
        {
            args.Pointer.Capture(this);
        }

        args.Handled = true;
    }

    private void BeginGroupResize(string groupId, string groupTitle, NodeCanvasGroupResizeEdge edge, PointerPressedEventArgs args)
    {
        if (ViewModel is null || !args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var group = ViewModel.GetNodeGroupSnapshots()
            .FirstOrDefault(candidate => string.Equals(candidate.Id, groupId, StringComparison.Ordinal));
        if (group is null)
        {
            return;
        }

        Focus();
        _resizeFeedbackCoordinator.BeginResizeSession(new GraphResizeFeedbackContext(
            GraphResizeFeedbackSurfaceKind.Group,
            edge switch
            {
                NodeCanvasGroupResizeEdge.Left => GraphResizeFeedbackHandle.LeftEdge,
                NodeCanvasGroupResizeEdge.Top => GraphResizeFeedbackHandle.TopEdge,
                NodeCanvasGroupResizeEdge.Right => GraphResizeFeedbackHandle.RightEdge,
                _ => GraphResizeFeedbackHandle.BottomEdge,
            }));
        _interactionSession.BeginGroupResize(group.Id, groupTitle, edge, group.Position, group.Size, args.GetPosition(this));
        ViewModel.BeginHistoryInteraction();
        args.Pointer.Capture(this);
        args.Handled = true;
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs args)
    {
        if (args.Source is Thumb)
        {
            return;
        }

        var props = args.GetCurrentPoint(this).Properties;
        var result = _pointerInteractionCoordinator.HandlePressed(
            args.Handled,
            args.GetPosition(this),
            props.IsLeftButtonPressed,
            props.IsMiddleButtonPressed,
            args.KeyModifiers);

        if (!result.Handled)
        {
            return;
        }

        if (result.CapturePointer)
        {
            args.Pointer.Capture(this);
        }

        args.Handled = true;
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs args)
    {
        if (_pointerInteractionCoordinator.HandleMoved(args.GetPosition(this), SelectionDragThreshold))
        {
            args.Handled = true;
        }
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        _pointerInteractionCoordinator.HandleReleased(args.GetPosition(this));
        args.Pointer.Capture(null);
    }

    private void UpdateResizeFeedback(Point currentScreenPosition)
        => _resizeFeedbackCoordinator.Update(currentScreenPosition);

    private void ClearResizeFeedback()
        => _resizeFeedbackCoordinator.Clear();

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

    private void UpdateGroupVisuals()
    {
        _sceneHost.UpdateGroupVisuals();
        RefreshResizeFeedbackGroupSurfaceIndex();
    }

    private void UpdateNodePosition(NodeViewModel node)
        => _sceneHost.UpdateNodePosition(node);

    private void UpdateNodeVisual(NodeViewModel node)
    {
        _sceneHost.UpdateNodeVisual(node);
        RefreshResizeFeedbackNodeSurfaceIndex();
    }

    private void RefreshResizeFeedbackNodeSurfaceIndex()
    {
        _resizeFeedbackNodesBySurface.Clear();
        foreach (var entry in _nodeVisuals)
        {
            _resizeFeedbackNodesBySurface[entry.Value.Root] = entry.Key;
        }
    }

    private void RefreshResizeFeedbackGroupSurfaceIndex()
    {
        _resizeFeedbackGroupsBySurface.Clear();
        foreach (var entry in _groupVisuals)
        {
            _resizeFeedbackGroupsBySurface[entry.Value.Root] = entry.Key;
        }

        _resizeFeedbackGroupSnapshots.Clear();
        foreach (var snapshot in ViewModel?.GetNodeGroupSnapshots() ?? [])
        {
            _resizeFeedbackGroupSnapshots[snapshot.Id] = snapshot;
        }
    }

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
