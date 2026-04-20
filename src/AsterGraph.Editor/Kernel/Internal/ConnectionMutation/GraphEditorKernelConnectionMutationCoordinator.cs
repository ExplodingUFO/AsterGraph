using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelConnectionMutationHost
{
    GraphEditorBehaviorOptions BehaviorOptions { get; }

    IPortCompatibilityService CompatibilityService { get; }

    GraphDocument Document { get; }

    GraphEditorPendingConnectionSnapshot PendingConnection { get; }

    string CreateConnectionId();

    void UpdateDocument(GraphDocument document);

    void SetPendingConnection(GraphEditorPendingConnectionSnapshot pendingConnection);

    void SetStatus(string statusMessage);

    void MarkDirty(
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds,
        IReadOnlyList<string>? connectionIds,
        bool preserveStatus = false);

    bool CanReplaceIncomingConnection();
}

internal sealed class GraphEditorKernelConnectionMutationCoordinator
{
    private readonly IGraphEditorKernelConnectionMutationHost _host;
    private readonly GraphEditorKernelDocumentMutator _documentMutator;

    public GraphEditorKernelConnectionMutationCoordinator(
        IGraphEditorKernelConnectionMutationHost host,
        GraphEditorKernelDocumentMutator documentMutator)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _documentMutator = documentMutator ?? throw new ArgumentNullException(nameof(documentMutator));
    }

    public void StartConnection(string sourceNodeId, string sourcePortId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowCreate)
        {
            _host.SetStatus("Connection creation is disabled by host permissions.");
            return;
        }

        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null)
        {
            return;
        }

        var nextPending = GraphEditorPendingConnectionSnapshot.Create(true, sourceNode.Id, sourcePort.Id);
        if (_host.PendingConnection == nextPending)
        {
            CancelPendingConnection();
            return;
        }

        _host.SetPendingConnection(nextPending);
        _host.SetStatus($"Connecting from {sourceNode.Title}.{sourcePort.Label}.");
    }

    public void CompleteConnection(string targetNodeId, string targetPortId)
    {
        if (!_host.PendingConnection.HasPendingConnection
            || _host.PendingConnection.SourceNodeId is null
            || _host.PendingConnection.SourcePortId is null)
        {
            return;
        }

        ConnectPorts(_host.PendingConnection.SourceNodeId, _host.PendingConnection.SourcePortId, targetNodeId, targetPortId);
    }

    public void CancelPendingConnection()
    {
        if (!_host.PendingConnection.HasPendingConnection)
        {
            return;
        }

        _host.SetPendingConnection(GraphEditorPendingConnectionSnapshot.Create(false, null, null));
    }

    public void DeleteConnection(string connectionId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDelete)
        {
            _host.SetStatus("Connection deletion is disabled by host permissions.");
            return;
        }

        var mutation = _documentMutator.DeleteConnection(_host.Document, connectionId);
        if (mutation.Connection is null)
        {
            return;
        }

        _host.UpdateDocument(mutation.Document);
        _host.MarkDirty(
            $"Deleted connection {mutation.Connection.Label}.",
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            null,
            [mutation.Connection.Id]);
    }

    public void DisconnectConnection(string connectionId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDisconnect)
        {
            _host.SetStatus("Disconnect is disabled by host permissions.");
            return;
        }

        var mutation = _documentMutator.DeleteConnection(_host.Document, connectionId);
        if (mutation.Connection is null)
        {
            return;
        }

        _host.UpdateDocument(mutation.Document);
        _host.MarkDirty(
            $"Disconnected connection {mutation.Connection.Label}.",
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            null,
            [mutation.Connection.Id]);
    }

    public void BreakConnectionsForPort(string nodeId, string portId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDisconnect)
        {
            _host.SetStatus("Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(
            connection =>
                (connection.SourceNodeId == nodeId && connection.SourcePortId == portId)
                || (connection.TargetNodeId == nodeId && connection.TargetPortId == portId),
            "Disconnected port links.");
    }

    public void DisconnectIncoming(string nodeId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDisconnect)
        {
            _host.SetStatus("Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(connection => connection.TargetNodeId == nodeId, "Disconnected incoming links.");
    }

    public void DisconnectOutgoing(string nodeId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDisconnect)
        {
            _host.SetStatus("Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(connection => connection.SourceNodeId == nodeId, "Disconnected outgoing links.");
    }

    public void DisconnectAll(string nodeId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDisconnect)
        {
            _host.SetStatus("Disconnect is disabled by host permissions.");
            return;
        }

        RemoveConnections(
            connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId,
            "Disconnected all links.");
    }

    private void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowCreate)
        {
            _host.SetStatus("Connection creation is disabled by host permissions.");
            return;
        }

        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        var targetNode = FindNode(targetNodeId);
        var targetPort = targetNode?.Inputs.FirstOrDefault(port => string.Equals(port.Id, targetPortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null || targetNode is null || targetPort is null)
        {
            return;
        }

        if (sourcePort.TypeId is null || targetPort.TypeId is null)
        {
            _host.SetStatus("Connection endpoints must expose stable type identifiers.");
            return;
        }

        var compatibility = _host.CompatibilityService.Evaluate(sourcePort.TypeId, targetPort.TypeId);
        if (!compatibility.IsCompatible)
        {
            _host.SetStatus($"Incompatible connection: {sourcePort.TypeId} -> {targetPort.TypeId}.");
            return;
        }

        if (_host.Document.Connections.Any(connection =>
                connection.SourceNodeId == sourceNode.Id
                && connection.SourcePortId == sourcePort.Id
                && connection.TargetNodeId == targetNode.Id
                && connection.TargetPortId == targetPort.Id))
        {
            CancelPendingConnection();
            _host.SetStatus("That connection already exists.");
            return;
        }

        var replacedConnections = _host.Document.Connections
            .Where(connection => connection.TargetNodeId == targetNode.Id && connection.TargetPortId == targetPort.Id)
            .ToList();
        if (replacedConnections.Count > 0 && !_host.CanReplaceIncomingConnection())
        {
            _host.SetStatus("Replacing an incoming connection requires delete or disconnect permission.");
            return;
        }

        var nextConnection = new GraphConnection(
            _host.CreateConnectionId(),
            sourceNode.Id,
            sourcePort.Id,
            targetNode.Id,
            targetPort.Id,
            $"{sourcePort.Label} to {targetPort.Label}",
            sourcePort.AccentHex,
            compatibility.ConversionId);

        _host.UpdateDocument(_host.Document with
        {
            Connections = _host.Document.Connections
                .Except(replacedConnections)
                .Concat([nextConnection])
                .ToList(),
        });
        _host.SetPendingConnection(GraphEditorPendingConnectionSnapshot.Create(false, null, null));
        var status = compatibility.Kind == PortCompatibilityKind.ImplicitConversion
            ? $"Connected {sourceNode.Title} to {targetNode.Title} with implicit conversion."
            : $"Connected {sourceNode.Title} to {targetNode.Title}.";
        _host.SetStatus(status);
        _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            [sourceNode.Id, targetNode.Id],
            [nextConnection.Id],
            preserveStatus: true);
    }

    private void RemoveConnections(Func<GraphConnection, bool> predicate, string status)
    {
        var mutation = _documentMutator.RemoveConnections(_host.Document, predicate);
        if (mutation.RemovedConnections.Count == 0)
        {
            _host.SetStatus("No matching connections to remove.");
            return;
        }

        _host.UpdateDocument(mutation.Document);
        _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            null,
            mutation.RemovedConnections.Select(connection => connection.Id).ToList());
    }

    private GraphNode? FindNode(string nodeId)
        => _host.Document.Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));
}
