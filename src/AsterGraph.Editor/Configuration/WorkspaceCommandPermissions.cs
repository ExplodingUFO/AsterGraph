namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 工作区相关命令权限。
/// </summary>
public sealed record WorkspaceCommandPermissions
{
    /// <summary>
    /// 是否允许保存快照。
    /// </summary>
    public bool AllowSave { get; init; } = true;

    /// <summary>
    /// 是否允许加载快照。
    /// </summary>
    public bool AllowLoad { get; init; } = true;
}
