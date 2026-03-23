namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 节点相关命令权限。
/// </summary>
public sealed record NodeCommandPermissions
{
    /// <summary>
    /// 是否允许创建节点。
    /// </summary>
    public bool AllowCreate { get; init; } = true;

    /// <summary>
    /// 是否允许删除节点。
    /// </summary>
    public bool AllowDelete { get; init; } = true;

    /// <summary>
    /// 是否允许移动节点。
    /// </summary>
    public bool AllowMove { get; init; } = true;

    /// <summary>
    /// 是否允许复制节点。
    /// </summary>
    public bool AllowDuplicate { get; init; } = true;

    /// <summary>
    /// 是否允许编辑节点参数。
    /// </summary>
    public bool AllowEditParameters { get; init; } = true;
}
