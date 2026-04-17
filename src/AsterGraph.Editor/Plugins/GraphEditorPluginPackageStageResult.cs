namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Captures one stable plugin-package staging result.
/// </summary>
public sealed record GraphEditorPluginPackageStageResult
{
    /// <summary>
    /// Initializes a plugin-package staging result.
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
    /// Gets the current staging snapshot.
    /// </summary>
    public GraphEditorPluginStageSnapshot Stage { get; }

    /// <summary>
    /// Gets the visible plugin manifest.
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// Gets the provenance and signature evidence.
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }

    /// <summary>
    /// Gets the trust-evaluation result.
    /// </summary>
    public GraphEditorPluginTrustEvaluation TrustEvaluation { get; }

    /// <summary>
    /// Gets the optional registration bridged from the staged package.
    /// </summary>
    public GraphEditorPluginRegistration? Registration { get; }
}
