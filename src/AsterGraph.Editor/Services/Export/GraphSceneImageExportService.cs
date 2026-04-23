using System.Text;
using AsterGraph.Editor.Runtime;
using SkiaSharp;
using Svg.Skia;

namespace AsterGraph.Editor.Services;

/// <summary>
/// Writes a stable raster snapshot of the current editor scene by rasterizing the canonical SVG scene document.
/// </summary>
public sealed class GraphSceneImageExportService : IGraphSceneImageExportService
{
    private readonly string? _storageRootPath;

    public GraphSceneImageExportService(string? storageRootPath = null)
    {
        _storageRootPath = string.IsNullOrWhiteSpace(storageRootPath)
            ? null
            : storageRootPath.Trim();
    }

    public string GetExportPath(GraphEditorSceneImageExportFormat format)
        => GraphEditorStorageDefaults.GetSceneImageExportPath(format, _storageRootPath);

    public string Export(
        GraphEditorSceneSnapshot scene,
        GraphEditorSceneImageExportFormat format,
        string? path = null,
        GraphEditorSceneImageExportOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var resolvedPath = string.IsNullOrWhiteSpace(path)
            ? GetExportPath(format)
            : path.Trim();
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var scale = options?.Scale ?? 1d;
        if (scale <= 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Image export scale must be positive.");
        }

        var quality = options?.Quality ?? 92;
        if (quality is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Image export quality must be between 0 and 100.");
        }

        var svgDocument = GraphSceneSvgDocumentBuilder.Build(scene, options?.BackgroundHex);
        using var svg = new SKSvg();
        using var svgStream = new MemoryStream(Encoding.UTF8.GetBytes(svgDocument));
        var picture = svg.Load(svgStream);
        if (picture is null)
        {
            throw new InvalidOperationException("Generated scene SVG could not be rasterized.");
        }

        svg.Save(
            resolvedPath,
            SKColors.Transparent,
            ResolveEncodedFormat(format),
            quality,
            (float)scale,
            (float)scale);
        return resolvedPath;
    }

    private static SKEncodedImageFormat ResolveEncodedFormat(GraphEditorSceneImageExportFormat format)
        => format switch
        {
            GraphEditorSceneImageExportFormat.Png => SKEncodedImageFormat.Png,
            GraphEditorSceneImageExportFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported scene image export format."),
        };
}
