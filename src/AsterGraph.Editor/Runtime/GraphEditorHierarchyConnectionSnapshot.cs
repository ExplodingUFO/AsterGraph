namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Canonical active-scope hierarchy state for one connection.
/// </summary>
/// <param name="ConnectionId">Stable connection identifier.</param>
/// <param name="SourceNodeId">Source node identifier.</param>
/// <param name="TargetNodeId">Target node identifier.</param>
/// <param name="SourceCollapsedByGroupId">Collapsed node-group currently hiding the source node, if any.</param>
/// <param name="TargetCollapsedByGroupId">Collapsed node-group currently hiding the target node, if any.</param>
/// <param name="IsInternalToCollapsedGroup">Whether both endpoints are hidden by the same collapsed group.</param>
/// <param name="IsCrossingCollapsedGroupBoundary">Whether exactly one endpoint, or two different endpoints, are hidden by collapsed groups.</param>
/// <param name="IsVisibleInActiveScope">Whether the connection should remain visible in the active scope projection.</param>
public sealed record GraphEditorHierarchyConnectionSnapshot(
    string ConnectionId,
    string SourceNodeId,
    string TargetNodeId,
    string? SourceCollapsedByGroupId,
    string? TargetCollapsedByGroupId,
    bool IsInternalToCollapsedGroup,
    bool IsCrossingCollapsedGroupBoundary,
    bool IsVisibleInActiveScope);
