namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 选择与参数编辑相关行为配置。
/// </summary>
public sealed record SelectionBehaviorOptions
{
    /// <summary>
    /// 是否启用 Shift/Ctrl 多选增强。
    /// </summary>
    public bool EnableModifierSelection { get; init; } = true;

    /// <summary>
    /// 是否允许同定义节点批量编辑公共参数。
    /// </summary>
    public bool EnableBatchParameterEditing { get; init; } = true;
}
