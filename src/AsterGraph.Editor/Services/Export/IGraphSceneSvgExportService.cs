using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Services;

/// <summary>
/// Defines the host-replaceable contract for exporting the current immutable editor scene as an SVG document.
/// </summary>
public interface IGraphSceneSvgExportService
{
    /// <summary>
    /// Gets the default SVG export path used when callers omit an explicit destination.
    /// </summary>
    string ExportPath { get; }

    /// <summary>
    /// Exports the supplied immutable scene snapshot to the default SVG path or a caller-specified destination.
    /// </summary>
    /// <param name="scene">Current immutable editor scene snapshot.</param>
    /// <param name="path">Optional SVG destination override.</param>
    /// <returns>The resolved SVG path that was written.</returns>
    string Export(GraphEditorSceneSnapshot scene, string? path = null);
}
