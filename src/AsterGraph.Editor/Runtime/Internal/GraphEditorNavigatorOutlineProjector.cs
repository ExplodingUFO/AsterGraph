using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorNavigatorOutlineProjector
{
    internal static GraphEditorNavigatorOutlineSnapshot Project(
        GraphDocument activeScopeDocument,
        GraphEditorHierarchyStateSnapshot hierarchy,
        GraphEditorSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(activeScopeDocument);
        ArgumentNullException.ThrowIfNull(hierarchy);
        ArgumentNullException.ThrowIfNull(selection);

        var selectedNodeIds = selection.SelectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var selectedConnectionIds = selection.SelectedConnectionIds.ToHashSet(StringComparer.Ordinal);
        var hierarchyNodesById = hierarchy.Nodes.ToDictionary(node => node.NodeId, StringComparer.Ordinal);
        var compositeChildScopeByNodeId = hierarchy.CompositeNodes.ToDictionary(
            composite => composite.NodeId,
            composite => composite.ChildGraphId,
            StringComparer.Ordinal);
        var nodesByGroupId = activeScopeDocument.Nodes
            .GroupBy(node => node.Surface?.GroupId ?? string.Empty, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        var items = new List<GraphEditorNavigatorOutlineItemSnapshot>
        {
            CreateScopeItem(activeScopeDocument, hierarchy.ScopeNavigation),
        };

        foreach (var group in hierarchy.NodeGroups.OrderBy(group => group.Title, StringComparer.Ordinal).ThenBy(group => group.Id, StringComparer.Ordinal))
        {
            var groupItemId = CreateItemId(GraphEditorNavigatorOutlineItemKind.Group, group.Id);
            items.Add(new GraphEditorNavigatorOutlineItemSnapshot(
                groupItemId,
                GraphEditorNavigatorOutlineItemKind.Group,
                group.Id,
                ParentItemId: items[0].Id,
                group.Title,
                $"{group.NodeIds.Count} nodes",
                Depth: 1,
                IsSelected: group.NodeIds.Any(selectedNodeIds.Contains),
                IsPrimarySelection: !string.IsNullOrWhiteSpace(selection.PrimarySelectedNodeId)
                    && group.NodeIds.Contains(selection.PrimarySelectedNodeId, StringComparer.Ordinal),
                IsVisibleInActiveScope: true,
                group.IsCollapsed));

            if (nodesByGroupId.TryGetValue(group.Id, out var groupNodes))
            {
                AddNodeItems(items, groupNodes, groupItemId, 2, hierarchyNodesById, compositeChildScopeByNodeId, selection, selectedNodeIds);
            }
        }

        if (nodesByGroupId.TryGetValue(string.Empty, out var ungroupedNodes))
        {
            AddNodeItems(items, ungroupedNodes, items[0].Id, 1, hierarchyNodesById, compositeChildScopeByNodeId, selection, selectedNodeIds);
        }

        AddConnectionItems(items, activeScopeDocument.Connections, items[0].Id, hierarchy, selection, selectedConnectionIds);
        return new GraphEditorNavigatorOutlineSnapshot(hierarchy.ScopeNavigation, items);
    }

    private static GraphEditorNavigatorOutlineItemSnapshot CreateScopeItem(
        GraphDocument activeScopeDocument,
        GraphEditorScopeNavigationSnapshot navigation)
    {
        var title = navigation.Breadcrumbs.LastOrDefault()?.Title;
        return new GraphEditorNavigatorOutlineItemSnapshot(
            CreateItemId(GraphEditorNavigatorOutlineItemKind.Scope, navigation.CurrentScopeId),
            GraphEditorNavigatorOutlineItemKind.Scope,
            navigation.CurrentScopeId,
            ParentItemId: null,
            string.IsNullOrWhiteSpace(title) ? activeScopeDocument.Title : title,
            activeScopeDocument.Description,
            Depth: 0,
            IsSelected: false,
            IsPrimarySelection: false,
            IsVisibleInActiveScope: true,
            IsCollapsed: false);
    }

    private static void AddNodeItems(
        List<GraphEditorNavigatorOutlineItemSnapshot> items,
        IReadOnlyList<GraphNode> nodes,
        string parentItemId,
        int depth,
        IReadOnlyDictionary<string, GraphEditorHierarchyNodeSnapshot> hierarchyNodesById,
        IReadOnlyDictionary<string, string> compositeChildScopeByNodeId,
        GraphEditorSelectionSnapshot selection,
        IReadOnlySet<string> selectedNodeIds)
    {
        foreach (var node in nodes)
        {
            hierarchyNodesById.TryGetValue(node.Id, out var hierarchyNode);
            compositeChildScopeByNodeId.TryGetValue(node.Id, out var childScopeId);

            items.Add(new GraphEditorNavigatorOutlineItemSnapshot(
                CreateItemId(GraphEditorNavigatorOutlineItemKind.Node, node.Id),
                GraphEditorNavigatorOutlineItemKind.Node,
                node.Id,
                parentItemId,
                node.Title,
                node.Subtitle,
                depth,
                selectedNodeIds.Contains(node.Id),
                string.Equals(selection.PrimarySelectedNodeId, node.Id, StringComparison.Ordinal),
                hierarchyNode?.IsVisibleInActiveScope ?? true,
                IsCollapsed: !string.IsNullOrWhiteSpace(hierarchyNode?.CollapsedByGroupId),
                childScopeId));
        }
    }

    private static void AddConnectionItems(
        List<GraphEditorNavigatorOutlineItemSnapshot> items,
        IReadOnlyList<GraphConnection> connections,
        string parentItemId,
        GraphEditorHierarchyStateSnapshot hierarchy,
        GraphEditorSelectionSnapshot selection,
        IReadOnlySet<string> selectedConnectionIds)
    {
        var hierarchyConnectionsById = hierarchy.Connections.ToDictionary(connection => connection.ConnectionId, StringComparer.Ordinal);
        var nodesById = items
            .Where(item => item.Kind == GraphEditorNavigatorOutlineItemKind.Node)
            .ToDictionary(item => item.SourceId, item => item.Title, StringComparer.Ordinal);

        foreach (var connection in connections)
        {
            hierarchyConnectionsById.TryGetValue(connection.Id, out var hierarchyConnection);
            nodesById.TryGetValue(connection.SourceNodeId, out var sourceTitle);
            nodesById.TryGetValue(connection.TargetNodeId, out var targetTitle);

            items.Add(new GraphEditorNavigatorOutlineItemSnapshot(
                CreateItemId(GraphEditorNavigatorOutlineItemKind.Connection, connection.Id),
                GraphEditorNavigatorOutlineItemKind.Connection,
                connection.Id,
                parentItemId,
                string.IsNullOrWhiteSpace(connection.Label) ? connection.Id : connection.Label,
                $"{sourceTitle ?? connection.SourceNodeId} -> {targetTitle ?? connection.TargetNodeId}",
                Depth: 1,
                selectedConnectionIds.Contains(connection.Id),
                string.Equals(selection.PrimarySelectedConnectionId, connection.Id, StringComparison.Ordinal),
                hierarchyConnection?.IsVisibleInActiveScope ?? true,
                IsCollapsed: hierarchyConnection?.IsInternalToCollapsedGroup ?? false));
        }
    }

    private static string CreateItemId(GraphEditorNavigatorOutlineItemKind kind, string sourceId)
        => $"{kind.ToString().ToLowerInvariant()}:{sourceId}";
}
