using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// Standalone overlay panel that positions host-owned content inside an Avalonia overlay surface.
/// </summary>
public sealed class AsterGraphPanel : UserControl
{
    /// <summary>
    /// Panel position within the available overlay bounds.
    /// </summary>
    public static readonly StyledProperty<AsterGraphPanelPosition> PositionProperty =
        AvaloniaProperty.Register<AsterGraphPanel, AsterGraphPanelPosition>(
            nameof(Position),
            AsterGraphPanelPosition.TopRight);

    /// <summary>
    /// Distance from the selected overlay edge.
    /// </summary>
    public static readonly StyledProperty<Thickness> OffsetProperty =
        AvaloniaProperty.Register<AsterGraphPanel, Thickness>(nameof(Offset), new Thickness(16d));

    /// <summary>
    /// Initializes a standalone overlay panel.
    /// </summary>
    public AsterGraphPanel()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;
        Padding = new Thickness(8d);
        CornerRadius = new CornerRadius(8d);
        ClipToBounds = false;
    }

    /// <summary>
    /// Current panel position within the overlay bounds.
    /// </summary>
    public AsterGraphPanelPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>
    /// Current distance from the selected overlay edge.
    /// </summary>
    public Thickness Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (Content is not Control content)
        {
            return new Size(
                ResolveAvailableLength(availableSize.Width),
                ResolveAvailableLength(availableSize.Height));
        }

        var padding = Normalize(Padding);
        var offset = Normalize(Offset);
        var contentAvailable = new Size(
            ResolveContentAvailableLength(availableSize.Width, padding.Left + padding.Right + offset.Left + offset.Right),
            ResolveContentAvailableLength(availableSize.Height, padding.Top + padding.Bottom + offset.Top + offset.Bottom));
        content.Measure(contentAvailable);

        return new Size(
            ResolveAvailableLength(availableSize.Width),
            ResolveAvailableLength(availableSize.Height));
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Content is Control content)
        {
            var padding = Normalize(Padding);
            var offset = Normalize(Offset);
            var contentSize = content.DesiredSize;
            var outerWidth = contentSize.Width + padding.Left + padding.Right;
            var outerHeight = contentSize.Height + padding.Top + padding.Bottom;
            var origin = ResolveOrigin(finalSize, new Size(outerWidth, outerHeight), offset, Position);

            content.Arrange(new Rect(
                origin.X + padding.Left,
                origin.Y + padding.Top,
                contentSize.Width,
                contentSize.Height));
        }

        return finalSize;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PositionProperty
            || change.Property == OffsetProperty
            || change.Property == PaddingProperty)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    private static Point ResolveOrigin(
        Size availableSize,
        Size outerSize,
        Thickness offset,
        AsterGraphPanelPosition position)
    {
        var x = position is AsterGraphPanelPosition.TopRight
            or AsterGraphPanelPosition.CenterRight
            or AsterGraphPanelPosition.BottomRight
            ? availableSize.Width - outerSize.Width - offset.Right
            : position is AsterGraphPanelPosition.TopCenter
                or AsterGraphPanelPosition.Center
                or AsterGraphPanelPosition.BottomCenter
                ? (availableSize.Width - outerSize.Width) / 2d
                : offset.Left;
        var y = position is AsterGraphPanelPosition.BottomLeft
            or AsterGraphPanelPosition.BottomCenter
            or AsterGraphPanelPosition.BottomRight
            ? availableSize.Height - outerSize.Height - offset.Bottom
            : position is AsterGraphPanelPosition.CenterLeft
                or AsterGraphPanelPosition.Center
                or AsterGraphPanelPosition.CenterRight
                ? (availableSize.Height - outerSize.Height) / 2d
                : offset.Top;

        return new Point(
            Math.Max(0d, x),
            Math.Max(0d, y));
    }

    private static Thickness Normalize(Thickness thickness)
        => new(
            NormalizeLength(thickness.Left),
            NormalizeLength(thickness.Top),
            NormalizeLength(thickness.Right),
            NormalizeLength(thickness.Bottom));

    private static double ResolveAvailableLength(double length)
        => double.IsFinite(length) ? Math.Max(0d, length) : 0d;

    private static double ResolveContentAvailableLength(double length, double reserved)
        => double.IsFinite(length) ? Math.Max(0d, length - reserved) : double.PositiveInfinity;

    private static double NormalizeLength(double length)
        => double.IsFinite(length) ? Math.Max(0d, length) : 0d;
}

/// <summary>
/// Defines stable overlay positions for <see cref="AsterGraphPanel" />.
/// </summary>
public enum AsterGraphPanelPosition
{
    /// <summary>
    /// Top-left overlay placement.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top-center overlay placement.
    /// </summary>
    TopCenter,

    /// <summary>
    /// Top-right overlay placement.
    /// </summary>
    TopRight,

    /// <summary>
    /// Center-left overlay placement.
    /// </summary>
    CenterLeft,

    /// <summary>
    /// Center overlay placement.
    /// </summary>
    Center,

    /// <summary>
    /// Center-right overlay placement.
    /// </summary>
    CenterRight,

    /// <summary>
    /// Bottom-left overlay placement.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom-center overlay placement.
    /// </summary>
    BottomCenter,

    /// <summary>
    /// Bottom-right overlay placement.
    /// </summary>
    BottomRight,
}
