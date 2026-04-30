namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes a runtime command and the stock surfaces that can present it.
/// </summary>
public sealed record GraphEditorCommandRegistryEntrySnapshot
{
    public GraphEditorCommandRegistryEntrySnapshot(
        GraphEditorCommandDescriptorSnapshot descriptor,
        IReadOnlyList<GraphEditorCommandPlacementSnapshot>? placements = null)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        Placements = placements ?? [];
    }

    public GraphEditorCommandDescriptorSnapshot Descriptor { get; }

    public IReadOnlyList<GraphEditorCommandPlacementSnapshot> Placements { get; }

    public string CommandId => Descriptor.Id;

    public string Title => Descriptor.Title;

    public string Group => Descriptor.Group;

    public string? IconKey => Descriptor.IconKey;

    public string? DefaultShortcut => Descriptor.DefaultShortcut;

    public GraphEditorCommandSourceKind Source => Descriptor.Source;
}
