namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 指示插件加载来源类型。
/// </summary>
public enum GraphEditorPluginLoadSourceKind
{
    /// <summary>
    /// 由直接插件实例注册产生。
    /// </summary>
    Direct,

    /// <summary>
    /// 由程序集路径注册产生。
    /// </summary>
    Assembly,
}

/// <summary>
/// 指示插件加载结果状态。
/// </summary>
public enum GraphEditorPluginLoadStatus
{
    /// <summary>
    /// 插件已成功加载。
    /// </summary>
    Loaded,

    /// <summary>
    /// 插件加载失败。
    /// </summary>
    Failed,
}

/// <summary>
/// 描述一次插件加载暴露出的贡献摘要。
/// </summary>
public sealed record GraphEditorPluginContributionSummarySnapshot
{
    /// <summary>
    /// 初始化贡献摘要。
    /// </summary>
    public GraphEditorPluginContributionSummarySnapshot(
        int nodeDefinitionProviderCount,
        int contextMenuAugmentorCount,
        int nodePresentationProviderCount,
        int localizationProviderCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(nodeDefinitionProviderCount);
        ArgumentOutOfRangeException.ThrowIfNegative(contextMenuAugmentorCount);
        ArgumentOutOfRangeException.ThrowIfNegative(nodePresentationProviderCount);
        ArgumentOutOfRangeException.ThrowIfNegative(localizationProviderCount);

        NodeDefinitionProviderCount = nodeDefinitionProviderCount;
        ContextMenuAugmentorCount = contextMenuAugmentorCount;
        NodePresentationProviderCount = nodePresentationProviderCount;
        LocalizationProviderCount = localizationProviderCount;
    }

    /// <summary>
    /// 节点定义提供器数量。
    /// </summary>
    public int NodeDefinitionProviderCount { get; }

    /// <summary>
    /// 右键菜单增强器数量。
    /// </summary>
    public int ContextMenuAugmentorCount { get; }

    /// <summary>
    /// 节点展示提供器数量。
    /// </summary>
    public int NodePresentationProviderCount { get; }

    /// <summary>
    /// 本地化提供器数量。
    /// </summary>
    public int LocalizationProviderCount { get; }

    /// <summary>
    /// 空贡献摘要。
    /// </summary>
    public static GraphEditorPluginContributionSummarySnapshot Empty { get; } = new(0, 0, 0, 0);
}

/// <summary>
/// 表示一次稳定的插件加载检查快照。
/// </summary>
public sealed record GraphEditorPluginLoadSnapshot
{
    /// <summary>
    /// 初始化插件加载快照。
    /// </summary>
    public GraphEditorPluginLoadSnapshot(
        GraphEditorPluginLoadSourceKind sourceKind,
        string source,
        GraphEditorPluginLoadStatus status,
        GraphEditorPluginContributionSummarySnapshot contributions,
        GraphEditorPluginDescriptor? descriptor = null,
        string? requestedPluginTypeName = null,
        string? resolvedPluginTypeName = null,
        string? failureMessage = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(contributions);

        SourceKind = sourceKind;
        Source = source.Trim();
        Status = status;
        Contributions = contributions;
        Descriptor = descriptor;
        RequestedPluginTypeName = string.IsNullOrWhiteSpace(requestedPluginTypeName) ? null : requestedPluginTypeName.Trim();
        ResolvedPluginTypeName = string.IsNullOrWhiteSpace(resolvedPluginTypeName) ? null : resolvedPluginTypeName.Trim();
        FailureMessage = string.IsNullOrWhiteSpace(failureMessage) ? null : failureMessage.Trim();
    }

    /// <summary>
    /// 加载来源类型。
    /// </summary>
    public GraphEditorPluginLoadSourceKind SourceKind { get; }

    /// <summary>
    /// 稳定来源标识。程序集注册时为绝对路径，直接注册时为插件类型名。
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// 当前加载状态。
    /// </summary>
    public GraphEditorPluginLoadStatus Status { get; }

    /// <summary>
    /// 插件贡献摘要。
    /// </summary>
    public GraphEditorPluginContributionSummarySnapshot Contributions { get; }

    /// <summary>
    /// 成功加载时的插件描述信息。
    /// </summary>
    public GraphEditorPluginDescriptor? Descriptor { get; }

    /// <summary>
    /// 注册项请求的显式插件类型名。
    /// </summary>
    public string? RequestedPluginTypeName { get; }

    /// <summary>
    /// 实际解析并激活的插件类型名。
    /// </summary>
    public string? ResolvedPluginTypeName { get; }

    /// <summary>
    /// 失败时的稳定错误消息。
    /// </summary>
    public string? FailureMessage { get; }
}
