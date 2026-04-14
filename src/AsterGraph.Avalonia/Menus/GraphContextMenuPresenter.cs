using Avalonia;
using Avalonia.Controls;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Menus.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Menus;

/// <summary>
/// 将编辑器层的菜单描述转换成 Avalonia 右键菜单控件。
/// </summary>
public sealed class GraphContextMenuPresenter : IGraphContextMenuPresenter
{
    /// <summary>
    /// 在指定目标控件上打开由描述符构建的默认上下文菜单。
    /// </summary>
    public void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style)
    {
        var menu = ContextMenuFactory.CreateContextMenu(
            descriptors.Select(descriptor => ContextMenuItemFactory.BuildMenuItem(descriptor, style)).ToList(),
            style);
        ContextMenuPlacement.Apply(menu, target);
        menu.Open(target);
    }

    /// <summary>
    /// 在指定目标控件上打开由 canonical 描述符构建的默认上下文菜单。
    /// </summary>
    public void Open(
        Control target,
        IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> descriptors,
        IGraphEditorCommands commands,
        ContextMenuStyleOptions style)
    {
        var menu = ContextMenuFactory.CreateContextMenu(
            descriptors.Select(descriptor => ContextMenuItemFactory.BuildMenuItem(descriptor, commands, style)).ToList(),
            style);
        ContextMenuPlacement.Apply(menu, target);
        menu.Open(target);
    }

    internal static object BuildMenuControlForTest(MenuItemDescriptor descriptor, ContextMenuStyleOptions style)
        => ContextMenuItemFactory.BuildMenuItem(descriptor, style);

    internal static ContextMenu CreateContextMenuForTest(
        IReadOnlyList<MenuItemDescriptor> descriptors,
        ContextMenuStyleOptions style)
        => ContextMenuFactory.CreateContextMenu(
            descriptors.Select(descriptor => ContextMenuItemFactory.BuildMenuItem(descriptor, style)).ToList(),
            style);

    internal static object BuildMenuControlForTest(
        GraphEditorMenuItemDescriptorSnapshot descriptor,
        IGraphEditorCommands commands,
        ContextMenuStyleOptions style)
        => ContextMenuItemFactory.BuildMenuItem(descriptor, commands, style);

    internal static PlacementMode ResolvePlacementForTest(Control target)
        => ContextMenuPlacement.Resolve(target);
}
