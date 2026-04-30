namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorSnapGuideSnapshot(
    bool IsAvailable,
    double GridSize,
    bool SelectionOnly,
    IReadOnlyList<GraphEditorSnapGuideItemSnapshot> Items,
    string? EmptyReason);
