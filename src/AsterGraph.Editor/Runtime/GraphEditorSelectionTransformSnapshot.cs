using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorSelectionTransformSnapshot(
    bool HasSelection,
    IReadOnlyList<string> SelectedNodeIds,
    IReadOnlyList<string> SelectedConnectionIds,
    IReadOnlyList<string> SelectedGroupIds,
    GraphPoint? BoundsPosition,
    GraphSize? BoundsSize,
    GraphPoint PreviewDelta,
    IReadOnlyList<GraphEditorSelectionTransformItemSnapshot> Items,
    IReadOnlyList<string> RectangleNodeIds,
    IReadOnlyList<string> RectangleConnectionIds,
    string? EmptyReason);
