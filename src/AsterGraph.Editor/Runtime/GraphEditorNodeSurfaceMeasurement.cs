using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Measured node-surface thresholds used by hosted UI layers to keep resizing, disclosure, and rendering aligned.
/// </summary>
public sealed record GraphEditorNodeSurfaceMeasurement(
    GraphSize BaselineSize,
    double HeightToRevealAdditionalInputs,
    double WidthToRevealParameterSummaries,
    double WidthToRevealInlineEditors,
    int RequiredParameterCount,
    int OptionalParameterCount,
    bool SupportsParameterSummaries,
    bool SupportsInlineEditors)
{
    public static GraphEditorNodeSurfaceMeasurement Default { get; } = new(
        new GraphSize(180d, 158d),
        158d,
        320d,
        420d,
        0,
        0,
        SupportsParameterSummaries: false,
        SupportsInlineEditors: false);
}
