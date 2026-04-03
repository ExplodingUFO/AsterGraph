using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Avalonia.Styling;
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

    private readonly Dictionary<NodeViewModel, RenderedNodeVisual> _nodeVisuals = new();
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

    /// <summary>
    /// 初始化节点画布。
    /// </summary>
    public NodeCanvas()
    {
        InitializeComponent();
        Focusable = true;

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
            AttachViewModel(change.GetOldValue<GraphEditorViewModel?>(), change.GetNewValue<GraphEditorViewModel?>());
        }
        else if (change.Property == NodeVisualPresenterProperty)
        {
            RebuildScene();
        }
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
    {
        if (previous is not null)
        {
            previous.PropertyChanged -= HandleViewModelPropertyChanged;
            previous.Nodes.CollectionChanged -= HandleNodesCollectionChanged;
            previous.Connections.CollectionChanged -= HandleConnectionsCollectionChanged;

            foreach (var node in previous.Nodes)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
            }
        }

        if (current is not null)
        {
            current.PropertyChanged += HandleViewModelPropertyChanged;
            current.Nodes.CollectionChanged += HandleNodesCollectionChanged;
            current.Connections.CollectionChanged += HandleConnectionsCollectionChanged;

            foreach (var node in current.Nodes)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
            }
        }

        ApplySelectionAdornerStyle();
        ApplyGuideAdornerStyle();
        RebuildScene();
    }

    private void RebuildScene()
    {
        if (_nodeLayer is null || _connectionLayer is null)
        {
            return;
        }

        _nodeLayer.Children.Clear();
        _connectionLayer.Children.Clear();
        _nodeVisuals.Clear();

        if (ViewModel is null)
        {
            return;
        }

        foreach (var node in ViewModel.Nodes)
        {
            var visual = CreateNodeVisual(node);
            _nodeVisuals[node] = visual;
            _nodeLayer.Children.Add(visual.Root);
            UpdateNodeVisual(node);
        }

        UpdateViewportTransform();
        RenderConnections();
        Dispatcher.UIThread.Post(RenderConnections, DispatcherPriority.Loaded);
    }

    private RenderedNodeVisual CreateNodeVisual(NodeViewModel node)
    {
        var presenter = NodeVisualPresenter ?? _stockNodeVisualPresenter;
        var visual = presenter.Create(CreateNodeVisualContext(node));
        return new RenderedNodeVisual(visual.Root, presenter, visual);
    }

    private GraphNodeVisualContext CreateNodeVisualContext(NodeViewModel node)
    {
        var editor = ViewModel ?? throw new InvalidOperationException("Node visuals require a bound editor view model.");
        return new GraphNodeVisualContext(
            editor,
            node,
            editor.StyleOptions,
            () => Focus(),
            BeginNodeDrag,
            ActivatePortFromVisual,
            OpenNodeContextMenu,
            OpenPortContextMenu);
    }

    private void ActivatePortFromVisual(NodeViewModel node, PortViewModel port)
    {
        Focus();
        ViewModel?.ActivatePort(node, port);
        RenderConnections();
    }

    private bool OpenNodeContextMenu(Control target, NodeViewModel node, ContextRequestedEventArgs args)
    {
        if (ViewModel is null)
        {
            return false;
        }

        Focus();
        var targetKind = ContextMenuTargetKind.Node;
        if (ViewModel.HasMultipleSelection && node.IsSelected)
        {
            ViewModel.SetSelection(ViewModel.SelectedNodes.ToList(), node);
            targetKind = ContextMenuTargetKind.Selection;
        }
        else
        {
            ViewModel.SelectSingleNode(node);
        }

        return OpenContextMenu(
            target,
            NodeCanvasContextMenuContextFactory.CreateNodeContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, this),
                node.Id,
                useSelectionTools: targetKind == ContextMenuTargetKind.Selection,
                hostContext: ViewModel.HostContext));
    }

    private bool OpenPortContextMenu(Control target, NodeViewModel node, PortViewModel port, ContextRequestedEventArgs args)
    {
        if (ViewModel is null)
        {
            return false;
        }

        Focus();
        ViewModel.SelectNode(node);
        return OpenContextMenu(
            target,
            NodeCanvasContextMenuContextFactory.CreatePortContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, this),
                node.Id,
                port.Id,
                hostContext: ViewModel.HostContext));
    }

    private StackPanel BuildPortPanel(
        NodeViewModel node,
        IEnumerable<PortViewModel> ports,
        bool isInput,
        Dictionary<string, Border> portAnchors)
    {
        var portStyle = GetPortStyle(ports.FirstOrDefault());
        var panel = new StackPanel
        {
            Spacing = portStyle.RowSpacing,
            HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
        };

        foreach (var port in ports)
        {
            var row = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = portStyle.RowSpacing,
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            };

            var dot = new Border
            {
                Width = portStyle.DotSize,
                Height = portStyle.DotSize,
                CornerRadius = new CornerRadius(999),
                Background = BrushFactory.Solid(port.AccentHex),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var text = new StackPanel
            {
                Spacing = portStyle.TextSpacing,
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            };
            text.Children.Add(new TextBlock
            {
                Text = port.Label,
                FontSize = portStyle.LabelFontSize,
                FontWeight = FontWeight.Medium,
                Foreground = BrushFactory.Solid(portStyle.LabelHex),
                TextAlignment = isInput ? TextAlignment.Left : TextAlignment.Right,
            });
            text.Children.Add(new TextBlock
            {
                Text = port.DataType,
                FontSize = portStyle.TypeFontSize,
                Foreground = BrushFactory.Solid(portStyle.TypeHex, portStyle.TypeOpacity),
                TextAlignment = isInput ? TextAlignment.Left : TextAlignment.Right,
            });

            if (isInput)
            {
                row.Children.Add(dot);
                row.Children.Add(text);
            }
            else
            {
                row.Children.Add(text);
                row.Children.Add(dot);
            }

            var button = new Button
            {
                DataContext = port,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                HorizontalContentAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                Content = row,
            };
            AutomationProperties.SetName(
                button,
                $"{node.Title} {(isInput ? "input" : "output")} {port.Label}");

            button.PointerPressed += (_, args) =>
            {
                if (args.GetCurrentPoint(button).Properties.IsLeftButtonPressed)
                {
                    args.Handled = true;
                }
            };
            button.Click += (_, _) =>
            {
                Focus();
                ViewModel?.ActivatePort(node, port);
                RenderConnections();
            };
            button.ContextRequested += (_, args) =>
            {
                if (ViewModel is null)
                {
                    return;
                }

                Focus();
                ViewModel.SelectNode(node);
                args.Handled = OpenContextMenu(
                    button,
                    NodeCanvasContextMenuContextFactory.CreatePortContext(
                        CreateContextMenuSnapshot(),
                        ResolveWorldPosition(args, this),
                        node.Id,
                        port.Id,
                        hostContext: ViewModel.HostContext));
            };

            portAnchors[port.Id] = dot;
            panel.Children.Add(button);
        }

        return panel;
    }

    private void UpdateViewportTransform()
    {
        if (_sceneRoot is null || ViewModel is null)
        {
            return;
        }

        var transforms = new TransformGroup();
        transforms.Children.Add(new ScaleTransform(ViewModel.Zoom, ViewModel.Zoom));
        transforms.Children.Add(new TranslateTransform(ViewModel.PanX, ViewModel.PanY));
        _sceneRoot.RenderTransform = transforms;
        _backgroundGrid?.InvalidateVisual();
    }

    private void RenderConnections()
    {
        if (_connectionLayer is null || ViewModel is null)
        {
            return;
        }

        _connectionLayer.Children.Clear();

        foreach (var connection in ViewModel.Connections)
        {
            var sourceNode = ViewModel.FindNode(connection.SourceNodeId);
            var targetNode = ViewModel.FindNode(connection.TargetNodeId);
            if (sourceNode is null || targetNode is null)
            {
                continue;
            }

            var sourcePort = sourceNode.GetPort(connection.SourcePortId);
            var targetPort = targetNode.GetPort(connection.TargetPortId);
            if (sourcePort is null || targetPort is null)
            {
                continue;
            }

            DrawConnection(
                GetPortAnchor(sourceNode, sourcePort),
                GetPortAnchor(targetNode, targetPort),
                connection);
        }

        if (ViewModel.HasPendingConnection
            && ViewModel.PendingSourceNode is not null
            && ViewModel.PendingSourcePort is not null
            && _interactionSession.PointerScreenPosition is not null)
        {
            var source = GetPortAnchor(ViewModel.PendingSourceNode, ViewModel.PendingSourcePort);
            var end = ViewModel.ScreenToWorld(
                new GraphPoint(
                    _interactionSession.PointerScreenPosition.Value.X,
                    _interactionSession.PointerScreenPosition.Value.Y));

            DrawConnection(source, end, new ConnectionViewModel(
                "pending",
                ViewModel.PendingSourceNode.Id,
                ViewModel.PendingSourcePort.Id,
                string.Empty,
                string.Empty,
                "pending",
                ViewModel.PendingSourcePort.AccentHex), isPreview: true);
        }
    }

    private void DrawConnection(GraphPoint start, GraphPoint end, ConnectionViewModel connection, bool isPreview = false)
    {
        if (_connectionLayer is null)
        {
            return;
        }

        var connectionStyle = GetConnectionStyle(connection);
        var curve = ConnectionPathBuilder.Build(start, end);
        var path = new global::Avalonia.Controls.Shapes.Path
        {
            Data = Geometry.Parse(
                $"M {curve.Start.X:0.##},{curve.Start.Y:0.##} " +
                $"C {curve.Control1.X:0.##},{curve.Control1.Y:0.##} " +
                $"{curve.Control2.X:0.##},{curve.Control2.Y:0.##} " +
                $"{curve.End.X:0.##},{curve.End.Y:0.##}"),
            Stroke = BrushFactory.Solid(connection.AccentHex, isPreview ? connectionStyle.PreviewStrokeOpacity : connectionStyle.StrokeOpacity),
            StrokeThickness = isPreview ? connectionStyle.PreviewThickness : connectionStyle.Thickness,
            StrokeLineCap = PenLineCap.Round,
        };
        _connectionLayer.Children.Add(path);

        if (isPreview)
        {
            return;
        }

        var midpoint = new Point((curve.Start.X + curve.End.X) / 2, (curve.Start.Y + curve.End.Y) / 2);

        var chip = new Border
        {
            Background = BrushFactory.Solid(connectionStyle.LabelBackgroundHex, connectionStyle.LabelBackgroundOpacity),
            BorderBrush = BrushFactory.Solid(connection.AccentHex, connectionStyle.LabelBorderOpacity),
            BorderThickness = new Thickness(connectionStyle.LabelBorderThickness),
            CornerRadius = new CornerRadius(connectionStyle.LabelCornerRadius),
            Padding = new Thickness(connectionStyle.LabelHorizontalPadding, connectionStyle.LabelVerticalPadding),
            Focusable = true,
            Child = new TextBlock
            {
                Text = connection.Label,
                FontSize = connectionStyle.LabelFontSize,
                Foreground = BrushFactory.Solid(connectionStyle.LabelForegroundHex, connectionStyle.LabelForegroundOpacity),
            },
        };
        AutomationProperties.SetName(chip, $"{connection.Label} connection");
        chip.KeyDown += (_, args) =>
        {
            if (ViewModel is null || string.IsNullOrWhiteSpace(connection.TargetNodeId))
            {
                return;
            }

            if (args.Key == Key.Apps || (args.Key == Key.F10 && args.KeyModifiers.HasFlag(KeyModifiers.Shift)))
            {
                args.Handled = OpenContextMenu(
                    chip,
                    NodeCanvasContextMenuContextFactory.CreateConnectionContext(
                        CreateContextMenuSnapshot(),
                        new GraphPoint(0, 0),
                        connection.Id,
                        hostContext: ViewModel.HostContext));
            }
        };
        chip.ContextRequested += (_, args) =>
        {
            if (ViewModel is null || string.IsNullOrWhiteSpace(connection.TargetNodeId))
            {
                return;
            }

            args.Handled = OpenContextMenu(
                chip,
                NodeCanvasContextMenuContextFactory.CreateConnectionContext(
                    CreateContextMenuSnapshot(),
                    ResolveWorldPosition(args, this),
                    connection.Id,
                    hostContext: ViewModel.HostContext));
        };

        Canvas.SetLeft(chip, midpoint.X + connectionStyle.LabelOffsetX);
        Canvas.SetTop(chip, midpoint.Y + connectionStyle.LabelOffsetY);
        _connectionLayer.Children.Add(chip);
    }

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
        if (ViewModel is null || !EnableDefaultWheelViewportGestures)
        {
            return;
        }

        var point = args.GetPosition(this);
        _interactionSession.UpdatePointerPosition(point);

        // 判断是否按住了 Control 键。多数精度触控板的"捏合"手势，会被转化为带 Control 修饰符的滚轮事件。
        if (args.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            // 执行缩放：向上滚动（Delta.Y > 0）或展开双指为放大，向下滚动或捏合双指为缩小。
            var factor = args.Delta.Y >= 0 ? 1.12 : 1 / 1.12;
            ViewModel.ZoomAt(factor, new GraphPoint(point.X, point.Y));
        }
        else
        {
            // 执行平移：常规滚轮或触控板双指平行滑动，将移动画布的视口。
            // 滚轮事件的 Delta 单位常常是行距，通过乘以平移常量转化为视口的像素偏移。
            const double scrollSpeedMultiplier = 40.0;
            ViewModel.PanBy(args.Delta.X * scrollSpeedMultiplier, args.Delta.Y * scrollSpeedMultiplier);
        }

        args.Handled = true;
    }

    private void HandleCanvasKeyDown(object? sender, KeyEventArgs args)
    {
        if (ViewModel is null || !EnableDefaultCommandShortcuts || ShortcutBelongsToInputControl(args.Source))
        {
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.S)
        {
            if (ViewModel.SaveCommand.CanExecute(null))
            {
                ViewModel.SaveCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.O)
        {
            if (ViewModel.LoadCommand.CanExecute(null))
            {
                ViewModel.LoadCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control)
            && (args.Key == Key.Y || (args.Key == Key.Z && args.KeyModifiers.HasFlag(KeyModifiers.Shift))))
        {
            if (ViewModel.RedoCommand.CanExecute(null))
            {
                ViewModel.RedoCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.Z)
        {
            if (ViewModel.UndoCommand.CanExecute(null))
            {
                ViewModel.UndoCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.C)
        {
            if (ViewModel.CopySelectionCommand.CanExecute(null))
            {
                ViewModel.CopySelectionCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.KeyModifiers.HasFlag(KeyModifiers.Control) && args.Key == Key.V)
        {
            if (ViewModel.PasteCommand.CanExecute(null))
            {
                ViewModel.PasteCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        if (args.Key == Key.Delete)
        {
            if (ViewModel.DeleteSelectionCommand.CanExecute(null))
            {
                ViewModel.DeleteSelectionCommand.Execute(null);
            }

            args.Handled = true;
            return;
        }

        switch (args.Key)
        {
            case Key.Escape:
                if (ViewModel.CancelPendingConnectionCommand.CanExecute(null))
                {
                    ViewModel.CancelPendingConnectionCommand.Execute(null);
                    args.Handled = true;
                }

                break;
        }
    }

    private void HandleCanvasContextRequested(object? sender, ContextRequestedEventArgs args)
    {
        if (ViewModel is null || args.Handled || !EnableDefaultContextMenu)
        {
            return;
        }

        // 多选激活时，空白画布右击同样复用批量选择菜单。
        args.Handled = OpenContextMenu(
            this,
            NodeCanvasContextMenuContextFactory.CreateCanvasContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, this),
                useSelectionTools: ViewModel.HasMultipleSelection,
                hostContext: ViewModel.HostContext));
    }

    private void HandleViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(GraphEditorViewModel.Zoom):
            case nameof(GraphEditorViewModel.PanX):
            case nameof(GraphEditorViewModel.PanY):
                UpdateViewportTransform();
                if (ViewModel?.HasPendingConnection == true)
                {
                    RenderConnections();
                }
                break;
            case nameof(GraphEditorViewModel.SelectedNode):
                UpdateSelectionState();
                RenderConnections();
                break;
            case nameof(GraphEditorViewModel.StyleOptions):
                ApplySelectionAdornerStyle();
                ApplyGuideAdornerStyle();
                break;
            case nameof(GraphEditorViewModel.BehaviorOptions):
                if (ViewModel?.BehaviorOptions.DragAssist.EnableAlignmentGuides != true)
                {
                    HideGuideAdorners();
                }
                break;
            case nameof(GraphEditorViewModel.PendingSourceNode):
            case nameof(GraphEditorViewModel.PendingSourcePort):
                RenderConnections();
                break;
        }
    }

    private void HandleNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (NodeViewModel node in args.OldItems)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (NodeViewModel node in args.NewItems)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
            }
        }

        RebuildScene();
    }

    private void HandleConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => RenderConnections();

    private void HandleNodePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is not NodeViewModel node)
        {
            return;
        }

        switch (args.PropertyName)
        {
            case nameof(NodeViewModel.X):
            case nameof(NodeViewModel.Y):
                UpdateNodePosition(node);
                RenderConnections();
                break;
            case nameof(NodeViewModel.Height):
                UpdateNodeVisual(node);
                RenderConnections();
                break;
            case nameof(NodeViewModel.IsSelected):
            case nameof(NodeViewModel.Presentation):
                UpdateNodeVisual(node);
                break;
        }
    }

    private void UpdateSelectionState()
    {
        foreach (var node in ViewModel?.Nodes ?? [])
        {
            UpdateNodeVisual(node);
        }
    }

    private void UpdateNodePosition(NodeViewModel node)
    {
        if (_nodeVisuals.TryGetValue(node, out var visual))
        {
            Canvas.SetLeft(visual.Root, node.X);
            Canvas.SetTop(visual.Root, node.Y);
        }
    }

    private void UpdateNodeVisual(NodeViewModel node)
    {
        if (!_nodeVisuals.TryGetValue(node, out var visual))
        {
            return;
        }

        visual.Presenter.Update(visual.Visual, CreateNodeVisualContext(node));
        Canvas.SetLeft(visual.Root, node.X);
        Canvas.SetTop(visual.Root, node.Y);
    }

    private GraphPoint GetPortAnchor(NodeViewModel node, PortViewModel port)
    {
        // 吸附拖动时，节点外层 Canvas 的绝对布局可能还没完成新一轮 Arrange。
        // 因此这里优先读取“端口圆点在节点卡内部的局部坐标”，再叠加当前节点的 X/Y，
        // 这样连线能跟随最新的节点位置，而不会因为布局时序晚一拍出现偏移。
        if (_nodeVisuals.TryGetValue(node, out var visual)
            && visual.Visual.PortAnchors.TryGetValue(port.Id, out var anchorDot))
        {
            var center = new Point(anchorDot.Bounds.Width / 2, anchorDot.Bounds.Height / 2);
            var localToNode = anchorDot.TranslatePoint(center, visual.Root);
            if (localToNode is not null)
            {
                return new GraphPoint(
                    node.X + localToNode.Value.X,
                    node.Y + localToNode.Value.Y);
            }

            if (_nodeLayer is not null)
            {
                var translated = anchorDot.TranslatePoint(center, _nodeLayer);
                if (translated is not null)
                {
                    return new GraphPoint(translated.Value.X, translated.Value.Y);
                }
            }
        }

        return node.GetPortAnchor(port);
    }

    private NodeCardStyleOptions GetNodeCardStyle(NodeViewModel node)
        => ViewModel?.StyleOptions.NodeOverrides.FirstOrDefault(overrideStyle => overrideStyle.DefinitionId == node.DefinitionId)?.Style
           ?? ViewModel?.StyleOptions.NodeCard
           ?? GraphEditorStyleOptions.Default.NodeCard;

    private PortStyleOptions GetPortStyle(PortViewModel? port)
        => port is null
            ? ViewModel?.StyleOptions.Port ?? GraphEditorStyleOptions.Default.Port
            : ViewModel?.StyleOptions.PortOverrides.FirstOrDefault(overrideStyle => overrideStyle.TypeId == port.TypeId)?.Style
              ?? ViewModel?.StyleOptions.Port
              ?? GraphEditorStyleOptions.Default.Port;

    private ConnectionStyleOptions GetConnectionStyle(ConnectionViewModel connection)
        => connection.ConversionId is not null
            ? ViewModel?.StyleOptions.ConnectionOverrides.FirstOrDefault(overrideStyle => overrideStyle.ConversionId == connection.ConversionId)?.Style
              ?? ViewModel?.StyleOptions.Connection
              ?? GraphEditorStyleOptions.Default.Connection
            : ViewModel?.StyleOptions.Connection
              ?? GraphEditorStyleOptions.Default.Connection;

    private GraphPoint ResolveWorldPosition(ContextRequestedEventArgs args, Control relativeTo)
    {
        if (ViewModel is not null && args.TryGetPosition(relativeTo, out var point))
        {
            return ViewModel.ScreenToWorld(new GraphPoint(point.X, point.Y));
        }

        return ViewModel is null ? new GraphPoint(0, 0) : ViewModel.ScreenToWorld(new GraphPoint(0, 0));
    }

    private bool OpenContextMenu(Control target, ContextMenuContext context)
    {
        if (ViewModel is null || !EnableDefaultContextMenu)
        {
            return false;
        }

        var descriptors = ViewModel.BuildContextMenu(context);
        if (descriptors.Count == 0)
        {
            return false;
        }

        (ContextMenuPresenter ?? _stockContextMenuPresenter).Open(target, descriptors, ViewModel.StyleOptions.ContextMenu);
        return true;
    }

    private static bool ShortcutBelongsToInputControl(object? source)
    {
        for (var current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (current is TextBox or ComboBox)
            {
                return true;
            }
        }

        return false;
    }

    private NodeCanvasContextMenuSnapshot CreateContextMenuSnapshot()
        => ViewModel is null
            ? new NodeCanvasContextMenuSnapshot(null, [], [])
            : new NodeCanvasContextMenuSnapshot(
                ViewModel.SelectedNode?.Id,
                ViewModel.SelectedNodes.Select(selected => selected.Id).ToList(),
                ViewModel.NodeTemplates.Select(template => template.Definition).ToList());

    private void ApplySelectionAdornerStyle()
    {
        if (_selectionAdorner is null)
        {
            return;
        }

        var style = ViewModel?.StyleOptions.Canvas ?? GraphEditorStyleOptions.Default.Canvas;
        _selectionAdorner.BorderBrush = BrushFactory.Solid(style.SelectionBorderHex);
        _selectionAdorner.Background = BrushFactory.Solid(style.SelectionFillHex, style.SelectionFillOpacity);
        _selectionAdorner.BorderThickness = new Thickness(style.SelectionBorderThickness);
        _selectionAdorner.CornerRadius = new CornerRadius(style.SelectionCornerRadius);
    }

    private void ApplyGuideAdornerStyle()
    {
        var style = ViewModel?.StyleOptions.Canvas ?? GraphEditorStyleOptions.Default.Canvas;
        if (_verticalGuideAdorner is not null)
        {
            _verticalGuideAdorner.Background = BrushFactory.Solid(style.GuideHex, style.GuideOpacity);
            _verticalGuideAdorner.Width = style.GuideThickness;
        }

        if (_horizontalGuideAdorner is not null)
        {
            _horizontalGuideAdorner.Background = BrushFactory.Solid(style.GuideHex, style.GuideOpacity);
            _horizontalGuideAdorner.Height = style.GuideThickness;
        }
    }

    private void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize)
    {
        if (ViewModel is null
            || _overlayLayer is null
            || _selectionAdorner is null
            || _interactionSession.SelectionStartScreenPosition is null)
        {
            return;
        }

        var start = _interactionSession.SelectionStartScreenPosition.Value;
        var left = Math.Min(start.X, currentScreenPosition.X);
        var top = Math.Min(start.Y, currentScreenPosition.Y);
        var width = Math.Abs(currentScreenPosition.X - start.X);
        var height = Math.Abs(currentScreenPosition.Y - start.Y);

        _selectionAdorner.IsVisible = true;
        _selectionAdorner.Width = width;
        _selectionAdorner.Height = height;
        Canvas.SetLeft(_selectionAdorner, left);
        Canvas.SetTop(_selectionAdorner, top);

        var worldStart = ViewModel.ScreenToWorld(new GraphPoint(start.X, start.Y));
        var worldEnd = ViewModel.ScreenToWorld(new GraphPoint(currentScreenPosition.X, currentScreenPosition.Y));
        var hitNodes = ViewModel.GetNodesInRectangle(worldStart, worldEnd).ToList();
        var nodes = ApplySelectionModifiers(hitNodes);
        var primaryNode = nodes.LastOrDefault();

        ViewModel.SetSelection(
            nodes,
            primaryNode,
            finalize
                ? nodes.Count switch
                {
                    0 => "No nodes inside marquee selection.",
                    1 => $"Selected {nodes[0].Title}.",
                    _ => $"Selected {nodes.Count} nodes.",
                }
                : null);
    }

    private void HideSelectionAdorner()
    {
        if (_selectionAdorner is null)
        {
            return;
        }

        _selectionAdorner.IsVisible = false;
        _selectionAdorner.Width = 0;
        _selectionAdorner.Height = 0;
    }

    private IReadOnlyList<NodeViewModel> ApplySelectionModifiers(IReadOnlyList<NodeViewModel> hitNodes)
    {
        if (ViewModel is null)
        {
            return hitNodes;
        }

        if (_interactionSession.SelectionModifiers.HasFlag(KeyModifiers.Control))
        {
            var toggled = _interactionSession.SelectionBaselineNodes.ToList();
            foreach (var node in hitNodes)
            {
                if (!toggled.Remove(node))
                {
                    toggled.Add(node);
                }
            }

            return toggled;
        }

        if (_interactionSession.SelectionModifiers.HasFlag(KeyModifiers.Shift))
        {
            return _interactionSession.SelectionBaselineNodes
                .Concat(hitNodes)
                .Distinct()
                .ToList();
        }

        return hitNodes;
    }

    private GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY)
    {
        if (ViewModel is null)
        {
            return new GraphPoint(deltaX, deltaY);
        }

        var style = ViewModel.StyleOptions.Canvas;
        var behavior = ViewModel.BehaviorOptions.DragAssist;
        HideGuideAdorners();

        if (!behavior.EnableGridSnapping && !behavior.EnableAlignmentGuides)
        {
            return new GraphPoint(deltaX, deltaY);
        }

        var tolerance = behavior.SnapTolerance / Math.Max(ViewModel.Zoom, 0.001);
        IEnumerable<NodeBounds> candidateBounds = [];
        if (behavior.EnableAlignmentGuides)
        {
            var movingNodeIds = dragSession.Nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
            candidateBounds = ViewModel.Nodes
                .Where(node => !movingNodeIds.Contains(node.Id))
                .Select(node => new NodeBounds(node.X, node.Y, node.Width, node.Height));
        }

        var result = NodeCanvasDragAssistCalculator.Calculate(
            dragSession.OriginBounds,
            deltaX,
            deltaY,
            candidateBounds,
            behavior.EnableGridSnapping,
            behavior.EnableAlignmentGuides,
            style.PrimaryGridSpacing,
            tolerance);

        ShowGuideAdorners(result.GuideWorldX, result.GuideWorldY);
        return result.AdjustedDelta;
    }

    private NodeBounds GetSelectionBounds(IReadOnlyList<NodeViewModel> nodes)
    {
        var left = nodes.Min(node => node.X);
        var top = nodes.Min(node => node.Y);
        var right = nodes.Max(node => node.X + node.Width);
        var bottom = nodes.Max(node => node.Y + node.Height);
        return new NodeBounds(left, top, right - left, bottom - top);
    }

    private NodeCanvasDragSession CreateDragSession(IReadOnlyList<NodeViewModel> nodes)
    {
        var originPositions = nodes.ToDictionary(
            node => node.Id,
            node => new GraphPoint(node.X, node.Y),
            StringComparer.Ordinal);

        return new NodeCanvasDragSession(nodes, originPositions, GetSelectionBounds(nodes));
    }

    private void ShowGuideAdorners(double? worldX, double? worldY)
    {
        if (ViewModel is null || _overlayLayer is null)
        {
            return;
        }

        if (_verticalGuideAdorner is not null)
        {
            if (worldX is double x)
            {
                var screenX = WorldToScreen(x, 0).X;
                _verticalGuideAdorner.IsVisible = true;
                _verticalGuideAdorner.Height = Bounds.Height;
                Canvas.SetLeft(_verticalGuideAdorner, screenX - (_verticalGuideAdorner.Width / 2));
                Canvas.SetTop(_verticalGuideAdorner, 0);
            }
            else
            {
                _verticalGuideAdorner.IsVisible = false;
            }
        }

        if (_horizontalGuideAdorner is not null)
        {
            if (worldY is double y)
            {
                var screenY = WorldToScreen(0, y).Y;
                _horizontalGuideAdorner.IsVisible = true;
                _horizontalGuideAdorner.Width = Bounds.Width;
                Canvas.SetLeft(_horizontalGuideAdorner, 0);
                Canvas.SetTop(_horizontalGuideAdorner, screenY - (_horizontalGuideAdorner.Height / 2));
            }
            else
            {
                _horizontalGuideAdorner.IsVisible = false;
            }
        }
    }

    private void HideGuideAdorners()
    {
        if (_verticalGuideAdorner is not null)
        {
            _verticalGuideAdorner.IsVisible = false;
        }

        if (_horizontalGuideAdorner is not null)
        {
            _horizontalGuideAdorner.IsVisible = false;
        }
    }

    private GraphPoint WorldToScreen(double x, double y)
        => ViewportMath.WorldToScreen(
            new ViewportState(ViewModel?.Zoom ?? 1, ViewModel?.PanX ?? 0, ViewModel?.PanY ?? 0),
            new GraphPoint(x, y));

    private sealed record RenderedNodeVisual(
        Control Root,
        IGraphNodeVisualPresenter Presenter,
        GraphNodeVisual Visual);

}
