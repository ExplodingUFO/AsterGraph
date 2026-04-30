namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorGraphItemSearchResultSnapshot(
    string Id,
    GraphEditorGraphItemSearchResultKind Kind,
    string ScopeId,
    string SourceId,
    string Title,
    string Subtitle,
    string? NodeId = null,
    string? GroupId = null,
    string? ConnectionId = null,
    string? IssueCode = null);
