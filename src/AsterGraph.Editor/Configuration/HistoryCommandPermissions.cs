namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 历史记录相关命令权限。
/// </summary>
public sealed record HistoryCommandPermissions
{
    /// <summary>
    /// 是否允许撤销。
    /// </summary>
    public bool AllowUndo { get; init; } = true;

    /// <summary>
    /// 是否允许重做。
    /// </summary>
    public bool AllowRedo { get; init; } = true;
}
