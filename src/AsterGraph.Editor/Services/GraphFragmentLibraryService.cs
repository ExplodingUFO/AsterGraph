using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 管理片段模板目录的扫描、读取与保存。
/// </summary>
public sealed class GraphFragmentLibraryService : IGraphFragmentLibraryService
{
    private readonly IGraphClipboardPayloadSerializer _clipboardPayloadSerializer;

    /// <summary>
    /// 初始化片段模板库服务。
    /// </summary>
    /// <param name="libraryPath">可选的模板库目录路径。</param>
    /// <param name="clipboardPayloadSerializer">用于序列化模板内容的剪贴板载荷序列化器。</param>
    public GraphFragmentLibraryService(
        string? libraryPath = null,
        IGraphClipboardPayloadSerializer? clipboardPayloadSerializer = null)
    {
        LibraryPath = libraryPath ?? GetDefaultLibraryPath();
        _clipboardPayloadSerializer = clipboardPayloadSerializer ?? new GraphClipboardPayloadSerializer();
    }

    /// <summary>
    /// 片段模板库目录路径。
    /// </summary>
    public string LibraryPath { get; }

    /// <summary>
    /// 获取系统默认片段模板库目录。
    /// </summary>
    public static string GetDefaultLibraryPath()
        => GraphEditorStorageDefaults.GetFragmentLibraryPath();

    /// <summary>
    /// 枚举模板库中的全部片段模板元数据。
    /// </summary>
    /// <returns>模板元数据集合。</returns>
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

    public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        Directory.CreateDirectory(LibraryPath);
        var safeName = SanitizeName(name);
        var fileName = $"{safeName}-{DateTime.Now:yyyyMMdd-HHmmss}.json";
        var fullPath = Path.Combine(LibraryPath, fileName);
        File.WriteAllText(fullPath, _clipboardPayloadSerializer.Serialize(fragment));
        return fullPath;
    }

    public GraphSelectionFragment LoadTemplate(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var text = File.ReadAllText(path);
        if (!_clipboardPayloadSerializer.TryDeserialize(text, out var fragment) || fragment is null)
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
        var fragment = _clipboardPayloadSerializer.TryDeserialize(text, out var parsedFragment)
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
