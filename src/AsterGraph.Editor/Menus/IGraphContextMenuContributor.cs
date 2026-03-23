namespace AsterGraph.Editor.Menus;

/// <summary>
/// 定义宿主侧上下文菜单扩展点，用于在编辑器内置菜单后追加自定义菜单项。
/// </summary>
public interface IGraphContextMenuContributor
{
    /// <summary>
    /// 根据当前菜单上下文生成要追加的菜单项。
    /// </summary>
    /// <param name="context">当前菜单的只读扩展上下文。</param>
    /// <returns>需要追加的菜单项集合；没有扩展内容时返回空集合。</returns>
    IReadOnlyList<MenuItemDescriptor> Contribute(GraphContextMenuExtensionContext context);
}
