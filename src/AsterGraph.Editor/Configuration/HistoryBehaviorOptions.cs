namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 撤销与重做相关行为配置。
/// </summary>
public sealed record HistoryBehaviorOptions
{
    /// <summary>
    /// 是否启用撤销与重做。
    /// </summary>
    public bool EnableUndoRedo { get; init; } = true;
}
