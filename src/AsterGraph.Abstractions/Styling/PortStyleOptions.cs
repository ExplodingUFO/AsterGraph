namespace AsterGraph.Abstractions.Styling;

public sealed record PortStyleOptions
{
    public double DotSize { get; init; } = 10;

    public double RowSpacing { get; init; } = 8;

    public double TextSpacing { get; init; } = 1;

    public double LabelFontSize { get; init; } = 12;

    public double TypeFontSize { get; init; } = 10;

    public string LabelHex { get; init; } = "#EFF8FF";

    public string TypeHex { get; init; } = "#9DB8CB";

    public double TypeOpacity { get; init; } = 0.72;
}
