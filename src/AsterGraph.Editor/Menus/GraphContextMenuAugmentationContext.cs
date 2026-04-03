using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 描述一次宿主右键菜单增强请求。
/// </summary>
public sealed record GraphContextMenuAugmentationContext
{
    /// <summary>
    /// 初始化菜单增强上下文。
    /// </summary>
    public GraphContextMenuAugmentationContext(
        IGraphEditorSession session,
        ContextMenuContext context,
        IReadOnlyList<MenuItemDescriptor> stockItems,
        GraphEditorCommandPermissions commandPermissions,
        GraphEditorViewModel compatibilityEditor)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        StockItems = stockItems ?? throw new ArgumentNullException(nameof(stockItems));
        CommandPermissions = commandPermissions ?? throw new ArgumentNullException(nameof(commandPermissions));
        CompatibilityEditor = compatibilityEditor ?? throw new ArgumentNullException(nameof(compatibilityEditor));
    }

    /// <summary>
    /// 当前稳定运行时会话。
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// 当前菜单命中上下文。
    /// </summary>
    public ContextMenuContext Context { get; }

    /// <summary>
    /// 编辑器默认生成的菜单项集合。
    /// </summary>
    public IReadOnlyList<MenuItemDescriptor> StockItems { get; }

    /// <summary>
    /// 当前命令权限快照。
    /// </summary>
    public GraphEditorCommandPermissions CommandPermissions { get; }

    /// <summary>
    /// 兼容路径下的编辑器 facade。
    /// </summary>
    /// <remarks>
    /// New implementations should prefer <see cref="Session"/>, <see cref="Context"/>, and
    /// <see cref="StockItems"/>. This property only exists to adapt legacy augmentors during
    /// the migration window.
    /// </remarks>
    public GraphEditorViewModel CompatibilityEditor { get; }
}
