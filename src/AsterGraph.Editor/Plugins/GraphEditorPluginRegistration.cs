using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Describes one explicit plugin registration request.
/// </summary>
public sealed record GraphEditorPluginRegistration
{
    private GraphEditorPluginRegistration(
        IGraphEditorPlugin? plugin,
        string? assemblyPath,
        string? packagePath,
        string? pluginTypeName,
        GraphEditorPluginManifest? manifest,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence,
        GraphEditorPluginStageSnapshot? stage)
    {
        Plugin = plugin;
        AssemblyPath = assemblyPath;
        PackagePath = packagePath;
        PluginTypeName = pluginTypeName;
        Manifest = manifest;
        ProvenanceEvidence = provenanceEvidence ?? GraphEditorPluginProvenanceEvidence.NotProvided;
        Stage = stage;
    }

    /// <summary>
    /// Gets the directly registered plugin instance.
    /// </summary>
    public IGraphEditorPlugin? Plugin { get; }

    /// <summary>
    /// Gets the absolute plugin assembly path for assembly-path registrations.
    /// </summary>
    public string? AssemblyPath { get; }

    /// <summary>
    /// Gets the absolute plugin package path for package-path registrations.
    /// </summary>
    public string? PackagePath { get; }

    /// <summary>
    /// Gets the optional explicit plugin type name.
    /// </summary>
    public string? PluginTypeName { get; }

    /// <summary>
    /// Gets the optional plugin manifest metadata.
    /// </summary>
    public GraphEditorPluginManifest? Manifest { get; }

    /// <summary>
    /// Gets the provenance and signature evidence carried with the registration.
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }

    /// <summary>
    /// Gets the optional staged-package metadata.
    /// </summary>
    public GraphEditorPluginStageSnapshot? Stage { get; }

    /// <summary>
    /// Gets whether this registration was created from a direct plugin instance.
    /// </summary>
    public bool IsDirectRegistration => Plugin is not null;

    /// <summary>
    /// Gets whether this registration was created from an assembly path.
    /// </summary>
    public bool IsAssemblyRegistration => AssemblyPath is not null;

    /// <summary>
    /// Gets whether this registration was created from a package path.
    /// </summary>
    public bool IsPackageRegistration => PackagePath is not null;

    /// <summary>
    /// Creates a registration from a direct plugin instance.
    /// </summary>
    public static GraphEditorPluginRegistration FromPlugin(
        IGraphEditorPlugin plugin,
        GraphEditorPluginManifest? manifest = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return new GraphEditorPluginRegistration(plugin, null, null, null, manifest, provenanceEvidence, null);
    }

    /// <summary>
    /// Creates a registration from a plugin assembly path.
    /// </summary>
    public static GraphEditorPluginRegistration FromAssemblyPath(
        string assemblyPath,
        string? pluginTypeName = null,
        GraphEditorPluginManifest? manifest = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        if (pluginTypeName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginTypeName);
            pluginTypeName = pluginTypeName.Trim();
        }

        return new GraphEditorPluginRegistration(
            plugin: null,
            assemblyPath: Path.GetFullPath(assemblyPath),
            packagePath: null,
            pluginTypeName: pluginTypeName,
            manifest: manifest,
            provenanceEvidence: provenanceEvidence,
            stage: null);
    }

    /// <summary>
    /// Creates a registration from a plugin package path.
    /// </summary>
    public static GraphEditorPluginRegistration FromPackagePath(
        string packagePath,
        GraphEditorPluginManifest? manifest = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        return new GraphEditorPluginRegistration(
            plugin: null,
            assemblyPath: null,
            packagePath: Path.GetFullPath(packagePath),
            pluginTypeName: null,
            manifest: manifest,
            provenanceEvidence: provenanceEvidence,
            stage: null);
    }

    /// <summary>
    /// Creates a registration from a previously staged package and its main assembly path.
    /// </summary>
    public static GraphEditorPluginRegistration FromStagedPackage(
        string packagePath,
        string assemblyPath,
        string? pluginTypeName,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        GraphEditorPluginStageSnapshot stage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);
        ArgumentNullException.ThrowIfNull(stage);

        if (pluginTypeName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginTypeName);
            pluginTypeName = pluginTypeName.Trim();
        }

        var fullPackagePath = Path.GetFullPath(packagePath);
        var fullAssemblyPath = Path.GetFullPath(assemblyPath);

        if (string.IsNullOrWhiteSpace(stage.MainAssemblyPath))
        {
            throw new ArgumentException("Stage snapshot must expose a main assembly path for staged package registrations.", nameof(stage));
        }

        if (!string.Equals(Path.GetFullPath(stage.PackagePath), fullPackagePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Stage snapshot package path must match the staged package registration path.", nameof(stage));
        }

        if (!string.Equals(Path.GetFullPath(stage.MainAssemblyPath), fullAssemblyPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Stage snapshot main assembly path must match the staged package registration assembly path.", nameof(stage));
        }

        if (pluginTypeName is not null
            && stage.PluginTypeName is not null
            && !string.Equals(stage.PluginTypeName, pluginTypeName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Stage snapshot plugin type name must match the staged package registration plugin type name.", nameof(stage));
        }

        return new GraphEditorPluginRegistration(
            plugin: null,
            assemblyPath: fullAssemblyPath,
            packagePath: fullPackagePath,
            pluginTypeName: pluginTypeName,
            manifest: manifest,
            provenanceEvidence: provenanceEvidence,
            stage: stage);
    }
}
