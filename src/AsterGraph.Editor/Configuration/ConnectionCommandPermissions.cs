namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 连线相关命令权限。
/// </summary>
public sealed record ConnectionCommandPermissions
{
    /// <summary>
    /// 是否允许创建连线。
    /// </summary>
    public bool AllowCreate { get; init; } = true;

    /// <summary>
    /// 是否允许删除连线。
    /// </summary>
    public bool AllowDelete { get; init; } = true;

    /// <summary>
    /// 是否允许断开连线。
    /// </summary>
    public bool AllowDisconnect { get; init; } = true;
}
