namespace AsterGraph.Abstractions.Styling;

public sealed record ContextMenuStyleOptions
{
    public string BackgroundHex { get; init; } = "#111C29";

    public string BorderHex { get; init; } = "#22354A";

    public string ForegroundHex { get; init; } = "#F4FBFF";

    public string HoverHex { get; init; } = "#193045";

    public string DisabledForegroundHex { get; init; } = "#6A8091";

    public string SeparatorHex { get; init; } = "#2A465D";

    public double BorderThickness { get; init; } = 1;

    public double CornerRadius { get; init; } = 10;

    public double ItemCornerRadius { get; init; } = 8;

    public double ItemHorizontalPadding { get; init; } = 12;

    public double ItemVerticalPadding { get; init; } = 8;

    public double ItemFontSize { get; init; } = 12;

    public double ItemMinWidth { get; init; } = 220;

    public double IconFontSize { get; init; } = 11;
}
