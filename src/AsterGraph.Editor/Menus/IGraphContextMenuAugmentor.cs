using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 定义宿主侧右键菜单增强器，用于在默认菜单基础上追加业务菜单项。
/// </summary>
public interface IGraphContextMenuAugmentor
{
    /// <summary>
    /// 基于默认菜单和当前上下文构建最终菜单。
    /// </summary>
    /// <param name="editor">当前编辑器视图模型。</param>
    /// <param name="context">当前右键菜单上下文。</param>
    /// <param name="stockItems">编辑器默认生成的菜单项。</param>
    /// <returns>最终菜单项集合；宿主通常应以 <paramref name="stockItems"/> 为基础追加自身业务项。</returns>
    IReadOnlyList<MenuItemDescriptor> Augment(
        GraphEditorViewModel editor,
        ContextMenuContext context,
        IReadOnlyList<MenuItemDescriptor> stockItems);
}
