namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted node-surface state that is independent from any UI toolkit.
/// </summary>
/// <param name="ExpansionState">Current persisted card expansion state.</param>
/// <param name="GroupId">Optional editor-only group attachment identifier.</param>
/// <param name="RotationDegrees">Clockwise node-card rotation in degrees, normalized to the 0-360 range by mutation paths.</param>
public sealed record GraphNodeSurfaceState(
    GraphNodeExpansionState ExpansionState = GraphNodeExpansionState.Collapsed,
    string? GroupId = null,
    double RotationDegrees = 0d)
{
    /// <summary>
    /// Default collapsed node surface state with no group attachment.
    /// </summary>
    public static GraphNodeSurfaceState Default { get; } = new();

    /// <summary>
    /// Returns this state with a finite normalized rotation angle.
    /// </summary>
    public GraphNodeSurfaceState NormalizeRotation()
        => this with { RotationDegrees = NormalizeRotationDegrees(RotationDegrees) };

    /// <summary>
    /// Normalizes any finite angle to the clockwise 0-360 degree range.
    /// </summary>
    public static double NormalizeRotationDegrees(double rotationDegrees)
    {
        if (double.IsNaN(rotationDegrees) || double.IsInfinity(rotationDegrees))
        {
            return 0d;
        }

        var normalized = rotationDegrees % 360d;
        if (normalized < 0d)
        {
            normalized += 360d;
        }

        return Math.Abs(normalized - 360d) < 0.0001d || Math.Abs(normalized) < 0.0001d
            ? 0d
            : normalized;
    }
}
