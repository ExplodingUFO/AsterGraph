namespace AsterGraph.Abstractions.Styling;

public sealed record ShellStyleOptions
{
    public string BackgroundStartHex { get; init; } = "#09111A";

    public string BackgroundMidHex { get; init; } = "#0C1825";

    public string BackgroundEndHex { get; init; } = "#0A121C";

    public string PanelBackgroundHex { get; init; } = "#111C29";

    public string PanelBorderHex { get; init; } = "#22354A";

    public string BadgeBackgroundHex { get; init; } = "#132231";

    public string HeadlineHex { get; init; } = "#F4FBFF";

    public string BodyHex { get; init; } = "#C6D7E3";

    public string EyebrowHex { get; init; } = "#7FA6C2";

    public string HighlightHex { get; init; } = "#7FE7D7";

    public double LibraryPanelWidth { get; init; } = 280;

    public double InspectorPanelWidth { get; init; } = 340;

    public double EyebrowFontSize { get; init; } = 11;

    public double TitleFontSize { get; init; } = 30;

    public double BodyFontSize { get; init; } = 13;

    public double DefaultCornerRadius { get; init; } = 24;

    public double DefaultSpacing { get; init; } = 16;
}
