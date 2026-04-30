namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable searchable/filterable runtime template palette projection.
/// </summary>
public sealed record GraphEditorTemplatePaletteSnapshot(
    string SearchText,
    GraphEditorTemplatePaletteItemKind? Kind,
    GraphEditorTemplatePaletteSourceKind? SourceKind,
    string? Category,
    IReadOnlyList<GraphEditorTemplatePaletteItemSnapshot> Items)
{
    public int Count => Items.Count;
}
