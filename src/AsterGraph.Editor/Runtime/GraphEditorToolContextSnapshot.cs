namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one stable contextual tooling request.
/// </summary>
public sealed record GraphEditorToolContextSnapshot
{
    private GraphEditorToolContextSnapshot(
        GraphEditorToolContextKind kind,
        IReadOnlyList<string> selectedNodeIds,
        string? primarySelectedNodeId,
        string? nodeId,
        string? connectionId)
    {
        Kind = kind;
        SelectedNodeIds = selectedNodeIds
            .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(candidate => candidate.Trim())
            .ToList();
        PrimarySelectedNodeId = string.IsNullOrWhiteSpace(primarySelectedNodeId) ? null : primarySelectedNodeId.Trim();
        NodeId = string.IsNullOrWhiteSpace(nodeId) ? null : nodeId.Trim();
        ConnectionId = string.IsNullOrWhiteSpace(connectionId) ? null : connectionId.Trim();
    }

    public GraphEditorToolContextKind Kind { get; }

    public IReadOnlyList<string> SelectedNodeIds { get; }

    public string? PrimarySelectedNodeId { get; }

    public string? NodeId { get; }

    public string? ConnectionId { get; }

    public static GraphEditorToolContextSnapshot ForSelection(
        IReadOnlyList<string> selectedNodeIds,
        string? primarySelectedNodeId = null)
    {
        ArgumentNullException.ThrowIfNull(selectedNodeIds);
        return new(GraphEditorToolContextKind.Selection, selectedNodeIds, primarySelectedNodeId, null, null);
    }

    public static GraphEditorToolContextSnapshot ForNode(
        string nodeId,
        IReadOnlyList<string>? selectedNodeIds = null,
        string? primarySelectedNodeId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        return new(GraphEditorToolContextKind.Node, selectedNodeIds ?? [], primarySelectedNodeId, nodeId, null);
    }

    public static GraphEditorToolContextSnapshot ForConnection(
        string connectionId,
        IReadOnlyList<string>? selectedNodeIds = null,
        string? primarySelectedNodeId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);
        return new(GraphEditorToolContextKind.Connection, selectedNodeIds ?? [], primarySelectedNodeId, null, connectionId);
    }
}
