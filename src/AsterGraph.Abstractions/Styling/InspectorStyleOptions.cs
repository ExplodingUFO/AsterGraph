namespace AsterGraph.Abstractions.Styling;

public sealed record InspectorStyleOptions
{
    public string CardBackgroundHex { get; init; } = "#0F1825";

    public string InputBackgroundHex { get; init; } = "#0C1520";

    public string InputBorderHex { get; init; } = "#2A465D";

    public string ValidationErrorHex { get; init; } = "#FF9B9B";

    public double CardCornerRadius { get; init; } = 14;
}
