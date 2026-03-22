using System.Collections.Specialized;
using System.ComponentModel;
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
using AsterGraph.Avalonia.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class NodeCanvas : UserControl
{
    public static readonly StyledProperty<GraphEditorViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<NodeCanvas, GraphEditorViewModel?>(nameof(ViewModel));

    private readonly Dictionary<NodeViewModel, NodeVisual> _nodeVisuals = new();
    private Grid? _sceneRoot;
    private Canvas? _connectionLayer;
    private Canvas? _nodeLayer;
    private GridBackground? _backgroundGrid;
    private NodeViewModel? _dragNode;
    private bool _isPanning;
    private Point _lastPointerPosition;
    private Point? _pointerScreenPosition;

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

    protected override Size ArrangeOverride(Size finalSize)
    {
        ViewModel?.UpdateViewportSize(finalSize.Width, finalSize.Height);
        return base.ArrangeOverride(finalSize);
    }

    public GraphEditorViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public void FitToScene(bool updateStatus = true)
        => ViewModel?.FitToViewport(Bounds.Width, Bounds.Height, updateStatus);

    public void ResetViewport()
        => ViewModel?.ResetView();

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
        _backgroundGrid = this.FindControl<GridBackground>("BackgroundGrid");
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
            Padding = new Thickness(16, 14, 16, 12),
            CornerRadius = new CornerRadius(20, 20, 0, 0),
        };

        var headerStack = new StackPanel { Spacing = 4 };
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Category.ToUpperInvariant(),
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = BrushFactory.Solid(nodeStyle.CategoryTextHex, nodeStyle.CategoryTextOpacity),
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Title,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = BrushFactory.Solid(nodeStyle.TitleTextHex),
        });
        headerStack.Children.Add(new TextBlock
        {
            Text = node.Subtitle,
            FontSize = 12,
            Foreground = BrushFactory.Solid(nodeStyle.SubtitleTextHex, nodeStyle.SubtitleTextOpacity),
        });
        header.Child = headerStack;
        root.Children.Add(header);

        var body = new Grid
        {
            Margin = new Thickness(16, 12, 16, 16),
            RowDefinitions = new RowDefinitions("Auto,*"),
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 14,
            RowSpacing = 12,
        };
        Grid.SetRow(body, 1);

        var description = new TextBlock
        {
            Text = node.Description,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Foreground = BrushFactory.Solid(nodeStyle.DescriptionTextHex, nodeStyle.DescriptionTextOpacity),
            MaxHeight = 40,
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
            ViewModel.SelectNode(node);
            OpenContextMenu(
                border,
                new ContextMenuContext(
                    ContextMenuTargetKind.Node,
                    ResolveWorldPosition(args, this),
                    selectedNodeId: ViewModel.SelectedNode?.Id,
                    clickedNodeId: node.Id,
                    availableNodeDefinitions: ViewModel.NodeTemplates.Select(template => template.Definition).ToList()));
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
                Spacing = 1,
                HorizontalAlignment = isInput ? HorizontalAlignment.Left : HorizontalAlignment.Right,
            };
            text.Children.Add(new TextBlock
            {
                Text = port.Label,
                FontSize = 12,
                FontWeight = FontWeight.Medium,
                Foreground = BrushFactory.Solid(portStyle.LabelHex),
                TextAlignment = isInput ? TextAlignment.Left : TextAlignment.Right,
            });
            text.Children.Add(new TextBlock
            {
                Text = port.DataType,
                FontSize = 10,
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
                    new ContextMenuContext(
                        ContextMenuTargetKind.Port,
                        ResolveWorldPosition(args, this),
                        selectedNodeId: ViewModel.SelectedNode?.Id,
                        clickedNodeId: node.Id,
                        clickedPortNodeId: node.Id,
                        clickedPortId: port.Id,
                        availableNodeDefinitions: ViewModel.NodeTemplates.Select(template => template.Definition).ToList()));
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
            && _pointerScreenPosition is not null)
        {
            var source = GetPortAnchor(ViewModel.PendingSourceNode, ViewModel.PendingSourcePort);
            var end = ViewModel.ScreenToWorld(new GraphPoint(_pointerScreenPosition.Value.X, _pointerScreenPosition.Value.Y));

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
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 4),
            Child = new TextBlock
            {
                Text = connection.Label,
                FontSize = 10,
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
                new ContextMenuContext(
                    ContextMenuTargetKind.Connection,
                    ResolveWorldPosition(args, this),
                    selectedNodeId: ViewModel.SelectedNode?.Id,
                    clickedConnectionId: connection.Id,
                    availableNodeDefinitions: ViewModel.NodeTemplates.Select(template => template.Definition).ToList()));
            args.Handled = true;
        };

        Canvas.SetLeft(chip, midpoint.X + 8);
        Canvas.SetTop(chip, midpoint.Y - 12);
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

        Focus();
        ViewModel.SelectNode(node);

        if (_nodeLayer is not null && _nodeVisuals.TryGetValue(node, out var visual))
        {
            _nodeLayer.Children.Remove(visual.Border);
            _nodeLayer.Children.Add(visual.Border);
        }

        _dragNode = node;
        _isPanning = false;
        _lastPointerPosition = args.GetPosition(this);
        _pointerScreenPosition = _lastPointerPosition;
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
        _lastPointerPosition = args.GetPosition(this);
        _pointerScreenPosition = _lastPointerPosition;

        if (props.IsMiddleButtonPressed)
        {
            _isPanning = true;
            _dragNode = null;
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

            ViewModel.SelectNode(null);
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
        _pointerScreenPosition = current;

        if (_isPanning || _dragNode is not null)
        {
            var delta = current - _lastPointerPosition;
            _lastPointerPosition = current;

            if (_dragNode is not null)
            {
                ViewModel.MoveNode(_dragNode, delta.X / ViewModel.Zoom, delta.Y / ViewModel.Zoom);
            }
            else if (_isPanning)
            {
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
        _dragNode = null;
        _isPanning = false;
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
        _pointerScreenPosition = point;
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

        OpenContextMenu(
            this,
            new ContextMenuContext(
                ContextMenuTargetKind.Canvas,
                ResolveWorldPosition(args, this),
                selectedNodeId: ViewModel.SelectedNode?.Id,
                availableNodeDefinitions: ViewModel.NodeTemplates.Select(template => template.Definition).ToList()));
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
        // Connection lines must attach to the actual rendered port-dot center, not a guessed layout formula.
        // This avoids visual drift when card content, fonts, spacing, or dynamic node height changes.
        if (_nodeLayer is not null
            && _nodeVisuals.TryGetValue(node, out var visual)
            && visual.PortAnchors.TryGetValue(port.Id, out var anchorDot))
        {
            var center = new Point(anchorDot.Bounds.Width / 2, anchorDot.Bounds.Height / 2);
            var translated = anchorDot.TranslatePoint(center, _nodeLayer);
            if (translated is not null)
            {
                return new GraphPoint(translated.Value.X, translated.Value.Y);
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

        var menuStyle = ViewModel.StyleOptions.ContextMenu;
        var menu = new ContextMenu
        {
            PlacementTarget = target,
            Placement = PlacementMode.Pointer,
            Background = BrushFactory.Solid(menuStyle.BackgroundHex),
            BorderBrush = BrushFactory.Solid(menuStyle.BorderHex),
            ItemsSource = descriptors.Select(BuildMenuControl).ToList(),
        };

        menu.Open(target);
    }

    private object BuildMenuControl(MenuItemDescriptor descriptor)
    {
        var menuStyle = ViewModel?.StyleOptions.ContextMenu ?? GraphEditorStyleOptions.Default.ContextMenu;

        if (descriptor.IsSeparator)
        {
            return new Separator();
        }

        var menuItem = new MenuItem
        {
            Header = descriptor.Header,
            Command = descriptor.Command,
            CommandParameter = descriptor.CommandParameter,
            IsEnabled = descriptor.IsEnabled,
            Foreground = BrushFactory.Solid(descriptor.IsEnabled ? ViewModel?.StyleOptions.Shell.HeadlineHex ?? GraphEditorStyleOptions.Default.Shell.HeadlineHex : menuStyle.DisabledForegroundHex),
            Background = BrushFactory.Solid(menuStyle.BackgroundHex),
        };

        if (descriptor.HasChildren)
        {
            menuItem.ItemsSource = descriptor.Children.Select(BuildMenuControl).ToList();
        }

        return menuItem;
    }

    private sealed record NodeVisual(
        Border Border,
        Border Header,
        IReadOnlyDictionary<string, Border> PortAnchors);
}
