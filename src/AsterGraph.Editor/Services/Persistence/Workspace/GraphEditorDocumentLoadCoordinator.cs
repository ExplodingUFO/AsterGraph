using System.Collections.ObjectModel;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorDocumentLoadCoordinatorHost
{
    bool IsDirtyTrackingSuspended { get; set; }

    bool IsHistoryTrackingSuspended { get; set; }

    bool IsSelectionTrackingSuspended { get; set; }

    string Title { get; set; }

    string Description { get; set; }

    ObservableCollection<NodeViewModel> SelectedNodes { get; }

    ObservableCollection<NodeParameterViewModel> SelectedNodeParameters { get; }

    NodeViewModel? SelectedNode { get; set; }

    bool IsDirty { get; set; }

    string StatusMessage { get; set; }

    void ApplyDocumentProjection(GraphDocument document);

    void ClearPendingInteractionState();

    GraphEditorHistoryState CaptureHistoryState();

    void ResetHistory(GraphEditorHistoryState state);

    void SetLastSavedDocumentSignature(string signature);

    void RefreshSelectionProjection();

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorDocumentLoadCoordinator
{
    private readonly IGraphEditorDocumentLoadCoordinatorHost _host;

    public GraphEditorDocumentLoadCoordinator(IGraphEditorDocumentLoadCoordinatorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void LoadDocument(GraphDocument document, string status, bool markClean, bool resetHistory = true)
    {
        ArgumentNullException.ThrowIfNull(document);

        _host.IsDirtyTrackingSuspended = true;
        _host.IsHistoryTrackingSuspended = true;

        _host.Title = document.Title;
        _host.Description = document.Description;
        _host.ApplyDocumentProjection(document);

        _host.IsSelectionTrackingSuspended = true;
        _host.SelectedNodes.Clear();
        _host.SelectedNode = null;
        _host.IsSelectionTrackingSuspended = false;
        _host.SelectedNodeParameters.Clear();
        _host.ClearPendingInteractionState();
        _host.IsDirty = !markClean;
        _host.StatusMessage = status;
        _host.IsHistoryTrackingSuspended = false;
        _host.IsDirtyTrackingSuspended = false;

        GraphEditorHistoryState? historyState = null;
        if (resetHistory || markClean)
        {
            historyState = _host.CaptureHistoryState();
        }

        if (resetHistory && historyState is not null)
        {
            _host.ResetHistory(historyState);
        }

        if (markClean && historyState is not null)
        {
            _host.SetLastSavedDocumentSignature(historyState.Signature);
        }

        _host.RefreshSelectionProjection();
        _host.RaiseComputedPropertyChanges();
    }
}
