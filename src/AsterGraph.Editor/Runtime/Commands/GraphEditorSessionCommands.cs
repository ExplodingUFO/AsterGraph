using AsterGraph.Editor;
using AsterGraph.Editor.Models;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    public void Undo()
        => Execute("history.undo", _host.Undo);

    public void Redo()
        => Execute("history.redo", _host.Redo);

    public void ClearSelection(bool updateStatus = false)
        => Execute("selection.clear", () => _host.ClearSelection(updateStatus));

    public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId = null, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(nodeIds);
        Execute("selection.set", () => _host.SetSelection(nodeIds, primaryNodeId, updateStatus));
    }

    public void SetConnectionSelection(IReadOnlyList<string> connectionIds, string? primaryConnectionId = null, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(connectionIds);
        Execute("selection.connections.set", () => _host.SetConnectionSelection(connectionIds, primaryConnectionId, updateStatus));
    }

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null)
    {
        ArgumentNullException.ThrowIfNull(definitionId);
        Execute("nodes.add", () => _host.AddNode(definitionId, preferredWorldPosition));
    }

    public bool TryCreateConnectedNodeFromPendingConnection(
        NodeDefinitionId definitionId,
        string targetId,
        GraphConnectionTargetKind targetKind,
        GraphPoint? preferredWorldPosition = null)
    {
        ArgumentNullException.ThrowIfNull(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetId);

        var compatibleTarget = GetCompatibleNodeDefinitionsForPendingConnection()
            .Results
            .FirstOrDefault(candidate =>
                candidate.DefinitionId == definitionId
                && string.Equals(candidate.TargetId, targetId, StringComparison.Ordinal)
                && candidate.TargetKind == targetKind);
        if (compatibleTarget is null)
        {
            return false;
        }

        using var mutation = BeginMutation("quick-add-connected-node");
        var beforeNodeIds = _host.CreateActiveScopeDocumentSnapshot()
            .Nodes
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);
        _host.AddNode(definitionId, preferredWorldPosition);
        var createdNode = _host.CreateActiveScopeDocumentSnapshot()
            .Nodes
            .FirstOrDefault(node => !beforeNodeIds.Contains(node.Id) && node.DefinitionId == definitionId);
        if (createdNode is null)
        {
            return false;
        }

        _host.CompleteConnection(new GraphConnectionTargetRef(createdNode.Id, targetId, targetKind));
        PublishCommandExecuted("nodes.quick-add-connected");
        return true;
    }

    public bool TryInsertNodeIntoConnection(
        string connectionId,
        NodeDefinitionId definitionId,
        string inputTargetId,
        GraphConnectionTargetKind inputTargetKind,
        string outputPortId,
        GraphPoint? preferredWorldPosition = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        ArgumentNullException.ThrowIfNull(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputTargetId);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPortId);

        var inserted = _host.TryInsertNodeIntoConnection(
            connectionId,
            definitionId,
            inputTargetId,
            inputTargetKind,
            outputPortId,
            preferredWorldPosition);
        if (inserted)
        {
            PublishCommandExecuted("nodes.insert-into-connection");
        }

        return inserted;
    }

    public void DeleteSelection()
        => Execute("selection.delete", _host.DeleteSelection);

    public bool TryDeleteSelectionAndReconnect()
    {
        var deleted = _host.TryDeleteSelectionAndReconnect();
        if (deleted)
        {
            PublishCommandExecuted("selection.delete-reconnect");
        }

        return deleted;
    }

    public bool TryDetachSelectionFromConnections()
    {
        var detached = _host.TryDetachSelectionFromConnections();
        if (detached)
        {
            PublishCommandExecuted("selection.detach-connections");
        }

        return detached;
    }

    public bool TryDeleteSelectedConnections()
    {
        var deleted = _host.TryDeleteSelectedConnections();
        if (deleted)
        {
            PublishCommandExecuted("connections.delete-selected");
        }

        return deleted;
    }

    public bool TrySliceConnections(GraphPoint start, GraphPoint end)
    {
        var sliced = _host.TrySliceConnections(start, end);
        if (sliced)
        {
            PublishCommandExecuted("connections.slice");
        }

        return sliced;
    }

    public async Task<bool> TryCopySelectionAsync(CancellationToken cancellationToken = default)
    {
        var copied = await _host.TryCopySelectionAsync(cancellationToken);
        if (copied)
        {
            PublishCommandExecuted("clipboard.copy");
        }

        return copied;
    }

    public async Task<bool> TryPasteSelectionAsync(CancellationToken cancellationToken = default)
    {
        var pasted = await _host.TryPasteSelectionAsync(cancellationToken);
        if (pasted)
        {
            PublishCommandExecuted("clipboard.paste");
        }

        return pasted;
    }

    public bool TryExportSelectionFragment(string? path = null)
    {
        var exported = _host.TryExportSelectionFragment(path);
        if (exported)
        {
            PublishCommandExecuted("fragments.export-selection");
        }

        return exported;
    }

    public bool TryImportFragment(string? path = null)
    {
        var imported = _host.TryImportFragment(path);
        if (imported)
        {
            PublishCommandExecuted("fragments.import");
        }

        return imported;
    }

    public bool TryClearWorkspaceFragment(string? path = null)
    {
        var cleared = _host.TryClearWorkspaceFragment(path);
        if (cleared)
        {
            PublishCommandExecuted("fragments.clear-workspace");
        }

        return cleared;
    }

    public string TryExportSelectionAsTemplate(string? name = null)
    {
        var templatePath = _host.TryExportSelectionAsTemplate(name);
        if (!string.IsNullOrWhiteSpace(templatePath))
        {
            PublishCommandExecuted("fragments.export-template");
        }

        return templatePath;
    }

    public bool TryExportSceneAsSvg(string? path = null)
    {
        var exported = _host.TryExportSceneAsSvg(path);
        if (exported)
        {
            PublishCommandExecuted("export.scene-svg");
        }

        return exported;
    }

    public bool TryExportSceneAsImage(
        GraphEditorSceneImageExportFormat format,
        string? path = null,
        GraphEditorSceneImageExportOptions? options = null)
    {
        var exported = _host.TryExportSceneAsImage(format, path, options);
        if (exported)
        {
            PublishCommandExecuted("export.scene-image");
        }

        return exported;
    }

    public bool TryImportFragmentTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var imported = _host.TryImportFragmentTemplate(path);
        if (imported)
        {
            PublishCommandExecuted("fragments.import-template");
        }

        return imported;
    }

    public bool TryDeleteFragmentTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var deleted = _host.TryDeleteFragmentTemplate(path);
        if (deleted)
        {
            PublishCommandExecuted("fragments.delete-template");
        }

        return deleted;
    }

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(positions);
        Execute("nodes.move", () => _host.SetNodePositions(positions, updateStatus));
    }

    public bool TrySetNodeWidth(string nodeId, double width, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var edited = _host.TrySetNodeWidth(nodeId, width, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("nodes.resize-width");
        }

        return edited;
    }

    public bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var edited = _host.TrySetNodeSize(nodeId, size, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("nodes.resize");
        }

        return edited;
    }

    public bool TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var edited = _host.TrySetNodeExpansionState(nodeId, expansionState);
        if (edited)
        {
            PublishCommandExecuted("nodes.surface.expand");
        }

        return edited;
    }

    public string TryCreateNodeGroupFromSelection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var groupId = _host.TryCreateNodeGroupFromSelection(title);
        if (!string.IsNullOrWhiteSpace(groupId))
        {
            PublishCommandExecuted("groups.create");
        }

        return groupId;
    }

    public bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var edited = _host.TrySetNodeGroupCollapsed(groupId, isCollapsed);
        if (edited)
        {
            PublishCommandExecuted("groups.collapse");
        }

        return edited;
    }

    public bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes = true, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var edited = _host.TrySetNodeGroupPosition(groupId, position, moveMemberNodes, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("groups.move");
        }

        return edited;
    }

    public bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var edited = _host.TrySetNodeGroupSize(groupId, size, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("groups.resize");
        }

        return edited;
    }

    public bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var edited = _host.TrySetNodeGroupExtraPadding(groupId, extraPadding, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("groups.resize");
        }

        return edited;
    }

    public bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var edited = _host.TrySetNodeGroupMemberships(changes, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("groups.membership.set");
        }

        return edited;
    }

    public string TryPromoteNodeGroupToComposite(string groupId, string? title = null, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var compositeNodeId = _host.TryPromoteNodeGroupToComposite(groupId, title, updateStatus);
        if (!string.IsNullOrWhiteSpace(compositeNodeId))
        {
            PublishCommandExecuted("groups.promote");
        }

        return compositeNodeId;
    }

    public string TryWrapSelectionToComposite(string? title = null, bool updateStatus = true)
    {
        var compositeNodeId = _host.TryWrapSelectionToComposite(title, updateStatus);
        if (!string.IsNullOrWhiteSpace(compositeNodeId))
        {
            PublishCommandExecuted("composites.wrap-selection");
        }

        return compositeNodeId;
    }

    public string TryExposeCompositePort(
        string compositeNodeId,
        string childNodeId,
        string childPortId,
        string? label = null,
        bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositeNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(childNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(childPortId);

        var boundaryPortId = _host.TryExposeCompositePort(compositeNodeId, childNodeId, childPortId, label, updateStatus);
        if (!string.IsNullOrWhiteSpace(boundaryPortId))
        {
            PublishCommandExecuted("composites.expose-port");
        }

        return boundaryPortId;
    }

    public bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositeNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryPortId);

        var edited = _host.TryUnexposeCompositePort(compositeNodeId, boundaryPortId, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("composites.unexpose-port");
        }

        return edited;
    }

    public bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositeNodeId);

        var navigated = _host.TryEnterCompositeChildGraph(compositeNodeId, updateStatus);
        if (navigated)
        {
            PublishCommandExecuted("scopes.enter");
        }

        return navigated;
    }

    public bool TryReturnToParentGraphScope(bool updateStatus = true)
    {
        var navigated = _host.TryReturnToParentGraphScope(updateStatus);
        if (navigated)
        {
            PublishCommandExecuted("scopes.exit");
        }

        return navigated;
    }

    public bool TrySetSelectedNodeParameterValue(string parameterKey, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

        var edited = _host.TrySetSelectedNodeParameterValue(parameterKey, value);
        if (edited)
        {
            PublishCommandExecuted("nodes.parameters.set");
        }

        return edited;
    }

    public bool TrySetSelectedNodeParameterValues(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var edited = _host.TrySetSelectedNodeParameterValues(values);
        if (edited)
        {
            PublishCommandExecuted("nodes.parameters.batch-set");
        }

        return edited;
    }

    [Obsolete("Use StartConnection instead.")]
    public void BeginConnection(string sourceNodeId, string sourcePortId)
        => StartConnection(sourceNodeId, sourcePortId);

    public void StartConnection(string sourceNodeId, string sourcePortId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePortId);

        Execute("connections.start", () => _host.StartConnection(sourceNodeId, sourcePortId));
    }

    public void CompleteConnection(string targetNodeId, string targetPortId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPortId);

        CompleteConnection(new GraphConnectionTargetRef(targetNodeId, targetPortId));
    }

    public void CompleteConnection(GraphConnectionTargetRef target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(target.NodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(target.TargetId);

        Execute("connections.complete", () => _host.CompleteConnection(target));
    }

    public void CancelPendingConnection()
        => Execute("connections.cancel", _host.CancelPendingConnection);

    public void DeleteConnection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        Execute("connections.delete", () => _host.DeleteConnection(connectionId));
    }

    public bool TryReconnectConnection(string connectionId, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var reconnected = _host.TryReconnectConnection(connectionId, updateStatus);
        if (reconnected)
        {
            PublishCommandExecuted("connections.reconnect");
        }

        return reconnected;
    }

    public bool TrySetConnectionLabel(string connectionId, string? label, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var edited = _host.TrySetConnectionLabel(connectionId, label, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("connections.label.set");
        }

        return edited;
    }

    public bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var edited = _host.TrySetConnectionNoteText(connectionId, noteText, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("connections.note.set");
        }

        return edited;
    }

    public bool TryInsertConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var edited = _host.TryInsertConnectionRouteVertex(connectionId, vertexIndex, position, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("connections.route-vertex.insert");
        }

        return edited;
    }

    public bool TryMoveConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var edited = _host.TryMoveConnectionRouteVertex(connectionId, vertexIndex, position, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("connections.route-vertex.move");
        }

        return edited;
    }

    public bool TryRemoveConnectionRouteVertex(string connectionId, int vertexIndex, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var edited = _host.TryRemoveConnectionRouteVertex(connectionId, vertexIndex, updateStatus);
        if (edited)
        {
            PublishCommandExecuted("connections.route-vertex.remove");
        }

        return edited;
    }

    public bool TrySetNodeParameterValue(string nodeId, string parameterKey, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

        var edited = _host.TrySetNodeParameterValue(nodeId, parameterKey, value);
        if (edited)
        {
            PublishCommandExecuted("nodes.parameter.set");
        }

        return edited;
    }

    public void BreakConnectionsForPort(string nodeId, string portId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(portId);

        Execute("connections.break", () => _host.BreakConnectionsForPort(nodeId, portId));
    }

    public void PanBy(double deltaX, double deltaY)
        => Execute("viewport.pan", () => _host.PanBy(deltaX, deltaY));

    public void ZoomAt(double factor, GraphPoint screenAnchor)
        => Execute("viewport.zoom", () => _host.ZoomAt(factor, screenAnchor));

    public void UpdateViewportSize(double width, double height)
        => Execute("viewport.resize", () => _host.UpdateViewportSize(width, height));

    public void ResetView(bool updateStatus = true)
        => Execute("viewport.reset", () => _host.ResetView(updateStatus));

    public void FitToViewport(bool updateStatus = true)
        => Execute("viewport.fit", () => _host.FitToViewport(updateStatus));

    public void CenterViewOnNode(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        Execute("viewport.center-node", () => _host.CenterViewOnNode(nodeId));
    }

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus = true)
        => Execute("viewport.center", () => _host.CenterViewAt(worldPoint, updateStatus));

    public void SaveWorkspace()
        => Execute("workspace.save", _host.SaveWorkspace);

    public bool LoadWorkspace()
        => Execute("workspace.load", _host.LoadWorkspace);

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var hostCommandIds = _host.GetCommandDescriptors()
            .Select(descriptor => descriptor.Id)
            .ToHashSet(StringComparer.Ordinal);
        var executed = hostCommandIds.Contains(command.CommandId)
            ? _host.TryExecuteCommand(command)
            : TryExecutePluginCommand(command);
        if (!executed)
        {
            return false;
        }

        PublishCommandExecuted(command.CommandId);
        return true;
    }

    private void Execute(string commandId, Action action)
    {
        action();
        PublishCommandExecuted(commandId);
    }

    private T Execute<T>(string commandId, Func<T> action)
    {
        var result = action();
        PublishCommandExecuted(commandId);
        return result;
    }

}
