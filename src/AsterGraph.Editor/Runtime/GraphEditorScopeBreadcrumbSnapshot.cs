namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one breadcrumb entry in the active graph scope navigation trail.
/// </summary>
/// <param name="ScopeId">Stable graph scope identifier.</param>
/// <param name="Title">Human-readable breadcrumb label.</param>
public sealed record GraphEditorScopeBreadcrumbSnapshot(
    string ScopeId,
    string Title);
