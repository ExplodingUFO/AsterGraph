using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Represents one host-consumable action descriptor that can be rendered into menus, rails, and palettes.
/// </summary>
public sealed class AsterGraphHostedActionDescriptor
{
    private readonly Func<bool> _execute;

    public AsterGraphHostedActionDescriptor(
        string id,
        string title,
        string group,
        Func<bool> execute,
        bool canExecute,
        string? iconKey = null,
        string? defaultShortcut = null,
        string? disabledReason = null,
        string? commandId = null,
        GraphEditorCommandSourceKind? commandSource = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        ArgumentNullException.ThrowIfNull(execute);

        Id = id;
        Title = title;
        Group = group;
        IconKey = iconKey;
        DefaultShortcut = defaultShortcut;
        DisabledReason = disabledReason;
        CommandId = commandId;
        CommandSource = commandSource;
        CanExecute = canExecute;
        _execute = execute;
    }

    public string Id { get; }

    public string Title { get; }

    public string Group { get; }

    public string? IconKey { get; }

    public string? DefaultShortcut { get; }

    public string? DisabledReason { get; }

    public string? CommandId { get; }

    public GraphEditorCommandSourceKind? CommandSource { get; }

    public bool CanExecute { get; }

    public bool TryExecute()
        => CanExecute && _execute();
}
