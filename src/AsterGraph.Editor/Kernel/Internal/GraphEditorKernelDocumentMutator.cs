using AsterGraph.Core.Models;
using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Kernel.Internal;

internal sealed class GraphEditorKernelDocumentMutator
{
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
}

internal sealed record GraphEditorKernelDeleteSelectionResult(
    GraphDocument Document,
    IReadOnlyList<GraphNode> RemovedNodes,
    IReadOnlyList<GraphConnection> RemovedConnections);

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
