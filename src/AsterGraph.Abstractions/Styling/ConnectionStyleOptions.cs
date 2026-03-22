namespace AsterGraph.Abstractions.Styling;

public sealed record ConnectionStyleOptions
{
    public double Thickness { get; init; } = 3.2;

    public double PreviewThickness { get; init; } = 2.4;

    public double StrokeOpacity { get; init; } = 0.84;

    public double PreviewStrokeOpacity { get; init; } = 0.52;

    public string LabelBackgroundHex { get; init; } = "#0F1825";

    public double LabelBackgroundOpacity { get; init; } = 0.95;

    public double LabelBorderThickness { get; init; } = 1;

    public double LabelCornerRadius { get; init; } = 999;

    public double LabelHorizontalPadding { get; init; } = 10;

    public double LabelVerticalPadding { get; init; } = 4;

    public double LabelFontSize { get; init; } = 10;

    public double LabelOffsetX { get; init; } = 8;

    public double LabelOffsetY { get; init; } = -12;

    public double LabelBorderOpacity { get; init; } = 0.42;

    public string LabelForegroundHex { get; init; } = "#EAF5FD";

    public double LabelForegroundOpacity { get; init; } = 0.82;
}
