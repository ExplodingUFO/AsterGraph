using AsterGraph.Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Defines the host inputs used by <see cref="AsterGraphAvaloniaViewFactory" /> when composing the default Avalonia view.
/// </summary>
/// <remarks>
/// This options contract is the canonical hosted-UI entry point for the stock Avalonia shell.
/// Direct <c>new GraphEditorView { Editor = ... }</c> usage is still supported as a retained compatibility path.
/// </remarks>
public sealed record AsterGraphAvaloniaViewOptions
{
    /// <summary>
    /// The editor view model that should be bound to the view.
    /// </summary>
    public GraphEditorViewModel? Editor { get; init; }

    /// <summary>
    /// Controls how much stock shell chrome is rendered around the editor surfaces.
    /// </summary>
    public GraphEditorViewChromeMode ChromeMode { get; init; } = GraphEditorViewChromeMode.Default;

    /// <summary>
    /// Enables or disables the stock context-menu wiring.
    /// </summary>
    public bool EnableDefaultContextMenu { get; init; } = true;

    /// <summary>
    /// Controls the stock command-shortcut routing applied by the hosted Avalonia shell.
    /// </summary>
    public AsterGraphCommandShortcutPolicy CommandShortcutPolicy { get; init; } = AsterGraphCommandShortcutPolicy.Default;

    /// <summary>
    /// Optional presentation overrides for the stock Avalonia presenters.
    /// </summary>
    public AsterGraphPresentationOptions? Presentation { get; init; }
}
