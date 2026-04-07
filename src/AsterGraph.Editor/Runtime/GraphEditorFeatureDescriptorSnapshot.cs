namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes a discoverable runtime feature, service, or integration seam.
/// </summary>
public sealed record GraphEditorFeatureDescriptorSnapshot
{
    /// <summary>
    /// Initializes a feature descriptor snapshot.
    /// </summary>
    /// <param name="id">Stable feature identifier.</param>
    /// <param name="category">Feature category, for example capability or service.</param>
    /// <param name="isAvailable">Whether the feature is available in the current session.</param>
    public GraphEditorFeatureDescriptorSnapshot(string id, string category, bool isAvailable)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        Id = id.Trim();
        Category = category.Trim();
        IsAvailable = isAvailable;
    }

    /// <summary>
    /// Stable feature identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Feature category.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Whether the feature is currently available.
    /// </summary>
    public bool IsAvailable { get; }
}
