namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing projection of one searchable node or fragment template palette item.
/// </summary>
public sealed record GraphEditorTemplatePaletteItemSnapshot(
    string Id,
    GraphEditorTemplatePaletteItemKind Kind,
    GraphEditorTemplatePaletteSourceKind SourceKind,
    string SourceId,
    string TemplateKey,
    string Title,
    string Category,
    string Subtitle,
    string Description,
    string Summary,
    string ActionDescription,
    int NodeCount,
    int ConnectionCount,
    int GroupCount)
{
    public bool HasGroups => GroupCount > 0;
}
