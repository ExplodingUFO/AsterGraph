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

    private readonly Dictionary<NodeViewModel, NodeVisual> _nodeVisuals = new();
    private Grid? _sceneRoot;
    private Canvas? _connectionLayer;
    private Canvas? _nodeLayer;
    private Canvas? _overlayLayer;
    private GridBackground? _backgroundGrid;
    private Border? _selectionAdorner;
    private Border? _verticalGuideAdorner;
    private Border? _horizontalGuideAdorner;
    private readonly GraphContextMenuPresenter _contextMenuPresenter = new();
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
            _nodeLayer.Children.Add(visual.Border);
            UpdateNodeVisual(node);
        }

        UpdateViewportTransform();
        RenderConnections();
        Dispatcher.UIThread.Post(RenderConnections, DispatcherPriority.Loaded);
    }

    private NodeVisual CreateNodeVisual(NodeViewModel node)
    {
        var portAnchors = new Dictionary<string, Border>(StringComparer.Ordinal);
        var nodeStyle = GetNodeCardStyle(node);
        var border = new Border
        {
            Width = node.Width,
            Height = node.Height,
            CornerRadius = new CornerRadius(nodeStyle.CornerRadius),
            BorderThickness = new Thickness(nodeStyle.BorderThickness),
        };
        AutomationProperties.SetName(border, $"{node.Title} node");

        var root = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };

        var header = new Border
        {
            Padding = new Thickness(
                nodeStyle.HeaderHorizontalPadding,
                nodeStyle.HeaderTopPadding,
                nodeStyle.HeaderHorizontalPadding,
                nodeStyle.HeaderBottomPadding),
            CornerRadius = new CornerRadius(nodeStyle.CornerRadius, nodeStyle.CornerRadius, 0, 0),
        };

        var headerStack = new StackPanel { Spacing = nodeStyle.HeaderSpacing };
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Category.ToUpperInvariant(),
            FontSize = nodeStyle.CategoryFontSize,
            FontWeight = FontWeight.Bold,
            Foreground = BrushFactory.Solid(nodeStyle.CategoryTextHex, nodeStyle.CategoryTextOpacity),
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Title,
            FontSize = nodeStyle.TitleFontSize,
            FontWeight = FontWeight.SemiBold,
            Foreground = BrushFactory.Solid(nodeStyle.TitleTextHex),
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Subtitle,
            FontSize = nodeStyle.SubtitleFontSize,
            Foreground = BrushFactory.Solid(nodeStyle.SubtitleTextHex, nodeStyle.SubtitleTextOpacity),
        });
        header.Child = headerStack;
        root.Children.Add(header);

        var body = new Grid
        {
            Margin = new Thickness(
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyTopPadding,
                nodeStyle.BodyHorizontalPadding,
                nodeStyle.BodyBottomPadding),
            RowDefinitions = new RowDefinitions("Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = nodeStyle.BodyColumnSpacing,
            RowSpacing = nodeStyle.BodyRowSpacing,
        };
        Grid.SetRow(body, 1);

        var description = new TextBlock
        {
            Text = node.Description,
            FontSize = nodeStyle.DescriptionFontSize,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid(nodeStyle.DescriptionTextHex, nodeStyle.DescriptionTextOpacity),
            MaxHeight = nodeStyle.DescriptionMaxHeight,
        };
        Grid.SetColumnSpan(description, 2);
        body.Children.Add(description);

        var inputs = BuildPortPanel(node, node.Inputs, isInput: true, portAnchors);
        Grid.SetRow(inputs, 1);
        body.Children.Add(inputs);

        var outputs = BuildPortPanel(node, node.Outputs, isInput: false, portAnchors);
        Grid.SetRow(outputs, 1);
        Grid.SetColumn(outputs, 1);
        body.Children.Add(outputs);

        root.Children.Add(body);
        border.Child = root;

        border.PointerPressed += (_, args) =>
        {
            if (args.Source is StyledElement { DataContext: PortViewModel })
            {
                return;
            }

            BeginNodeDrag(node, args);
        };
        border.ContextRequested += (_, args) =>
        {
            if (ViewModel is null)
            {
                return;
            }

            Focus();
            var targetKind = ContextMenuTargetKind.Node;
            if (ViewModel.HasMultipleSelection && node.IsSelected)
            {
                // 在多选集合内部右击时保留当前选择，并切换到批量工具菜单。
                ViewModel.SetSelection(ViewModel.SelectedNodes.ToList(), node);
                targetKind = ContextMenuTargetKind.Selection;
            }
            else
            {
                ViewModel.SelectSingleNode(node);
            }

            OpenContextMenu(
                border,
                NodeCanvasContextMenuContextFactory.CreateNodeContext(
                    CreateContextMenuSnapshot(),
                    ResolveWorldPosition(args, this),
                    node.Id,
                    useSelectionTools: targetKind == ContextMenuTargetKind.Selection,
                    hostContext: ViewModel.HostContext));
            args.Handled = true;
        };

        return new NodeVisual(border, header, portAnchors);
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
                OpenContextMenu(
                    button,
                    NodeCanvasContextMenuContextFactory.CreatePortContext(
                        CreateContextMenuSnapshot(),
                        ResolveWorldPosition(args, this),
                        node.Id,
                        port.Id,
                        hostContext: ViewModel.HostContext));
                args.Handled = true;
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
            Child = new TextBlock
            {
                Text = connection.Label,
                FontSize = connectionStyle.LabelFontSize,
                Foreground = BrushFactory.Solid(connectionStyle.LabelForegroundHex, connectionStyle.LabelForegroundOpacity),
            },
        };
        AutomationProperties.SetName(chip, $"{connection.Label} connection");
        chip.ContextRequested += (_, args) =>
        {
            if (ViewModel is null || string.IsNullOrWhiteSpace(connection.TargetNodeId))
            {
                return;
            }

            OpenContextMenu(
                chip,
                NodeCanvasContextMenuContextFactory.CreateConnectionContext(
                    CreateContextMenuSnapshot(),
                    ResolveWorldPosition(args, this),
                    connection.Id,
                    hostContext: ViewModel.HostContext));
            args.Handled = true;
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
            _nodeLayer.Children.Remove(visual.Border);
            _nodeLayer.Children.Add(visual.Border);
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

        if (props.IsMiddleButtonPressed)
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
        if (ViewModel is null)
        {
            return;
        }

        var factor = args.Delta.Y >= 0 ? 1.12 : 1 / 1.12;
        var point = args.GetPosition(this);
        _interactionSession.UpdatePointerPosition(point);
        ViewModel.ZoomAt(factor, new GraphPoint(point.X, point.Y));
        args.Handled = true;
    }

    private void HandleCanvasKeyDown(object? sender, KeyEventArgs args)
    {
        if (ViewModel is null)
        {
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
        if (ViewModel is null || args.Handled)
        {
            return;
        }

        // 多选激活时，空白画布右击同样复用批量选择菜单。
        OpenContextMenu(
            this,
            NodeCanvasContextMenuContextFactory.CreateCanvasContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, this),
                useSelectionTools: ViewModel.HasMultipleSelection,
                hostContext: ViewModel.HostContext));
        args.Handled = true;
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
            case nameof(NodeViewModel.IsSelected):
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
            Canvas.SetLeft(visual.Border, node.X);
            Canvas.SetTop(visual.Border, node.Y);
        }
    }

    private void UpdateNodeVisual(NodeViewModel node)
    {
        if (!_nodeVisuals.TryGetValue(node, out var visual))
        {
            return;
        }

        var nodeStyle = GetNodeCardStyle(node);
        visual.Border.Background = BrushFactory.Solid(node.IsSelected ? nodeStyle.SelectedBackgroundHex : nodeStyle.BackgroundHex);
        visual.Border.BorderBrush = BrushFactory.Solid(node.IsSelected ? node.AccentHex : nodeStyle.BorderHex);
        visual.Header.Background = BrushFactory.Solid(node.AccentHex, node.IsSelected ? nodeStyle.SelectedHeaderOpacity : nodeStyle.HeaderOpacity);
        visual.Border.BorderThickness = new Thickness(node.IsSelected ? nodeStyle.SelectedBorderThickness : nodeStyle.BorderThickness);

        Canvas.SetLeft(visual.Border, node.X);
        Canvas.SetTop(visual.Border, node.Y);
    }

    private GraphPoint GetPortAnchor(NodeViewModel node, PortViewModel port)
    {
        // 吸附拖动时，节点外层 Canvas 的绝对布局可能还没完成新一轮 Arrange。
        // 因此这里优先读取“端口圆点在节点卡内部的局部坐标”，再叠加当前节点的 X/Y，
        // 这样连线能跟随最新的节点位置，而不会因为布局时序晚一拍出现偏移。
        if (_nodeVisuals.TryGetValue(node, out var visual)
            && visual.PortAnchors.TryGetValue(port.Id, out var anchorDot))
        {
            var center = new Point(anchorDot.Bounds.Width / 2, anchorDot.Bounds.Height / 2);
            var localToNode = anchorDot.TranslatePoint(center, visual.Border);
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

    private void OpenContextMenu(Control target, ContextMenuContext context)
    {
        if (ViewModel is null)
        {
            return;
        }

        var descriptors = ViewModel.BuildContextMenu(context);
        if (descriptors.Count == 0)
        {
            return;
        }

        _contextMenuPresenter.Open(target, descriptors, ViewModel.StyleOptions.ContextMenu);
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

    private sealed record NodeVisual(
        Border Border,
        Border Header,
        IReadOnlyDictionary<string, Border> PortAnchors);

}
