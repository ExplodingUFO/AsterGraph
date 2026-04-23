using System.Text;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Services;

/// <summary>
/// Writes a stable SVG snapshot of the current editor scene without depending on a UI adapter.
/// </summary>
public sealed class GraphSceneSvgExportService : IGraphSceneSvgExportService
{
    public GraphSceneSvgExportService(string? exportPath = null)
    {
        ExportPath = string.IsNullOrWhiteSpace(exportPath)
            ? GraphEditorStorageDefaults.GetSceneSvgExportPath()
            : exportPath.Trim();
    }

    public string ExportPath { get; }

    public string Export(GraphEditorSceneSnapshot scene, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var resolvedPath = string.IsNullOrWhiteSpace(path) ? ExportPath : path.Trim();
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(resolvedPath, GraphSceneSvgDocumentBuilder.Build(scene), Encoding.UTF8);
        return resolvedPath;
    }
}
