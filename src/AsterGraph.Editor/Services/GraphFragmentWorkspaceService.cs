namespace AsterGraph.Editor.Services;

/// <summary>
/// 管理图片段 JSON 文件的默认存储位置与读写操作。
/// </summary>
public sealed class GraphFragmentWorkspaceService
{
    /// <summary>
    /// 初始化片段工作区服务。
    /// </summary>
    /// <param name="fragmentPath">可选的默认片段文件路径。</param>
    public GraphFragmentWorkspaceService(string? fragmentPath = null)
    {
        FragmentPath = fragmentPath ?? GetDefaultFragmentPath();
    }

    /// <summary>
    /// 默认片段文件路径。
    /// </summary>
    public string FragmentPath { get; }

    /// <summary>
    /// 获取系统默认片段文件路径。
    /// </summary>
    public static string GetDefaultFragmentPath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraphDemo",
            "selection-fragment.json");

    /// <summary>
    /// 将片段保存到默认路径或指定路径。
    /// </summary>
    /// <param name="fragment">要保存的片段。</param>
    /// <param name="path">可选的目标路径。</param>
    public void Save(GraphSelectionFragment fragment, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        var resolvedPath = ResolvePath(path);
        var directory = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(resolvedPath, GraphClipboardPayloadSerializer.Serialize(fragment));
    }

    /// <summary>
    /// 从默认路径或指定路径读取片段。
    /// </summary>
    /// <param name="path">可选的源路径。</param>
    public GraphSelectionFragment Load(string? path = null)
    {
        var text = File.ReadAllText(ResolvePath(path));
        if (!GraphClipboardPayloadSerializer.TryDeserialize(text, out var fragment) || fragment is null)
        {
            throw new InvalidOperationException("Failed to deserialize the saved selection fragment.");
        }

        return fragment;
    }

    /// <summary>
    /// 判断默认路径或指定路径的片段文件是否存在。
    /// </summary>
    /// <param name="path">可选的目标路径。</param>
    public bool Exists(string? path = null)
        => File.Exists(ResolvePath(path));

    /// <summary>
    /// 删除默认路径或指定路径的片段文件。
    /// </summary>
    /// <param name="path">可选的目标路径。</param>
    public void Delete(string? path = null)
    {
        var resolvedPath = ResolvePath(path);
        if (File.Exists(resolvedPath))
        {
            File.Delete(resolvedPath);
        }
    }

    private string ResolvePath(string? path)
        => string.IsNullOrWhiteSpace(path) ? FragmentPath : path.Trim();
}
