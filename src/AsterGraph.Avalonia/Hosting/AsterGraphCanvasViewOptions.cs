using AsterGraph.Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 定义通过 <see cref="AsterGraphCanvasViewFactory"/> 组合独立节点画布的宿主输入。
/// </summary>
/// <remarks>
/// 该选项面向只想复用交互画布而不引入完整 <see cref="GraphEditorView"/> 外壳的宿主。
/// 默认仍保留内置上下文菜单与命令快捷键，宿主可以显式关闭它们以接管行为。
/// </remarks>
public sealed record AsterGraphCanvasViewOptions
{
    /// <summary>
    /// 要绑定到画布上的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor { get; init; }

    /// <summary>
    /// 是否启用默认内置上下文菜单。
    /// </summary>
    public bool EnableDefaultContextMenu { get; init; } = true;

    /// <summary>
    /// 是否启用默认内置命令快捷键。
    /// </summary>
    public bool EnableDefaultCommandShortcuts { get; init; } = true;
}
