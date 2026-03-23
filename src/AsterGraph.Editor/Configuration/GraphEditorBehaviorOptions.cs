namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 图编辑器的宿主行为配置入口。
/// </summary>
public sealed record GraphEditorBehaviorOptions
{
    /// <summary>
    /// 默认行为配置。
    /// </summary>
    public static GraphEditorBehaviorOptions Default { get; } = new();

    /// <summary>
    /// 撤销与重做相关配置。
    /// </summary>
    public HistoryBehaviorOptions History { get; init; } = new();

    /// <summary>
    /// 选择与参数编辑相关配置。
    /// </summary>
    public SelectionBehaviorOptions Selection { get; init; } = new();

    /// <summary>
    /// 拖动辅助相关配置。
    /// </summary>
    public DragAssistBehaviorOptions DragAssist { get; init; } = new();

    /// <summary>
    /// 片段与剪贴板相关配置。
    /// </summary>
    public FragmentBehaviorOptions Fragments { get; init; } = new();

    /// <summary>
    /// 视图层辅助功能配置。
    /// </summary>
    public ViewBehaviorOptions View { get; init; } = new();
}
