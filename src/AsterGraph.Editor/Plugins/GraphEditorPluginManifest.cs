namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 指示插件清单的来源类型。
/// </summary>
public enum GraphEditorPluginManifestSourceKind
{
    /// <summary>
    /// 来自直接插件实例注册。
    /// </summary>
    DirectRegistration,

    /// <summary>
    /// 来自程序集路径注册。
    /// </summary>
    AssemblyPath,

    /// <summary>
    /// 来自包归档。
    /// </summary>
    PackageArchive,

    /// <summary>
    /// 来自独立清单或未来的候选发现源。
    /// </summary>
    Manifest,
}

/// <summary>
/// 描述插件清单的来源和分发线索。
/// </summary>
public sealed record GraphEditorPluginManifestProvenance
{
    /// <summary>
    /// 初始化插件来源信息。
    /// </summary>
    public GraphEditorPluginManifestProvenance(
        GraphEditorPluginManifestSourceKind sourceKind,
        string source,
        string? publisher = null,
        string? packageId = null,
        string? packageVersion = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        SourceKind = sourceKind;
        Source = source.Trim();
        Publisher = string.IsNullOrWhiteSpace(publisher) ? null : publisher.Trim();
        PackageId = string.IsNullOrWhiteSpace(packageId) ? null : packageId.Trim();
        PackageVersion = string.IsNullOrWhiteSpace(packageVersion) ? null : packageVersion.Trim();
    }

    /// <summary>
    /// 来源类型。
    /// </summary>
    public GraphEditorPluginManifestSourceKind SourceKind { get; }

    /// <summary>
    /// 稳定来源标识。
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// 可选发布者信息。
    /// </summary>
    public string? Publisher { get; }

    /// <summary>
    /// 可选包标识。
    /// </summary>
    public string? PackageId { get; }

    /// <summary>
    /// 可选包版本。
    /// </summary>
    public string? PackageVersion { get; }
}

/// <summary>
/// 描述插件宿主兼容性元数据。
/// </summary>
public sealed record GraphEditorPluginCompatibilityManifest
{
    /// <summary>
    /// 初始化兼容性清单。
    /// </summary>
    public GraphEditorPluginCompatibilityManifest(
        string? minimumAsterGraphVersion = null,
        string? maximumAsterGraphVersion = null,
        string? targetFramework = null,
        string? runtimeSurface = null)
    {
        MinimumAsterGraphVersion = string.IsNullOrWhiteSpace(minimumAsterGraphVersion) ? null : minimumAsterGraphVersion.Trim();
        MaximumAsterGraphVersion = string.IsNullOrWhiteSpace(maximumAsterGraphVersion) ? null : maximumAsterGraphVersion.Trim();
        TargetFramework = string.IsNullOrWhiteSpace(targetFramework) ? null : targetFramework.Trim();
        RuntimeSurface = string.IsNullOrWhiteSpace(runtimeSurface) ? null : runtimeSurface.Trim();
    }

    /// <summary>
    /// 最低支持的 AsterGraph 版本。
    /// </summary>
    public string? MinimumAsterGraphVersion { get; }

    /// <summary>
    /// 最高支持的 AsterGraph 版本。
    /// </summary>
    public string? MaximumAsterGraphVersion { get; }

    /// <summary>
    /// 目标框架标识。
    /// </summary>
    public string? TargetFramework { get; }

    /// <summary>
    /// 运行时集成表面摘要。
    /// </summary>
    public string? RuntimeSurface { get; }

    /// <summary>
    /// 空兼容性清单。
    /// </summary>
    public static GraphEditorPluginCompatibilityManifest Empty { get; } = new();
}

/// <summary>
/// 描述一个可在加载前读取的插件清单。
/// </summary>
public sealed record GraphEditorPluginManifest
{
    /// <summary>
    /// 初始化插件清单。
    /// </summary>
    public GraphEditorPluginManifest(
        string id,
        string displayName,
        GraphEditorPluginManifestProvenance provenance,
        string? description = null,
        string? version = null,
        GraphEditorPluginCompatibilityManifest? compatibility = null,
        string? capabilitySummary = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(provenance);

        Id = id.Trim();
        DisplayName = displayName.Trim();
        Provenance = provenance;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim();
        Compatibility = compatibility ?? GraphEditorPluginCompatibilityManifest.Empty;
        CapabilitySummary = string.IsNullOrWhiteSpace(capabilitySummary) ? null : capabilitySummary.Trim();
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

    /// <summary>
    /// 兼容性摘要。
    /// </summary>
    public GraphEditorPluginCompatibilityManifest Compatibility { get; }

    /// <summary>
    /// 能力摘要。
    /// </summary>
    public string? CapabilitySummary { get; }

    /// <summary>
    /// 来源和分发线索。
    /// </summary>
    public GraphEditorPluginManifestProvenance Provenance { get; }
}
