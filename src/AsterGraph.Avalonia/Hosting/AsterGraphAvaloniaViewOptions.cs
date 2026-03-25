using AsterGraph.Avalonia.Controls;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 定义通过 <see cref="AsterGraphAvaloniaViewFactory"/> 组合默认 Avalonia 视图的宿主输入。
/// </summary>
/// <remarks>
/// 该选项契约提供 Phase 1 的规范 Avalonia 宿主入口；直接使用
/// <c>new GraphEditorView { Editor = ... }</c> 仍然是受支持的兼容路径。
/// </remarks>
public sealed record AsterGraphAvaloniaViewOptions
{
    /// <summary>
    /// 要绑定到视图上的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor { get; init; }

    /// <summary>
    /// 视图外壳显示模式。
    /// </summary>
    public GraphEditorViewChromeMode ChromeMode { get; init; } = GraphEditorViewChromeMode.Default;
}
