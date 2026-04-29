namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one stable command on the editor control plane.
/// </summary>
public sealed record GraphEditorCommandDescriptorSnapshot
{
    public GraphEditorCommandDescriptorSnapshot(
        string id,
        bool isEnabled,
        string? disabledReason = null,
        string? recoveryHint = null,
        string? recoveryCommandId = null)
        : this(
            id,
            GraphEditorCommandDescriptorCatalog.GetTitle(id),
            GraphEditorCommandDescriptorCatalog.GetGroup(id),
            GraphEditorCommandDescriptorCatalog.GetIconKey(id),
            GraphEditorCommandDescriptorCatalog.GetDefaultShortcut(id),
            GraphEditorCommandSourceKind.Kernel,
            isEnabled,
            disabledReason,
            recoveryHint,
            recoveryCommandId)
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
        string? disabledReason = null,
        string? recoveryHint = null,
        string? recoveryCommandId = null)
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
        RecoveryHint = string.IsNullOrWhiteSpace(recoveryHint) ? null : recoveryHint.Trim();
        RecoveryCommandId = string.IsNullOrWhiteSpace(recoveryCommandId) ? null : recoveryCommandId.Trim();
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

    public string? RecoveryHint { get; }

    public string? RecoveryCommandId { get; }
}
