namespace AsterGraph.Abstractions.Styling;

/// <summary>
/// Framework-neutral canvas styling tokens for the main graph surface.
/// </summary>
public sealed record CanvasStyleOptions
{
    /// <summary>
    /// Frame or viewport background color.
    /// </summary>
    public string FrameBackgroundHex { get; init; } = "#0D1622";

    /// <summary>
    /// Base grid background color.
    /// </summary>
    public string GridBackgroundHex { get; init; } = "#08121D";

    /// <summary>
    /// Color of the primary grid lines.
    /// </summary>
    public string PrimaryGridHex { get; init; } = "#24445B";

    /// <summary>
    /// Color of the secondary grid lines.
    /// </summary>
    public string SecondaryGridHex { get; init; } = "#3F6A89";

    /// <summary>
    /// Opacity of primary grid lines.
    /// </summary>
    public double PrimaryGridOpacity { get; init; } = 0.38;

    /// <summary>
    /// Opacity of secondary grid lines.
    /// </summary>
    public double SecondaryGridOpacity { get; init; } = 0.44;

    /// <summary>
    /// Spacing between primary grid lines.
    /// </summary>
    public double PrimaryGridSpacing { get; init; } = 48;

    /// <summary>
    /// Spacing between secondary grid lines.
    /// </summary>
    public double SecondaryGridSpacing { get; init; } = 192;

    /// <summary>
    /// Whether node dragging should snap to the grid.
    /// </summary>
    public bool EnableGridSnapping { get; init; }

    /// <summary>
    /// Whether alignment guides should be shown during drag.
    /// </summary>
    public bool EnableAlignmentGuides { get; init; }

    /// <summary>
    /// Tolerance used when evaluating snap targets.
    /// </summary>
    public double SnapTolerance { get; init; } = 14;

    /// <summary>
    /// Color of alignment guides.
    /// </summary>
    public string GuideHex { get; init; } = "#9EF6C9";

    /// <summary>
    /// Opacity of alignment guides.
    /// </summary>
    public double GuideOpacity { get; init; } = 0.9;

    /// <summary>
    /// Thickness of alignment guides.
    /// </summary>
    public double GuideThickness { get; init; } = 1.2;

    /// <summary>
    /// Border color of the marquee selection rectangle.
    /// </summary>
    public string SelectionBorderHex { get; init; } = "#7FE7D7";

    /// <summary>
    /// Fill color of the marquee selection rectangle.
    /// </summary>
    public string SelectionFillHex { get; init; } = "#7FE7D7";

    /// <summary>
    /// Fill opacity of the marquee selection rectangle.
    /// </summary>
    public double SelectionFillOpacity { get; init; } = 0.16;

    /// <summary>
    /// Border thickness of the marquee selection rectangle.
    /// </summary>
    public double SelectionBorderThickness { get; init; } = 1.4;

    /// <summary>
    /// Corner radius of the marquee selection rectangle.
    /// </summary>
    public double SelectionCornerRadius { get; init; } = 8;
}
