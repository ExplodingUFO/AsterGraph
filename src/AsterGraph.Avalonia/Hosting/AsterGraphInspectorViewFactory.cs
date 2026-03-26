using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 提供独立 <see cref="GraphInspectorView"/> 的规范宿主组合入口。
/// </summary>
public static class AsterGraphInspectorViewFactory
{
    /// <summary>
    /// 使用宿主提供的选项创建一个独立检查器视图。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的独立检查器视图。</returns>
    public static GraphInspectorView Create(AsterGraphInspectorViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Editor);

        return new GraphInspectorView
        {
            Editor = options.Editor,
            InspectorPresenter = options.Presentation?.InspectorPresenter,
        };
    }
}
