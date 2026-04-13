using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 定义一个本地目录插件发现源。
/// </summary>
public sealed record GraphEditorPluginDirectoryDiscoverySource
{
    /// <summary>
    /// 初始化目录发现源。
    /// </summary>
    public GraphEditorPluginDirectoryDiscoverySource(
        string directoryPath,
        string? searchPattern = null,
        bool includeSubdirectories = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);

        DirectoryPath = Path.GetFullPath(directoryPath);
        SearchPattern = string.IsNullOrWhiteSpace(searchPattern) ? "*.dll" : searchPattern.Trim();
        IncludeSubdirectories = includeSubdirectories;
    }

    /// <summary>
    /// 要扫描的目录绝对路径。
    /// </summary>
    public string DirectoryPath { get; }

    /// <summary>
    /// 文件匹配模式。
    /// </summary>
    public string SearchPattern { get; }

    /// <summary>
    /// 是否递归扫描子目录。
    /// </summary>
    public bool IncludeSubdirectories { get; }
}
