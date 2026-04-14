using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorRetainedEventPublisherHost
{
    IReadOnlyList<NodeViewModel> SelectionNodes { get; }

    NodeViewModel? PrimarySelectedNode { get; }

    bool HasPendingConnection { get; }

    NodeViewModel? PendingSourceNode { get; }

    PortViewModel? PendingSourcePort { get; }

    string CurrentStatusMessage { get; }

    void RaiseDocumentChanged(GraphEditorDocumentChangedEventArgs args);

    void RaiseSelectionChanged(GraphEditorSelectionChangedEventArgs args);

    void RaisePendingConnectionChanged(GraphEditorPendingConnectionChangedEventArgs args);
}

internal sealed class GraphEditorRetainedEventPublisher
{
    private readonly IGraphEditorRetainedEventPublisherHost _host;
    private GraphEditorPendingConnectionSnapshot _lastPendingConnectionSnapshot = GraphEditorPendingConnectionSnapshot.Create(false, null, null);

    public GraphEditorRetainedEventPublisher(IGraphEditorRetainedEventPublisherHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void PublishDocumentChanged(
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null,
        string? statusMessage = null)
    {
        _host.RaiseDocumentChanged(new GraphEditorDocumentChangedEventArgs(
            changeKind,
            nodeIds,
            connectionIds,
            statusMessage ?? _host.CurrentStatusMessage));
    }

    public void PublishSelectionChanged()
    {
        _host.RaiseSelectionChanged(new GraphEditorSelectionChangedEventArgs(
            _host.SelectionNodes.Select(node => node.Id).ToList(),
            _host.PrimarySelectedNode?.Id));
    }

    public void PublishPendingConnectionChanged()
    {
        var snapshot = GraphEditorPendingConnectionSnapshot.Create(
            _host.HasPendingConnection,
            _host.PendingSourceNode?.Id,
            _host.PendingSourcePort?.Id);
        if (_lastPendingConnectionSnapshot == snapshot)
        {
            return;
        }

        _lastPendingConnectionSnapshot = snapshot;
        _host.RaisePendingConnectionChanged(new GraphEditorPendingConnectionChangedEventArgs(snapshot));
    }
}
