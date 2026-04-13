using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 表示宿主提供的插件清单候选源。
/// </summary>
public interface IGraphEditorPluginManifestSource
{
    /// <summary>
    /// 读取当前可见的插件候选项集合。
    /// </summary>
    /// <returns>候选项集合。</returns>
    IReadOnlyList<GraphEditorPluginManifestSourceCandidate> GetCandidates();
}

/// <summary>
/// 表示由宿主清单源提供的一个本地插件候选项。
/// </summary>
public sealed record GraphEditorPluginManifestSourceCandidate
{
    /// <summary>
    /// 初始化清单源候选项。
    /// </summary>
    public GraphEditorPluginManifestSourceCandidate(
        string source,
        string assemblyPath,
        GraphEditorPluginManifest manifest,
        string? pluginTypeName = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        ArgumentNullException.ThrowIfNull(manifest);

        Source = source.Trim();
        AssemblyPath = Path.GetFullPath(assemblyPath);
        Manifest = manifest;
        PluginTypeName = string.IsNullOrWhiteSpace(pluginTypeName) ? null : pluginTypeName.Trim();
        ProvenanceEvidence = provenanceEvidence ?? GraphEditorPluginProvenanceEvidence.NotProvided;
    }

    /// <summary>
    /// 候选项来源标识。
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// 候选项对应的本地程序集绝对路径。
    /// </summary>
    public string AssemblyPath { get; }

    /// <summary>
    /// 候选项的显式清单。
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// 可选的显式插件类型名。
    /// </summary>
    public string? PluginTypeName { get; }

    /// <summary>
    /// 可选的来源和签名证据。
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }
}
