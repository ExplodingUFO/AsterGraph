namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one stable command on the editor control plane.
/// </summary>
public sealed record GraphEditorCommandDescriptorSnapshot
{
    public GraphEditorCommandDescriptorSnapshot(string id, bool isEnabled, string? disabledReason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id.Trim();
        IsEnabled = isEnabled;
        DisabledReason = string.IsNullOrWhiteSpace(disabledReason) ? null : disabledReason.Trim();
    }

    public string Id { get; }

    public bool IsEnabled { get; }

    public string? DisabledReason { get; }
}
