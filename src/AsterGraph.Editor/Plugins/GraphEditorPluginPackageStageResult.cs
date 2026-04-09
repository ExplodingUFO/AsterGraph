namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 表示一次稳定的插件包暂存结果。
/// </summary>
public sealed record GraphEditorPluginPackageStageResult
{
    /// <summary>
    /// 初始化插件包暂存结果。
    /// </summary>
    public GraphEditorPluginPackageStageResult(
        GraphEditorPluginStageSnapshot stage,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        GraphEditorPluginTrustEvaluation trustEvaluation,
        GraphEditorPluginRegistration? registration = null)
    {
        ArgumentNullException.ThrowIfNull(stage);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);
        ArgumentNullException.ThrowIfNull(trustEvaluation);

        if (registration is not null)
        {
            if (registration.Stage is null)
            {
                throw new ArgumentException("Staged package results must carry registration stage metadata when a registration is returned.", nameof(registration));
            }

            if (registration.PackagePath is null
                || !string.Equals(registration.PackagePath, stage.PackagePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Staged package result registration must preserve the staged package path.", nameof(registration));
            }

            if (registration.AssemblyPath is null
                || string.IsNullOrWhiteSpace(stage.MainAssemblyPath)
                || !string.Equals(registration.AssemblyPath, stage.MainAssemblyPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Staged package result registration must preserve the staged main assembly path.", nameof(registration));
            }
        }

        Stage = stage;
        Manifest = manifest;
        ProvenanceEvidence = provenanceEvidence;
        TrustEvaluation = trustEvaluation;
        Registration = registration;
    }

    /// <summary>
    /// 当前暂存结果快照。
    /// </summary>
    public GraphEditorPluginStageSnapshot Stage { get; }

    /// <summary>
    /// 当前可见的插件清单。
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// 当前来源和签名证据。
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }

    /// <summary>
    /// 当前信任评估结果。
    /// </summary>
    public GraphEditorPluginTrustEvaluation TrustEvaluation { get; }

    /// <summary>
    /// 可选的已桥接加载注册项。
    /// </summary>
    public GraphEditorPluginRegistration? Registration { get; }
}
