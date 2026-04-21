using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 提供独立 <see cref="GraphMiniMap"/> 的规范宿主组合入口。
/// </summary>
public static class AsterGraphMiniMapViewFactory
{
    /// <summary>
    /// 使用宿主提供的选项创建一个独立缩略图控件。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的独立缩略图控件。</returns>
    public static GraphMiniMap Create(AsterGraphMiniMapViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Session);

        return new GraphMiniMap
        {
            Session = options.Session,
            StyleOptions = options.StyleOptions,
            MiniMapPresenter = options.Presentation?.MiniMapPresenter,
        };
    }
}
