using Avalonia.Controls;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Menus;

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
}
