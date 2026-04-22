using AsterGraph.Wpf.Controls;

namespace AsterGraph.Wpf.Hosting;

/// <summary>
/// Provides the canonical hosted-UI composition entry point for the WPF shell.
/// </summary>
/// <remarks>
/// This factory applies the host-supplied editor to the shell and returns a compact default
/// WPF shell surface that binds only to the shared editor façade.
/// </remarks>
public static class AsterGraphWpfViewFactory
{
    /// <summary>
    /// Creates a default <see cref="GraphEditorView" /> from host-supplied options.
    /// </summary>
    /// <param name="options">The hosted-UI composition options.</param>
    /// <returns>A new graph-editor shell view.</returns>
    public static GraphEditorView Create(AsterGraphWpfViewOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Editor);

        return new GraphEditorView
        {
            Editor = options.Editor,
            ApplyHostServices = options.ApplyHostServices,
        };
    }
}
