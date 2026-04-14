using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorHistoryStateHost
{
    IReadOnlyList<NodeViewModel> SelectedNodes { get; }

    NodeViewModel? SelectedNode { get; }

    string? LastSavedDocumentSignature { get; }

    bool IsHistoryTrackingSuspended { get; set; }

    bool IsDirtyTrackingSuspended { get; }

    GraphDocument CreateViewModelDocumentSnapshot();

    NodeViewModel? FindNode(string nodeId);

    void LoadDocumentCore(GraphDocument document, string status, bool markClean, bool resetHistory);

    void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode, string? status);

    void SetStatusMessage(string status);

    void SetDirtyState(bool isDirty);

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorHistoryStateCoordinator
{
    private readonly IGraphEditorHistoryStateHost _host;
    private readonly GraphEditorHistoryService _historyService;

    public GraphEditorHistoryStateCoordinator(
        IGraphEditorHistoryStateHost host,
        GraphEditorHistoryService historyService)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
    }

    public void MarkDirty(string status)
    {
        _host.SetStatusMessage(status);
        var currentState = CaptureHistoryState();
        UpdateDirtyState(currentState.Signature);
        PushHistoryState(currentState);
        _host.RaiseComputedPropertyChanges();
    }

    public GraphEditorHistoryState CaptureHistoryState()
    {
        var document = _host.CreateViewModelDocumentSnapshot();
        return new GraphEditorHistoryState(
            document,
            _host.SelectedNodes.Select(node => node.Id).ToList(),
            _host.SelectedNode?.Id,
            CreateDocumentSignature(document));
    }

    public void RestoreHistoryState(GraphEditorHistoryState state, string status)
    {
        ArgumentNullException.ThrowIfNull(state);

        var previousSuspendState = _host.IsHistoryTrackingSuspended;
        _host.IsHistoryTrackingSuspended = true;
        try
        {
            _host.LoadDocumentCore(state.Document, status, markClean: false, resetHistory: false);

            var restoredSelection = state.SelectedNodeIds
                .Select(_host.FindNode)
                .Where(node => node is not null)
                .Cast<NodeViewModel>()
                .ToList();
            var primaryNode = !string.IsNullOrWhiteSpace(state.PrimarySelectedNodeId)
                ? restoredSelection.FirstOrDefault(node => node.Id == state.PrimarySelectedNodeId)
                : restoredSelection.LastOrDefault();

            _host.SetSelection(restoredSelection, primaryNode, status);
        }
        finally
        {
            _host.IsHistoryTrackingSuspended = previousSuspendState;
        }

        UpdateDirtyState(CaptureHistoryState().Signature);
        _host.RaiseComputedPropertyChanges();
    }

    public void PushCurrentHistoryState()
    {
        if (_host.IsHistoryTrackingSuspended)
        {
            return;
        }

        PushHistoryState(CaptureHistoryState());
    }

    public void PushHistoryState(GraphEditorHistoryState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (_host.IsHistoryTrackingSuspended)
        {
            return;
        }

        _historyService.Push(state);
    }

    public void UpdateDirtyState()
    {
        if (_host.IsDirtyTrackingSuspended)
        {
            return;
        }

        UpdateDirtyState(CreateDocumentSignature(_host.CreateViewModelDocumentSnapshot()));
    }

    public void UpdateDirtyState(string currentSignature)
    {
        ArgumentNullException.ThrowIfNull(currentSignature);

        if (_host.IsDirtyTrackingSuspended)
        {
            return;
        }

        _host.SetDirtyState(!string.Equals(currentSignature, _host.LastSavedDocumentSignature, StringComparison.Ordinal));
    }

    public static string CreateDocumentSignature(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return GraphDocumentSerializer.Serialize(document);
    }
}
