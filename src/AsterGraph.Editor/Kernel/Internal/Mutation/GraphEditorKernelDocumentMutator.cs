using AsterGraph.Core.Models;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Parameters;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Kernel.Internal;

internal sealed class GraphEditorKernelDocumentMutator
{
    private static readonly GraphPadding DefaultGroupPadding = new(24d, 44d, 24d, 28d);

    internal GraphDocument NormalizeNodeGroupBounds(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        return RefreshNodeGroupBounds(document);
    }

    public GraphEditorKernelDeleteNodeResult DeleteNode(GraphDocument document, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, nodeId, StringComparison.Ordinal));
        if (node is null)
        {
            return GraphEditorKernelDeleteNodeResult.NotFound(document);
        }

        var removedConnections = document.Connections
            .Where(connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId)
            .ToList();

        var updatedDocument = document with
        {
            Nodes = document.Nodes.Where(candidate => !ReferenceEquals(candidate, node)).ToList(),
            Connections = document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
            Groups = RemoveNodeIdsFromGroups(document.Groups, [nodeId]),
        };

        return new GraphEditorKernelDeleteNodeResult(
            RefreshNodeGroupBounds(updatedDocument),
            node,
            removedConnections);
    }

    public GraphEditorKernelDeleteSelectionResult DeleteSelection(
        GraphDocument document,
        IReadOnlyCollection<string> selectedNodeIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedNodeIds);

        var removedNodeIdSet = selectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var removedNodes = document.Nodes.Where(node => removedNodeIdSet.Contains(node.Id)).ToList();
        var removedConnections = document.Connections
            .Where(connection => removedNodeIdSet.Contains(connection.SourceNodeId) || removedNodeIdSet.Contains(connection.TargetNodeId))
            .ToList();

        var updatedDocument = document with
        {
            Nodes = document.Nodes.Where(node => !removedNodeIdSet.Contains(node.Id)).ToList(),
            Connections = document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
            Groups = RemoveNodeIdsFromGroups(document.Groups, removedNodeIdSet),
        };

        return new GraphEditorKernelDeleteSelectionResult(
            RefreshNodeGroupBounds(updatedDocument),
            removedNodes,
            removedConnections);
    }

    public GraphEditorKernelDuplicateNodeResult DuplicateNode(
        GraphDocument document,
        string nodeId,
        string duplicateNodeId,
        GraphPoint duplicatePosition)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(duplicateNodeId);

        var node = document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, nodeId, StringComparison.Ordinal));
        if (node is null)
        {
            return GraphEditorKernelDuplicateNodeResult.NotFound(document);
        }

        var duplicate = node with
        {
            Id = duplicateNodeId,
            Position = duplicatePosition,
            Surface = node.Surface is null
                ? null
                : node.Surface with { GroupId = null },
        };

        var updatedDocument = document with
        {
            Nodes = document.Nodes.Concat([duplicate]).ToList(),
        };

        return new GraphEditorKernelDuplicateNodeResult(
            RefreshNodeGroupBounds(updatedDocument),
            duplicate);
    }

    public GraphEditorKernelNodePositionMutationResult SetNodePositions(
        GraphDocument document,
        IReadOnlyList<NodePositionSnapshot> positions)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(positions);

        var requested = positions
            .GroupBy(snapshot => snapshot.NodeId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToDictionary(snapshot => snapshot.NodeId, snapshot => snapshot.Position, StringComparer.Ordinal);

        if (requested.Count == 0)
        {
            return GraphEditorKernelNodePositionMutationResult.Empty(document);
        }

        var changedNodeIds = new List<string>();
        var updatedNodes = document.Nodes
            .Select(node =>
            {
                if (!requested.TryGetValue(node.Id, out var position) || node.Position == position)
                {
                    return node;
                }

                changedNodeIds.Add(node.Id);
                return node with { Position = position };
            })
            .ToList();

        return changedNodeIds.Count == 0
            ? GraphEditorKernelNodePositionMutationResult.Empty(document)
            : new GraphEditorKernelNodePositionMutationResult(
                RefreshNodeGroupBounds(document with { Nodes = updatedNodes }),
                changedNodeIds);
    }

    public GraphEditorKernelDeleteConnectionResult DeleteConnection(GraphDocument document, string connectionId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var connection = document.Connections.FirstOrDefault(candidate => string.Equals(candidate.Id, connectionId, StringComparison.Ordinal));
        if (connection is null)
        {
            return GraphEditorKernelDeleteConnectionResult.NotFound(document);
        }

        return new GraphEditorKernelDeleteConnectionResult(
            document with
            {
                Connections = document.Connections.Where(candidate => !ReferenceEquals(candidate, connection)).ToList(),
            },
            connection);
    }

    public GraphEditorKernelRemoveConnectionsResult RemoveConnections(
        GraphDocument document,
        Func<GraphConnection, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(predicate);

        var removedConnections = document.Connections
            .Where(predicate)
            .ToList();

        return removedConnections.Count == 0
            ? GraphEditorKernelRemoveConnectionsResult.Empty(document)
            : new GraphEditorKernelRemoveConnectionsResult(
                document with
                {
                    Connections = document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
                },
                removedConnections);
    }

    public GraphEditorKernelNodeParameterMutationResult SetNodeParameterValue(
        GraphDocument document,
        IReadOnlyCollection<string> selectedNodeIds,
        NodeParameterDefinition parameterDefinition,
        object? normalizedValue)
        => SetNodeParameterValues(
            document,
            selectedNodeIds,
            [new GraphEditorKernelParameterValueUpdate(parameterDefinition, normalizedValue)]);

    public GraphEditorKernelNodeParameterMutationResult SetNodeParameterValues(
        GraphDocument document,
        IReadOnlyCollection<string> selectedNodeIds,
        IReadOnlyList<GraphEditorKernelParameterValueUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedNodeIds);
        ArgumentNullException.ThrowIfNull(updates);

        if (selectedNodeIds.Count == 0 || updates.Count == 0)
        {
            return GraphEditorKernelNodeParameterMutationResult.Empty(document);
        }

        var selectedNodeIdSet = selectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var changedNodeIds = new List<string>();
        var updatedNodes = document.Nodes
            .Select(node =>
            {
                if (!selectedNodeIdSet.Contains(node.Id))
                {
                    return node;
                }

                var existingValues = node.ParameterValues?.ToList() ?? [];
                var nodeChanged = false;

                foreach (var update in updates)
                {
                    var existingIndex = existingValues.FindIndex(parameter => string.Equals(parameter.Key, update.Definition.Key, StringComparison.Ordinal));
                    var currentEffectiveValue = existingIndex >= 0
                        ? NodeParameterValueAdapter.NormalizeIncomingValue(existingValues[existingIndex].Value)
                        : NodeParameterValueAdapter.NormalizeIncomingValue(update.Definition.DefaultValue);
                    if (NodeParameterValueAdapter.AreEquivalent(currentEffectiveValue, update.Value))
                    {
                        continue;
                    }

                    nodeChanged = true;
                    if (existingIndex >= 0)
                    {
                        existingValues[existingIndex] = existingValues[existingIndex] with { Value = update.Value };
                    }
                    else
                    {
                        existingValues.Add(new GraphParameterValue(update.Definition.Key, update.Definition.ValueType, update.Value));
                    }
                }

                if (!nodeChanged)
                {
                    return node;
                }

                changedNodeIds.Add(node.Id);
                return node with { ParameterValues = existingValues };
            })
            .ToList();

        return changedNodeIds.Count == 0
            ? GraphEditorKernelNodeParameterMutationResult.Empty(document)
            : new GraphEditorKernelNodeParameterMutationResult(
                document with { Nodes = updatedNodes },
                changedNodeIds);
    }

    public GraphEditorKernelNodeWidthMutationResult SetNodeWidth(
        GraphDocument document,
        string nodeId,
        double width)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (width <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Node width must be positive.");
        }

        GraphNode? updatedNode = null;
        var updatedNodes = document.Nodes
            .Select(node =>
            {
                if (!string.Equals(node.Id, nodeId, StringComparison.Ordinal) || Math.Abs(node.Size.Width - width) < 0.001d)
                {
                    return node;
                }

                updatedNode = node with
                {
                    Size = node.Size with { Width = width },
                };
                return updatedNode;
            })
            .ToList();

        return updatedNode is null
            ? GraphEditorKernelNodeWidthMutationResult.Empty(document)
            : new GraphEditorKernelNodeWidthMutationResult(
                RefreshNodeGroupBounds(document with { Nodes = updatedNodes }),
                updatedNode.Id,
                updatedNode.Size.Width);
    }

    public GraphEditorKernelNodeSurfaceMutationResult SetNodeExpansionState(
        GraphDocument document,
        string nodeId,
        GraphNodeExpansionState expansionState)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        GraphNode? updatedNode = null;
        var updatedNodes = document.Nodes
            .Select(node =>
            {
                if (!string.Equals(node.Id, nodeId, StringComparison.Ordinal))
                {
                    return node;
                }

                var currentSurface = node.Surface ?? GraphNodeSurfaceState.Default;
                if (currentSurface.ExpansionState == expansionState)
                {
                    return node;
                }

                updatedNode = node with
                {
                    Surface = currentSurface with { ExpansionState = expansionState },
                };
                return updatedNode;
            })
            .ToList();

        return updatedNode is null
            ? GraphEditorKernelNodeSurfaceMutationResult.Empty(document)
            : new GraphEditorKernelNodeSurfaceMutationResult(
                RefreshNodeGroupBounds(document with { Nodes = updatedNodes }),
                updatedNode.Id,
                updatedNode.Surface ?? GraphNodeSurfaceState.Default);
    }

    public GraphEditorKernelNodeGroupMutationResult CreateNodeGroupFromSelection(
        GraphDocument document,
        IReadOnlyCollection<string> selectedNodeIds,
        string groupId,
        string title)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedNodeIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var selectedNodeIdSet = selectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var selectedNodes = document.Nodes
            .Where(node => selectedNodeIdSet.Contains(node.Id))
            .ToList();
        if (selectedNodes.Count == 0)
        {
            return GraphEditorKernelNodeGroupMutationResult.Empty(document);
        }

        var group = new GraphNodeGroup(
            groupId,
            title.Trim(),
            Position: default,
            Size: default,
            NodeIds: selectedNodes.Select(node => node.Id).ToList(),
            ExtraPadding: DefaultGroupPadding);

        var updatedGroups = RemoveNodeIdsFromGroups(document.Groups, selectedNodeIdSet).ToList();
        updatedGroups.Add(group);

        var updatedNodes = document.Nodes
            .Select(node => selectedNodeIdSet.Contains(node.Id)
                ? UpdateNodeGroupId(node, groupId)
                : node)
            .ToList();

        var updatedDocument = RefreshNodeGroupBounds(
            document with
            {
                Nodes = updatedNodes,
                Groups = updatedGroups,
            });
        var createdGroup = updatedDocument.Groups!.Single(candidate => string.Equals(candidate.Id, groupId, StringComparison.Ordinal));

        return new GraphEditorKernelNodeGroupMutationResult(
            updatedDocument,
            createdGroup,
            selectedNodes.Select(node => node.Id).ToList());
    }

    public GraphEditorKernelNodeGroupMutationResult SetNodeGroupCollapsed(
        GraphDocument document,
        string groupId,
        bool isCollapsed)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        GraphNodeGroup? updatedGroup = null;
        var updatedGroups = (document.Groups ?? [])
            .Select(group =>
            {
                if (!string.Equals(group.Id, groupId, StringComparison.Ordinal) || group.IsCollapsed == isCollapsed)
                {
                    return group;
                }

                updatedGroup = group with { IsCollapsed = isCollapsed };
                return updatedGroup;
            })
            .ToList();

        return updatedGroup is null
            ? GraphEditorKernelNodeGroupMutationResult.Empty(document)
            : new GraphEditorKernelNodeGroupMutationResult(
                document with { Groups = updatedGroups },
                updatedGroup,
                updatedGroup.NodeIds.ToList());
    }

    public GraphEditorKernelNodeGroupMutationResult SetNodeGroupPosition(
        GraphDocument document,
        string groupId,
        GraphPoint position,
        bool moveMemberNodes)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        GraphNodeGroup? previousGroup = null;
        GraphNodeGroup? updatedGroup = null;
        var updatedGroups = (document.Groups ?? [])
            .Select(group =>
            {
                if (!string.Equals(group.Id, groupId, StringComparison.Ordinal))
                {
                    return group;
                }

                if (group.Position == position)
                {
                    previousGroup = group;
                    return group;
                }

                previousGroup = group;
                updatedGroup = group with { Position = position };
                return updatedGroup;
            })
            .ToList();

        if (updatedGroup is null || previousGroup is null)
        {
            return GraphEditorKernelNodeGroupMutationResult.Empty(document);
        }

        var updatedDocument = document with { Groups = updatedGroups };
        if (moveMemberNodes)
        {
            var deltaX = updatedGroup.Position.X - previousGroup.Position.X;
            var deltaY = updatedGroup.Position.Y - previousGroup.Position.Y;
            var memberNodeIds = updatedGroup.NodeIds.ToHashSet(StringComparer.Ordinal);
            updatedDocument = updatedDocument with
            {
                Nodes = updatedDocument.Nodes
                    .Select(node => memberNodeIds.Contains(node.Id)
                        ? node with
                        {
                            Position = new GraphPoint(node.Position.X + deltaX, node.Position.Y + deltaY),
                        }
                        : node)
                    .ToList(),
            };
        }
        else
        {
            var currentRight = previousGroup.Position.X + previousGroup.Size.Width;
            var currentBottom = previousGroup.Position.Y + previousGroup.Size.Height;
            updatedDocument = SetNodeGroupExtraPaddingCore(
                updatedDocument,
                groupId,
                CreatePaddingFromResolvedBounds(
                    updatedDocument,
                    groupId,
                    position,
                    new GraphSize(currentRight - position.X, currentBottom - position.Y)));
        }

        updatedDocument = RefreshNodeGroupBounds(updatedDocument);
        updatedGroup = (updatedDocument.Groups ?? [])
            .FirstOrDefault(group => string.Equals(group.Id, groupId, StringComparison.Ordinal));

        if (updatedGroup is null)
        {
            return GraphEditorKernelNodeGroupMutationResult.Empty(document);
        }

        return new GraphEditorKernelNodeGroupMutationResult(
            updatedDocument,
            updatedGroup,
            updatedGroup.NodeIds.ToList());
    }

    public GraphEditorKernelNodeGroupMutationResult SetNodeGroupExtraPadding(
        GraphDocument document,
        string groupId,
        GraphPadding extraPadding)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var normalizedPadding = extraPadding.ClampNonNegative();
        var updatedDocument = SetNodeGroupExtraPaddingCore(document, groupId, normalizedPadding);
        if (ReferenceEquals(updatedDocument, document))
        {
            return GraphEditorKernelNodeGroupMutationResult.Empty(document);
        }

        updatedDocument = RefreshNodeGroupBounds(updatedDocument);
        var updatedGroup = (updatedDocument.Groups ?? [])
            .FirstOrDefault(group => string.Equals(group.Id, groupId, StringComparison.Ordinal));

        return updatedGroup is null
            ? GraphEditorKernelNodeGroupMutationResult.Empty(document)
            : new GraphEditorKernelNodeGroupMutationResult(
                updatedDocument,
                updatedGroup,
                updatedGroup.NodeIds.ToList());
    }

    private static IReadOnlyList<GraphNodeGroup> RemoveNodeIdsFromGroups(
        IReadOnlyList<GraphNodeGroup>? groups,
        IEnumerable<string> nodeIds)
    {
        var removedNodeIdSet = nodeIds.ToHashSet(StringComparer.Ordinal);
        if (removedNodeIdSet.Count == 0)
        {
            return groups?.ToList() ?? [];
        }

        return (groups ?? [])
            .Select(group => group with
            {
                NodeIds = group.NodeIds
                    .Where(nodeId => !removedNodeIdSet.Contains(nodeId))
                    .ToList(),
            })
            .Where(group => group.NodeIds.Count > 0)
            .ToList();
    }

    private static GraphNode UpdateNodeGroupId(GraphNode node, string? groupId)
    {
        var surface = node.Surface ?? GraphNodeSurfaceState.Default;
        return node with
        {
            Surface = surface with { GroupId = groupId },
        };
    }

    private static GraphDocument SetNodeGroupExtraPaddingCore(
        GraphDocument document,
        string groupId,
        GraphPadding extraPadding)
    {
        var changed = false;
        var updatedGroups = (document.Groups ?? [])
            .Select(group =>
            {
                if (!string.Equals(group.Id, groupId, StringComparison.Ordinal)
                    || group.ExtraPadding == extraPadding)
                {
                    return group;
                }

                changed = true;
                return group with { ExtraPadding = extraPadding };
            })
            .ToList();

        return changed
            ? document with { Groups = updatedGroups }
            : document;
    }

    private static GraphDocument RefreshNodeGroupBounds(GraphDocument document)
    {
        if (document.Groups is not { Count: > 0 })
        {
            return document;
        }

        var boundsByNodeId = document.Nodes.ToDictionary(
            node => node.Id,
            node => new GraphEditorNodeGroupMemberBounds(
                node.Position,
                new GraphSize(node.Size.Width, GraphEditorNodeSurfaceMetrics.CalculateRenderedHeight(node))),
            StringComparer.Ordinal);

        var updatedGroups = document.Groups
            .Select(group => GraphEditorNodeGroupLayoutResolver.ResolvePersistedBounds(group, boundsByNodeId))
            .ToList();
        return document with { Groups = updatedGroups };
    }

    private static GraphPadding CreatePaddingFromResolvedBounds(
        GraphDocument document,
        string groupId,
        GraphPoint position,
        GraphSize size)
    {
        var group = (document.Groups ?? [])
            .FirstOrDefault(candidate => string.Equals(candidate.Id, groupId, StringComparison.Ordinal));
        if (group is null)
        {
            return default;
        }

        var boundsByNodeId = document.Nodes.ToDictionary(
            node => node.Id,
            node => new GraphEditorNodeGroupMemberBounds(
                node.Position,
                new GraphSize(node.Size.Width, GraphEditorNodeSurfaceMetrics.CalculateRenderedHeight(node))),
            StringComparer.Ordinal);
        var snapshot = GraphEditorNodeGroupLayoutResolver.CreateSnapshot(group, boundsByNodeId);
        var requestedRight = position.X + Math.Max(0d, size.Width);
        var requestedBottom = position.Y + Math.Max(0d, size.Height);
        var contentRight = snapshot.ContentPosition.X + snapshot.ContentSize.Width;
        var contentBottom = snapshot.ContentPosition.Y + snapshot.ContentSize.Height;

        return new GraphPadding(
            snapshot.ContentPosition.X - position.X,
            snapshot.ContentPosition.Y - position.Y,
            requestedRight - contentRight,
            requestedBottom - contentBottom).ClampNonNegative();
    }
}

internal sealed record GraphEditorKernelDeleteNodeResult(
    GraphDocument Document,
    GraphNode? Node,
    IReadOnlyList<GraphConnection> RemovedConnections)
{
    public static GraphEditorKernelDeleteNodeResult NotFound(GraphDocument document)
        => new(document, null, []);
}

internal sealed record GraphEditorKernelDeleteSelectionResult(
    GraphDocument Document,
    IReadOnlyList<GraphNode> RemovedNodes,
    IReadOnlyList<GraphConnection> RemovedConnections);

internal sealed record GraphEditorKernelDuplicateNodeResult(
    GraphDocument Document,
    GraphNode? Node)
{
    public static GraphEditorKernelDuplicateNodeResult NotFound(GraphDocument document)
        => new(document, null);
}

internal sealed record GraphEditorKernelNodePositionMutationResult(
    GraphDocument Document,
    IReadOnlyList<string> ChangedNodeIds)
{
    public static GraphEditorKernelNodePositionMutationResult Empty(GraphDocument document)
        => new(document, []);
}

internal sealed record GraphEditorKernelNodeWidthMutationResult(
    GraphDocument Document,
    string? NodeId,
    double? Width)
{
    public static GraphEditorKernelNodeWidthMutationResult Empty(GraphDocument document)
        => new(document, null, null);
}

internal sealed record GraphEditorKernelNodeSurfaceMutationResult(
    GraphDocument Document,
    string? NodeId,
    GraphNodeSurfaceState? Surface)
{
    public static GraphEditorKernelNodeSurfaceMutationResult Empty(GraphDocument document)
        => new(document, null, null);
}

internal sealed record GraphEditorKernelNodeGroupMutationResult(
    GraphDocument Document,
    GraphNodeGroup? Group,
    IReadOnlyList<string> ChangedNodeIds)
{
    public static GraphEditorKernelNodeGroupMutationResult Empty(GraphDocument document)
        => new(document, null, []);
}

internal sealed record GraphEditorKernelDeleteConnectionResult(
    GraphDocument Document,
    GraphConnection? Connection)
{
    public static GraphEditorKernelDeleteConnectionResult NotFound(GraphDocument document)
        => new(document, null);
}

internal sealed record GraphEditorKernelRemoveConnectionsResult(
    GraphDocument Document,
    IReadOnlyList<GraphConnection> RemovedConnections)
{
    public static GraphEditorKernelRemoveConnectionsResult Empty(GraphDocument document)
        => new(document, []);
}

internal sealed record GraphEditorKernelNodeParameterMutationResult(
    GraphDocument Document,
    IReadOnlyList<string> ChangedNodeIds)
{
    public static GraphEditorKernelNodeParameterMutationResult Empty(GraphDocument document)
        => new(document, []);
}

internal sealed record GraphEditorKernelParameterValueUpdate(
    NodeParameterDefinition Definition,
    object? Value);
