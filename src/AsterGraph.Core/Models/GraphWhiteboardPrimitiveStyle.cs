namespace AsterGraph.Core.Models;

/// <summary>
/// Internal renderer-neutral style metadata for a future whiteboard primitive.
/// </summary>
internal sealed record GraphWhiteboardPrimitiveStyle
{
    private double _strokeThickness;
    private double _opacity;

    /// <summary>
    /// Creates primitive style metadata.
    /// </summary>
    public GraphWhiteboardPrimitiveStyle(
        string? FillHex,
        string? StrokeHex,
        double StrokeThickness,
        double Opacity = 1d)
    {
        this.FillHex = FillHex;
        this.StrokeHex = StrokeHex;
        this.StrokeThickness = StrokeThickness;
        this.Opacity = Opacity;
    }

    /// <summary>
    /// Default transparent-fill dark-stroke primitive style.
    /// </summary>
    public static GraphWhiteboardPrimitiveStyle Default { get; } = new(
        FillHex: null,
        StrokeHex: "#1A1F2E",
        StrokeThickness: 1d);

    /// <summary>
    /// Optional fill color in hex form.
    /// </summary>
    public string? FillHex { get; init; }

    /// <summary>
    /// Optional stroke color in hex form.
    /// </summary>
    public string? StrokeHex { get; init; }

    /// <summary>
    /// Non-negative stroke thickness in graph world units.
    /// </summary>
    public double StrokeThickness
    {
        get => _strokeThickness;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Stroke thickness must be a non-negative finite number.");
            }

            _strokeThickness = value;
        }
    }

    /// <summary>
    /// Opacity from fully transparent 0 to fully opaque 1.
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0d || value > 1d)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Opacity must be a finite number from 0 to 1.");
            }

            _opacity = value;
        }
    }
}
