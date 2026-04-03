using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 提供默认 <see cref="GraphEditorView"/> 的规范宿主组合入口。
/// </summary>
/// <remarks>
/// 该工厂只负责应用宿主提供的编辑器实例与视图选项，
/// 并返回由独立画布、检查器与缩略图表面组合而成的默认完整外壳；
/// 直接构造 <see cref="GraphEditorView"/> 仍然受支持，便于宿主渐进迁移。
/// </remarks>
public static class AsterGraphAvaloniaViewFactory
{
    /// <summary>
    /// 使用宿主提供的选项创建一个默认 <see cref="GraphEditorView"/>。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的图编辑器视图。</returns>
    public static GraphEditorView Create(AsterGraphAvaloniaViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Editor);

        return new GraphEditorView
        {
            Editor = options.Editor,
            ChromeMode = options.ChromeMode,
            EnableDefaultContextMenu = options.EnableDefaultContextMenu,
            EnableDefaultCommandShortcuts = options.EnableDefaultCommandShortcuts,
            Presentation = options.Presentation,
        };
    }
}
