namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral styling tokens for the full editor shell.
/// </summary>
public sealed record ShellStyleOptions
{
    /// <summary>
    /// Gradient start color for the shell background.
    /// </summary>
    public string BackgroundStartHex { get; init; } = "#09111A";

    /// <summary>
    /// Gradient middle color for the shell background.
    /// </summary>
    public string BackgroundMidHex { get; init; } = "#0C1825";

    /// <summary>
    /// Gradient end color for the shell background.
    /// </summary>
    public string BackgroundEndHex { get; init; } = "#0A121C";

    /// <summary>
    /// Panel background color.
    /// </summary>
    public string PanelBackgroundHex { get; init; } = "#111C29";

    /// <summary>
    /// Panel border color.
    /// </summary>
    public string PanelBorderHex { get; init; } = "#22354A";

    /// <summary>
    /// Background color used for badges and small emphasis chips.
    /// </summary>
    public string BadgeBackgroundHex { get; init; } = "#132231";

    /// <summary>
    /// Foreground color for large headings.
    /// </summary>
    public string HeadlineHex { get; init; } = "#F4FBFF";

    /// <summary>
    /// Foreground color for body text.
    /// </summary>
    public string BodyHex { get; init; } = "#C6D7E3";

    /// <summary>
    /// Foreground color for eyebrow or supporting text.
    /// </summary>
    public string EyebrowHex { get; init; } = "#7FA6C2";

    /// <summary>
    /// Highlight or accent color used across the shell.
    /// </summary>
    public string HighlightHex { get; init; } = "#7FE7D7";

    /// <summary>
    /// Preferred width of the library panel.
    /// </summary>
    public double LibraryPanelWidth { get; init; } = 280;

    /// <summary>
    /// Preferred width of the inspector panel.
    /// </summary>
    public double InspectorPanelWidth { get; init; } = 340;

    /// <summary>
    /// Font size for eyebrow text.
    /// </summary>
    public double EyebrowFontSize { get; init; } = 11;

    /// <summary>
    /// Font size for main shell titles.
    /// </summary>
    public double TitleFontSize { get; init; } = 30;

    /// <summary>
    /// Base body font size.
    /// </summary>
    public double BodyFontSize { get; init; } = 13;

    /// <summary>
    /// Default corner radius used by shell surfaces.
    /// </summary>
    public double DefaultCornerRadius { get; init; } = 24;

    /// <summary>
    /// Default spacing unit used by shell layout.
    /// </summary>
    public double DefaultSpacing { get; init; } = 16;
}
