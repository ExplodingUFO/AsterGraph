namespace AsterGraph.Editor.Services;

/// <summary>
/// Describes optional raster scene export shaping.
/// </summary>
public sealed record GraphEditorSceneImageExportOptions
{
    /// <summary>
    /// Optional export scale multiplier applied during rasterization.
    /// </summary>
    public double Scale { get; init; } = 1d;

    /// <summary>
    /// Optional encoded image quality. Applies to JPEG output and is ignored by PNG.
    /// </summary>
    public int Quality { get; init; } = 92;

    /// <summary>
    /// Optional background fill override expressed as <c>#RRGGBB</c> or <c>#AARRGGBB</c>.
    /// </summary>
    public string? BackgroundHex { get; init; }
}
