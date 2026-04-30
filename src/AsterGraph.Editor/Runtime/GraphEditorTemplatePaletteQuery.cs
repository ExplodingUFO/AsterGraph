namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Search and filter request for the runtime template palette projection.
/// </summary>
public sealed record GraphEditorTemplatePaletteQuery(
    string? SearchText = null,
    GraphEditorTemplatePaletteItemKind? Kind = null,
    GraphEditorTemplatePaletteSourceKind? SourceKind = null,
    string? Category = null);
