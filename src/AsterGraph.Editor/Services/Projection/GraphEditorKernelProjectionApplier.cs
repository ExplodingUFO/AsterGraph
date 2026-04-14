using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorKernelProjectionHost
{
    void RunInKernelProjectionScope(Action action);

    void ApplyKernelDocumentCore(GraphDocument document, string status, bool markClean);

    void ApplyKernelSelectionCore(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode);

    void ApplyKernelViewportCore(GraphEditorViewportSnapshot snapshot);

    void ApplyKernelPendingConnectionCore(NodeViewModel? pendingNode, PortViewModel? pendingPort);

    NodeViewModel? FindNode(string nodeId);
}

internal sealed class GraphEditorKernelProjectionApplier
{
    private readonly IGraphEditorKernelProjectionHost _host;

    public GraphEditorKernelProjectionApplier(IGraphEditorKernelProjectionHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void ApplyDocument(GraphDocument document, string status, bool markClean)
    {
        ArgumentNullException.ThrowIfNull(document);

        _host.RunInKernelProjectionScope(() => _host.ApplyKernelDocumentCore(document, status, markClean));
    }

    public void ApplySelection(IReadOnlyList<string> nodeIds, string? primaryNodeId)
    {
        ArgumentNullException.ThrowIfNull(nodeIds);

        _host.RunInKernelProjectionScope(() =>
        {
            var selectedNodes = nodeIds
                .Select(_host.FindNode)
                .Where(node => node is not null)
                .Cast<NodeViewModel>()
                .ToList();
            var primaryNode = !string.IsNullOrWhiteSpace(primaryNodeId)
                ? selectedNodes.FirstOrDefault(node => node.Id == primaryNodeId)
                : selectedNodes.LastOrDefault();

            _host.ApplyKernelSelectionCore(selectedNodes, primaryNode);
        });
    }

    public void ApplyViewport(GraphEditorViewportSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _host.ApplyKernelViewportCore(snapshot);
    }

    public void ApplyPendingConnection(GraphEditorPendingConnectionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        _host.RunInKernelProjectionScope(() =>
        {
            var pendingNode = !string.IsNullOrWhiteSpace(snapshot.SourceNodeId)
                ? _host.FindNode(snapshot.SourceNodeId)
                : null;
            var pendingPort = pendingNode is not null && !string.IsNullOrWhiteSpace(snapshot.SourcePortId)
                ? pendingNode.GetPort(snapshot.SourcePortId)
                : null;

            _host.ApplyKernelPendingConnectionCore(pendingNode, pendingPort);
        });
    }
}
