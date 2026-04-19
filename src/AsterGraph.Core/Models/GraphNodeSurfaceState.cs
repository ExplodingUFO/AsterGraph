namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted node-surface state that is independent from any UI toolkit.
/// </summary>
/// <param name="ExpansionState">Current persisted card expansion state.</param>
/// <param name="GroupId">Optional editor-only group attachment identifier.</param>
public sealed record GraphNodeSurfaceState(
    GraphNodeExpansionState ExpansionState = GraphNodeExpansionState.Collapsed,
    string? GroupId = null)
{
    /// <summary>
    /// Default collapsed node surface state with no group attachment.
    /// </summary>
    public static GraphNodeSurfaceState Default { get; } = new();
}
