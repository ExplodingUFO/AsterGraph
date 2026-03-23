namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 拖动辅助相关行为配置。
/// </summary>
public sealed record DragAssistBehaviorOptions
{
    /// <summary>
    /// 是否启用网格吸附。
    /// </summary>
    public bool EnableGridSnapping { get; init; }

    /// <summary>
    /// 是否启用对齐辅助线。
    /// </summary>
    public bool EnableAlignmentGuides { get; init; }

    /// <summary>
    /// 吸附判定容差。
    /// </summary>
    public double SnapTolerance { get; init; } = 14;
}
