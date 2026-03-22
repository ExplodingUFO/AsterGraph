namespace AsterGraph.Abstractions.Styling;

public sealed record NodeCardStyleOptions
{
    public string BackgroundHex { get; init; } = "#0E1724";

    public string SelectedBackgroundHex { get; init; } = "#162435";

    public string BorderHex { get; init; } = "#233547";

    public string CategoryTextHex { get; init; } = "#B4D6ED";

    public double CategoryTextOpacity { get; init; } = 0.62;

    public double CategoryFontSize { get; init; } = 11;

    public string TitleTextHex { get; init; } = "#F6FBFF";

    public double TitleFontSize { get; init; } = 18;

    public string SubtitleTextHex { get; init; } = "#B8CFE0";

    public double SubtitleTextOpacity { get; init; } = 0.74;

    public double SubtitleFontSize { get; init; } = 12;

    public string DescriptionTextHex { get; init; } = "#C5D7E5";

    public double DescriptionTextOpacity { get; init; } = 0.86;

    public double DescriptionFontSize { get; init; } = 12;

    public double DescriptionMaxHeight { get; init; } = 40;

    public double HeaderHorizontalPadding { get; init; } = 16;

    public double HeaderTopPadding { get; init; } = 14;

    public double HeaderBottomPadding { get; init; } = 12;

    public double HeaderSpacing { get; init; } = 4;

    public double BodyHorizontalPadding { get; init; } = 16;

    public double BodyTopPadding { get; init; } = 12;

    public double BodyBottomPadding { get; init; } = 16;

    public double BodyColumnSpacing { get; init; } = 14;

    public double BodyRowSpacing { get; init; } = 12;

    public double CornerRadius { get; init; } = 22;

    public double HeaderOpacity { get; init; } = 0.18;

    public double SelectedHeaderOpacity { get; init; } = 0.26;

    public double BorderThickness { get; init; } = 1.4;

    public double SelectedBorderThickness { get; init; } = 2.2;
}
