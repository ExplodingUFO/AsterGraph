using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Editor.Menus;

namespace AsterGraph.Avalonia.Menus;

/// <summary>
/// 将编辑器层的菜单描述转换成 Avalonia 右键菜单控件。
/// </summary>
internal sealed class GraphContextMenuPresenter
{
    public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
    {
        var menu = new ContextMenu
        {
            PlacementTarget = target,
            Placement = PlacementMode.Pointer,
            Background = BrushFactory.Solid(style.BackgroundHex),
            BorderBrush = BrushFactory.Solid(style.BorderHex),
            BorderThickness = new Thickness(style.BorderThickness),
            CornerRadius = new CornerRadius(style.CornerRadius),
            MinWidth = style.ItemMinWidth,
            ItemsSource = descriptors.Select(descriptor => BuildMenuControl(descriptor, style)).ToList(),
        };

        menu.Classes.Add("astergraph-context-menu");
        menu.Open(target);
    }

    private static object BuildMenuControl(MenuItemDescriptor descriptor, ContextMenuStyleOptions style)
    {
        if (descriptor.IsSeparator)
        {
            return new Border
            {
                Height = 1,
                Margin = new Thickness(style.ItemHorizontalPadding, style.ItemVerticalPadding / 2),
                Background = BrushFactory.Solid(style.SeparatorHex),
                IsHitTestVisible = false,
            };
        }

        var menuItem = new MenuItem
        {
            Header = descriptor.Header,
            Command = descriptor.Command,
            CommandParameter = descriptor.CommandParameter,
            IsEnabled = descriptor.IsEnabled,
            Background = BrushFactory.Solid(style.BackgroundHex),
            Foreground = BrushFactory.Solid(descriptor.IsEnabled ? style.ForegroundHex : style.DisabledForegroundHex),
            CornerRadius = new CornerRadius(style.ItemCornerRadius),
            Padding = new Thickness(style.ItemHorizontalPadding, style.ItemVerticalPadding),
            FontSize = style.ItemFontSize,
            MinWidth = style.ItemMinWidth,
        };

        menuItem.Classes.Add("astergraph-context-menu-item");

        var icon = CreateIcon(descriptor.IconKey, descriptor.IsEnabled, style);
        if (icon is not null)
        {
            menuItem.Icon = icon;
        }

        if (descriptor.HasChildren)
        {
            menuItem.ItemsSource = descriptor.Children.Select(child => BuildMenuControl(child, style)).ToList();
        }

        if (descriptor.IsEnabled)
        {
            var defaultBackground = BrushFactory.Solid(style.BackgroundHex);
            var hoverBackground = BrushFactory.Solid(style.HoverHex);

            menuItem.PointerEntered += (_, _) => menuItem.Background = hoverBackground;
            menuItem.PointerExited += (_, _) => menuItem.Background = defaultBackground;
        }

        return menuItem;
    }

    private static Control? CreateIcon(string? iconKey, bool isEnabled, ContextMenuStyleOptions style)
    {
        var glyph = ResolveIconGlyph(iconKey);
        if (glyph is null)
        {
            return null;
        }

        return new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(style.ItemCornerRadius),
            Background = BrushFactory.Solid(style.HoverHex),
            Child = new TextBlock
            {
                Text = glyph,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontSize = style.IconFontSize,
                FontWeight = FontWeight.Bold,
                Foreground = BrushFactory.Solid(isEnabled ? style.ForegroundHex : style.DisabledForegroundHex),
            },
        };
    }

    private static string? ResolveIconGlyph(string? iconKey)
        => iconKey switch
        {
            "add" => "+",
            "node" => "N",
            "fit" => "F",
            "reset" => "R",
            "save" => "S",
            "load" => "L",
            "copy" => "C",
            "paste" => "P",
            "cancel" => "X",
            "inspect" => "I",
            "center" => "C",
            "delete" => "-",
            "duplicate" => "D",
            "disconnect" => "/",
            "connect" => ">",
            "compatible" => "=",
            "type" => "T",
            "conversion" => "~",
            "align" => "|",
            "align-left" => "L",
            "align-center" => "C",
            "align-right" => "R",
            "align-top" => "T",
            "align-middle" => "M",
            "align-bottom" => "B",
            "distribute" => "#",
            "distribute-horizontal" => "H",
            "distribute-vertical" => "V",
            "info" => "i",
            _ => null,
        };
}
