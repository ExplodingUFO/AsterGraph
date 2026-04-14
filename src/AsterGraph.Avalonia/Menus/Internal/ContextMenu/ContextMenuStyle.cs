using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Styling;

namespace AsterGraph.Avalonia.Menus.Internal;

/// <summary>
/// 负责应用上下文菜单样式覆盖。
/// </summary>
internal static class ContextMenuStyle
{
    internal static void ApplyOverrides(ContextMenu menu, ContextMenuStyleOptions style)
    {
        var backgroundBrush = BrushFactory.Solid(style.BackgroundHex);
        var hoverBrush = BrushFactory.Solid(style.HoverHex);
        var foregroundBrush = BrushFactory.Solid(style.ForegroundHex);
        var disabledForegroundBrush = BrushFactory.Solid(style.DisabledForegroundHex);

        menu.Resources["MenuFlyoutPresenterBackground"] = Brushes.Transparent;
        menu.Resources["MenuFlyoutPresenterBorderBrush"] = Brushes.Transparent;
        menu.Resources["MenuFlyoutPresenterBorderThemeThickness"] = new Thickness(0);
        menu.Resources["MenuFlyoutPresenterThemePadding"] = new Thickness(0);
        menu.Resources["MenuFlyoutItemBackground"] = backgroundBrush;
        menu.Resources["MenuFlyoutItemBackgroundPointerOver"] = hoverBrush;
        menu.Resources["MenuFlyoutItemBackgroundPressed"] = hoverBrush;
        menu.Resources["MenuFlyoutItemBackgroundDisabled"] = backgroundBrush;
        menu.Resources["MenuFlyoutItemForeground"] = foregroundBrush;
        menu.Resources["MenuFlyoutItemForegroundPointerOver"] = foregroundBrush;
        menu.Resources["MenuFlyoutItemForegroundPressed"] = foregroundBrush;
        menu.Resources["MenuFlyoutItemForegroundDisabled"] = disabledForegroundBrush;
        menu.Resources["MenuFlyoutItemKeyboardAcceleratorTextForeground"] = foregroundBrush;
        menu.Resources["MenuFlyoutItemKeyboardAcceleratorTextForegroundPointerOver"] = foregroundBrush;
        menu.Resources["MenuFlyoutItemKeyboardAcceleratorTextForegroundPressed"] = foregroundBrush;
        menu.Resources["MenuFlyoutItemKeyboardAcceleratorTextForegroundDisabled"] = disabledForegroundBrush;
        menu.Resources["MenuFlyoutSubItemChevron"] = foregroundBrush;
        menu.Resources["MenuFlyoutSubItemChevronPointerOver"] = foregroundBrush;
        menu.Resources["MenuFlyoutSubItemChevronPressed"] = foregroundBrush;
        menu.Resources["MenuFlyoutSubItemChevronDisabled"] = disabledForegroundBrush;
        menu.Resources["MenuFlyoutSubItemChevronSubMenuOpened"] = foregroundBrush;
    }

    internal static Style CreateFlyoutPresenterStyle()
    {
        var transparentBrush = Brushes.Transparent;
        return new Style(selector => selector.OfType<MenuFlyoutPresenter>())
        {
            Setters =
            {
                new Setter(TemplatedControl.BackgroundProperty, transparentBrush),
                new Setter(TemplatedControl.BorderBrushProperty, transparentBrush),
                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0)),
                new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
            },
        };
    }
}
