using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Menus;

public sealed record ContextMenuContext
{
    public ContextMenuContext(
        ContextMenuTargetKind targetKind,
        GraphPoint worldPosition,
        string? selectedNodeId = null,
        string? selectedConnectionId = null,
        string? clickedNodeId = null,
        string? clickedPortNodeId = null,
        string? clickedPortId = null,
        string? clickedConnectionId = null,
        IReadOnlyList<INodeDefinition>? availableNodeDefinitions = null)
    {
        if ((clickedPortNodeId is null) != (clickedPortId is null))
        {
            throw new ArgumentException("Clicked port node and port IDs must be provided together.");
        }

        TargetKind = targetKind;
        WorldPosition = worldPosition;
        SelectedNodeId = selectedNodeId;
        SelectedConnectionId = selectedConnectionId;
        ClickedNodeId = clickedNodeId;
        ClickedPortNodeId = clickedPortNodeId;
        ClickedPortId = clickedPortId;
        ClickedConnectionId = clickedConnectionId;
        AvailableNodeDefinitions = availableNodeDefinitions ?? [];
    }

    public ContextMenuTargetKind TargetKind { get; }

    public GraphPoint WorldPosition { get; }

    public string? SelectedNodeId { get; }

    public string? SelectedConnectionId { get; }

    public string? ClickedNodeId { get; }

    public string? ClickedPortNodeId { get; }

    public string? ClickedPortId { get; }

    public string? ClickedConnectionId { get; }

    public IReadOnlyList<INodeDefinition> AvailableNodeDefinitions { get; }
}
