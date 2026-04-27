namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Host-owned runtime feedback for one node.
/// </summary>
public sealed record GraphEditorNodeRuntimeOverlaySnapshot(
    string NodeId,
    GraphEditorRuntimeOverlayStatus Status,
    double? ElapsedMilliseconds = null,
    string? OutputPreview = null,
    int WarningCount = 0,
    int ErrorCount = 0,
    string? ErrorMessage = null,
    DateTimeOffset? LastRunAtUtc = null);
