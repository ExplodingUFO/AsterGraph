namespace AsterGraph.Abstractions.Styling;

public sealed record InspectorStyleOptions
{
    public string CardBackgroundHex { get; init; } = "#0F1825";

    public string InputBackgroundHex { get; init; } = "#0C1520";

    public string InputBorderHex { get; init; } = "#2A465D";

    public string ValidationErrorHex { get; init; } = "#FF9B9B";

    public double TitleFontSize { get; init; } = 24;

    public double BodyFontSize { get; init; } = 13;

    public double CaptionFontSize { get; init; } = 11;

    public double SectionCornerRadius { get; init; } = 18;

    public double SectionPadding { get; init; } = 14;

    public double SectionSpacing { get; init; } = 12;

    public double CardCornerRadius { get; init; } = 14;
}
