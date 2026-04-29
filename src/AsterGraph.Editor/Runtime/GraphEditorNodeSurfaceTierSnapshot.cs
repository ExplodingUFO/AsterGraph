using System.ComponentModel;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of one resolved size-driven node-surface tier.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed record GraphEditorNodeSurfaceTierSnapshot(
    string Key,
    double MinWidth,
    double MinHeight,
    IReadOnlyList<string> VisibleSectionKeys)
{
    public bool ShowsSection(string sectionKey)
        => VisibleSectionKeys.Contains(sectionKey, StringComparer.Ordinal);
}
