namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 指示插件候选项的发现来源类型。
/// </summary>
public enum GraphEditorPluginCandidateSourceKind
{
    /// <summary>
    /// 来自本地目录扫描。
    /// </summary>
    Directory,

    /// <summary>
    /// 来自宿主提供的清单源。
    /// </summary>
    ManifestSource,
}

/// <summary>
/// 表示一次稳定的插件候选项检查快照。
/// </summary>
public sealed record GraphEditorPluginCandidateSnapshot
{
    /// <summary>
    /// 初始化插件候选项快照。
    /// </summary>
    public GraphEditorPluginCandidateSnapshot(
        GraphEditorPluginCandidateSourceKind sourceKind,
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginCompatibilityEvaluation compatibility,
        GraphEditorPluginTrustEvaluation trustEvaluation,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        string? assemblyPath = null,
        string? pluginTypeName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(compatibility);
        ArgumentNullException.ThrowIfNull(trustEvaluation);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);

        SourceKind = sourceKind;
        Source = source.Trim();
        Manifest = manifest;
        Compatibility = compatibility;
        TrustEvaluation = trustEvaluation;
        ProvenanceEvidence = provenanceEvidence;
        AssemblyPath = string.IsNullOrWhiteSpace(assemblyPath) ? null : Path.GetFullPath(assemblyPath);
        PluginTypeName = string.IsNullOrWhiteSpace(pluginTypeName) ? null : pluginTypeName.Trim();
    }

    /// <summary>
    /// 发现来源类型。
    /// </summary>
    public GraphEditorPluginCandidateSourceKind SourceKind { get; }

    /// <summary>
    /// 稳定来源标识。目录发现时通常为目录路径，清单源发现时为源标识。
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// 候选项对应的本地程序集绝对路径。
    /// </summary>
    public string? AssemblyPath { get; }

    /// <summary>
    /// 可选的显式插件类型名。
    /// </summary>
    public string? PluginTypeName { get; }

    /// <summary>
    /// 候选项可见的插件清单。
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// 当前兼容性评估结果。
    /// </summary>
    public GraphEditorPluginCompatibilityEvaluation Compatibility { get; }

    /// <summary>
    /// 当前信任评估结果。
    /// </summary>
    public GraphEditorPluginTrustEvaluation TrustEvaluation { get; }

    /// <summary>
    /// 当前来源和签名证据。
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }
}
