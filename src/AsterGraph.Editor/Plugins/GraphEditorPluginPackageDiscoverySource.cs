namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 定义本地插件包目录发现源。
/// </summary>
public sealed record GraphEditorPluginPackageDiscoverySource
{
    /// <summary>
    /// 初始化包目录发现源。
    /// </summary>
    public GraphEditorPluginPackageDiscoverySource(
        string directoryPath,
        string searchPattern = "*.nupkg",
        bool includeSubdirectories = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        DirectoryPath = Path.GetFullPath(directoryPath);
        SearchPattern = searchPattern.Trim();
        IncludeSubdirectories = includeSubdirectories;
    }

    /// <summary>
    /// 要扫描的本地包目录绝对路径。
    /// </summary>
    public string DirectoryPath { get; }

    /// <summary>
    /// 匹配包归档的搜索模式。
    /// </summary>
    public string SearchPattern { get; }

    /// <summary>
    /// 是否递归扫描子目录。
    /// </summary>
    public bool IncludeSubdirectories { get; }
}
