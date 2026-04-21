namespace AsterGraph.Editor.Services;

/// <summary>
/// 提供图编辑器默认存储路径的统一解析规则。
/// </summary>
public static class GraphEditorStorageDefaults
{
    private const string DefaultRootDirectoryName = "AsterGraph";
    private const string WorkspaceFileName = "workspace.json";
    private const string FragmentFileName = "selection-fragment.json";
    private const string FragmentLibraryDirectoryName = "fragments";
    private const string SceneSvgExportFileName = "graph-scene.svg";
    private const string PluginStagingDirectoryName = "plugin-staging";

    /// <summary>
    /// 解析默认存储根目录。
    /// </summary>
    /// <param name="storageRootPath">宿主显式提供的根目录；为空时回退到包中立默认目录。</param>
    /// <returns>解析后的根目录。</returns>
    public static string ResolveStorageRootPath(string? storageRootPath = null)
    {
        if (!string.IsNullOrWhiteSpace(storageRootPath))
        {
            return storageRootPath.Trim();
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            DefaultRootDirectoryName);
    }

    /// <summary>
    /// 获取默认工作区文件路径。
    /// </summary>
    /// <param name="storageRootPath">可选的显式根目录。</param>
    /// <returns>工作区文件路径。</returns>
    public static string GetWorkspacePath(string? storageRootPath = null)
        => Path.Combine(ResolveStorageRootPath(storageRootPath), WorkspaceFileName);

    /// <summary>
    /// 获取默认片段文件路径。
    /// </summary>
    /// <param name="storageRootPath">可选的显式根目录。</param>
    /// <returns>片段文件路径。</returns>
    public static string GetFragmentPath(string? storageRootPath = null)
        => Path.Combine(ResolveStorageRootPath(storageRootPath), FragmentFileName);

    /// <summary>
    /// 获取默认片段模板库目录路径。
    /// </summary>
    /// <param name="storageRootPath">可选的显式根目录。</param>
    /// <returns>模板库目录路径。</returns>
    public static string GetFragmentLibraryPath(string? storageRootPath = null)
        => Path.Combine(ResolveStorageRootPath(storageRootPath), FragmentLibraryDirectoryName);

    /// <summary>
    /// 获取默认 SVG 场景导出文件路径。
    /// </summary>
    /// <param name="storageRootPath">可选的显式根目录。</param>
    /// <returns>SVG 场景导出文件路径。</returns>
    public static string GetSceneSvgExportPath(string? storageRootPath = null)
        => Path.Combine(ResolveStorageRootPath(storageRootPath), SceneSvgExportFileName);

    /// <summary>
    /// 获取默认插件包暂存目录路径。
    /// </summary>
    /// <param name="storageRootPath">可选的显式根目录。</param>
    /// <returns>插件包暂存目录路径。</returns>
    public static string GetPluginStagingPath(string? storageRootPath = null)
        => Path.Combine(ResolveStorageRootPath(storageRootPath), PluginStagingDirectoryName);
}
