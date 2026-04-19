using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of one resolved node-group boundary.
/// </summary>
/// <param name="Id">Stable group identifier.</param>
/// <param name="Title">Display title for the group.</param>
/// <param name="Position">Resolved top-left world position.</param>
/// <param name="Size">Resolved group size in world space.</param>
/// <param name="ContentPosition">Minimum member-node top-left world position.</param>
/// <param name="ContentSize">Minimum member-node bounds size without padding.</param>
/// <param name="ExtraPadding">Persisted per-edge padding envelope around the member-node bounds.</param>
/// <param name="NodeIds">Stable member node identifiers.</param>
/// <param name="IsCollapsed">Whether the group is currently collapsed.</param>
public sealed record GraphEditorNodeGroupSnapshot(
    string Id,
    string Title,
    GraphPoint Position,
    GraphSize Size,
    GraphPoint ContentPosition,
    GraphSize ContentSize,
    GraphPadding ExtraPadding,
    IReadOnlyList<string> NodeIds,
    bool IsCollapsed);
