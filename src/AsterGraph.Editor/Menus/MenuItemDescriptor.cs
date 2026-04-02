using System.Windows.Input;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 与 UI 框架无关的菜单项描述，具体控件由视图层自行渲染。
/// </summary>
public sealed record MenuItemDescriptor
{
    /// <summary>
    /// 初始化菜单项描述。
    /// </summary>
    /// <param name="id">稳定菜单项标识。</param>
    /// <param name="header">菜单显示文本。</param>
    /// <param name="command">菜单命令。</param>
    /// <param name="commandParameter">命令参数。</param>
    /// <param name="children">子菜单项集合。</param>
    /// <param name="iconKey">可选图标键。</param>
    /// <param name="disabledReason">可选禁用原因。</param>
    /// <param name="isEnabled">菜单项是否启用。</param>
    /// <param name="isSeparator">是否为分隔符。</param>
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

    /// <summary>
    /// 稳定菜单项标识。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 菜单显示文本。
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// 菜单命令。
    /// </summary>
    public ICommand? Command { get; }

    /// <summary>
    /// 命令参数。
    /// </summary>
    public object? CommandParameter { get; }

    /// <summary>
    /// 子菜单项集合。
    /// </summary>
    public IReadOnlyList<MenuItemDescriptor> Children { get; }

    /// <summary>
    /// 可选图标键。
    /// </summary>
    public string? IconKey { get; }

    /// <summary>
    /// 可选禁用原因。
    /// </summary>
    public string? DisabledReason { get; }

    /// <summary>
    /// 菜单项是否启用。
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// 菜单项是否为分隔符。
    /// </summary>
    public bool IsSeparator { get; }

    /// <summary>
    /// 菜单项是否包含子菜单。
    /// </summary>
    public bool HasChildren => Children.Count > 0;

    /// <summary>
    /// 创建一个分隔符菜单项。
    /// </summary>
    /// <param name="id">稳定菜单项标识。</param>
    /// <returns>分隔符菜单项描述。</returns>
    public static MenuItemDescriptor Separator(string id)
        => new(id, string.Empty, isEnabled: false, isSeparator: true);
}
