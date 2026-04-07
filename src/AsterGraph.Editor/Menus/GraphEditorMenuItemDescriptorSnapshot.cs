using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// Framework-neutral menu item descriptor using stable command invocation data.
/// </summary>
public sealed record GraphEditorMenuItemDescriptorSnapshot
{
    public GraphEditorMenuItemDescriptorSnapshot(
        string id,
        string header,
        GraphEditorCommandInvocationSnapshot? command = null,
        IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot>? children = null,
        string? iconKey = null,
        string? disabledReason = null,
        bool isEnabled = true,
        bool isSeparator = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (!isSeparator)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(header);
        }

        if (isSeparator && command is not null)
        {
            throw new ArgumentException("Separator items cannot carry commands.", nameof(command));
        }

        var normalizedChildren = children ?? [];
        if (isSeparator && normalizedChildren.Count > 0)
        {
            throw new ArgumentException("Separator items cannot have children.", nameof(children));
        }

        Id = id.Trim();
        Header = isSeparator ? string.Empty : header.Trim();
        Command = command;
        Children = normalizedChildren;
        IconKey = string.IsNullOrWhiteSpace(iconKey) ? null : iconKey.Trim();
        DisabledReason = string.IsNullOrWhiteSpace(disabledReason) ? null : disabledReason.Trim();
        IsEnabled = isSeparator ? false : isEnabled;
        IsSeparator = isSeparator;
    }

    public string Id { get; }

    public string Header { get; }

    public GraphEditorCommandInvocationSnapshot? Command { get; }

    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Children { get; }

    public string? IconKey { get; }

    public string? DisabledReason { get; }

    public bool IsEnabled { get; }

    public bool IsSeparator { get; }

    public bool HasChildren => Children.Count > 0;

    public static GraphEditorMenuItemDescriptorSnapshot Separator(string id)
        => new(id, string.Empty, isEnabled: false, isSeparator: true);
}
