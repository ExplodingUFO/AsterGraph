namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one stable command on the editor control plane.
/// </summary>
public sealed record GraphEditorCommandDescriptorSnapshot
{
    public GraphEditorCommandDescriptorSnapshot(string id, bool isEnabled, string? disabledReason = null)
        : this(
            id,
            GraphEditorCommandDescriptorCatalog.GetTitle(id),
            GraphEditorCommandDescriptorCatalog.GetGroup(id),
            GraphEditorCommandDescriptorCatalog.GetIconKey(id),
            GraphEditorCommandDescriptorCatalog.GetDefaultShortcut(id),
            GraphEditorCommandSourceKind.Kernel,
            isEnabled,
            disabledReason)
    {
    }

    public GraphEditorCommandDescriptorSnapshot(
        string id,
        string title,
        string group,
        string? iconKey,
        string? defaultShortcut,
        GraphEditorCommandSourceKind source,
        bool isEnabled,
        string? disabledReason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(group);

        Id = id.Trim();
        Title = title.Trim();
        Group = group.Trim();
        IconKey = string.IsNullOrWhiteSpace(iconKey) ? null : iconKey.Trim();
        DefaultShortcut = string.IsNullOrWhiteSpace(defaultShortcut) ? null : defaultShortcut.Trim();
        Source = source;
        IsEnabled = isEnabled;
        DisabledReason = string.IsNullOrWhiteSpace(disabledReason) ? null : disabledReason.Trim();
    }

    public string Id { get; }

    public string Title { get; }

    public string Group { get; }

    public string? IconKey { get; }

    public string? DefaultShortcut { get; }

    public GraphEditorCommandSourceKind Source { get; }

    public bool CanExecute => IsEnabled;

    public bool IsEnabled { get; }

    public string? DisabledReason { get; }
}
