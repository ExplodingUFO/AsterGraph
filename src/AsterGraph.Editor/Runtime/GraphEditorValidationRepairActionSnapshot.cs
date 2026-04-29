namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Previewable quick repair action projected from a concrete validation issue.
/// </summary>
public sealed record GraphEditorValidationRepairActionSnapshot(
    string ActionId,
    string Label,
    string PreviewText,
    string IssueCode,
    string ScopeId,
    string? NodeId = null,
    string? ConnectionId = null,
    string? EndpointId = null,
    string? ParameterKey = null,
    int RouteVertexCount = 0);
