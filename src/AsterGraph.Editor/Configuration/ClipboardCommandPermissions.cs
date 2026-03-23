namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 剪贴板相关命令权限。
/// </summary>
public sealed record ClipboardCommandPermissions
{
    /// <summary>
    /// 是否允许复制。
    /// </summary>
    public bool AllowCopy { get; init; } = true;

    /// <summary>
    /// 是否允许粘贴。
    /// </summary>
    public bool AllowPaste { get; init; } = true;
}
