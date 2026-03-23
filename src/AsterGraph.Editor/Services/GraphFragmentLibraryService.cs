using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 管理片段模板目录的扫描、读取与保存。
/// </summary>
public sealed class GraphFragmentLibraryService
{
    public GraphFragmentLibraryService(string? libraryPath = null)
    {
        LibraryPath = libraryPath ?? GetDefaultLibraryPath();
    }

    public string LibraryPath { get; }

    public static string GetDefaultLibraryPath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraphDemo",
            "fragments");

    public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
    {
        if (!Directory.Exists(LibraryPath))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(LibraryPath, "*.json", SearchOption.TopDirectoryOnly)
            .Select(CreateTemplateInfo)
            .OrderByDescending(item => item.LastModified)
            .ToList();
    }

    internal string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        Directory.CreateDirectory(LibraryPath);
        var safeName = SanitizeName(name);
        var fileName = $"{safeName}-{DateTime.Now:yyyyMMdd-HHmmss}.json";
        var fullPath = Path.Combine(LibraryPath, fileName);
        File.WriteAllText(fullPath, GraphClipboardPayloadSerializer.Serialize(fragment));
        return fullPath;
    }

    internal GraphSelectionFragment LoadTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var text = File.ReadAllText(path);
        if (!GraphClipboardPayloadSerializer.TryDeserialize(text, out var fragment) || fragment is null)
        {
            throw new InvalidOperationException("Failed to deserialize the fragment template.");
        }

        return fragment;
    }

    public void DeleteTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private FragmentTemplateInfo CreateTemplateInfo(string path)
    {
        var text = File.ReadAllText(path);
        var fragment = GraphClipboardPayloadSerializer.TryDeserialize(text, out var parsedFragment)
            ? parsedFragment
            : null;
        return new FragmentTemplateInfo(
            System.IO.Path.GetFileNameWithoutExtension(path),
            path,
            fragment?.Nodes.Count ?? 0,
            fragment?.Connections.Count ?? 0,
            File.GetLastWriteTime(path));
    }

    private static string SanitizeName(string? name)
    {
        var fallback = string.IsNullOrWhiteSpace(name) ? "fragment" : name.Trim();
        foreach (var invalid in System.IO.Path.GetInvalidFileNameChars())
        {
            fallback = fallback.Replace(invalid, '-');
        }

        return fallback;
    }
}
