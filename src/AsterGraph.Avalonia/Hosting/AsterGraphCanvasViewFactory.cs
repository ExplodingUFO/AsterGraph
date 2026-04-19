using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 提供独立 <see cref="NodeCanvas"/> 的规范宿主组合入口。
/// </summary>
public static class AsterGraphCanvasViewFactory
{
    /// <summary>
    /// 使用宿主提供的选项创建一个独立节点画布。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的独立节点画布。</returns>
    public static NodeCanvas Create(AsterGraphCanvasViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Editor);

        return new NodeCanvas
        {
            ViewModel = options.Editor,
            EnableDefaultContextMenu = options.EnableDefaultContextMenu,
            EnableDefaultCommandShortcuts = options.EnableDefaultCommandShortcuts,
            NodeVisualPresenter = options.Presentation?.NodeVisualPresenter,
            ContextMenuPresenter = options.Presentation?.ContextMenuPresenter,
            NodeParameterEditorRegistry = options.Presentation?.NodeParameterEditorRegistry,
        };
    }
}
