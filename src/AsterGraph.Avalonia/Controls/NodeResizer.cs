using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Standalone node resizer control that commits node size changes through the canonical editor session command route.
/// </summary>
public sealed class NodeResizer : UserControl
{
    /// <summary>
    /// Bound canonical editor session.
    /// </summary>
    public static readonly StyledProperty<IGraphEditorSession?> SessionProperty =
        AvaloniaProperty.Register<NodeResizer, IGraphEditorSession?>(nameof(Session));

    /// <summary>
    /// Bound node identifier.
    /// </summary>
    public static readonly StyledProperty<string?> NodeIdProperty =
        AvaloniaProperty.Register<NodeResizer, string?>(nameof(NodeId));

    /// <summary>
    /// Size delta applied by each accessible handle activation.
    /// </summary>
    public static readonly StyledProperty<double> ResizeStepProperty =
        AvaloniaProperty.Register<NodeResizer, double>(nameof(ResizeStep), 20d);

    /// <summary>
    /// Minimum committed node width.
    /// </summary>
    public static readonly StyledProperty<double> MinimumWidthProperty =
        AvaloniaProperty.Register<NodeResizer, double>(nameof(MinimumWidth), GraphEditorNodeSurfaceMeasurement.Default.BaselineSize.Width);

    /// <summary>
    /// Minimum committed node height.
    /// </summary>
    public static readonly StyledProperty<double> MinimumHeightProperty =
        AvaloniaProperty.Register<NodeResizer, double>(nameof(MinimumHeight), GraphEditorNodeSurfaceMeasurement.Default.BaselineSize.Height);

    private static readonly Cursor HorizontalResizeCursor = new(StandardCursorType.SizeWestEast);
    private static readonly Cursor VerticalResizeCursor = new(StandardCursorType.SizeNorthSouth);
    private static readonly Cursor CornerResizeCursor = new(StandardCursorType.BottomRightCorner);

    private readonly Button _rightHandle;
    private readonly Button _bottomHandle;
    private readonly Button _cornerHandle;
    private IGraphEditorSession? _observedSession;

    /// <summary>
    /// Initializes a standalone node resizer.
    /// </summary>
    public NodeResizer()
    {
        Focusable = false;
        _rightHandle = CreateHandle("PART_NodeResizerRightHandle", "Resize Node Right", HorizontalResizeCursor);
        _bottomHandle = CreateHandle("PART_NodeResizerBottomHandle", "Resize Node Down", VerticalResizeCursor);
        _cornerHandle = CreateHandle("PART_NodeResizerCornerHandle", "Resize Node Diagonal", CornerResizeCursor);
        _rightHandle.Click += (_, _) => ResizeBy(ResizeStep, 0d);
        _bottomHandle.Click += (_, _) => ResizeBy(0d, ResizeStep);
        _cornerHandle.Click += (_, _) => ResizeBy(ResizeStep, ResizeStep);

        Content = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Children =
            {
                ConfigurePlacement(_rightHandle, row: 0, column: 1),
                ConfigurePlacement(_bottomHandle, row: 1, column: 0),
                ConfigurePlacement(_cornerHandle, row: 1, column: 1),
            },
        };
        RefreshHandles();
    }

    /// <summary>
    /// Current canonical editor session.
    /// </summary>
    public IGraphEditorSession? Session
    {
        get => GetValue(SessionProperty);
        set => SetValue(SessionProperty, value);
    }

    /// <summary>
    /// Current node identifier.
    /// </summary>
    public string? NodeId
    {
        get => GetValue(NodeIdProperty);
        set => SetValue(NodeIdProperty, value);
    }

    /// <summary>
    /// Size delta applied by each accessible handle activation.
    /// </summary>
    public double ResizeStep
    {
        get => GetValue(ResizeStepProperty);
        set => SetValue(ResizeStepProperty, value);
    }

    /// <summary>
    /// Minimum committed node width.
    /// </summary>
    public double MinimumWidth
    {
        get => GetValue(MinimumWidthProperty);
        set => SetValue(MinimumWidthProperty, value);
    }

    /// <summary>
    /// Minimum committed node height.
    /// </summary>
    public double MinimumHeight
    {
        get => GetValue(MinimumHeightProperty);
        set => SetValue(MinimumHeightProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachSession(Session);
        RefreshHandles();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        AttachSession(null);
        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SessionProperty)
        {
            AttachSession(change.GetNewValue<IGraphEditorSession?>());
        }

        if (change.Property == SessionProperty || change.Property == NodeIdProperty)
        {
            RefreshHandles();
        }
    }

    private static Button CreateHandle(string name, string automationName, Cursor cursor)
    {
        var button = new Button
        {
            Name = name,
            Width = 18,
            Height = 18,
            MinWidth = 18,
            MinHeight = 18,
            Padding = new Thickness(0),
            Margin = new Thickness(2),
            Cursor = cursor,
            Content = string.Empty,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        AutomationProperties.SetName(button, automationName);
        return button;
    }

    private static Button ConfigurePlacement(Button handle, int row, int column)
    {
        Grid.SetRow(handle, row);
        Grid.SetColumn(handle, column);
        return handle;
    }

    private void AttachSession(IGraphEditorSession? session)
    {
        if (ReferenceEquals(_observedSession, session))
        {
            return;
        }

        if (_observedSession is not null)
        {
            _observedSession.Events.DocumentChanged -= HandleSessionChanged;
        }

        _observedSession = session;
        if (_observedSession is not null)
        {
            _observedSession.Events.DocumentChanged += HandleSessionChanged;
        }
    }

    private void HandleSessionChanged(object? sender, EventArgs args)
        => RefreshHandles();

    private void RefreshHandles()
    {
        var canResize = Session is not null
            && !string.IsNullOrWhiteSpace(NodeId)
            && TryGetNodeSize(out _);
        _rightHandle.IsEnabled = canResize;
        _bottomHandle.IsEnabled = canResize;
        _cornerHandle.IsEnabled = canResize;
    }

    private bool ResizeBy(double deltaWidth, double deltaHeight)
    {
        if (Session is null || string.IsNullOrWhiteSpace(NodeId) || !TryGetNodeSize(out var currentSize))
        {
            return false;
        }

        var nextSize = new GraphSize(
            Math.Max(NormalizeMinimum(MinimumWidth), currentSize.Width + deltaWidth),
            Math.Max(NormalizeMinimum(MinimumHeight), currentSize.Height + deltaHeight));
        return Session.Commands.TrySetNodeSize(NodeId, nextSize, updateStatus: false);
    }

    private bool TryGetNodeSize(out GraphSize size)
    {
        if (Session is not null && !string.IsNullOrWhiteSpace(NodeId))
        {
            foreach (var surface in Session.Queries.GetNodeSurfaceSnapshots())
            {
                if (string.Equals(surface.NodeId, NodeId, StringComparison.Ordinal))
                {
                    size = surface.Size;
                    return true;
                }
            }
        }

        size = default;
        return false;
    }

    private static double NormalizeMinimum(double minimum)
        => double.IsFinite(minimum) ? Math.Max(0d, minimum) : 0d;
}
