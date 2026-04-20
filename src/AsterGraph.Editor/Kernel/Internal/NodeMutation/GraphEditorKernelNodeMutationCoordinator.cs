using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelNodeMutationHost
{
    GraphEditorBehaviorOptions BehaviorOptions { get; }

    INodeCatalog NodeCatalog { get; }

    GraphDocument Document { get; }

    IReadOnlyList<string> SelectedNodeIds { get; }

    GraphEditorPendingConnectionSnapshot PendingConnection { get; }

    GraphPoint GetViewportCenter();

    string CreateNodeId(NodeDefinitionId definitionId);

    string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey);

    void UpdateDocument(GraphDocument document);

    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);

    void CancelPendingConnection();

    bool CanRemoveConnectionsAsSideEffect();

    void SetStatus(string statusMessage);

    void MarkDirty(
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds,
        IReadOnlyList<string>? connectionIds,
        bool preserveStatus = false);
}

internal sealed class GraphEditorKernelNodeMutationCoordinator
{
    private readonly IGraphEditorKernelNodeMutationHost _host;
    private readonly GraphEditorKernelDocumentMutator _documentMutator;

    public GraphEditorKernelNodeMutationCoordinator(
        IGraphEditorKernelNodeMutationHost host,
        GraphEditorKernelDocumentMutator documentMutator)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _documentMutator = documentMutator ?? throw new ArgumentNullException(nameof(documentMutator));
    }

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
    {
        ArgumentNullException.ThrowIfNull(definitionId);

        if (!_host.BehaviorOptions.Commands.Nodes.AllowCreate)
        {
            _host.SetStatus("Node creation is disabled by host permissions.");
            return;
        }

        if (!_host.NodeCatalog.TryGetDefinition(definitionId, out var definition) || definition is null)
        {
            throw new InvalidOperationException($"Node definition '{definitionId}' is not registered in the current editor catalog.");
        }

        var position = preferredWorldPosition ?? _host.GetViewportCenter();
        var offset = 26 * (_host.Document.Nodes.Count % 4);
        var normalizedSize = GraphEditorNodeSurfaceMetrics.NormalizePersistedSize(
            new GraphSize(definition.DefaultWidth, definition.DefaultHeight),
            definition.InputPorts.Count,
            definition.OutputPorts.Count);
        var node = new GraphNode(
            _host.CreateNodeId(definitionId),
            definition.DisplayName,
            definition.Category,
            definition.Subtitle,
            definition.Description ?? definition.Subtitle,
            new GraphPoint(
                position.X - (normalizedSize.Width / 2) + offset,
                position.Y - (normalizedSize.Height / 2) + offset),
            normalizedSize,
            definition.InputPorts.Select(port => new GraphPort(port.Key, port.DisplayName, PortDirection.Input, port.TypeId.Value, port.AccentHex, port.TypeId)).ToList(),
            definition.OutputPorts.Select(port => new GraphPort(port.Key, port.DisplayName, PortDirection.Output, port.TypeId.Value, port.AccentHex, port.TypeId)).ToList(),
            definition.AccentHex,
            definition.Id,
            definition.Parameters.Select(parameter => new GraphParameterValue(parameter.Key, parameter.ValueType, parameter.DefaultValue)).ToList(),
            GraphNodeSurfaceState.Default);

        _host.UpdateDocument(_host.Document with
        {
            Nodes = _host.Document.Nodes.Concat([node]).ToList(),
        });
        _host.SetSelection([node.Id], node.Id, updateStatus: false);
        _host.MarkDirty($"Added {node.Title}.", GraphEditorDocumentChangeKind.NodesAdded, [node.Id], null);
    }

    public void DeleteNodeById(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_host.BehaviorOptions.Commands.Nodes.AllowDelete)
        {
            _host.SetStatus("Node deletion is disabled by host permissions.");
            return;
        }

        var mutation = _documentMutator.DeleteNode(_host.Document, nodeId);
        if (mutation.Node is null)
        {
            return;
        }

        if (mutation.RemovedConnections.Count > 0 && !_host.CanRemoveConnectionsAsSideEffect())
        {
            _host.SetStatus("Deleting a connected node requires delete or disconnect permission for the affected links.");
            return;
        }

        _host.UpdateDocument(mutation.Document);
        if (string.Equals(_host.PendingConnection.SourceNodeId, mutation.Node.Id, StringComparison.Ordinal))
        {
            _host.CancelPendingConnection();
        }

        var remainingSelection = _host.SelectedNodeIds
            .Where(selectedNodeId => !string.Equals(selectedNodeId, mutation.Node.Id, StringComparison.Ordinal))
            .ToList();
        _host.SetSelection(remainingSelection, remainingSelection.LastOrDefault(), updateStatus: false);
        _host.MarkDirty(
            $"Deleted {mutation.Node.Title}.",
            GraphEditorDocumentChangeKind.NodesRemoved,
            [mutation.Node.Id],
            mutation.RemovedConnections.Select(connection => connection.Id).ToList());
    }

    public void DuplicateNode(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_host.BehaviorOptions.Commands.Nodes.AllowDuplicate)
        {
            _host.SetStatus("Node duplication is disabled by host permissions.");
            return;
        }

        var node = _host.Document.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Id, nodeId, StringComparison.Ordinal));
        if (node is null)
        {
            return;
        }

        var mutation = _documentMutator.DuplicateNode(
            _host.Document,
            nodeId,
            _host.CreateNodeId(node.DefinitionId, node.Id),
            new GraphPoint(node.Position.X + 48, node.Position.Y + 48));
        if (mutation.Node is null)
        {
            return;
        }

        _host.UpdateDocument(mutation.Document);
        _host.SetSelection([mutation.Node.Id], mutation.Node.Id, updateStatus: false);
        _host.MarkDirty(
            $"Duplicated {mutation.Node.Title}.",
            GraphEditorDocumentChangeKind.NodesAdded,
            [mutation.Node.Id],
            null);
    }

    public void DeleteSelection()
    {
        if (!_host.BehaviorOptions.Commands.Nodes.AllowDelete)
        {
            _host.SetStatus("Node deletion is disabled by host permissions.");
            return;
        }

        if (_host.SelectedNodeIds.Count == 0)
        {
            _host.SetStatus("Select a node before deleting.");
            return;
        }

        var mutation = _documentMutator.DeleteSelection(_host.Document, _host.SelectedNodeIds);
        if (mutation.RemovedConnections.Count > 0 && !_host.CanRemoveConnectionsAsSideEffect())
        {
            _host.SetStatus("Deleting connected nodes requires delete or disconnect permission for the affected links.");
            return;
        }

        _host.UpdateDocument(mutation.Document);
        _host.CancelPendingConnection();
        _host.SetSelection([], null, updateStatus: false);
        var status = mutation.RemovedNodes.Count == 1
            ? $"Deleted {mutation.RemovedNodes[0].Title}."
            : $"Deleted {mutation.RemovedNodes.Count} nodes.";
        _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.NodesRemoved,
            mutation.RemovedNodes.Select(node => node.Id).ToList(),
            mutation.RemovedConnections.Select(connection => connection.Id).ToList());
    }

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
    {
        ArgumentNullException.ThrowIfNull(positions);

        if (!_host.BehaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                _host.SetStatus("Node movement is disabled by host permissions.");
            }

            return;
        }

        var mutation = _documentMutator.SetNodePositions(_host.Document, positions);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            if (updateStatus)
            {
                _host.SetStatus(
                    positions.Count == 0
                        ? "No node positions were provided."
                        : "No matching nodes were found for the provided positions.");
            }

            return;
        }

        _host.UpdateDocument(mutation.Document);
        if (updateStatus)
        {
            _host.SetStatus(
                mutation.ChangedNodeIds.Count == 1
                    ? "Updated 1 node position."
                    : $"Updated {mutation.ChangedNodeIds.Count} node positions.");
        }

        _host.MarkDirty(
            updateStatus
                ? mutation.ChangedNodeIds.Count == 1
                    ? "Updated 1 node position."
                    : $"Updated {mutation.ChangedNodeIds.Count} node positions."
                : string.Empty,
            GraphEditorDocumentChangeKind.LayoutChanged,
            mutation.ChangedNodeIds,
            null,
            preserveStatus: true);
    }
}
