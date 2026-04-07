using Avalonia.Controls;
using AsterGraph.Avalonia.Menus;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 定义 Avalonia 侧右键菜单的展示层替换契约。
/// </summary>
public interface IGraphContextMenuPresenter
{
    /// <summary>
    /// 在目标控件上打开基于菜单描述符树构建的上下文菜单。
    /// </summary>
    void Open(Control target, IReadOnlyList<MenuItemDescriptor> descriptors, ContextMenuStyleOptions style);

    /// <summary>
    /// 在目标控件上打开基于 canonical 菜单描述符树构建的上下文菜单。
    /// </summary>
    /// <remarks>
    /// 尚未迁移到 descriptor-first 路径的自定义展示器可继续仅实现兼容
    /// <see cref="Open(Control, IReadOnlyList{MenuItemDescriptor}, ContextMenuStyleOptions)"/>
    /// 重载；默认实现会将 canonical 描述符适配回兼容菜单项。
    /// </remarks>
    void Open(
        Control target,
        IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> descriptors,
        IGraphEditorCommands commands,
        ContextMenuStyleOptions style)
        => Open(target, GraphContextMenuDescriptorAdapter.Adapt(descriptors, commands), style);
}
