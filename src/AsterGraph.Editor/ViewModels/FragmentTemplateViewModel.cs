using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// 片段模板视图模型。
/// </summary>
public sealed class FragmentTemplateViewModel
{
    /// <summary>
    /// 使用模板元数据初始化片段模板视图模型。
    /// </summary>
    /// <param name="info">模板的基础元数据。</param>
    public FragmentTemplateViewModel(FragmentTemplateInfo info)
    {
        Info = info;
        Name = info.Name;
        Path = info.Path;
        NodeCount = info.NodeCount;
        ConnectionCount = info.ConnectionCount;
        LastModified = info.LastModified;
    }

    /// <summary>
    /// 提供此视图模型数据的原始模板元数据。
    /// </summary>
    public FragmentTemplateInfo Info { get; }

    /// <summary>
    /// 模板名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 模板文件路径。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 模板中包含的节点数量。
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    /// 模板中包含的连线数量。
    /// </summary>
    public int ConnectionCount { get; }

    /// <summary>
    /// 模板文件的最后修改时间。
    /// </summary>
    public DateTime LastModified { get; }

    /// <summary>
    /// 基于节点和连线计数生成的摘要文本。
    /// </summary>
    public string Summary => $"{NodeCount} nodes  ·  {ConnectionCount} links";
}
