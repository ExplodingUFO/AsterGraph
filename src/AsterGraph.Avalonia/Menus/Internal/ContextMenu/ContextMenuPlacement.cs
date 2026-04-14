using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Menus.Internal;

/// <summary>
/// 负责菜单定位策略选择。
/// </summary>
internal static class ContextMenuPlacement
{
    internal static void Apply(ContextMenu menu, Control target)
    {
        menu.PlacementTarget = target;
        menu.Placement = Resolve(target);
    }

    internal static PlacementMode Resolve(Control target)
        => target is NodeCanvas
            ? PlacementMode.Pointer
            : target.IsFocused || target.IsKeyboardFocusWithin
            ? PlacementMode.Bottom
            : PlacementMode.Pointer;
}
