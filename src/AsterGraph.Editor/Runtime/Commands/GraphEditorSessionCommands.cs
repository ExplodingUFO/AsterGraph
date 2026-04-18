using AsterGraph.Editor;
using AsterGraph.Editor.Models;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using System;
using System.Collections.Generic;

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

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null)
    {
        ArgumentNullException.ThrowIfNull(definitionId);
        Execute("nodes.add", () => _host.AddNode(definitionId, preferredWorldPosition));
    }

    public void DeleteSelection()
        => Execute("selection.delete", _host.DeleteSelection);

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(positions);
        Execute("nodes.move", () => _host.SetNodePositions(positions, updateStatus));
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

        Execute("connections.begin", () => _host.StartConnection(sourceNodeId, sourcePortId));
    }

    public void CompleteConnection(string targetNodeId, string targetPortId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPortId);

        Execute("connections.complete", () => _host.CompleteConnection(targetNodeId, targetPortId));
    }

    public void CancelPendingConnection()
        => Execute("connections.cancel", _host.CancelPendingConnection);

    public void DeleteConnection(string connectionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        Execute("connections.delete", () => _host.DeleteConnection(connectionId));
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

        var executed = _host.TryExecuteCommand(command);
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
