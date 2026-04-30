namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorGraphItemSearchQuery(
    string? SearchText = null,
    GraphEditorGraphItemSearchResultKind? Kind = null,
    string? ScopeId = null,
    int Limit = 100);
