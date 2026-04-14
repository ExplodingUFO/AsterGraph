using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using AsterGraph.Avalonia.Styling;
using AsterGraph.Abstractions.Styling;

namespace AsterGraph.Avalonia.Menus.Internal;

/// <summary>
/// 负责构造统一外观的右键菜单控件。
/// </summary>
internal static class ContextMenuFactory
{
    internal static ContextMenu CreateContextMenu(IReadOnlyList<object> items, ContextMenuStyleOptions style)
    {
        var menu = new ContextMenu
        {
            Background = BrushFactory.Solid(style.BackgroundHex),
            BorderBrush = BrushFactory.Solid(style.BorderHex),
            BorderThickness = new Thickness(style.BorderThickness),
            CornerRadius = new CornerRadius(style.CornerRadius),
            MinWidth = style.ItemMinWidth,
            ItemsSource = items,
        };

        menu.Classes.Add("astergraph-context-menu");
        menu.Styles.Add(ContextMenuStyle.CreateFlyoutPresenterStyle());
        ContextMenuStyle.ApplyOverrides(menu, style);
        return menu;
    }

    internal static void ApplyPlacement(ContextMenu menu, Control target)
    {
        menu.PlacementTarget = target;
        menu.Placement = ContextMenuPlacement.Resolve(target);
    }
}
