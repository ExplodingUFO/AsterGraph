using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 表示一次显式插件包暂存请求。
/// </summary>
public sealed record GraphEditorPluginPackageStageRequest
{
    /// <summary>
    /// 初始化插件包暂存请求。
    /// </summary>
    public GraphEditorPluginPackageStageRequest(
        GraphEditorPluginCandidateSnapshot candidate,
        string? stagingRootPath = null)
    {
        ArgumentNullException.ThrowIfNull(candidate);

        Candidate = candidate;
        StagingRootPath = string.IsNullOrWhiteSpace(stagingRootPath) ? null : Path.GetFullPath(stagingRootPath);
    }

    /// <summary>
    /// 要进入暂存流程的候选项快照。
    /// </summary>
    public GraphEditorPluginCandidateSnapshot Candidate { get; }

    /// <summary>
    /// 宿主可选提供的暂存根目录绝对路径。
    /// </summary>
    public string? StagingRootPath { get; }
}
