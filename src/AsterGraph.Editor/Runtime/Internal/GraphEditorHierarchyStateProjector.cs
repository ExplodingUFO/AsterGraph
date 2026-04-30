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
        IReadOnlyList<GraphConnection> connections,
        bool allowGroupMovement)
    {
        ArgumentNullException.ThrowIfNull(scopeNavigation);
        ArgumentNullException.ThrowIfNull(compositeNodes);
        ArgumentNullException.ThrowIfNull(nodeGroups);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(connections);

        var collapsedGroupIds = nodeGroups
            .Where(group => group.IsCollapsed)
            .Select(group => group.Id)
            .ToHashSet(StringComparer.Ordinal);
        var nodeCollapseById = nodes.ToDictionary(
            node => node.Id,
            node => ResolveCollapsedGroupId(node, collapsedGroupIds),
            StringComparer.Ordinal);

        return new GraphEditorHierarchyStateSnapshot(
            scopeNavigation,
            parentCompositeNodeId,
            compositeNodes.ToList(),
            nodeGroups.ToList(),
            nodes.Select(node => CreateNodeSnapshot(node, nodeCollapseById)).ToList(),
            connections.Select(connection => CreateConnectionSnapshot(connection, nodeCollapseById)).ToList(),
            new GraphEditorGroupMoveConstraintsSnapshot(
                CanMoveFrameIndependently: allowGroupMovement,
                CanMoveFrameWithMembers: allowGroupMovement));
    }

    private static GraphEditorHierarchyNodeSnapshot CreateNodeSnapshot(
        GraphNode node,
        IReadOnlyDictionary<string, string?> nodeCollapseById)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(nodeCollapseById);

        var parentGroupId = node.Surface?.GroupId;
        nodeCollapseById.TryGetValue(node.Id, out var collapsedByGroupId);

        return new GraphEditorHierarchyNodeSnapshot(
            node.Id,
            parentGroupId,
            collapsedByGroupId,
            collapsedByGroupId is null);
    }

    private static GraphEditorHierarchyConnectionSnapshot CreateConnectionSnapshot(
        GraphConnection connection,
        IReadOnlyDictionary<string, string?> nodeCollapseById)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(nodeCollapseById);

        nodeCollapseById.TryGetValue(connection.SourceNodeId, out var sourceCollapsedByGroupId);
        nodeCollapseById.TryGetValue(connection.TargetNodeId, out var targetCollapsedByGroupId);

        var sourceCollapsed = !string.IsNullOrWhiteSpace(sourceCollapsedByGroupId);
        var targetCollapsed = !string.IsNullOrWhiteSpace(targetCollapsedByGroupId);
        var isInternalToCollapsedGroup = sourceCollapsed
            && targetCollapsed
            && string.Equals(sourceCollapsedByGroupId, targetCollapsedByGroupId, StringComparison.Ordinal);
        var isCrossingCollapsedGroupBoundary = !isInternalToCollapsedGroup
            && (sourceCollapsed || targetCollapsed);

        return new GraphEditorHierarchyConnectionSnapshot(
            connection.Id,
            connection.SourceNodeId,
            connection.TargetNodeId,
            sourceCollapsedByGroupId,
            targetCollapsedByGroupId,
            isInternalToCollapsedGroup,
            isCrossingCollapsedGroupBoundary,
            !isInternalToCollapsedGroup);
    }

    private static string? ResolveCollapsedGroupId(
        GraphNode node,
        IReadOnlySet<string> collapsedGroupIds)
    {
        var parentGroupId = node.Surface?.GroupId;
        return !string.IsNullOrWhiteSpace(parentGroupId) && collapsedGroupIds.Contains(parentGroupId)
            ? parentGroupId
            : null;
    }
}
