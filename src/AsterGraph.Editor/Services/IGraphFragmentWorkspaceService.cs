namespace AsterGraph.Editor.Services;

/// <summary>
/// 定义片段工作区文件的宿主可替换存储契约。
/// </summary>
public interface IGraphFragmentWorkspaceService
{
    /// <summary>
    /// 获取默认片段文件路径。
    /// </summary>
    string FragmentPath { get; }

    /// <summary>
    /// 保存片段到默认路径或指定路径。
    /// </summary>
    /// <param name="fragment">要保存的片段。</param>
    /// <param name="path">可选目标路径。</param>
    void Save(GraphSelectionFragment fragment, string? path = null);

    /// <summary>
    /// 从默认路径或指定路径加载片段。
    /// </summary>
    /// <param name="path">可选源路径。</param>
    /// <returns>已加载的片段。</returns>
    GraphSelectionFragment Load(string? path = null);

    /// <summary>
    /// 判断默认路径或指定路径的片段文件是否存在。
    /// </summary>
    /// <param name="path">可选目标路径。</param>
    /// <returns>存在时返回 <see langword="true"/>。</returns>
    bool Exists(string? path = null);

    /// <summary>
    /// 删除默认路径或指定路径的片段文件。
    /// </summary>
    /// <param name="path">可选目标路径。</param>
    void Delete(string? path = null);
}
