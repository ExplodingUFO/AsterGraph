namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Describes one contextual authoring tool backed by the shared command route.
/// </summary>
public sealed record GraphEditorToolDescriptorSnapshot
{
    public GraphEditorToolDescriptorSnapshot(
        string id,
        GraphEditorToolContextKind contextKind,
        GraphEditorCommandDescriptorSnapshot command,
        GraphEditorCommandInvocationSnapshot invocation,
        int order = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Command = command ?? throw new ArgumentNullException(nameof(command));
        Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));

        Id = id.Trim();
        ContextKind = contextKind;
        Order = order;
    }

    public string Id { get; }

    public GraphEditorToolContextKind ContextKind { get; }

    public GraphEditorCommandDescriptorSnapshot Command { get; }

    public GraphEditorCommandInvocationSnapshot Invocation { get; }

    public int Order { get; }

    public string Title => Command.Title;

    public string Group => Command.Group;

    public string? IconKey => Command.IconKey;

    public string? DefaultShortcut => Command.DefaultShortcut;

    public GraphEditorCommandSourceKind Source => Command.Source;

    public bool CanExecute => Command.CanExecute;

    public string? DisabledReason => Command.DisabledReason;
}
