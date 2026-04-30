using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;
using System.Threading;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelClipboardHost
{
    GraphEditorCommandPermissions CommandPermissions { get; }

    GraphDocument ActiveScopeDocument { get; }

    IReadOnlyList<string> SelectedNodeIds { get; }

    string? PrimarySelectedNodeId { get; }

    IGraphTextClipboardBridge? TextClipboardBridge { get; }

    IGraphClipboardPayloadSerializer ClipboardPayloadSerializer { get; }

    bool HasClipboardContent { get; }

    void StoreSelectionClipboard(GraphSelectionFragment fragment);

    GraphSelectionFragment? PeekSelectionClipboard();

    GraphPoint GetNextPasteOrigin();

    string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey);

    string CreateConnectionId();

    void UpdateActiveScopeDocument(GraphDocument document);

    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);

    void SetStatus(string statusMessage);

    void MarkDirty(
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds,
        IReadOnlyList<string>? connectionIds,
        bool preserveStatus = false);
}

internal sealed class GraphEditorKernelClipboardCoordinator
{
    private readonly IGraphEditorKernelClipboardHost _host;

    public GraphEditorKernelClipboardCoordinator(IGraphEditorKernelClipboardHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public bool CanCopySelection
        => _host.CommandPermissions.Clipboard.AllowCopy
        && _host.SelectedNodeIds.Count > 0;

    public bool CanPaste
        => _host.CommandPermissions.Clipboard.AllowPaste
        && _host.CommandPermissions.Nodes.AllowCreate
        && (_host.HasClipboardContent || _host.TextClipboardBridge is not null);

    public async Task<bool> TryCopySelectionAsync(CancellationToken cancellationToken)
    {
        if (!_host.CommandPermissions.Clipboard.AllowCopy)
        {
            _host.SetStatus("Copy is disabled by host permissions.");
            return false;
        }

        var fragment = CreateSelectionFragment();
        if (fragment is null)
        {
            _host.SetStatus("Select at least one node before copying.");
            return false;
        }

        _host.StoreSelectionClipboard(fragment);
        if (_host.TextClipboardBridge is not null)
        {
            var clipboardJson = _host.ClipboardPayloadSerializer.Serialize(fragment);
            await _host.TextClipboardBridge.WriteTextAsync(clipboardJson, cancellationToken);
        }

        _host.SetStatus(
            fragment.Nodes.Count == 1
                ? $"Copied {fragment.Nodes[0].Title}."
                : $"Copied {fragment.Nodes.Count} nodes.");
        return true;
    }

    public GraphSelectionFragment? CreateSelectionFragment()
        => TryCreateSelectionFragment();

    public async Task<bool> TryPasteSelectionAsync(CancellationToken cancellationToken)
    {
        if (!_host.CommandPermissions.Clipboard.AllowPaste)
        {
            _host.SetStatus("Paste is disabled by host permissions.");
            return false;
        }

        var fragment = await GetBestAvailableClipboardFragmentAsync(cancellationToken);
        if (fragment is null || fragment.Nodes.Count == 0)
        {
            _host.SetStatus("Nothing copied yet.");
            return false;
        }

        return TryPasteFragment(fragment, "Pasted");
    }

    public bool TryPasteFragment(GraphSelectionFragment fragment, string actionPrefix)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentException.ThrowIfNullOrWhiteSpace(actionPrefix);

        if (!_host.CommandPermissions.Nodes.AllowCreate)
        {
            _host.SetStatus("Fragment insertion is disabled by host permissions.");
            return false;
        }

        if (fragment.Connections.Count > 0 && !_host.CommandPermissions.Connections.AllowCreate)
        {
            _host.SetStatus("This fragment contains connections, but connection creation is disabled by host permissions.");
            return false;
        }

        _host.StoreSelectionClipboard(fragment);
        var targetOrigin = _host.GetNextPasteOrigin();
        var nodeIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var groupIdMap = new Dictionary<string, string>(StringComparer.Ordinal);
        var pastedNodes = new List<GraphNode>(fragment.Nodes.Count);
        var pastedConnections = new List<GraphConnection>(fragment.Connections.Count);

        foreach (var copiedGroup in fragment.Groups)
        {
            groupIdMap[copiedGroup.Id] = CreateGroupId(copiedGroup.Id, groupIdMap.Values);
        }

        var nodeGroupIds = CreateNodeGroupIdMap(fragment.Groups, groupIdMap);

        foreach (var copiedNode in fragment.Nodes)
        {
            var newId = _host.CreateNodeId(copiedNode.DefinitionId, copiedNode.Id);
            nodeIdMap[copiedNode.Id] = newId;

            var relativePosition = copiedNode.Position - fragment.Origin;
            pastedNodes.Add(copiedNode with
            {
                Id = newId,
                Position = targetOrigin + relativePosition,
                Surface = RemapNodeGroup(copiedNode.Surface, copiedNode.Id, nodeGroupIds),
            });
        }

        foreach (var copiedConnection in fragment.Connections)
        {
            if (!nodeIdMap.TryGetValue(copiedConnection.SourceNodeId, out var sourceNodeId)
                || !nodeIdMap.TryGetValue(copiedConnection.TargetNodeId, out var targetNodeId))
            {
                continue;
            }

            pastedConnections.Add(copiedConnection with
            {
                Id = _host.CreateConnectionId(),
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId,
            });
        }

        var pastedGroups = fragment.Groups
            .Select(group => RemapGroup(group, groupIdMap, nodeIdMap, targetOrigin - fragment.Origin))
            .ToList();

        if (pastedNodes.Count == 0)
        {
            _host.SetStatus("Nothing copied yet.");
            return false;
        }

        var activeScopeDocument = _host.ActiveScopeDocument;
        _host.UpdateActiveScopeDocument(activeScopeDocument with
        {
            Nodes = activeScopeDocument.Nodes.Concat(pastedNodes).ToList(),
            Connections = activeScopeDocument.Connections.Concat(pastedConnections).ToList(),
            Groups = (activeScopeDocument.Groups ?? []).Concat(pastedGroups).ToList(),
        });

        string? primaryNodeId = null;
        if (!string.IsNullOrWhiteSpace(fragment.PrimaryNodeId)
            && nodeIdMap.TryGetValue(fragment.PrimaryNodeId, out var remappedPrimaryNodeId))
        {
            primaryNodeId = remappedPrimaryNodeId;
        }

        var selectedNodeIds = pastedNodes.Select(node => node.Id).ToList();
        _host.SetSelection(selectedNodeIds, primaryNodeId ?? selectedNodeIds[^1], updateStatus: false);
        var status = pastedNodes.Count == 1
            ? $"{actionPrefix} {pastedNodes[0].Title}."
            : $"{actionPrefix} {pastedNodes.Count} nodes.";
        _host.MarkDirty(
            status,
            GraphEditorDocumentChangeKind.FragmentPasted,
            selectedNodeIds,
            pastedConnections.Select(connection => connection.Id).ToList());
        return true;
    }

    private GraphSelectionFragment? TryCreateSelectionFragment()
    {
        var activeScope = _host.ActiveScopeDocument;
        var selectedNodes = activeScope.Nodes
            .Where(node => _host.SelectedNodeIds.Contains(node.Id, StringComparer.Ordinal))
            .ToList();
        if (selectedNodes.Count == 0)
        {
            return null;
        }

        var selectedNodeIds = selectedNodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var copiedConnections = activeScope.Connections
            .Where(connection =>
                selectedNodeIds.Contains(connection.SourceNodeId)
                && selectedNodeIds.Contains(connection.TargetNodeId))
            .ToList();
        var copiedGroups = (activeScope.Groups ?? [])
            .Where(group => group.NodeIds.Count > 0 && group.NodeIds.All(selectedNodeIds.Contains))
            .ToList();
        var origin = new GraphPoint(
            selectedNodes.Min(node => node.Position.X),
            selectedNodes.Min(node => node.Position.Y));

        return new GraphSelectionFragment(
            selectedNodes,
            copiedConnections,
            origin,
            _host.PrimarySelectedNodeId,
            copiedGroups);
    }

    private string CreateGroupId(string fallbackKey, IEnumerable<string> reservedIds)
    {
        var existingIds = _host.ActiveScopeDocument.Groups?.Select(group => group.Id).ToHashSet(StringComparer.Ordinal)
            ?? new HashSet<string>(StringComparer.Ordinal);
        existingIds.UnionWith(reservedIds);
        if (!existingIds.Contains(fallbackKey))
        {
            return fallbackKey;
        }

        var suffix = 2;
        string candidate;
        do
        {
            candidate = $"{fallbackKey}-{suffix++}";
        }
        while (existingIds.Contains(candidate));

        return candidate;
    }

    private static GraphNodeSurfaceState? RemapNodeGroup(
        GraphNodeSurfaceState? surface,
        string nodeId,
        IReadOnlyDictionary<string, string> nodeGroupIds)
    {
        if (!nodeGroupIds.TryGetValue(nodeId, out var groupId))
        {
            return surface is null || string.IsNullOrWhiteSpace(surface.GroupId)
                ? surface
                : surface with { GroupId = null };
        }

        return (surface ?? GraphNodeSurfaceState.Default) with { GroupId = groupId };
    }

    private static GraphNodeGroup RemapGroup(
        GraphNodeGroup group,
        IReadOnlyDictionary<string, string> groupIdMap,
        IReadOnlyDictionary<string, string> nodeIdMap,
        GraphPoint offset)
        => group with
        {
            Id = groupIdMap[group.Id],
            Position = group.Position + offset,
            NodeIds = group.NodeIds
                .Where(nodeIdMap.ContainsKey)
                .Select(nodeId => nodeIdMap[nodeId])
                .ToList(),
        };

    private static Dictionary<string, string> CreateNodeGroupIdMap(
        IReadOnlyList<GraphNodeGroup> groups,
        IReadOnlyDictionary<string, string> groupIdMap)
    {
        var nodeGroupIds = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var group in groups)
        {
            if (!groupIdMap.TryGetValue(group.Id, out var groupId))
            {
                continue;
            }

            foreach (var nodeId in group.NodeIds)
            {
                nodeGroupIds[nodeId] = groupId;
            }
        }

        return nodeGroupIds;
    }

    private async Task<GraphSelectionFragment?> GetBestAvailableClipboardFragmentAsync(CancellationToken cancellationToken)
    {
        if (_host.TextClipboardBridge is not null)
        {
            var clipboardText = await _host.TextClipboardBridge.ReadTextAsync(cancellationToken);
            if (_host.ClipboardPayloadSerializer.TryDeserialize(clipboardText, out var systemFragment))
            {
                return systemFragment;
            }
        }

        return _host.PeekSelectionClipboard();
    }
}
