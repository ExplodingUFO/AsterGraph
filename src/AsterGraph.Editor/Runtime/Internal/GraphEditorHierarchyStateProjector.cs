using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorHierarchyStateProjector
{
    internal static GraphEditorHierarchyStateSnapshot Create(
        GraphEditorScopeNavigationSnapshot scopeNavigation,
        string? parentCompositeNodeId,
        IReadOnlyList<GraphEditorCompositeNodeSnapshot> compositeNodes,
        IReadOnlyList<GraphEditorNodeGroupSnapshot> nodeGroups,
        IReadOnlyList<GraphNode> nodes,
        bool allowGroupMovement)
    {
        ArgumentNullException.ThrowIfNull(scopeNavigation);
        ArgumentNullException.ThrowIfNull(compositeNodes);
        ArgumentNullException.ThrowIfNull(nodeGroups);
        ArgumentNullException.ThrowIfNull(nodes);

        var collapsedGroupIds = nodeGroups
            .Where(group => group.IsCollapsed)
            .Select(group => group.Id)
            .ToHashSet(StringComparer.Ordinal);

        return new GraphEditorHierarchyStateSnapshot(
            scopeNavigation,
            parentCompositeNodeId,
            compositeNodes.ToList(),
            nodeGroups.ToList(),
            nodes.Select(node => CreateNodeSnapshot(node, collapsedGroupIds)).ToList(),
            new GraphEditorGroupMoveConstraintsSnapshot(
                CanMoveFrameIndependently: allowGroupMovement,
                CanMoveFrameWithMembers: allowGroupMovement));
    }

    private static GraphEditorHierarchyNodeSnapshot CreateNodeSnapshot(
        GraphNode node,
        IReadOnlySet<string> collapsedGroupIds)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(collapsedGroupIds);

        var parentGroupId = node.Surface?.GroupId;
        var collapsedByGroupId = !string.IsNullOrWhiteSpace(parentGroupId) && collapsedGroupIds.Contains(parentGroupId)
            ? parentGroupId
            : null;

        return new GraphEditorHierarchyNodeSnapshot(
            node.Id,
            parentGroupId,
            collapsedByGroupId,
            collapsedByGroupId is null);
    }
}
