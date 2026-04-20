using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal sealed class GraphEditorDocumentProjectionApplier
{
    private readonly Dictionary<string, NodeViewModel> _nodesById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ConnectionViewModel> _connectionsById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ConnectionViewModel>> _incomingConnectionsByNodeId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<ConnectionViewModel>> _outgoingConnectionsByNodeId = new(StringComparer.Ordinal);

    public void ApplyDocument(
        GraphDocument document,
        ObservableCollection<NodeViewModel> nodes,
        ObservableCollection<ConnectionViewModel> connections,
        Action<NodeViewModel> applyNodePresentation,
        Action<NodeViewModel>? finalizeNodeProjection,
        PropertyChangedEventHandler nodePropertyChangedHandler)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(connections);
        ArgumentNullException.ThrowIfNull(applyNodePresentation);
        ArgumentNullException.ThrowIfNull(nodePropertyChangedHandler);

        foreach (var node in nodes.ToList())
        {
            node.PropertyChanged -= nodePropertyChangedHandler;
        }

        _nodesById.Clear();
        _connectionsById.Clear();
        _incomingConnectionsByNodeId.Clear();
        _outgoingConnectionsByNodeId.Clear();

        nodes.Clear();
        connections.Clear();

        foreach (var node in document.Nodes)
        {
            var viewModel = new NodeViewModel(node);
            applyNodePresentation(viewModel);
            nodes.Add(viewModel);
        }

        foreach (var connection in document.Connections)
        {
            connections.Add(new ConnectionViewModel(
                connection.Id,
                connection.SourceNodeId,
                connection.SourcePortId,
                connection.TargetNodeId,
                connection.TargetPortId,
                connection.Label,
                connection.AccentHex,
                connection.ConversionId,
                connection.Presentation?.NoteText,
                connection.TargetKind));
        }

        if (finalizeNodeProjection is not null)
        {
            foreach (var node in nodes)
            {
                finalizeNodeProjection(node);
            }
        }
    }

    public void HandleNodesCollectionChanged(
        NotifyCollectionChangedEventArgs args,
        PropertyChangedEventHandler nodePropertyChangedHandler)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(nodePropertyChangedHandler);

        if (args.OldItems is not null)
        {
            foreach (NodeViewModel node in args.OldItems)
            {
                node.PropertyChanged -= nodePropertyChangedHandler;
                _nodesById.Remove(node.Id);
            }
        }

        if (args.NewItems is not null)
        {
            foreach (NodeViewModel node in args.NewItems)
            {
                node.PropertyChanged += nodePropertyChangedHandler;
                _nodesById[node.Id] = node;
            }
        }
    }

    public void HandleConnectionsCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.OldItems is not null)
        {
            foreach (ConnectionViewModel connection in args.OldItems)
            {
                _connectionsById.Remove(connection.Id);
                RemoveIndexedConnection(_outgoingConnectionsByNodeId, connection.SourceNodeId, connection);
                RemoveIndexedConnection(_incomingConnectionsByNodeId, connection.TargetNodeId, connection);
            }
        }

        if (args.NewItems is not null)
        {
            foreach (ConnectionViewModel connection in args.NewItems)
            {
                _connectionsById[connection.Id] = connection;
                AddIndexedConnection(_outgoingConnectionsByNodeId, connection.SourceNodeId, connection);
                AddIndexedConnection(_incomingConnectionsByNodeId, connection.TargetNodeId, connection);
            }
        }
    }

    public NodeViewModel? FindNode(string nodeId)
        => _nodesById.GetValueOrDefault(nodeId);

    public ConnectionViewModel? FindConnection(string connectionId)
        => _connectionsById.GetValueOrDefault(connectionId);

    public IReadOnlyList<ConnectionViewModel> GetIncomingConnections(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return GetIndexedConnections(_incomingConnectionsByNodeId, node.Id);
    }

    public IReadOnlyList<ConnectionViewModel> GetOutgoingConnections(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return GetIndexedConnections(_outgoingConnectionsByNodeId, node.Id);
    }

    private static IReadOnlyList<ConnectionViewModel> GetIndexedConnections(
        Dictionary<string, List<ConnectionViewModel>> lookup,
        string nodeId)
        => lookup.TryGetValue(nodeId, out var connections)
            ? connections
            : Array.Empty<ConnectionViewModel>();

    private static void AddIndexedConnection(
        Dictionary<string, List<ConnectionViewModel>> lookup,
        string nodeId,
        ConnectionViewModel connection)
    {
        if (!lookup.TryGetValue(nodeId, out var connections))
        {
            connections = [];
            lookup[nodeId] = connections;
        }

        connections.Add(connection);
    }

    private static void RemoveIndexedConnection(
        Dictionary<string, List<ConnectionViewModel>> lookup,
        string nodeId,
        ConnectionViewModel connection)
    {
        if (!lookup.TryGetValue(nodeId, out var connections))
        {
            return;
        }

        connections.Remove(connection);
        if (connections.Count == 0)
        {
            lookup.Remove(nodeId);
        }
    }
}
