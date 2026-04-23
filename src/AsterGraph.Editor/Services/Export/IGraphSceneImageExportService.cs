using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Services;

/// <summary>
/// Writes stable raster snapshots of the current editor scene without depending on a UI adapter.
/// </summary>
public interface IGraphSceneImageExportService
{
    /// <summary>
    /// Returns the default image export path for one raster format.
    /// </summary>
    /// <param name="format">Requested raster output format.</param>
    /// <returns>The default export path for the requested format.</returns>
    string GetExportPath(GraphEditorSceneImageExportFormat format);

    /// <summary>
    /// Writes the supplied scene snapshot to a raster image file.
    /// </summary>
    /// <param name="scene">The immutable scene snapshot to export.</param>
    /// <param name="format">Requested raster output format.</param>
    /// <param name="path">Optional destination override.</param>
    /// <param name="options">Optional image shaping overrides.</param>
    /// <returns>The resolved written image path.</returns>
    string Export(
        GraphEditorSceneSnapshot scene,
        GraphEditorSceneImageExportFormat format,
        string? path = null,
        GraphEditorSceneImageExportOptions? options = null);
}
