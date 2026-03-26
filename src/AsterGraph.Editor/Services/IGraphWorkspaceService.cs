using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 定义整图工作区的宿主可替换存储契约。
/// </summary>
public interface IGraphWorkspaceService
{
    /// <summary>
    /// 获取默认工作区文件路径。
    /// </summary>
    string WorkspacePath { get; }

    /// <summary>
    /// 保存图文档。
    /// </summary>
    /// <param name="document">要保存的图文档。</param>
    void Save(GraphDocument document);

    /// <summary>
    /// 加载图文档。
    /// </summary>
    /// <returns>已加载的图文档。</returns>
    GraphDocument Load();

    /// <summary>
    /// 判断默认工作区文件是否存在。
    /// </summary>
    /// <returns>存在时返回 <see langword="true"/>。</returns>
    bool Exists();
}
