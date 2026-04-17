using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Kernel.Internal;

internal interface IGraphEditorKernelCommandRouterHost
{
    GraphEditorBehaviorOptions BehaviorOptions { get; }

    GraphDocument Document { get; }

    int SelectedNodeCount { get; }

    bool CanEditSelectedNodeParameters { get; }

    GraphEditorPendingConnectionSnapshot PendingConnection { get; }

    double ViewportWidth { get; }

    double ViewportHeight { get; }

    bool WorkspaceExists { get; }

    void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition);

    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);

    void DeleteSelection();

    void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus);

    bool TrySetSelectedNodeParameterValue(string parameterKey, object? value);

    void StartConnection(string sourceNodeId, string sourcePortId);

    void CompleteConnection(string targetNodeId, string targetPortId);

    void CancelPendingConnection();

    void DeleteConnection(string connectionId);

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
            new GraphEditorCommandDescriptorSnapshot(
                "nodes.add",
                _host.BehaviorOptions.Commands.Nodes.AllowCreate),
            new GraphEditorCommandDescriptorSnapshot(
                "selection.set",
                true),
            new GraphEditorCommandDescriptorSnapshot(
                "selection.delete",
                _host.SelectedNodeCount > 0 && _host.BehaviorOptions.Commands.Nodes.AllowDelete),
            new GraphEditorCommandDescriptorSnapshot(
                "nodes.move",
                _host.BehaviorOptions.Commands.Nodes.AllowMove),
            new GraphEditorCommandDescriptorSnapshot(
                "nodes.parameters.set",
                _host.CanEditSelectedNodeParameters,
                _host.CanEditSelectedNodeParameters
                    ? null
                    : "Parameter editing requires node-edit permissions and a shared node definition selection."),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.start",
                _host.BehaviorOptions.Commands.Connections.AllowCreate),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.complete",
                _host.PendingConnection.HasPendingConnection && _host.BehaviorOptions.Commands.Connections.AllowCreate),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.connect",
                _host.BehaviorOptions.Commands.Connections.AllowCreate),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.cancel",
                _host.PendingConnection.HasPendingConnection),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.delete",
                _host.BehaviorOptions.Commands.Connections.AllowDelete),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.break-port",
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.disconnect-incoming",
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.disconnect-outgoing",
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            new GraphEditorCommandDescriptorSnapshot(
                "connections.disconnect-all",
                _host.BehaviorOptions.Commands.Connections.AllowDisconnect),
            new GraphEditorCommandDescriptorSnapshot(
                "viewport.fit",
                _host.Document.Nodes.Count > 0 && _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            new GraphEditorCommandDescriptorSnapshot(
                "viewport.pan",
                true),
            new GraphEditorCommandDescriptorSnapshot(
                "viewport.resize",
                true),
            new GraphEditorCommandDescriptorSnapshot(
                "viewport.reset",
                true),
            new GraphEditorCommandDescriptorSnapshot(
                "viewport.center-node",
                _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            new GraphEditorCommandDescriptorSnapshot(
                "viewport.center",
                _host.ViewportWidth > 0 && _host.ViewportHeight > 0),
            new GraphEditorCommandDescriptorSnapshot(
                "workspace.save",
                _host.BehaviorOptions.Commands.Workspace.AllowSave,
                _host.BehaviorOptions.Commands.Workspace.AllowSave ? null : "Snapshot saving is disabled by host permissions."),
            new GraphEditorCommandDescriptorSnapshot(
                "workspace.load",
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

            case "nodes.parameters.set":
                if (!TryGetRequiredArgument(command, "parameterKey", out var parameterKey)
                    || !command.TryGetArgument("value", out var parameterValue))
                {
                    return false;
                }

                return _host.TrySetSelectedNodeParameterValue(parameterKey, parameterValue);

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

                _host.CompleteConnection(completeTargetNodeId, completeTargetPortId);
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
                _host.CompleteConnection(targetNodeId, targetPortId);
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
