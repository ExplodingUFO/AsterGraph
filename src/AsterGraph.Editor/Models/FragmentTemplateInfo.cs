namespace AsterGraph.Editor.Models;

/// <summary>
/// 片段模板元数据。
/// </summary>
public sealed record FragmentTemplateInfo(
    string Name,
    string Path,
    int NodeCount,
    int ConnectionCount,
    DateTime LastModified,
    int GroupCount = 0);
