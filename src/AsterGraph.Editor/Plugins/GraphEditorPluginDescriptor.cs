namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 描述一个可加载图编辑器插件的稳定元数据。
/// </summary>
public sealed record GraphEditorPluginDescriptor
{
    /// <summary>
    /// 初始化插件描述。
    /// </summary>
    public GraphEditorPluginDescriptor(
        string id,
        string displayName,
        string? description = null,
        string? version = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        Id = id.Trim();
        DisplayName = displayName.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim();
    }

    /// <summary>
    /// 稳定插件标识。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 宿主可读名称。
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// 可选描述。
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 可选版本文本。
    /// </summary>
    public string? Version { get; }
}
