namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Host-owned runtime feedback for one connection.
/// </summary>
public sealed record GraphEditorConnectionRuntimeOverlaySnapshot(
    string ConnectionId,
    GraphEditorRuntimeOverlayStatus Status,
    string? ValuePreview = null,
    string? PayloadType = null,
    int? ItemCount = null,
    bool IsStale = false);
