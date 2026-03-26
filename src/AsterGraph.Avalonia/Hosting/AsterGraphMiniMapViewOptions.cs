using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 定义通过 <see cref="AsterGraphMiniMapViewFactory"/> 组合独立缩略图视图的宿主输入。
/// </summary>
public sealed record AsterGraphMiniMapViewOptions
{
    /// <summary>
    /// 要绑定到缩略图上的编辑器视图模型。
    /// </summary>
    public GraphEditorViewModel? Editor { get; init; }
}
