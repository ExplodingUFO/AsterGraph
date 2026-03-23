using System.Windows.Input;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 与 UI 框架无关的菜单项描述，具体控件由视图层自行渲染。
/// </summary>
public sealed record MenuItemDescriptor
{
    public MenuItemDescriptor(
        string id,
        string header,
        ICommand? command = null,
        object? commandParameter = null,
        IReadOnlyList<MenuItemDescriptor>? children = null,
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
        CommandParameter = commandParameter;
        Children = normalizedChildren;
        IconKey = string.IsNullOrWhiteSpace(iconKey) ? null : iconKey.Trim();
        DisabledReason = string.IsNullOrWhiteSpace(disabledReason) ? null : disabledReason.Trim();
        IsEnabled = isSeparator ? false : isEnabled;
        IsSeparator = isSeparator;
    }

    public string Id { get; }

    public string Header { get; }

    public ICommand? Command { get; }

    public object? CommandParameter { get; }

    public IReadOnlyList<MenuItemDescriptor> Children { get; }

    public string? IconKey { get; }

    public string? DisabledReason { get; }

    public bool IsEnabled { get; }

    public bool IsSeparator { get; }

    public bool HasChildren => Children.Count > 0;

    public static MenuItemDescriptor Separator(string id)
        => new(id, string.Empty, isEnabled: false, isSeparator: true);
}
