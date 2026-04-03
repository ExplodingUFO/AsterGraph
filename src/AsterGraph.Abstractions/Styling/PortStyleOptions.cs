namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral styling tokens for port rows.
/// </summary>
public sealed record PortStyleOptions
{
    /// <summary>
    /// Diameter of the rendered port dot.
    /// </summary>
    public double DotSize { get; init; } = 10;

    /// <summary>
    /// Vertical spacing between port rows.
    /// </summary>
    public double RowSpacing { get; init; } = 8;

    /// <summary>
    /// Spacing between label and type text.
    /// </summary>
    public double TextSpacing { get; init; } = 1;

    /// <summary>
    /// Font size of the port label.
    /// </summary>
    public double LabelFontSize { get; init; } = 12;

    /// <summary>
    /// Font size of the port type caption.
    /// </summary>
    public double TypeFontSize { get; init; } = 10;

    /// <summary>
    /// Foreground color of the port label.
    /// </summary>
    public string LabelHex { get; init; } = "#EFF8FF";

    /// <summary>
    /// Foreground color of the port type caption.
    /// </summary>
    public string TypeHex { get; init; } = "#9DB8CB";

    /// <summary>
    /// Opacity of the port type caption.
    /// </summary>
    public double TypeOpacity { get; init; } = 0.72;
}
