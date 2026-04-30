namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorViewportBookmarkSnapshot(
    string Id,
    string Title,
    string ScopeId,
    double Zoom,
    double PanX,
    double PanY);
