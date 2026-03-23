using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// 片段模板视图模型。
/// </summary>
public sealed class FragmentTemplateViewModel
{
    public FragmentTemplateViewModel(FragmentTemplateInfo info)
    {
        Info = info;
        Name = info.Name;
        Path = info.Path;
        NodeCount = info.NodeCount;
        ConnectionCount = info.ConnectionCount;
        LastModified = info.LastModified;
    }

    public FragmentTemplateInfo Info { get; }

    public string Name { get; }

    public string Path { get; }

    public int NodeCount { get; }

    public int ConnectionCount { get; }

    public DateTime LastModified { get; }

    public string Summary => $"{NodeCount} nodes  ·  {ConnectionCount} links";
}
