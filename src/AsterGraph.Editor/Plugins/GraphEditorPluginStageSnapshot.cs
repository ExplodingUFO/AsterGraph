using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 表示一次稳定的插件包暂存结果快照。
/// </summary>
public sealed record GraphEditorPluginStageSnapshot
{
    /// <summary>
    /// 初始化包暂存结果快照。
    /// </summary>
    public GraphEditorPluginStageSnapshot(
        GraphEditorPluginStageOutcome outcome,
        string packagePath,
        GraphEditorPluginPackageIdentity? packageIdentity = null,
        string? stagingDirectory = null,
        string? mainAssemblyPath = null,
        string? pluginTypeName = null,
        bool usedCache = false,
        string? reasonCode = null,
        string? reasonMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        Outcome = outcome;
        PackagePath = Path.GetFullPath(packagePath);
        PackageIdentity = packageIdentity;
        StagingDirectory = string.IsNullOrWhiteSpace(stagingDirectory) ? null : Path.GetFullPath(stagingDirectory);
        MainAssemblyPath = string.IsNullOrWhiteSpace(mainAssemblyPath) ? null : Path.GetFullPath(mainAssemblyPath);
        PluginTypeName = string.IsNullOrWhiteSpace(pluginTypeName) ? null : pluginTypeName.Trim();
        UsedCache = usedCache;
        ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode.Trim();
        ReasonMessage = string.IsNullOrWhiteSpace(reasonMessage) ? null : reasonMessage.Trim();
    }

    /// <summary>
    /// 暂存结果状态。
    /// </summary>
    public GraphEditorPluginStageOutcome Outcome { get; }

    /// <summary>
    /// 当前包归档绝对路径。
    /// </summary>
    public string PackagePath { get; }

    /// <summary>
    /// 可见的包标识。
    /// </summary>
    public GraphEditorPluginPackageIdentity? PackageIdentity { get; }

    /// <summary>
    /// 可选的暂存目录绝对路径。
    /// </summary>
    public string? StagingDirectory { get; }

    /// <summary>
    /// 可选的暂存主程序集绝对路径。
    /// </summary>
    public string? MainAssemblyPath { get; }

    /// <summary>
    /// 可选的插件类型名。
    /// </summary>
    public string? PluginTypeName { get; }

    /// <summary>
    /// 是否复用了已有缓存。
    /// </summary>
    public bool UsedCache { get; }

    /// <summary>
    /// 可选的稳定原因代码。
    /// </summary>
    public string? ReasonCode { get; }

    /// <summary>
    /// 可选的宿主可读原因文本。
    /// </summary>
    public string? ReasonMessage { get; }
}
