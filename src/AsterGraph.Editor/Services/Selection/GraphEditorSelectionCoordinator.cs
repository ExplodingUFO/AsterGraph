using System.Collections.ObjectModel;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorSelectionCoordinatorHost
{
    IReadOnlyList<NodeViewModel> AllNodes { get; }

    ObservableCollection<NodeViewModel> SelectionNodes { get; }

    NodeViewModel? PrimarySelectedNode { get; set; }

    bool HasPendingConnection { get; }

    bool IsApplyingKernelProjection { get; }

    bool IsSelectionTrackingSuspended { get; set; }

    string StatusText(string key, string fallback);

    string StatusText(string key, string fallback, params object?[] arguments);

    void SetStatusMessage(string status);

    void SetKernelSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);

    void RefreshSelectionProjection();

    void NotifySelectionChanged();

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorSelectionCoordinator
{
    private readonly IGraphEditorSelectionCoordinatorHost _host;

    public GraphEditorSelectionCoordinator(IGraphEditorSelectionCoordinatorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void ClearSelection(bool updateStatus = false)
        => SetSelection(
            [],
            null,
            updateStatus ? _host.StatusText("editor.status.selection.cleared", "Selection cleared.") : null);

    public void SelectSingleNode(NodeViewModel? node, bool updateStatus = true)
    {
        if (node is null)
        {
            ClearSelection(updateStatus);
            return;
        }

        SetSelection(
            [node],
            node,
            updateStatus && !_host.HasPendingConnection
                ? _host.StatusText("editor.status.selection.selectedNode", "Selected {0}.", node.Title)
                : null);
    }

    public void AddNodeToSelection(NodeViewModel node, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (_host.SelectionNodes.Contains(node))
        {
            return;
        }

        var nextSelection = _host.SelectionNodes.ToList();
        nextSelection.Add(node);
        SetSelection(
            nextSelection,
            node,
            updateStatus
                ? _host.StatusText("editor.status.selection.addedNode", "Added {0} to the selection.", node.Title)
                : null);
    }

    public void ToggleNodeSelection(NodeViewModel node, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(node);

        var nextSelection = _host.SelectionNodes.ToList();
        if (nextSelection.Remove(node))
        {
            SetSelection(
                nextSelection,
                nextSelection.LastOrDefault(),
                updateStatus
                    ? _host.StatusText("editor.status.selection.removedNode", "Removed {0} from the selection.", node.Title)
                    : null);
            return;
        }

        nextSelection.Add(node);
        SetSelection(
            nextSelection,
            node,
            updateStatus
                ? _host.StatusText("editor.status.selection.addedNode", "Added {0} to the selection.", node.Title)
                : null);
    }

    public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
    {
        var (normalizedNodes, normalizedPrimary) = NormalizeSelection(nodes, primaryNode);

        if (_host.IsApplyingKernelProjection)
        {
            SetSelectionCore(normalizedNodes, normalizedPrimary, status);
            return;
        }

        _host.SetKernelSelection(
            normalizedNodes.Select(node => node.Id).ToList(),
            normalizedPrimary?.Id,
            !string.IsNullOrWhiteSpace(status));

        if (!string.IsNullOrWhiteSpace(status))
        {
            _host.SetStatusMessage(status);
        }
    }

    public void SetSelectionCore(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode = null, string? status = null)
    {
        var (normalizedNodes, normalizedPrimary) = NormalizeSelection(nodes, primaryNode);

        foreach (var node in _host.AllNodes)
        {
            node.IsSelected = normalizedNodes.Contains(node);
        }

        var previousSuspendState = _host.IsSelectionTrackingSuspended;
        _host.IsSelectionTrackingSuspended = true;
        try
        {
            _host.SelectionNodes.Clear();
            foreach (var node in normalizedNodes)
            {
                _host.SelectionNodes.Add(node);
            }

            _host.PrimarySelectedNode = normalizedPrimary;
        }
        finally
        {
            _host.IsSelectionTrackingSuspended = previousSuspendState;
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            _host.SetStatusMessage(status);
        }

        _host.RefreshSelectionProjection();
        _host.NotifySelectionChanged();
        _host.RaiseComputedPropertyChanges();
    }

    private (IReadOnlyList<NodeViewModel> Nodes, NodeViewModel? PrimaryNode) NormalizeSelection(
        IReadOnlyList<NodeViewModel> nodes,
        NodeViewModel? primaryNode)
    {
        ArgumentNullException.ThrowIfNull(nodes);

        var uniqueNodes = nodes
            .Where(node => _host.AllNodes.Contains(node))
            .Distinct()
            .ToList();
        var nextPrimary = primaryNode is not null && uniqueNodes.Contains(primaryNode)
            ? primaryNode
            : uniqueNodes.LastOrDefault();

        return (uniqueNodes, nextPrimary);
    }
}
