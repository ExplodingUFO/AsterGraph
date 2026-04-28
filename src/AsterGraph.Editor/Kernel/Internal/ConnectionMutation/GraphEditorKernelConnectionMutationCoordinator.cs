using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelConnectionMutationHost
{
    GraphEditorBehaviorOptions BehaviorOptions { get; }

    IPortCompatibilityService CompatibilityService { get; }

    INodeCatalog NodeCatalog { get; }

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

    public void CompleteConnection(GraphConnectionTargetRef target)
    {
        if (!_host.PendingConnection.HasPendingConnection
            || _host.PendingConnection.SourceNodeId is null
            || _host.PendingConnection.SourcePortId is null)
        {
            return;
        }

        ConnectToTarget(_host.PendingConnection.SourceNodeId, _host.PendingConnection.SourcePortId, target);
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

    public bool TryReconnectConnection(string connectionId, bool updateStatus)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowDisconnect)
        {
            if (updateStatus)
            {
                _host.SetStatus("Disconnect is disabled by host permissions.");
            }

            return false;
        }

        if (!_host.BehaviorOptions.Commands.Connections.AllowCreate)
        {
            if (updateStatus)
            {
                _host.SetStatus("Connection creation is disabled by host permissions.");
            }

            return false;
        }

        var existingConnection = _host.Document.Connections
            .FirstOrDefault(connection => string.Equals(connection.Id, connectionId, StringComparison.Ordinal));
        if (existingConnection is null)
        {
            if (updateStatus)
            {
                _host.SetStatus("No matching connection was found to reconnect.");
            }

            return false;
        }

        var sourceNode = FindNode(existingConnection.SourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, existingConnection.SourcePortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null)
        {
            if (updateStatus)
            {
                _host.SetStatus("Reconnect requires a valid source output.");
            }

            return false;
        }

        var mutation = _documentMutator.DeleteConnection(_host.Document, connectionId);
        if (mutation.Connection is null)
        {
            if (updateStatus)
            {
                _host.SetStatus("No matching connection was found to reconnect.");
            }

            return false;
        }

        _host.UpdateDocument(mutation.Document);
        _host.SetPendingConnection(GraphEditorPendingConnectionSnapshot.Create(true, sourceNode.Id, sourcePort.Id));
        var status = $"Reconnect {sourceNode.Title}.{sourcePort.Label} by choosing a new target.";
        if (updateStatus)
        {
            _host.SetStatus(status);
        }

        _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            [sourceNode.Id, mutation.Connection.TargetNodeId],
            [mutation.Connection.Id],
            preserveStatus: !updateStatus);
        return true;
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
                || (connection.TargetKind == GraphConnectionTargetKind.Port
                    && connection.TargetNodeId == nodeId
                    && connection.TargetPortId == portId),
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

    private void ConnectToTarget(string sourceNodeId, string sourcePortId, GraphConnectionTargetRef target)
    {
        if (!_host.BehaviorOptions.Commands.Connections.AllowCreate)
        {
            _host.SetStatus("Connection creation is disabled by host permissions.");
            return;
        }

        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        var resolvedTarget = ResolveTarget(target);
        if (sourceNode is null || sourcePort is null || resolvedTarget is null)
        {
            return;
        }

        if (sourcePort.TypeId is null || resolvedTarget.TypeId is null)
        {
            var reason = "Connection endpoints must expose stable type identifiers.";
            SetPendingTargetFeedback(sourceNode, sourcePort, resolvedTarget, isCompatible: false, reason);
            _host.SetStatus(reason);
            return;
        }

        var compatibility = _host.CompatibilityService.Evaluate(sourcePort.TypeId, resolvedTarget.TypeId);
        if (!compatibility.IsCompatible)
        {
            var reason = $"Incompatible connection: {sourcePort.TypeId.Value} -> {resolvedTarget.TypeId.Value}.";
            SetPendingTargetFeedback(sourceNode, sourcePort, resolvedTarget, isCompatible: false, reason);
            _host.SetStatus(reason);
            return;
        }

        if (_host.Document.Connections.Any(connection =>
                connection.SourceNodeId == sourceNode.Id
                && connection.SourcePortId == sourcePort.Id
                && connection.TargetNodeId == resolvedTarget.Node.Id
                && connection.TargetPortId == resolvedTarget.Target.TargetId
                && connection.TargetKind == resolvedTarget.Target.Kind))
        {
            CancelPendingConnection();
            _host.SetStatus("That connection already exists.");
            return;
        }

        var replacedConnections = _host.Document.Connections
            .Where(connection =>
                connection.TargetNodeId == resolvedTarget.Node.Id
                && connection.TargetPortId == resolvedTarget.Target.TargetId
                && connection.TargetKind == resolvedTarget.Target.Kind)
            .ToList();
        if (replacedConnections.Count > 0 && !_host.CanReplaceIncomingConnection())
        {
            _host.SetStatus("Replacing an incoming connection requires delete or disconnect permission.");
            return;
        }

        var outgoingCount = _host.Document.Connections.Count(connection =>
            connection.SourceNodeId == sourceNode.Id
            && connection.SourcePortId == sourcePort.Id);
        if (outgoingCount >= sourcePort.MaxConnections)
        {
            var reason = $"Source port '{sourcePort.Label}' has reached its maximum connection limit ({sourcePort.MaxConnections}).";
            SetPendingTargetFeedback(sourceNode, sourcePort, resolvedTarget, isCompatible: false, reason);
            _host.SetStatus(reason);
            return;
        }

        var incomingCount = _host.Document.Connections.Count(connection =>
            connection.TargetNodeId == resolvedTarget.Node.Id
            && connection.TargetPortId == resolvedTarget.Target.TargetId
            && connection.TargetKind == resolvedTarget.Target.Kind);
        var finalIncomingCount = incomingCount - replacedConnections.Count + 1;
        var targetPort = resolvedTarget.Node.Inputs.FirstOrDefault(port => string.Equals(port.Id, resolvedTarget.Target.TargetId, StringComparison.Ordinal));
        if (targetPort is not null && finalIncomingCount > targetPort.MaxConnections)
        {
            var reason = $"Target port '{targetPort.Label}' would exceed its maximum connection limit ({targetPort.MaxConnections}).";
            SetPendingTargetFeedback(sourceNode, sourcePort, resolvedTarget, isCompatible: false, reason);
            _host.SetStatus(reason);
            return;
        }

        var nextConnection = new GraphConnection(
            _host.CreateConnectionId(),
            sourceNode.Id,
            sourcePort.Id,
            resolvedTarget.Node.Id,
            resolvedTarget.Target.TargetId,
            $"{sourcePort.Label} to {resolvedTarget.Label}",
            sourcePort.AccentHex,
            compatibility.ConversionId)
        {
            TargetKind = resolvedTarget.Target.Kind,
        };

        _host.UpdateDocument(_host.Document with
        {
            Connections = _host.Document.Connections
                .Except(replacedConnections)
                .Concat([nextConnection])
                .ToList(),
        });
        _host.SetPendingConnection(GraphEditorPendingConnectionSnapshot.Create(false, null, null));
        var status = compatibility.Kind == PortCompatibilityKind.ImplicitConversion
            ? $"Connected {sourceNode.Title} to {resolvedTarget.Node.Title} with implicit conversion."
            : $"Connected {sourceNode.Title} to {resolvedTarget.Node.Title}.";
        _host.SetStatus(status);
        _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            [sourceNode.Id, resolvedTarget.Node.Id],
            [nextConnection.Id],
            preserveStatus: true);
    }

    private void SetPendingTargetFeedback(
        GraphNode sourceNode,
        GraphPort sourcePort,
        ResolvedConnectionTarget resolvedTarget,
        bool isCompatible,
        string reason)
        => _host.SetPendingConnection(GraphEditorPendingConnectionSnapshot.Create(
            true,
            sourceNode.Id,
            sourcePort.Id,
            resolvedTarget.Node.Id,
            resolvedTarget.Target.TargetId,
            resolvedTarget.Target.Kind,
            isCompatible,
            reason));

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

    private ResolvedConnectionTarget? ResolveTarget(GraphConnectionTargetRef target)
    {
        var targetNode = FindNode(target.NodeId);
        if (targetNode is null)
        {
            return null;
        }

        if (target.Kind == GraphConnectionTargetKind.Port)
        {
            var targetPort = targetNode.Inputs.FirstOrDefault(port => string.Equals(port.Id, target.TargetId, StringComparison.Ordinal));
            return targetPort is null
                ? null
                : new ResolvedConnectionTarget(
                    targetNode,
                    target,
                    targetPort.Label,
                    targetPort.TypeId,
                    targetPort.AccentHex);
        }

        if (targetNode.DefinitionId is null
            || !_host.NodeCatalog.TryGetDefinition(targetNode.DefinitionId, out var definition)
            || definition is null)
        {
            _host.SetStatus("Parameter targets require a registered node definition.");
            return null;
        }

        var parameter = definition.Parameters.FirstOrDefault(candidate => string.Equals(candidate.Key, target.TargetId, StringComparison.Ordinal));
        if (parameter is null)
        {
            return null;
        }

        return new ResolvedConnectionTarget(
            targetNode,
            target,
            parameter.DisplayName,
            parameter.ValueType,
            targetNode.AccentHex);
    }

    private sealed record ResolvedConnectionTarget(
        GraphNode Node,
        GraphConnectionTargetRef Target,
        string Label,
        PortTypeId? TypeId,
        string AccentHex);
}
