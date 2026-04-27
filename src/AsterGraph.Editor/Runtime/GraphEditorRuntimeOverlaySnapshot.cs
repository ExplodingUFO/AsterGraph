namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Aggregates host-owned runtime overlays and recent logs for the current graph.
/// </summary>
public sealed record GraphEditorRuntimeOverlaySnapshot(
    bool IsAvailable,
    IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> NodeOverlays,
    IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> ConnectionOverlays,
    IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> RecentLogs)
{
    public static GraphEditorRuntimeOverlaySnapshot Empty { get; } = new(false, [], [], []);
}
