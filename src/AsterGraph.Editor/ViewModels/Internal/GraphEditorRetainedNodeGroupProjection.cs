using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Runtime.Internal;

namespace AsterGraph.Editor.ViewModels;

internal static class GraphEditorRetainedNodeGroupProjection
{
    internal static IReadOnlyList<GraphNodeGroup> CreateResolvedGroups(
        IReadOnlyList<GraphNodeGroup>? kernelGroups,
        IReadOnlyList<NodeViewModel> nodes)
    {
        if (kernelGroups is not { Count: > 0 })
        {
            return [];
        }

        var nodeIdsByGroupId = nodes
            .Where(node => !string.IsNullOrWhiteSpace(node.GroupId))
            .GroupBy(node => node.GroupId!, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(node => node.Id).ToList(),
                StringComparer.Ordinal);
        var boundsByNodeId = CreateBoundsByNodeId(nodes);

        return kernelGroups
            .Select(group => nodeIdsByGroupId.TryGetValue(group.Id, out var nodeIds)
                ? GraphEditorNodeGroupLayoutResolver.ResolvePersistedBounds(
                    group with
                    {
                        NodeIds = nodeIds.ToList(),
                    },
                    boundsByNodeId)
                : null)
            .Where(group => group is { NodeIds.Count: > 0 })
            .Select(group => group!)
            .ToList();
    }

    internal static IReadOnlyList<GraphEditorNodeGroupSnapshot> CreateResolvedSnapshots(
        IReadOnlyList<GraphNodeGroup>? kernelGroups,
        IReadOnlyList<NodeViewModel> nodes)
    {
        var resolvedGroups = CreateResolvedGroups(kernelGroups, nodes);
        if (resolvedGroups.Count == 0)
        {
            return [];
        }

        var boundsByNodeId = CreateBoundsByNodeId(nodes);
        return resolvedGroups
            .Select(group => GraphEditorNodeGroupLayoutResolver.CreateSnapshot(group, boundsByNodeId))
            .ToList();
    }

    private static IReadOnlyDictionary<string, GraphEditorNodeGroupMemberBounds> CreateBoundsByNodeId(IReadOnlyList<NodeViewModel> nodes)
        => nodes.ToDictionary(
            node => node.Id,
            node => new GraphEditorNodeGroupMemberBounds(
                new GraphPoint(node.X, node.Y),
                new GraphSize(node.Width, node.Height)),
            StringComparer.Ordinal);
}
