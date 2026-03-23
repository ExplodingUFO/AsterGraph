namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 布局整理相关命令权限。
/// </summary>
public sealed record LayoutCommandPermissions
{
    /// <summary>
    /// 是否允许对齐。
    /// </summary>
    public bool AllowAlign { get; init; } = true;

    /// <summary>
    /// 是否允许分布。
    /// </summary>
    public bool AllowDistribute { get; init; } = true;
}
