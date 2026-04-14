using System.Collections.Specialized;
using System.ComponentModel;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorDocumentCollectionSynchronizerHost
{
    void CoerceSelectionToExistingNodes();

    void NotifyFitViewCommandCanExecuteChanged();

    void RefreshSelectionProjection();

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorDocumentCollectionSynchronizer
{
    private readonly IGraphEditorDocumentCollectionSynchronizerHost _host;
    private readonly GraphEditorDocumentProjectionApplier _projectionApplier;

    public GraphEditorDocumentCollectionSynchronizer(
        IGraphEditorDocumentCollectionSynchronizerHost host,
        GraphEditorDocumentProjectionApplier projectionApplier)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _projectionApplier = projectionApplier ?? throw new ArgumentNullException(nameof(projectionApplier));
    }

    public void HandleNodesCollectionChanged(
        NotifyCollectionChangedEventArgs args,
        PropertyChangedEventHandler nodePropertyChangedHandler)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(nodePropertyChangedHandler);

        _projectionApplier.HandleNodesCollectionChanged(args, nodePropertyChangedHandler);
        _host.CoerceSelectionToExistingNodes();
        _host.NotifyFitViewCommandCanExecuteChanged();
        _host.RefreshSelectionProjection();
        _host.RaiseComputedPropertyChanges();
    }

    public void HandleConnectionsCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        _projectionApplier.HandleConnectionsCollectionChanged(args);
        _host.RefreshSelectionProjection();
        _host.RaiseComputedPropertyChanges();
    }
}
