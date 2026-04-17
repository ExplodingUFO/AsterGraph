namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Defines the host inputs used for canonical plugin-candidate discovery.
/// </summary>
public sealed record GraphEditorPluginDiscoveryOptions
{
    /// <summary>
    /// Gets the local directory discovery sources.
    /// </summary>
    public IReadOnlyList<GraphEditorPluginDirectoryDiscoverySource> DirectorySources { get; init; } = [];

    /// <summary>
    /// Gets the local package-directory discovery sources.
    /// </summary>
    public IReadOnlyList<GraphEditorPluginPackageDiscoverySource> PackageDirectorySources { get; init; } = [];

    /// <summary>
    /// Gets the host-supplied manifest discovery sources.
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginManifestSource> ManifestSources { get; init; } = [];

    /// <summary>
    /// Gets the optional host trust policy that evaluates candidates during discovery.
    /// </summary>
    public IGraphEditorPluginTrustPolicy? TrustPolicy { get; init; }
}
