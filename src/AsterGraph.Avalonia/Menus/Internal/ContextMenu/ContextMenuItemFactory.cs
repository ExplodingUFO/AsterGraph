using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System.Windows.Input;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using CommunityToolkit.Mvvm.Input;

namespace AsterGraph.Avalonia.Menus.Internal;

/// <summary>
/// 负责将菜单描述符转换为 Avalonia 菜单控件节点。
/// </summary>
internal static class ContextMenuItemFactory
{
    internal static object BuildMenuItem(MenuItemDescriptor descriptor, ContextMenuStyleOptions style)
    {
        if (descriptor.IsSeparator)
        {
            return CreateSeparator(style);
        }

        var menuItem = CreateMenuItemCore(
            descriptor.Header,
            descriptor.Command,
            descriptor.CommandParameter,
            descriptor.IsEnabled,
            descriptor.DisabledReason,
            descriptor.IconKey,
            style);

        if (descriptor.HasChildren)
        {
            menuItem.ItemsSource = descriptor.Children
                .Select(child => BuildMenuItem(child, style))
                .ToList();
        }

        return menuItem;
    }

    internal static object BuildMenuItem(
        GraphEditorMenuItemDescriptorSnapshot descriptor,
        IGraphEditorCommands commands,
        ContextMenuStyleOptions style)
    {
        if (descriptor.IsSeparator)
        {
            return CreateSeparator(style);
        }

        IRelayCommand? command = null;
        if (descriptor.Command is not null)
        {
            command = new RelayCommand(
                () => commands.TryExecuteCommand(descriptor.Command),
                () => descriptor.IsEnabled);
        }

        var menuItem = CreateMenuItemCore(
            descriptor.Header,
            command,
            null,
            descriptor.IsEnabled,
            descriptor.DisabledReason,
            descriptor.IconKey,
            style);

        if (descriptor.HasChildren)
        {
            menuItem.ItemsSource = descriptor.Children
                .Select(child => BuildMenuItem(child, commands, style))
                .ToList();
        }

        return menuItem;
    }

    private static MenuItem CreateMenuItemCore(
        string header,
        ICommand? command,
        object? commandParameter,
        bool isEnabled,
        string? disabledReason,
        string? iconKey,
        ContextMenuStyleOptions style)
    {
        var menuItem = new MenuItem
        {
            Header = header,
            Command = command,
            CommandParameter = commandParameter,
            IsEnabled = isEnabled,
            Background = AsterGraph.Avalonia.Styling.BrushFactory.Solid(style.BackgroundHex),
            Foreground = AsterGraph.Avalonia.Styling.BrushFactory.Solid(
                isEnabled ? style.ForegroundHex : style.DisabledForegroundHex),
            CornerRadius = new CornerRadius(style.ItemCornerRadius),
            Padding = new Thickness(style.ItemHorizontalPadding, style.ItemVerticalPadding),
            FontSize = style.ItemFontSize,
            MinWidth = style.ItemMinWidth,
        };

        menuItem.Classes.Add("astergraph-context-menu-item");

        var icon = ContextMenuIconResolver.CreateIcon(iconKey, isEnabled, style);
        if (icon is not null)
        {
            menuItem.Icon = icon;
        }

        if (isEnabled)
        {
            var defaultBackground = AsterGraph.Avalonia.Styling.BrushFactory.Solid(style.BackgroundHex);
            var hoverBackground = AsterGraph.Avalonia.Styling.BrushFactory.Solid(style.HoverHex);

            menuItem.PointerEntered += (_, _) => menuItem.Background = hoverBackground;
            menuItem.PointerExited += (_, _) => menuItem.Background = defaultBackground;
        }
        else if (!string.IsNullOrWhiteSpace(disabledReason))
        {
            ToolTip.SetTip(menuItem, disabledReason);
        }

        return menuItem;
    }

    private static object CreateSeparator(ContextMenuStyleOptions style)
        => new Border
        {
            Height = 1,
            Margin = new Thickness(style.ItemHorizontalPadding, style.ItemVerticalPadding / 2),
            Background = AsterGraph.Avalonia.Styling.BrushFactory.Solid(style.SeparatorHex),
            IsHitTestVisible = false,
        };
}
