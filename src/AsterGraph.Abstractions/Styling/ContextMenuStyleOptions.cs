namespace AsterGraph.Abstractions.Styling;

public sealed record ContextMenuStyleOptions
{
    public string BackgroundHex { get; init; } = "#111C29";

    public string BorderHex { get; init; } = "#22354A";

    public string HoverHex { get; init; } = "#193045";

    public string DisabledForegroundHex { get; init; } = "#6A8091";

    public double CornerRadius { get; init; } = 10;
}
