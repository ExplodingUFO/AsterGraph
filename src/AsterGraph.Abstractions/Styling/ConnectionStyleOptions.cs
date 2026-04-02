namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral styling tokens for rendered graph connections.
/// </summary>
public sealed record ConnectionStyleOptions
{
    /// <summary>
    /// Stroke thickness for committed connections.
    /// </summary>
    public double Thickness { get; init; } = 3.2;

    /// <summary>
    /// Stroke thickness for preview connections.
    /// </summary>
    public double PreviewThickness { get; init; } = 2.4;

    /// <summary>
    /// Stroke opacity for committed connections.
    /// </summary>
    public double StrokeOpacity { get; init; } = 0.84;

    /// <summary>
    /// Stroke opacity for preview connections.
    /// </summary>
    public double PreviewStrokeOpacity { get; init; } = 0.52;

    /// <summary>
    /// Background color of conversion or label badges.
    /// </summary>
    public string LabelBackgroundHex { get; init; } = "#0F1825";

    /// <summary>
    /// Background opacity of conversion or label badges.
    /// </summary>
    public double LabelBackgroundOpacity { get; init; } = 0.95;

    /// <summary>
    /// Border thickness of conversion or label badges.
    /// </summary>
    public double LabelBorderThickness { get; init; } = 1;

    /// <summary>
    /// Corner radius of conversion or label badges.
    /// </summary>
    public double LabelCornerRadius { get; init; } = 999;

    /// <summary>
    /// Horizontal padding inside conversion or label badges.
    /// </summary>
    public double LabelHorizontalPadding { get; init; } = 10;

    /// <summary>
    /// Vertical padding inside conversion or label badges.
    /// </summary>
    public double LabelVerticalPadding { get; init; } = 4;

    /// <summary>
    /// Font size used for conversion or label badges.
    /// </summary>
    public double LabelFontSize { get; init; } = 10;

    /// <summary>
    /// Horizontal offset applied to the badge anchor.
    /// </summary>
    public double LabelOffsetX { get; init; } = 8;

    /// <summary>
    /// Vertical offset applied to the badge anchor.
    /// </summary>
    public double LabelOffsetY { get; init; } = -12;

    /// <summary>
    /// Border opacity of conversion or label badges.
    /// </summary>
    public double LabelBorderOpacity { get; init; } = 0.42;

    /// <summary>
    /// Foreground color of conversion or label badges.
    /// </summary>
    public string LabelForegroundHex { get; init; } = "#EAF5FD";

    /// <summary>
    /// Foreground opacity of conversion or label badges.
    /// </summary>
    public double LabelForegroundOpacity { get; init; } = 0.82;
}
