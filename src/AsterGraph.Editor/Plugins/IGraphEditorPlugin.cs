namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Defines the minimal public contract for an editor plugin.
/// </summary>
public interface IGraphEditorPlugin
{
    /// <summary>
    /// Gets the plugin descriptor.
    /// </summary>
    GraphEditorPluginDescriptor Descriptor { get; }

    /// <summary>
    /// Registers the plugin contributions into the composition builder.
    /// </summary>
    /// <param name="builder">The plugin composition builder.</param>
    void Register(GraphEditorPluginBuilder builder);
}
