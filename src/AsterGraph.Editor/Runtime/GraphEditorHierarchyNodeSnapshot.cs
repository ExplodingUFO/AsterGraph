namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Canonical active-scope hierarchy state for one node.
/// </summary>
/// <param name="NodeId">Stable node identifier.</param>
/// <param name="ParentGroupId">Owning node-group identifier, if any.</param>
/// <param name="CollapsedByGroupId">Collapsed node-group identifier that currently hides the node, if any.</param>
/// <param name="IsVisibleInActiveScope">Whether the node should be treated as directly visible in the active scope.</param>
public sealed record GraphEditorHierarchyNodeSnapshot(
    string NodeId,
    string? ParentGroupId,
    string? CollapsedByGroupId,
    bool IsVisibleInActiveScope);
