namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral styling tokens for node cards.
/// </summary>
public sealed record NodeCardStyleOptions
{
    /// <summary>
    /// Background color for the normal node state.
    /// </summary>
    public string BackgroundHex { get; init; } = "#0E1724";

    /// <summary>
    /// Background color for the selected node state.
    /// </summary>
    public string SelectedBackgroundHex { get; init; } = "#162435";

    /// <summary>
    /// Border color for the normal node state.
    /// </summary>
    public string BorderHex { get; init; } = "#233547";

    /// <summary>
    /// Foreground color for the category text.
    /// </summary>
    public string CategoryTextHex { get; init; } = "#B4D6ED";

    /// <summary>
    /// Opacity of the category text.
    /// </summary>
    public double CategoryTextOpacity { get; init; } = 0.62;

    /// <summary>
    /// Font size of the category text.
    /// </summary>
    public double CategoryFontSize { get; init; } = 11;

    /// <summary>
    /// Foreground color for the title text.
    /// </summary>
    public string TitleTextHex { get; init; } = "#F6FBFF";

    /// <summary>
    /// Font size of the title text.
    /// </summary>
    public double TitleFontSize { get; init; } = 18;

    /// <summary>
    /// Foreground color for the subtitle text.
    /// </summary>
    public string SubtitleTextHex { get; init; } = "#B8CFE0";

    /// <summary>
    /// Opacity of the subtitle text.
    /// </summary>
    public double SubtitleTextOpacity { get; init; } = 0.74;

    /// <summary>
    /// Font size of the subtitle text.
    /// </summary>
    public double SubtitleFontSize { get; init; } = 12;

    /// <summary>
    /// Foreground color for the description text.
    /// </summary>
    public string DescriptionTextHex { get; init; } = "#C5D7E5";

    /// <summary>
    /// Opacity of the description text.
    /// </summary>
    public double DescriptionTextOpacity { get; init; } = 0.86;

    /// <summary>
    /// Font size of the description text.
    /// </summary>
    public double DescriptionFontSize { get; init; } = 12;

    /// <summary>
    /// Maximum rendered height of the description block.
    /// </summary>
    public double DescriptionMaxHeight { get; init; } = 40;

    /// <summary>
    /// Horizontal padding inside the header area.
    /// </summary>
    public double HeaderHorizontalPadding { get; init; } = 16;

    /// <summary>
    /// Top padding inside the header area.
    /// </summary>
    public double HeaderTopPadding { get; init; } = 14;

    /// <summary>
    /// Bottom padding inside the header area.
    /// </summary>
    public double HeaderBottomPadding { get; init; } = 12;

    /// <summary>
    /// Vertical spacing between header elements.
    /// </summary>
    public double HeaderSpacing { get; init; } = 4;

    /// <summary>
    /// Horizontal padding inside the body area.
    /// </summary>
    public double BodyHorizontalPadding { get; init; } = 16;

    /// <summary>
    /// Top padding inside the body area.
    /// </summary>
    public double BodyTopPadding { get; init; } = 12;

    /// <summary>
    /// Bottom padding inside the body area.
    /// </summary>
    public double BodyBottomPadding { get; init; } = 16;

    /// <summary>
    /// Spacing between body columns.
    /// </summary>
    public double BodyColumnSpacing { get; init; } = 14;

    /// <summary>
    /// Spacing between body rows.
    /// </summary>
    public double BodyRowSpacing { get; init; } = 12;

    /// <summary>
    /// Corner radius of the node card.
    /// </summary>
    public double CornerRadius { get; init; } = 22;

    /// <summary>
    /// Opacity of the header background in the normal state.
    /// </summary>
    public double HeaderOpacity { get; init; } = 0.18;

    /// <summary>
    /// Opacity of the header background in the selected state.
    /// </summary>
    public double SelectedHeaderOpacity { get; init; } = 0.26;

    /// <summary>
    /// Border thickness of the normal node card.
    /// </summary>
    public double BorderThickness { get; init; } = 1.4;

    /// <summary>
    /// Border thickness of the selected node card.
    /// </summary>
    public double SelectedBorderThickness { get; init; } = 2.2;
}
