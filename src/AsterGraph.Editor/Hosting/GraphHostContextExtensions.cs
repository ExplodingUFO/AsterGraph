using AsterGraph.Editor.Menus;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// 提供宿主上下文的安全类型提取辅助方法。
/// </summary>
public static class GraphHostContextExtensions
{
    /// <summary>
    /// 尝试按指定类型提取宿主拥有者对象。
    /// </summary>
    /// <typeparam name="T">目标拥有者类型。</typeparam>
    /// <param name="hostContext">宿主上下文。</param>
    /// <param name="owner">成功时返回匹配的拥有者对象；失败时返回默认值。</param>
    /// <returns>提取成功时返回 <see langword="true"/>。</returns>
    public static bool TryGetOwner<T>(this IGraphHostContext? hostContext, out T? owner)
    {
        if (hostContext?.Owner is T typedOwner)
        {
            owner = typedOwner;
            return true;
        }

        owner = default;
        return false;
    }

    /// <summary>
    /// 尝试按指定类型提取宿主顶层对象。
    /// </summary>
    /// <typeparam name="T">目标顶层对象类型。</typeparam>
    /// <param name="hostContext">宿主上下文。</param>
    /// <param name="topLevel">成功时返回匹配的顶层对象；失败时返回默认值。</param>
    /// <returns>提取成功时返回 <see langword="true"/>。</returns>
    public static bool TryGetTopLevel<T>(this IGraphHostContext? hostContext, out T? topLevel)
    {
        if (hostContext?.TopLevel is T typedTopLevel)
        {
            topLevel = typedTopLevel;
            return true;
        }

        topLevel = default;
        return false;
    }

    /// <summary>
    /// 尝试从菜单上下文中按指定类型提取宿主拥有者对象。
    /// </summary>
    /// <typeparam name="T">目标拥有者类型。</typeparam>
    /// <param name="context">菜单上下文。</param>
    /// <param name="owner">成功时返回匹配的拥有者对象；失败时返回默认值。</param>
    /// <returns>提取成功时返回 <see langword="true"/>。</returns>
    public static bool TryGetOwner<T>(this ContextMenuContext context, out T? owner)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.HostContext.TryGetOwner(out owner);
    }

    /// <summary>
    /// 尝试从菜单上下文中按指定类型提取宿主顶层对象。
    /// </summary>
    /// <typeparam name="T">目标顶层对象类型。</typeparam>
    /// <param name="context">菜单上下文。</param>
    /// <param name="topLevel">成功时返回匹配的顶层对象；失败时返回默认值。</param>
    /// <returns>提取成功时返回 <see langword="true"/>。</returns>
    public static bool TryGetTopLevel<T>(this ContextMenuContext context, out T? topLevel)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.HostContext.TryGetTopLevel(out topLevel);
    }
}
