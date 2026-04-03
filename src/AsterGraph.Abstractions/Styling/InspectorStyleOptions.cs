namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral styling tokens for the inspector surface.
/// </summary>
public sealed record InspectorStyleOptions
{
    /// <summary>
    /// Background color for inspector cards.
    /// </summary>
    public string CardBackgroundHex { get; init; } = "#0F1825";

    /// <summary>
    /// Background color for text inputs and editors.
    /// </summary>
    public string InputBackgroundHex { get; init; } = "#0C1520";

    /// <summary>
    /// Border color for text inputs and editors.
    /// </summary>
    public string InputBorderHex { get; init; } = "#2A465D";

    /// <summary>
    /// Color used for validation failures.
    /// </summary>
    public string ValidationErrorHex { get; init; } = "#FF9B9B";

    /// <summary>
    /// Font size for inspector titles.
    /// </summary>
    public double TitleFontSize { get; init; } = 24;

    /// <summary>
    /// Base body font size.
    /// </summary>
    public double BodyFontSize { get; init; } = 13;

    /// <summary>
    /// Caption font size.
    /// </summary>
    public double CaptionFontSize { get; init; } = 11;

    /// <summary>
    /// Corner radius of inspector sections.
    /// </summary>
    public double SectionCornerRadius { get; init; } = 18;

    /// <summary>
    /// Internal padding for inspector sections.
    /// </summary>
    public double SectionPadding { get; init; } = 14;

    /// <summary>
    /// Spacing between inspector sections.
    /// </summary>
    public double SectionSpacing { get; init; } = 12;

    /// <summary>
    /// Corner radius of inspector cards.
    /// </summary>
    public double CardCornerRadius { get; init; } = 14;
}
