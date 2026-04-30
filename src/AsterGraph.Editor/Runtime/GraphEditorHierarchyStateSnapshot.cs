using System.Collections.Generic;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Canonical active-scope hierarchy state projected for host-side authoring surfaces.
/// </summary>
/// <param name="ScopeNavigation">Current active-scope navigation snapshot.</param>
/// <param name="ParentCompositeNodeId">Composite shell node that owns the active scope, if the current scope is nested.</param>
/// <param name="CompositeNodes">Composite shells present in the active scope.</param>
/// <param name="NodeGroups">Resolved node-group snapshots present in the active scope.</param>
/// <param name="Nodes">Per-node hierarchy state for the active scope.</param>
/// <param name="Connections">Per-connection hierarchy state for collapse and boundary-edge projection.</param>
/// <param name="GroupMoveConstraints">Explicit group-movement constraints for the active scope.</param>
public sealed record GraphEditorHierarchyStateSnapshot(
    GraphEditorScopeNavigationSnapshot ScopeNavigation,
    string? ParentCompositeNodeId,
    IReadOnlyList<GraphEditorCompositeNodeSnapshot> CompositeNodes,
    IReadOnlyList<GraphEditorNodeGroupSnapshot> NodeGroups,
    IReadOnlyList<GraphEditorHierarchyNodeSnapshot> Nodes,
    IReadOnlyList<GraphEditorHierarchyConnectionSnapshot> Connections,
    GraphEditorGroupMoveConstraintsSnapshot GroupMoveConstraints);
