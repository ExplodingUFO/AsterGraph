using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasContextMenuSnapshot(
    string? SelectedNodeId,
    IReadOnlyList<string> SelectedNodeIds,
    IReadOnlyList<INodeDefinition> AvailableNodeDefinitions);

internal static class NodeCanvasContextMenuContextFactory
{
    public static ContextMenuContext CreateCanvasContext(
        NodeCanvasContextMenuSnapshot snapshot,
        GraphPoint worldPosition,
        bool useSelectionTools,
        IGraphHostContext? hostContext = null)
        => new(
            useSelectionTools ? ContextMenuTargetKind.Selection : ContextMenuTargetKind.Canvas,
            worldPosition,
            selectedNodeId: snapshot.SelectedNodeId,
            selectedNodeIds: snapshot.SelectedNodeIds,
            availableNodeDefinitions: snapshot.AvailableNodeDefinitions,
            hostContext: hostContext);

    public static ContextMenuContext CreateNodeContext(
        NodeCanvasContextMenuSnapshot snapshot,
        GraphPoint worldPosition,
        string clickedNodeId,
        bool useSelectionTools,
        IGraphHostContext? hostContext = null)
        => new(
            useSelectionTools ? ContextMenuTargetKind.Selection : ContextMenuTargetKind.Node,
            worldPosition,
            selectedNodeId: snapshot.SelectedNodeId,
            selectedNodeIds: snapshot.SelectedNodeIds,
            clickedNodeId: clickedNodeId,
            availableNodeDefinitions: snapshot.AvailableNodeDefinitions,
            hostContext: hostContext);

    public static ContextMenuContext CreatePortContext(
        NodeCanvasContextMenuSnapshot snapshot,
        GraphPoint worldPosition,
        string nodeId,
        string portId,
        IGraphHostContext? hostContext = null)
        => new(
            ContextMenuTargetKind.Port,
            worldPosition,
            selectedNodeId: snapshot.SelectedNodeId,
            selectedNodeIds: snapshot.SelectedNodeIds,
            clickedNodeId: nodeId,
            clickedPortNodeId: nodeId,
            clickedPortId: portId,
            availableNodeDefinitions: snapshot.AvailableNodeDefinitions,
            hostContext: hostContext);

    public static ContextMenuContext CreateConnectionContext(
        NodeCanvasContextMenuSnapshot snapshot,
        GraphPoint worldPosition,
        string connectionId,
        IGraphHostContext? hostContext = null)
        => new(
            ContextMenuTargetKind.Connection,
            worldPosition,
            selectedNodeId: snapshot.SelectedNodeId,
            selectedNodeIds: snapshot.SelectedNodeIds,
            selectedConnectionId: connectionId,
            selectedConnectionIds: [connectionId],
            clickedConnectionId: connectionId,
            availableNodeDefinitions: snapshot.AvailableNodeDefinitions,
            hostContext: hostContext);
}
