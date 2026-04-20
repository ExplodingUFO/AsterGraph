namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 定义宿主可按表面选择性替换的 Avalonia 展示器集合。
/// </summary>
/// <remarks>
/// 所有展示器均为可选项；当某个属性为 <see langword="null"/> 时，
/// AsterGraph 会继续使用该表面的内置默认实现。
/// </remarks>
public sealed record AsterGraphPresentationOptions
{
    /// <summary>
    /// 节点可视呈现展示器。
    /// </summary>
    public IGraphNodeVisualPresenter? NodeVisualPresenter { get; init; }

    /// <summary>
    /// 上下文菜单展示器。
    /// </summary>
    public IGraphContextMenuPresenter? ContextMenuPresenter { get; init; }

    /// <summary>
    /// 检查器展示器。
    /// </summary>
    public IGraphInspectorPresenter? InspectorPresenter { get; init; }

    /// <summary>
    /// 缩略图展示器。
    /// </summary>
    public IGraphMiniMapPresenter? MiniMapPresenter { get; init; }

    /// <summary>
    /// Optional registry used by shipped node-side parameter and inspector surfaces to create parameter-editor bodies.
    /// </summary>
    public INodeParameterEditorRegistry? NodeParameterEditorRegistry { get; init; }

    /// <summary>
    /// Optional host override for stock resize-hover cursor feedback.
    /// </summary>
    public IGraphResizeFeedbackPolicy? ResizeFeedbackPolicy { get; init; }
}
