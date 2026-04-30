namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable source-backed item projected for host-side navigator and outline surfaces.
/// </summary>
/// <param name="Id">Stable projection item identifier.</param>
/// <param name="Kind">Source kind represented by this item.</param>
/// <param name="SourceId">Stable source identifier in the current graph scope.</param>
/// <param name="ParentItemId">Parent projection item identifier, if this item is nested.</param>
/// <param name="Title">Display title projected from the source snapshot.</param>
/// <param name="Subtitle">Optional source-backed secondary label.</param>
/// <param name="Depth">Zero-based outline nesting depth.</param>
/// <param name="IsSelected">Whether the represented source is selected.</param>
/// <param name="IsPrimarySelection">Whether the represented source is the primary selection.</param>
/// <param name="IsVisibleInActiveScope">Whether the represented source is directly visible in the active scope projection.</param>
/// <param name="IsCollapsed">Whether the represented source is collapsed.</param>
/// <param name="ChildScopeId">Composite child scope owned by the represented node, if any.</param>
public sealed record GraphEditorNavigatorOutlineItemSnapshot(
    string Id,
    GraphEditorNavigatorOutlineItemKind Kind,
    string SourceId,
    string? ParentItemId,
    string Title,
    string? Subtitle,
    int Depth,
    bool IsSelected,
    bool IsPrimarySelection,
    bool IsVisibleInActiveScope,
    bool IsCollapsed,
    string? ChildScopeId = null);
