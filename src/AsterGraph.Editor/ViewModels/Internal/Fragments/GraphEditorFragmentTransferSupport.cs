using AsterGraph.Core.Models;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorFragmentTransferSupport
{
    private readonly GraphEditorViewModel.IGraphEditorFragmentCommandHost _host;

    public GraphEditorFragmentTransferSupport(GraphEditorViewModel.IGraphEditorFragmentCommandHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public GraphSelectionFragment? CreateSelectionFragment()
    {
        var selectedNodes = _host.SelectedNodes.ToList();
        if (selectedNodes.Count == 0)
        {
            return null;
        }

        // 复制时只保留当前选择诱导出的子图，避免粘贴结果依赖外部未复制节点。
        var selectedIds = selectedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var copiedNodes = selectedNodes
            .Select(node => node.ToModel())
            .ToList();

        var copiedConnections = _host.Connections
            .Where(connection =>
                selectedIds.Contains(connection.SourceNodeId)
                && selectedIds.Contains(connection.TargetNodeId))
            .Select(connection => connection.ToModel())
            .ToList();

        var origin = new GraphPoint(
            copiedNodes.Min(node => node.Position.X),
            copiedNodes.Min(node => node.Position.Y));

        return new GraphSelectionFragment(
            copiedNodes,
            copiedConnections,
            origin,
            _host.SelectedNodeId);
    }

    public string? PasteFragment(GraphSelectionFragment fragment, string actionPrefix)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionPrefix);

        if (!_host.CommandPermissions.Nodes.AllowCreate)
        {
            _host.SetStatus("editor.status.fragment.insert.disabledByPermissions", "Fragment insertion is disabled by host permissions.");
            return null;
        }

        if (fragment.Connections.Count > 0 && !_host.CommandPermissions.Connections.AllowCreate)
        {
            _host.SetStatus("editor.status.fragment.insert.connectionCreateDisabled", "This fragment contains connections, but connection creation is disabled by host permissions.");
            return null;
        }

        _host.StoreSelectionClipboard(fragment);
        var targetOrigin = _host.GetNextPasteOrigin();
        var nodeIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var pastedNodes = new List<NodeViewModel>(fragment.Nodes.Count);
        var pastedConnectionIds = new List<string>(fragment.Connections.Count);

        foreach (var copiedNode in fragment.Nodes)
        {
            var newId = _host.CreateNodeId(copiedNode.DefinitionId, copiedNode.Id);
            nodeIdMap[copiedNode.Id] = newId;

            var relativePosition = copiedNode.Position - fragment.Origin;
            var pastedNode = new NodeViewModel(copiedNode with
            {
                Id = newId,
                Position = targetOrigin + relativePosition,
            });

            _host.ApplyNodePresentation(pastedNode);
            _host.AddNode(pastedNode);
            pastedNodes.Add(pastedNode);
        }

        foreach (var copiedConnection in fragment.Connections)
        {
            if (!nodeIdMap.TryGetValue(copiedConnection.SourceNodeId, out var sourceNodeId)
                || !nodeIdMap.TryGetValue(copiedConnection.TargetNodeId, out var targetNodeId))
            {
                continue;
            }

            var connectionId = _host.CreateConnectionId();
            pastedConnectionIds.Add(connectionId);
            _host.AddConnection(new ConnectionViewModel(
                connectionId,
                sourceNodeId,
                copiedConnection.SourcePortId,
                targetNodeId,
                copiedConnection.TargetPortId,
                copiedConnection.Label,
                copiedConnection.AccentHex,
                copiedConnection.ConversionId));
        }

        if (pastedNodes.Count == 0)
        {
            return null;
        }

        NodeViewModel? primaryNode = null;
        if (!string.IsNullOrWhiteSpace(fragment.PrimaryNodeId)
            && nodeIdMap.TryGetValue(fragment.PrimaryNodeId, out var remappedPrimaryNodeId))
        {
            primaryNode = pastedNodes.FirstOrDefault(node => node.Id == remappedPrimaryNodeId);
        }

        _host.SetSelection(pastedNodes, primaryNode ?? pastedNodes[^1]);
        var status = pastedNodes.Count == 1
            ? _host.StatusText("editor.status.fragment.action.single", "{0} {1}.", actionPrefix, pastedNodes[0].Title)
            : _host.StatusText("editor.status.fragment.action.multiple", "{0} {1} nodes.", actionPrefix, pastedNodes.Count);
        return _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.FragmentPasted,
            nodeIds: pastedNodes.Select(node => node.Id).ToList(),
            connectionIds: pastedConnectionIds);
    }
}
