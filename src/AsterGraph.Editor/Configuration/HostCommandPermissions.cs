namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 宿主扩展相关命令权限。
/// </summary>
public sealed record HostCommandPermissions
{
    /// <summary>
    /// 是否允许追加宿主右键菜单扩展项。
    /// </summary>
    public bool AllowContextMenuExtensions { get; init; } = true;
}
