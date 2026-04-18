using AsterGraph.Core.Models;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Parameters;

namespace AsterGraph.Editor.Kernel.Internal;

internal sealed class GraphEditorKernelDocumentMutator
{
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

        return new GraphEditorKernelDeleteNodeResult(
            document with
            {
                Nodes = document.Nodes.Where(candidate => !ReferenceEquals(candidate, node)).ToList(),
                Connections = document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
            },
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

        return new GraphEditorKernelDeleteSelectionResult(
            document with
            {
                Nodes = document.Nodes.Where(node => !removedNodeIdSet.Contains(node.Id)).ToList(),
                Connections = document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
            },
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
        };

        return new GraphEditorKernelDuplicateNodeResult(
            document with
            {
                Nodes = document.Nodes.Concat([duplicate]).ToList(),
            },
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
                document with { Nodes = updatedNodes },
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
