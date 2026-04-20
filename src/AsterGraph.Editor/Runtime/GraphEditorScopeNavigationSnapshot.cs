using System.Collections.Generic;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes the current active-scope navigation state.
/// </summary>
/// <param name="CurrentScopeId">Stable identifier of the current active graph scope.</param>
/// <param name="ParentScopeId">Stable identifier of the immediate parent graph scope, if any.</param>
/// <param name="CanNavigateToParent">Whether the session can navigate back to the parent scope.</param>
/// <param name="Breadcrumbs">Stable breadcrumb trail from the root scope to the active scope.</param>
public sealed record GraphEditorScopeNavigationSnapshot(
    string CurrentScopeId,
    string? ParentScopeId,
    bool CanNavigateToParent,
    IReadOnlyList<GraphEditorScopeBreadcrumbSnapshot> Breadcrumbs);
