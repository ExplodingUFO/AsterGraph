namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral styling tokens for context menu presentation.
/// </summary>
public sealed record ContextMenuStyleOptions
{
    /// <summary>
    /// Menu background color.
    /// </summary>
    public string BackgroundHex { get; init; } = "#111C29";

    /// <summary>
    /// Menu border color.
    /// </summary>
    public string BorderHex { get; init; } = "#22354A";

    /// <summary>
    /// Foreground color for enabled menu items.
    /// </summary>
    public string ForegroundHex { get; init; } = "#F4FBFF";

    /// <summary>
    /// Hover background color for menu items.
    /// </summary>
    public string HoverHex { get; init; } = "#193045";

    /// <summary>
    /// Foreground color for disabled menu items.
    /// </summary>
    public string DisabledForegroundHex { get; init; } = "#6A8091";

    /// <summary>
    /// Separator line color.
    /// </summary>
    public string SeparatorHex { get; init; } = "#2A465D";

    /// <summary>
    /// Menu border thickness.
    /// </summary>
    public double BorderThickness { get; init; } = 1;

    /// <summary>
    /// Menu corner radius.
    /// </summary>
    public double CornerRadius { get; init; } = 10;

    /// <summary>
    /// Corner radius for individual menu items.
    /// </summary>
    public double ItemCornerRadius { get; init; } = 8;

    /// <summary>
    /// Horizontal padding for menu items.
    /// </summary>
    public double ItemHorizontalPadding { get; init; } = 12;

    /// <summary>
    /// Vertical padding for menu items.
    /// </summary>
    public double ItemVerticalPadding { get; init; } = 8;

    /// <summary>
    /// Font size for menu item labels.
    /// </summary>
    public double ItemFontSize { get; init; } = 12;

    /// <summary>
    /// Minimum width of the menu surface.
    /// </summary>
    public double ItemMinWidth { get; init; } = 220;

    /// <summary>
    /// Font size used for icon glyphs.
    /// </summary>
    public double IconFontSize { get; init; } = 11;
}
