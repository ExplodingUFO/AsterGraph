namespace AsterGraph.Editor.Hosting;

/// <summary>
/// 描述图编辑器宿主在菜单请求时提供的运行时上下文。
/// </summary>
public interface IGraphHostContext
{
    /// <summary>
    /// 获取宿主拥有者对象，通常是承载编辑器的视图或窗口。
    /// </summary>
    object Owner { get; }

    /// <summary>
    /// 获取当前顶层宿主对象。
    /// </summary>
    object? TopLevel { get; }

    /// <summary>
    /// 获取可选的宿主服务提供器。
    /// </summary>
    IServiceProvider? Services { get; }
}
