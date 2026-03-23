using AsterGraph.Editor.Hosting;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 提供 Avalonia 宿主上下文信息。
/// </summary>
internal sealed record AvaloniaGraphHostContext(object Owner, object? TopLevel) : IGraphHostContext
{
    /// <summary>
    /// 当顶层对象实现了 <see cref="IServiceProvider"/> 时返回对应服务提供器。
    /// </summary>
    public IServiceProvider? Services => TopLevel as IServiceProvider;
}
