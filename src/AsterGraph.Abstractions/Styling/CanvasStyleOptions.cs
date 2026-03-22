namespace AsterGraph.Abstractions.Styling;

public sealed record CanvasStyleOptions
{
    public string FrameBackgroundHex { get; init; } = "#0D1622";

    public string GridBackgroundHex { get; init; } = "#08121D";

    public string PrimaryGridHex { get; init; } = "#24445B";

    public string SecondaryGridHex { get; init; } = "#3F6A89";

    public double PrimaryGridOpacity { get; init; } = 0.38;

    public double SecondaryGridOpacity { get; init; } = 0.44;

    public double PrimaryGridSpacing { get; init; } = 48;

    public double SecondaryGridSpacing { get; init; } = 192;

    public string SelectionBorderHex { get; init; } = "#7FE7D7";

    public string SelectionFillHex { get; init; } = "#7FE7D7";

    public double SelectionFillOpacity { get; init; } = 0.16;

    public double SelectionBorderThickness { get; init; } = 1.4;

    public double SelectionCornerRadius { get; init; } = 8;
}
