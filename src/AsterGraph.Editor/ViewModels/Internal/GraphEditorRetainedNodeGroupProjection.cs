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
        return kernelGroups
            .Select(group => GraphEditorNodeGroupLayoutResolver.ResolvePersistedBounds(
                group with
                {
                    NodeIds = nodeIdsByGroupId.TryGetValue(group.Id, out var nodeIds)
                        ? nodeIds.ToList()
                        : [],
                }))
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

        return resolvedGroups
            .Select(GraphEditorNodeGroupLayoutResolver.CreateSnapshot)
            .ToList();
    }
}
