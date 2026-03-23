namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 片段与模板相关命令权限。
/// </summary>
public sealed record FragmentCommandPermissions
{
    /// <summary>
    /// 是否允许导入片段。
    /// </summary>
    public bool AllowImport { get; init; } = true;

    /// <summary>
    /// 是否允许导出片段。
    /// </summary>
    public bool AllowExport { get; init; } = true;

    /// <summary>
    /// 是否允许清理当前片段文件。
    /// </summary>
    public bool AllowClearWorkspaceFragment { get; init; } = true;

    /// <summary>
    /// 是否允许管理模板库。
    /// </summary>
    public bool AllowTemplateManagement { get; init; } = true;
}
