namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes compatible node definitions for the current pending connection.
/// </summary>
public sealed record GraphEditorCompatibleNodeSearchSnapshot(
    bool HasPendingConnection,
    string? SourceNodeId,
    string? SourcePortId,
    IReadOnlyList<GraphEditorCompatibleNodeDefinitionSnapshot> Results,
    string? EmptyReason);
