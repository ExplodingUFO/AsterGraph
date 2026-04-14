using System.Collections.ObjectModel;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorSelectionStateSynchronizerHost
{
    IReadOnlyList<NodeViewModel> AllNodes { get; }

    ObservableCollection<NodeViewModel> SelectionNodes { get; }

    NodeViewModel? PrimarySelectedNode { get; set; }

    bool IsSelectionTrackingSuspended { get; }

    void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode);

    void RefreshSelectionProjection();

    void NotifySelectionChanged();

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorSelectionStateSynchronizer
{
    private readonly IGraphEditorSelectionStateSynchronizerHost _host;

    public GraphEditorSelectionStateSynchronizer(IGraphEditorSelectionStateSynchronizerHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void HandleSelectedNodesCollectionChanged()
    {
        if (_host.IsSelectionTrackingSuspended)
        {
            return;
        }

        foreach (var node in _host.AllNodes)
        {
            node.IsSelected = _host.SelectionNodes.Contains(node);
        }

        var nextPrimary = _host.PrimarySelectedNode is not null && _host.SelectionNodes.Contains(_host.PrimarySelectedNode)
            ? _host.PrimarySelectedNode
            : _host.SelectionNodes.LastOrDefault();
        if (!ReferenceEquals(nextPrimary, _host.PrimarySelectedNode))
        {
            _host.PrimarySelectedNode = nextPrimary;
        }

        _host.RefreshSelectionProjection();
        _host.NotifySelectionChanged();
        _host.RaiseComputedPropertyChanges();
    }

    public void CoerceSelectionToExistingNodes()
    {
        if (_host.SelectionNodes.Count == 0 && _host.PrimarySelectedNode is null)
        {
            return;
        }

        var nextSelection = _host.SelectionNodes
            .Where(_host.AllNodes.Contains)
            .Distinct()
            .ToList();

        var nextPrimary = _host.PrimarySelectedNode is not null && nextSelection.Contains(_host.PrimarySelectedNode)
            ? _host.PrimarySelectedNode
            : nextSelection.LastOrDefault();

        if (nextSelection.SequenceEqual(_host.SelectionNodes) && ReferenceEquals(nextPrimary, _host.PrimarySelectedNode))
        {
            return;
        }

        _host.SetSelection(nextSelection, nextPrimary);
    }
}
