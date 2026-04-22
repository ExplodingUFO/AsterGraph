using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Wpf.Hosting;

/// <summary>
/// Defines the host inputs used by <see cref="AsterGraphWpfViewFactory" /> when composing the default WPF shell.
/// </summary>
/// <remarks>
/// This options contract keeps the hosted surface path compact and focused on the editor facade.
/// </remarks>
public sealed record AsterGraphWpfViewOptions
{
    /// <summary>
    /// The editor view model that should be bound to the view.
    /// </summary>
    public GraphEditorViewModel? Editor { get; init; }

    /// <summary>
    /// Optional host switch for applying host services from the helper factory.
    /// </summary>
    public bool ApplyHostServices { get; init; } = true;
}
