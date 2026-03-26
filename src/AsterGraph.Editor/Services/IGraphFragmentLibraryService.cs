using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 定义片段模板库的宿主可替换契约。
/// </summary>
public interface IGraphFragmentLibraryService
{
    /// <summary>
    /// 获取模板库目录路径。
    /// </summary>
    string LibraryPath { get; }

    /// <summary>
    /// 枚举全部模板元数据。
    /// </summary>
    /// <returns>模板元数据集合。</returns>
    IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates();

    /// <summary>
    /// 保存片段模板。
    /// </summary>
    /// <param name="fragment">要保存的片段。</param>
    /// <param name="name">可选模板名。</param>
    /// <returns>保存后的模板路径。</returns>
    string SaveTemplate(GraphSelectionFragment fragment, string? name = null);

    /// <summary>
    /// 加载片段模板。
    /// </summary>
    /// <param name="path">模板路径。</param>
    /// <returns>已加载片段。</returns>
    GraphSelectionFragment LoadTemplate(string path);

    /// <summary>
    /// 删除片段模板。
    /// </summary>
    /// <param name="path">模板路径。</param>
    void DeleteTemplate(string path);
}
