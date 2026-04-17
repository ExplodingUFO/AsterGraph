using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Provides the canonical hosted-UI composition entry point for the default <see cref="GraphEditorView" />.
/// </summary>
/// <remarks>
/// This factory applies host-supplied editor and view options and returns the stock shell that composes
/// the canvas, inspector, and mini-map surfaces. Direct <see cref="GraphEditorView" /> construction remains
/// supported for gradual migration, but new hosted-UI code should prefer this factory.
/// </remarks>
public static class AsterGraphAvaloniaViewFactory
{
    /// <summary>
    /// Creates a default <see cref="GraphEditorView" /> from host-supplied options.
    /// </summary>
    /// <param name="options">The hosted-UI composition options.</param>
    /// <returns>A new graph-editor view.</returns>
    public static GraphEditorView Create(AsterGraphAvaloniaViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Editor);

        return new GraphEditorView
        {
            Editor = options.Editor,
            ChromeMode = options.ChromeMode,
            EnableDefaultContextMenu = options.EnableDefaultContextMenu,
            EnableDefaultCommandShortcuts = options.EnableDefaultCommandShortcuts,
            Presentation = options.Presentation,
        };
    }
}
