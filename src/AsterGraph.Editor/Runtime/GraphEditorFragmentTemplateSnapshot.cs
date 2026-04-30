namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing snapshot of one persisted fragment template entry.
/// </summary>
public sealed record GraphEditorFragmentTemplateSnapshot(
    string Name,
    string Path,
    int NodeCount,
    int ConnectionCount,
    DateTime LastModified,
    int GroupCount = 0)
{
    /// <summary>
    /// Human-readable summary of the template payload size.
    /// </summary>
    public string Summary => GroupCount > 0
        ? $"{NodeCount} nodes  ·  {ConnectionCount} connections  ·  {GroupCount} groups"
        : $"{NodeCount} nodes  ·  {ConnectionCount} connections";

    /// <summary>
    /// Describes the action performed when this template is imported.
    /// </summary>
    public string ActionDescription => "Import fragment into workspace";
}
