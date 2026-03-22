namespace AsterGraph.Abstractions.Styling;

public sealed record NodeCardStyleOptions
{
    public string BackgroundHex { get; init; } = "#0E1724";

    public string SelectedBackgroundHex { get; init; } = "#162435";

    public string BorderHex { get; init; } = "#233547";

    public string CategoryTextHex { get; init; } = "#B4D6ED";

    public double CategoryTextOpacity { get; init; } = 0.62;

    public string TitleTextHex { get; init; } = "#F6FBFF";

    public string SubtitleTextHex { get; init; } = "#B8CFE0";

    public double SubtitleTextOpacity { get; init; } = 0.74;

    public string DescriptionTextHex { get; init; } = "#C5D7E5";

    public double DescriptionTextOpacity { get; init; } = 0.86;

    public double CornerRadius { get; init; } = 22;

    public double HeaderOpacity { get; init; } = 0.18;

    public double SelectedHeaderOpacity { get; init; } = 0.26;

    public double BorderThickness { get; init; } = 1.4;

    public double SelectedBorderThickness { get; init; } = 2.2;
}
