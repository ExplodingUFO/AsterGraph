using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 管理整张图快照的默认存储位置与读写操作。
/// </summary>
public sealed class GraphWorkspaceService
{
    /// <summary>
    /// 初始化工作区服务。
    /// </summary>
    /// <param name="workspacePath">可选的默认工作区文件路径。</param>
    public GraphWorkspaceService(string? workspacePath = null)
    {
        WorkspacePath = workspacePath ?? GetDefaultWorkspacePath();
    }

    /// <summary>
    /// 默认工作区文件路径。
    /// </summary>
    public string WorkspacePath { get; }

    /// <summary>
    /// 获取系统默认工作区文件路径。
    /// </summary>
    public static string GetDefaultWorkspacePath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraphDemo",
            "demo-graph.json");

    /// <summary>
    /// 将整张图快照保存到默认工作区路径。
    /// </summary>
    /// <param name="document">要保存的图文档。</param>
    public void Save(GraphDocument document)
        => GraphDocumentSerializer.Save(document, WorkspacePath);

    /// <summary>
    /// 从默认工作区路径加载图快照。
    /// </summary>
    /// <returns>已读取的图文档。</returns>
    public GraphDocument Load()
        => GraphDocumentSerializer.Load(WorkspacePath);

    /// <summary>
    /// 判断默认工作区文件是否存在。
    /// </summary>
    /// <returns>存在时返回 <see langword="true"/>。</returns>
    public bool Exists()
        => File.Exists(WorkspacePath);
}
