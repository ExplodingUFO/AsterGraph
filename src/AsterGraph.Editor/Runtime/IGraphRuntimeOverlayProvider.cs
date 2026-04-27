namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Provides host-owned runtime feedback without making the editor execute graphs.
/// </summary>
public interface IGraphRuntimeOverlayProvider
{
    IReadOnlyList<GraphEditorNodeRuntimeOverlaySnapshot> GetNodeOverlays();

    IReadOnlyList<GraphEditorConnectionRuntimeOverlaySnapshot> GetConnectionOverlays();

    IReadOnlyList<GraphEditorRuntimeLogEntrySnapshot> GetRecentLogs();
}
