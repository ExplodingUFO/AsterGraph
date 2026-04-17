namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Describes the stable metadata for a loadable editor plugin.
/// </summary>
public sealed record GraphEditorPluginDescriptor
{
    /// <summary>
    /// Initializes a plugin descriptor.
    /// </summary>
    public GraphEditorPluginDescriptor(
        string id,
        string displayName,
        string? description = null,
        string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Id = id.Trim();
        DisplayName = displayName.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim();
    }

    /// <summary>
    /// Gets the stable plugin identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the host-readable plugin name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the optional plugin description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the optional plugin version text.
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Builds a minimal plugin manifest from the current descriptor.
    /// </summary>
    /// <param name="provenance">The provenance information.</param>
    /// <param name="compatibility">The optional compatibility summary.</param>
    /// <param name="capabilitySummary">The optional capability summary.</param>
    /// <returns>A new plugin manifest.</returns>
    public GraphEditorPluginManifest ToManifest(
        GraphEditorPluginManifestProvenance provenance,
        GraphEditorPluginCompatibilityManifest? compatibility = null,
        string? capabilitySummary = null)
    {
        ArgumentNullException.ThrowIfNull(provenance);
        return new GraphEditorPluginManifest(
            Id,
            DisplayName,
            provenance,
            Description,
            Version,
            compatibility,
            capabilitySummary);
    }
}
