using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Kernel.Internal.Layout;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using System.Threading;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelCommandRouterHost
{
    GraphEditorBehaviorOptions BehaviorOptions { get; }

    GraphDocument Document { get; }

    int SelectedNodeCount { get; }

    bool CanUndo { get; }

    bool CanRedo { get; }

    bool CanCopySelection { get; }

    bool CanPaste { get; }

    bool CanEditSelectedNodeParameters { get; }

    bool CanAlignSelection { get; }

    bool CanDistributeSelection { get; }

    GraphEditorPendingConnectionSnapshot PendingConnection { get; }

    double ViewportWidth { get; }

    double ViewportHeight { get; }

    bool WorkspaceExists { get; }

    bool CanNavigateToParentGraphScope { get; }

    void Undo();

    void Redo();

    void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition);

    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);

    void DeleteSelection();

    Task<bool> TryCopySelectionAsync(CancellationToken cancellationToken);

    Task<bool> TryPasteSelectionAsync(CancellationToken cancellationToken);

    void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus);

    bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus);

    string TryCreateNodeGroupFromSelection(string title);

    bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed);

    bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus);

    bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus);

    bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus);

    bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus);

    string TryPromoteNodeGroupToComposite(string groupId, string? title, bool updateStatus);

    string TryWrapSelectionToComposite(string? title, bool updateStatus);

    string TryExposeCompositePort(string compositeNodeId, string childNodeId, string childPortId, string? label, bool updateStatus);

    bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus);

    bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus);

    bool TryReturnToParentGraphScope(bool updateStatus);

    bool TrySetSelectedNodeParameterValue(string parameterKey, object? value);

    bool TryApplySelectionLayout(GraphEditorSelectionLayoutOperation operation, bool updateStatus);

    void StartConnection(string sourceNodeId, string sourcePortId);

    void CompleteConnection(GraphConnectionTargetRef target);

    void CancelPendingConnection();

    void DeleteConnection(string connectionId);

    bool TryReconnectConnection(string connectionId, bool updateStatus);

    bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus);

    void DisconnectConnection(string connectionId);

    void BreakConnectionsForPort(string nodeId, string portId);

    void DisconnectIncoming(string nodeId);

    void DisconnectOutgoing(string nodeId);

    void DisconnectAll(string nodeId);

    void FitToViewport(bool updateStatus);

    void PanBy(double deltaX, double deltaY);

    void UpdateViewportSize(double width, double height);

    void ResetView(bool updateStatus);

    void CenterViewOnNode(string nodeId);

    void CenterViewAt(GraphPoint worldPoint, bool updateStatus);

    void SaveWorkspace();

    bool LoadWorkspace();
}

internal sealed class GraphEditorKernelCommandRouter
{
    private readonly IGraphEditorKernelCommandRouterHost _host;

    public GraphEditorKernelCommandRouter(IGraphEditorKernelCommandRouterHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        =>
        [
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.add",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "selection.set",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "selection.delete",
                GraphEditorCommandSourceKind.Kernel,
                _host.SelectedNodeCount > 0 && _host.BehaviorOptions.Commands.Nodes.AllowDelete),
            GraphEditorCommandDescriptorCatalog.Create(
                "clipboard.copy",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanCopySelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "clipboard.paste",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanPaste),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.move",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.resize",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "nodes.parameters.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanEditSelectedNodeParameters,
                _host.CanEditSelectedNodeParameters
                    ? null
                    : "Parameter editing requires node-edit permissions and a shared node definition selection."),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.create",
                GraphEditorCommandSourceKind.Kernel,
                _host.SelectedNodeCount > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.collapse",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.move",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.resize",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.membership.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "groups.promote",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Groups?.Count > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-left",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-center",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-right",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-top",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-middle",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.align-bottom",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanAlignSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.distribute-horizontal",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanDistributeSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "layout.distribute-vertical",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanDistributeSelection),
            GraphEditorCommandDescriptorCatalog.Create(
                "composites.wrap-selection",
                GraphEditorCommandSourceKind.Kernel,
                _host.SelectedNodeCount > 0 && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "composites.expose-port",
                GraphEditorCommandSourceKind.Kernel,
                (_host.Document.Nodes.Any(node => node.Composite is not null) || _host.CanNavigateToParentGraphScope)
                && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "composites.unexpose-port",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Any(node =>
                    node.Composite is not null
                    && ((node.Composite.Inputs?.Count ?? 0) > 0 || (node.Composite.Outputs?.Count ?? 0) > 0))
                && _host.BehaviorOptions.Commands.Nodes.AllowMove),
            GraphEditorCommandDescriptorCatalog.Create(
                "scopes.enter",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Any(node => node.Composite is not null)),
            GraphEditorCommandDescriptorCatalog.Create(
                "scopes.exit",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanNavigateToParentGraphScope),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.start",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.complete",
                GraphEditorCommandSourceKind.Kernel,
                _host.PendingConnection.HasPendingConnection && _host.BehaviorOptions.Commands.Connections.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.connect",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowCreate),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.cancel",
                GraphEditorCommandSourceKind.Kernel,
                _host.PendingConnection.HasPendingConnection),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.delete",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDelete),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.note.set",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && (_host.BehaviorOptions.Commands.Connections.AllowCreate
                    || _host.BehaviorOptions.Commands.Connections.AllowDelete
                    || _host.BehaviorOptions.Commands.Connections.AllowDisconnect)),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.reconnect",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Connections.Count > 0
                && _host.BehaviorOptions.Commands.Connections.AllowCreate
                && _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.break-port",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect-incoming",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect-outgoing",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "connections.disconnect-all",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create(
                "history.undo",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanUndo),
            GraphEditorCommandDescriptorCatalog.Create(
                "history.redo",
                GraphEditorCommandSourceKind.Kernel,
                _host.CanRedo),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.fit",
                GraphEditorCommandSourceKind.Kernel,
                _host.Document.Nodes.Count > 0 && _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.pan",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.resize",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.reset",
                GraphEditorCommandSourceKind.Kernel,
                true),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.center-node",
                GraphEditorCommandSourceKind.Kernel,
                _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "viewport.center",
                GraphEditorCommandSourceKind.Kernel,
                _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            GraphEditorCommandDescriptorCatalog.Create(
                "workspace.save",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Workspace.AllowSave,
                _host.BehaviorOptions.Commands.Workspace.AllowSave ? null : "Snapshot saving is disabled by host permissions."),
            GraphEditorCommandDescriptorCatalog.Create(
                "workspace.load",
                GraphEditorCommandSourceKind.Kernel,
                _host.BehaviorOptions.Commands.Workspace.AllowLoad && _host.WorkspaceExists,
                !_host.BehaviorOptions.Commands.Workspace.AllowLoad
                    ? "Snapshot loading is disabled by host permissions."
                    : _host.WorkspaceExists
                        ? null
                        : "No saved snapshot yet. Save once to create one."),
        ];

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (TryResolveSelectionLayoutOperation(command.CommandId, out var layoutOperation))
        {
            return _host.TryApplySelectionLayout(layoutOperation, ResolveOptionalUpdateStatus(command, "updateStatus"));
        }

        switch (command.CommandId)
        {
            case "nodes.add":
                if (!TryGetRequiredArgument(command, "definitionId", out var definitionValue))
                {
                    return false;
                }

                var definitionId = new NodeDefinitionId(definitionValue);
                GraphPoint? worldPosition = null;
                if (command.TryGetArgument("worldX", out var worldX)
                    && command.TryGetArgument("worldY", out var worldY)
                    && double.TryParse(worldX, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedX)
                    && double.TryParse(worldY, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedY))
                {
                    worldPosition = new GraphPoint(parsedX, parsedY);
                }

                _host.AddNode(definitionId, worldPosition);
                return true;

            case "selection.set":
                var nodeIds = command.GetArguments("nodeId")
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList();
                if (nodeIds.Count == 0)
                {
                    return false;
                }

                command.TryGetArgument("primaryNodeId", out var primaryNodeId);
                _host.SetSelection(nodeIds, primaryNodeId, ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "selection.delete":
                _host.DeleteSelection();
                return true;

            case "clipboard.copy":
                _ = _host.TryCopySelectionAsync(CancellationToken.None);
                return true;

            case "clipboard.paste":
                _ = _host.TryPasteSelectionAsync(CancellationToken.None);
                return true;

            case "nodes.move":
                var positions = command.GetArguments("position")
                    .Select(ParseNodePosition)
                    .ToList();
                if (positions.Count == 0 || positions.Any(position => position is null))
                {
                    return false;
                }

                _host.SetNodePositions(positions.Select(position => position!).ToList(), ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "nodes.resize":
                if (!TryGetRequiredArgument(command, "nodeId", out var resizeNodeId)
                    || !TryGetDoubleArgument(command, "width", out var nodeWidth)
                    || !TryGetDoubleArgument(command, "height", out var nodeHeight))
                {
                    return false;
                }

                return _host.TrySetNodeSize(
                    resizeNodeId,
                    new GraphSize(nodeWidth, nodeHeight),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "nodes.parameters.set":
                if (!TryGetRequiredArgument(command, "parameterKey", out var parameterKey)
                    || !command.TryGetArgument("value", out var parameterValue))
                {
                    return false;
                }

                return _host.TrySetSelectedNodeParameterValue(parameterKey, parameterValue);

            case "groups.create":
                var title = command.TryGetArgument("title", out var rawTitle) && !string.IsNullOrWhiteSpace(rawTitle)
                    ? rawTitle
                    : "Group";
                return !string.IsNullOrWhiteSpace(_host.TryCreateNodeGroupFromSelection(title));

            case "groups.collapse":
                if (!TryGetRequiredArgument(command, "groupId", out var collapseGroupId)
                    || !TryGetBoolArgument(command, "isCollapsed", out var isCollapsed))
                {
                    return false;
                }

                return _host.TrySetNodeGroupCollapsed(collapseGroupId, isCollapsed);

            case "groups.move":
                if (!TryGetRequiredArgument(command, "groupId", out var moveGroupId)
                    || !TryGetDoubleArgument(command, "worldX", out var groupX)
                    || !TryGetDoubleArgument(command, "worldY", out var groupY))
                {
                    return false;
                }

                var moveMembers = !command.TryGetArgument("moveMemberNodes", out var rawMoveMembers)
                    || !bool.TryParse(rawMoveMembers, out var parsedMoveMembers)
                    || parsedMoveMembers;
                return _host.TrySetNodeGroupPosition(
                    moveGroupId,
                    new GraphPoint(groupX, groupY),
                    moveMembers,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "groups.resize":
                if (!TryGetRequiredArgument(command, "groupId", out var resizeGroupId)
                    || !TryGetDoubleArgument(command, "width", out var resizeWidth)
                    || !TryGetDoubleArgument(command, "height", out var resizeHeight))
                {
                    return false;
                }

                return _host.TrySetNodeGroupSize(
                    resizeGroupId,
                    new GraphSize(resizeWidth, resizeHeight),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "groups.membership.set":
                var membershipChanges = command.GetArguments("membership")
                    .Select(ParseGroupMembershipChange)
                    .ToList();
                if (membershipChanges.Count == 0 || membershipChanges.Any(change => change is null))
                {
                    return false;
                }

                return _host.TrySetNodeGroupMemberships(
                    membershipChanges.Select(change => change!).ToList(),
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "groups.promote":
                if (!TryGetRequiredArgument(command, "groupId", out var promoteGroupId))
                {
                    return false;
                }

                var promoteTitle = command.TryGetArgument("title", out var rawPromoteTitle) && !string.IsNullOrWhiteSpace(rawPromoteTitle)
                    ? rawPromoteTitle
                    : null;
                return !string.IsNullOrWhiteSpace(
                    _host.TryPromoteNodeGroupToComposite(
                        promoteGroupId,
                        promoteTitle,
                        ResolveOptionalUpdateStatus(command, "updateStatus")));

            case "composites.wrap-selection":
                var wrapTitle = command.TryGetArgument("title", out var rawWrapTitle) && !string.IsNullOrWhiteSpace(rawWrapTitle)
                    ? rawWrapTitle
                    : null;
                return !string.IsNullOrWhiteSpace(
                    _host.TryWrapSelectionToComposite(
                        wrapTitle,
                        ResolveOptionalUpdateStatus(command, "updateStatus")));

            case "composites.expose-port":
                if (!TryGetRequiredArgument(command, "compositeNodeId", out var compositeNodeId)
                    || !TryGetRequiredArgument(command, "childNodeId", out var childNodeId)
                    || !TryGetRequiredArgument(command, "childPortId", out var childPortId))
                {
                    return false;
                }

                var label = command.TryGetArgument("label", out var rawLabel) && !string.IsNullOrWhiteSpace(rawLabel)
                    ? rawLabel
                    : null;
                return !string.IsNullOrWhiteSpace(
                    _host.TryExposeCompositePort(
                        compositeNodeId,
                        childNodeId,
                        childPortId,
                        label,
                        ResolveOptionalUpdateStatus(command, "updateStatus")));

            case "composites.unexpose-port":
                if (!TryGetRequiredArgument(command, "compositeNodeId", out var unexposeCompositeNodeId)
                    || !TryGetRequiredArgument(command, "boundaryPortId", out var boundaryPortId))
                {
                    return false;
                }

                return _host.TryUnexposeCompositePort(
                    unexposeCompositeNodeId,
                    boundaryPortId,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "scopes.enter":
                if (!TryGetRequiredArgument(command, "compositeNodeId", out var scopeCompositeNodeId))
                {
                    return false;
                }

                return _host.TryEnterCompositeChildGraph(
                    scopeCompositeNodeId,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "scopes.exit":
                return _host.TryReturnToParentGraphScope(ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.start":
                if (!TryGetRequiredArgument(command, "sourceNodeId", out var sourceNodeId)
                    || !TryGetRequiredArgument(command, "sourcePortId", out var sourcePortId))
                {
                    return false;
                }

                _host.StartConnection(sourceNodeId, sourcePortId);
                return true;

            case "connections.complete":
                if (!TryGetRequiredArgument(command, "targetNodeId", out var completeTargetNodeId)
                    || !TryGetRequiredArgument(command, "targetPortId", out var completeTargetPortId))
                {
                    return false;
                }

                _host.CompleteConnection(CreateConnectionTarget(command, completeTargetNodeId, completeTargetPortId));
                return true;

            case "connections.connect":
                if (!TryGetRequiredArgument(command, "sourceNodeId", out var connectSourceNodeId)
                    || !TryGetRequiredArgument(command, "sourcePortId", out var connectSourcePortId)
                    || !TryGetRequiredArgument(command, "targetNodeId", out var targetNodeId)
                    || !TryGetRequiredArgument(command, "targetPortId", out var targetPortId))
                {
                    return false;
                }

                _host.StartConnection(connectSourceNodeId, connectSourcePortId);
                _host.CompleteConnection(CreateConnectionTarget(command, targetNodeId, targetPortId));
                return true;

            case "connections.cancel":
                _host.CancelPendingConnection();
                return true;

            case "connections.delete":
                if (!TryGetRequiredArgument(command, "connectionId", out var connectionId))
                {
                    return false;
                }

                _host.DeleteConnection(connectionId);
                return true;

            case "connections.disconnect":
                if (!TryGetRequiredArgument(command, "connectionId", out var disconnectConnectionId))
                {
                    return false;
                }

                _host.DisconnectConnection(disconnectConnectionId);
                return true;

            case "connections.note.set":
                if (!TryGetRequiredArgument(command, "connectionId", out var noteConnectionId))
                {
                    return false;
                }

                command.TryGetArgument("text", out var noteText);
                return _host.TrySetConnectionNoteText(
                    noteConnectionId,
                    noteText,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.reconnect":
                if (!TryGetRequiredArgument(command, "connectionId", out var reconnectConnectionId))
                {
                    return false;
                }

                return _host.TryReconnectConnection(
                    reconnectConnectionId,
                    ResolveOptionalUpdateStatus(command, "updateStatus"));

            case "connections.break-port":
                if (!TryGetRequiredArgument(command, "nodeId", out var nodeId)
                    || !TryGetRequiredArgument(command, "portId", out var portId))
                {
                    return false;
                }

                _host.BreakConnectionsForPort(nodeId, portId);
                return true;

            case "connections.disconnect-incoming":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectIncomingNodeId))
                {
                    return false;
                }

                _host.DisconnectIncoming(disconnectIncomingNodeId);
                return true;

            case "connections.disconnect-outgoing":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectOutgoingNodeId))
                {
                    return false;
                }

                _host.DisconnectOutgoing(disconnectOutgoingNodeId);
                return true;

            case "connections.disconnect-all":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectAllNodeId))
                {
                    return false;
                }

                _host.DisconnectAll(disconnectAllNodeId);
                return true;

            case "history.undo":
                _host.Undo();
                return true;

            case "history.redo":
                _host.Redo();
                return true;

            case "viewport.fit":
                _host.FitToViewport(updateStatus: true);
                return true;

            case "viewport.pan":
                if (!TryGetDoubleArgument(command, "deltaX", out var deltaX)
                    || !TryGetDoubleArgument(command, "deltaY", out var deltaY))
                {
                    return false;
                }

                _host.PanBy(deltaX, deltaY);
                return true;

            case "viewport.resize":
                if (!TryGetDoubleArgument(command, "width", out var width)
                    || !TryGetDoubleArgument(command, "height", out var height))
                {
                    return false;
                }

                _host.UpdateViewportSize(width, height);
                return true;

            case "viewport.reset":
                _host.ResetView(updateStatus: true);
                return true;

            case "viewport.center-node":
                if (!TryGetRequiredArgument(command, "nodeId", out var centerNodeId))
                {
                    return false;
                }

                _host.CenterViewOnNode(centerNodeId);
                return true;

            case "viewport.center":
                if (!TryGetDoubleArgument(command, "worldX", out var centerX)
                    || !TryGetDoubleArgument(command, "worldY", out var centerY))
                {
                    return false;
                }

                _host.CenterViewAt(new GraphPoint(centerX, centerY), ResolveOptionalUpdateStatus(command, "updateStatus"));
                return true;

            case "workspace.save":
                _host.SaveWorkspace();
                return true;

            case "workspace.load":
                _host.LoadWorkspace();
                return true;

            default:
                return false;
        }
    }

    private static bool ResolveOptionalUpdateStatus(GraphEditorCommandInvocationSnapshot command, string argumentName)
        => !command.TryGetArgument(argumentName, out var updateStatusValue)
            || !bool.TryParse(updateStatusValue, out var parsedUpdateStatus)
            || parsedUpdateStatus;

    private static GraphConnectionTargetRef CreateConnectionTarget(
        GraphEditorCommandInvocationSnapshot command,
        string targetNodeId,
        string targetId)
    {
        if (!command.TryGetArgument("targetKind", out var rawKind)
            || string.IsNullOrWhiteSpace(rawKind)
            || !Enum.TryParse<GraphConnectionTargetKind>(rawKind, ignoreCase: true, out var targetKind))
        {
            targetKind = GraphConnectionTargetKind.Port;
        }

        return new GraphConnectionTargetRef(targetNodeId, targetId, targetKind);
    }

    private static bool TryGetDoubleArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        out double value)
    {
        if (!command.TryGetArgument(name, out var rawValue)
            || !double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static bool TryGetBoolArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        out bool value)
    {
        if (!command.TryGetArgument(name, out var rawValue)
            || !bool.TryParse(rawValue, out value))
        {
            value = default;
            return false;
        }

        return true;
    }

    private static bool TryResolveSelectionLayoutOperation(
        string commandId,
        out GraphEditorSelectionLayoutOperation operation)
    {
        switch (commandId)
        {
            case "layout.align-left":
                operation = GraphEditorSelectionLayoutOperation.AlignLeft;
                return true;
            case "layout.align-center":
                operation = GraphEditorSelectionLayoutOperation.AlignCenter;
                return true;
            case "layout.align-right":
                operation = GraphEditorSelectionLayoutOperation.AlignRight;
                return true;
            case "layout.align-top":
                operation = GraphEditorSelectionLayoutOperation.AlignTop;
                return true;
            case "layout.align-middle":
                operation = GraphEditorSelectionLayoutOperation.AlignMiddle;
                return true;
            case "layout.align-bottom":
                operation = GraphEditorSelectionLayoutOperation.AlignBottom;
                return true;
            case "layout.distribute-horizontal":
                operation = GraphEditorSelectionLayoutOperation.DistributeHorizontally;
                return true;
            case "layout.distribute-vertical":
                operation = GraphEditorSelectionLayoutOperation.DistributeVertically;
                return true;
            default:
                operation = default;
                return false;
        }
    }

    private static NodePositionSnapshot? ParseNodePosition(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length != 3
            || string.IsNullOrWhiteSpace(parts[0])
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return null;
        }

        return new NodePositionSnapshot(parts[0], new GraphPoint(x, y));
    }

    private static GraphEditorNodeGroupMembershipChange? ParseGroupMembershipChange(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]))
        {
            return null;
        }

        return new GraphEditorNodeGroupMembershipChange(
            parts[0],
            string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1]);
    }

    private static bool TryGetRequiredArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        [NotNullWhen(true)] out string? value)
    {
        if (!command.TryGetArgument(name, out value) || string.IsNullOrWhiteSpace(value))
        {
            value = null;
            return false;
        }

        return true;
    }
}
