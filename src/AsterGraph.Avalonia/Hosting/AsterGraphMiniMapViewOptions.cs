using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// 定义通过 <see cref="AsterGraphMiniMapViewFactory"/> 组合独立缩略图视图的宿主输入。
/// </summary>
public sealed record AsterGraphMiniMapViewOptions
{
    /// <summary>
    /// 要绑定到缩略图上的 canonical 编辑器 session。
    /// </summary>
    public IGraphEditorSession? Session { get; init; }

    /// <summary>
    /// 可选的样式选项输入；未提供时使用默认样式。
    /// </summary>
    public GraphEditorStyleOptions? StyleOptions { get; init; }

    /// <summary>
     /// 可选的 Avalonia 展示器替换配置。
    /// </summary>
    public AsterGraphPresentationOptions? Presentation { get; init; }
}
