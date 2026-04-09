namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 定义规范插件候选项发现所需的宿主输入。
/// </summary>
public sealed record GraphEditorPluginDiscoveryOptions
{
    /// <summary>
    /// 本地目录发现源集合。
    /// </summary>
    public IReadOnlyList<GraphEditorPluginDirectoryDiscoverySource> DirectorySources { get; init; } = [];

    /// <summary>
    /// 宿主提供的清单发现源集合。
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginManifestSource> ManifestSources { get; init; } = [];

    /// <summary>
    /// 可选的插件信任策略；用于在候选发现阶段执行宿主治理决策。
    /// </summary>
    public IGraphEditorPluginTrustPolicy? TrustPolicy { get; init; }
}
