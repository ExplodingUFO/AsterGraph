using AsterGraph.Avalonia.Controls;

namespace AsterGraph.Avalonia.Hosting;

/// <summary>
/// Provides the canonical hosted-UI composition entry point for the default <see cref="GraphEditorView" />.
/// </summary>
/// <remarks>
/// This factory applies host-supplied editor and view options and returns the stock shell that composes
/// the canvas, inspector, and mini-map surfaces. Direct <see cref="GraphEditorView" /> construction remains
/// supported for gradual migration, but new hosted-UI code should prefer this factory. The canonical runtime
/// authority still lives on <c>CreateSession(...)</c> and <c>IGraphEditorSession</c>; this factory composes the
/// current Avalonia adapter on top of the retained editor facade.
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

        var workbench = options.Workbench;
        return new GraphEditorView
        {
            Editor = options.Editor,
            ChromeMode = options.ChromeMode,
            IsHeaderChromeVisible = workbench.ShowHeaderChrome,
            IsLibraryChromeVisible = workbench.ShowNodePalette,
            IsInspectorChromeVisible = workbench.ShowInspector,
            IsStatusChromeVisible = workbench.ShowStatus,
            EnableDefaultContextMenu = options.EnableDefaultContextMenu,
            CommandShortcutPolicy = options.CommandShortcutPolicy,
            EnableDefaultWheelViewportGestures = workbench.EnableDefaultWheelViewportGestures,
            EnableAltLeftDragPanning = workbench.EnableAltLeftDragPanning,
            WorkbenchPerformanceMode = workbench.PerformanceMode,
            Presentation = options.Presentation,
        };
    }
}
