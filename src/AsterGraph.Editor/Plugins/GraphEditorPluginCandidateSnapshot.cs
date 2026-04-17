namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Identifies the discovery source kind for a plugin candidate.
/// </summary>
public enum GraphEditorPluginCandidateSourceKind
{
    /// <summary>
    /// Discovered by scanning a local directory.
    /// </summary>
    Directory,

    /// <summary>
    /// Discovered by scanning a local package directory.
    /// </summary>
    PackageDirectory,

    /// <summary>
    /// Discovered through a host-supplied manifest source.
    /// </summary>
    ManifestSource,
}

/// <summary>
/// Captures one stable plugin-candidate discovery snapshot.
/// </summary>
public sealed record GraphEditorPluginCandidateSnapshot
{
    /// <summary>
    /// Initializes a plugin-candidate snapshot.
    /// </summary>
    public GraphEditorPluginCandidateSnapshot(
        GraphEditorPluginCandidateSourceKind sourceKind,
        string source,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginCompatibilityEvaluation compatibility,
        GraphEditorPluginTrustEvaluation trustEvaluation,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        string? assemblyPath = null,
        string? pluginTypeName = null,
        string? packagePath = null)
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
        PackagePath = string.IsNullOrWhiteSpace(packagePath) ? null : Path.GetFullPath(packagePath);
        PluginTypeName = string.IsNullOrWhiteSpace(pluginTypeName) ? null : pluginTypeName.Trim();
    }

    /// <summary>
    /// Gets the discovery source kind.
    /// </summary>
    public GraphEditorPluginCandidateSourceKind SourceKind { get; }

    /// <summary>
    /// Gets the stable source identifier. Directory discovery typically uses a directory path and
    /// manifest discovery typically uses a source identifier.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets the absolute local assembly path for the candidate when one is available.
    /// </summary>
    public string? AssemblyPath { get; }

    /// <summary>
    /// Gets the absolute local package-archive path for the candidate when one is available.
    /// </summary>
    public string? PackagePath { get; }

    /// <summary>
    /// Gets the optional explicit plugin type name.
    /// </summary>
    public string? PluginTypeName { get; }

    /// <summary>
    /// Gets the visible plugin manifest for the candidate.
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// Gets the current compatibility evaluation.
    /// </summary>
    public GraphEditorPluginCompatibilityEvaluation Compatibility { get; }

    /// <summary>
    /// Gets the current trust evaluation.
    /// </summary>
    public GraphEditorPluginTrustEvaluation TrustEvaluation { get; }

    /// <summary>
    /// Gets the current provenance and signature evidence.
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }
}
