namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing batch membership change for editor-only node groups.
/// </summary>
/// <param name="NodeId">Stable node identifier.</param>
/// <param name="GroupId">Target group identifier, or <see langword="null"/> to detach the node from any group.</param>
public sealed record GraphEditorNodeGroupMembershipChange(
    string NodeId,
    string? GroupId);
