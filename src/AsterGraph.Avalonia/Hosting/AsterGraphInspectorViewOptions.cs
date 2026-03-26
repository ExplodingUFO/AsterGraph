using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 定义通过 <see cref="AsterGraphInspectorViewFactory"/> 组合独立检查器视图的宿主输入。
/// </summary>
public sealed record AsterGraphInspectorViewOptions
{
    /// <summary>
    /// 要绑定到检查器上的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor { get; init; }

    /// <summary>
    /// 可选的 Avalonia 展示器替换配置。
    /// </summary>
    public AsterGraphPresentationOptions? Presentation { get; init; }
}
