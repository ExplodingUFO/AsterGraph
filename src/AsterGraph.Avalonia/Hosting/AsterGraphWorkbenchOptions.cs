using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Defines the stock hosted workbench surfaces composed by the Avalonia route.
/// </summary>
/// <remarks>
/// These options only control the existing <see cref="GraphEditorView" /> chrome. They do not create
/// a new runtime model; editor ownership remains on the session created by the editor factory.
/// </remarks>
public sealed record AsterGraphWorkbenchOptions
{
    /// <summary>
    /// Gets the default hosted workbench composition.
    /// </summary>
    public static AsterGraphWorkbenchOptions Default { get; } = new();

    /// <summary>
    /// Shows the header chrome that hosts the stock toolbar and command-palette entry.
    /// </summary>
    public bool ShowHeaderChrome { get; init; } = true;

    /// <summary>
    /// Shows the node palette / stencil chrome.
    /// </summary>
    public bool ShowNodePalette { get; init; } = true;

    /// <summary>
    /// Shows the inspector chrome, including validation, fragment, authoring, and mini-map sections.
    /// </summary>
    public bool ShowInspector { get; init; } = true;

    /// <summary>
    /// Shows the status chrome used for host diagnostics and validation summaries.
    /// </summary>
    public bool ShowStatus { get; init; } = true;

    /// <summary>
    /// Enables default wheel zoom and pan gestures on the hosted canvas.
    /// </summary>
    public bool EnableDefaultWheelViewportGestures { get; init; } = true;

    /// <summary>
    /// Enables Alt+left-drag panning on the hosted canvas.
    /// </summary>
    public bool EnableAltLeftDragPanning { get; init; } = true;
}
