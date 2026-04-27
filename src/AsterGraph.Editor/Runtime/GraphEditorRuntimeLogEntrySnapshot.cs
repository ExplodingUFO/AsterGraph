namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Host-owned runtime log entry safe for debug panels and support evidence.
/// </summary>
public sealed record GraphEditorRuntimeLogEntrySnapshot(
    string Id,
    DateTimeOffset TimestampUtc,
    GraphEditorRuntimeOverlayStatus Status,
    string Message,
    string? ScopeId = null,
    string? NodeId = null,
    string? ConnectionId = null);
