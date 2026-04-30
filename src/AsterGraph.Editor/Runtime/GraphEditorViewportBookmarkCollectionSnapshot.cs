using System.Collections.Generic;

namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorViewportBookmarkCollectionSnapshot(
    string CurrentScopeId,
    IReadOnlyList<GraphEditorViewportBookmarkSnapshot> Bookmarks,
    string? EmptyReason);
