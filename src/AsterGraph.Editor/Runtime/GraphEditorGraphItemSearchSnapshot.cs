namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorGraphItemSearchSnapshot(
    string SearchText,
    GraphEditorGraphItemSearchResultKind? Kind,
    string? ScopeId,
    int Limit,
    IReadOnlyList<GraphEditorGraphItemSearchResultSnapshot> Results,
    string? EmptyReason);
