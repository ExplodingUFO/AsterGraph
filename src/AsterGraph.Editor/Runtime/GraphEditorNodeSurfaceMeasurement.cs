using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Measured node-surface thresholds used by hosted UI layers to keep resizing, disclosure, and rendering aligned.
/// </summary>
public sealed record GraphEditorNodeSurfaceMeasurement(
    GraphSize BaselineSize,
    double HeightToRevealAdditionalInputs,
    double WidthToRevealInputSummaries,
    double WidthToRevealInputEditors,
    int RequiredParameterCount,
    int OptionalParameterCount,
    bool SupportsInputSummaries,
    bool SupportsInputEditors)
{
    public static GraphEditorNodeSurfaceMeasurement Default { get; } = new(
        new GraphSize(180d, 158d),
        158d,
        320d,
        420d,
        0,
        0,
        SupportsInputSummaries: false,
        SupportsInputEditors: false);
}
