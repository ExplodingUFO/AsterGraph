using AsterGraph.Editor.Runtime;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Represents one host-consumable action descriptor that can be rendered into menus, rails, and palettes.
/// </summary>
public sealed class AsterGraphHostedActionDescriptor
{
    private readonly Func<bool> _execute;

    public AsterGraphHostedActionDescriptor(
        GraphEditorCommandDescriptorSnapshot descriptor,
        Func<bool> execute,
        string? commandId = null)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        ArgumentNullException.ThrowIfNull(execute);

        CommandId = commandId;
        _execute = execute;
    }

    public GraphEditorCommandDescriptorSnapshot Descriptor { get; }

    public string Id => Descriptor.Id;

    public string Title => Descriptor.Title;

    public string Group => Descriptor.Group;

    public string? IconKey => Descriptor.IconKey;

    public string? DefaultShortcut => Descriptor.DefaultShortcut;

    public string? DisabledReason => Descriptor.DisabledReason;

    public string? RecoveryHint => Descriptor.RecoveryHint;

    public string? RecoveryCommandId => Descriptor.RecoveryCommandId;

    public string? CommandId { get; }

    public GraphEditorCommandSourceKind CommandSource => Descriptor.Source;

    public bool CanExecute => Descriptor.CanExecute;

    public bool TryExecute()
        => CanExecute && _execute();
}
