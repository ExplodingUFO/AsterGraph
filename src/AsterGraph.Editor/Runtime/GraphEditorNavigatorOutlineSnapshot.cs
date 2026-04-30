using System.Collections.Generic;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable source-backed navigator/outline projection for the active graph scope.
/// </summary>
/// <param name="ScopeNavigation">Current active-scope navigation snapshot.</param>
/// <param name="Items">Flattened outline items projected from scope, group, node, connection, and selection snapshots.</param>
public sealed record GraphEditorNavigatorOutlineSnapshot(
    GraphEditorScopeNavigationSnapshot ScopeNavigation,
    IReadOnlyList<GraphEditorNavigatorOutlineItemSnapshot> Items);
