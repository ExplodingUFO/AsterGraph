using System.IO;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 表示一次显式插件注册请求。
/// </summary>
public sealed record GraphEditorPluginRegistration
{
    private GraphEditorPluginRegistration(
        IGraphEditorPlugin? plugin,
        string? assemblyPath,
        string? packagePath,
        string? pluginTypeName,
        GraphEditorPluginManifest? manifest,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence,
        GraphEditorPluginStageSnapshot? stage)
    {
        Plugin = plugin;
        AssemblyPath = assemblyPath;
        PackagePath = packagePath;
        PluginTypeName = pluginTypeName;
        Manifest = manifest;
        ProvenanceEvidence = provenanceEvidence ?? GraphEditorPluginProvenanceEvidence.NotProvided;
        Stage = stage;
    }

    /// <summary>
    /// 直接注册的插件实例。
    /// </summary>
    public IGraphEditorPlugin? Plugin { get; }

    /// <summary>
    /// 程序集路径注册时的插件程序集绝对路径。
    /// </summary>
    public string? AssemblyPath { get; }

    /// <summary>
    /// 包路径注册时的插件包归档绝对路径。
    /// </summary>
    public string? PackagePath { get; }

    /// <summary>
    /// 可选的显式插件类型名。
    /// </summary>
    public string? PluginTypeName { get; }

    /// <summary>
    /// 可选的插件清单元数据。
    /// </summary>
    public GraphEditorPluginManifest? Manifest { get; }

    /// <summary>
    /// 可选的来源和签名证据。
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }

    /// <summary>
    /// 可选的包暂存结果元数据。
    /// </summary>
    public GraphEditorPluginStageSnapshot? Stage { get; }

    /// <summary>
    /// 是否为直接实例注册。
    /// </summary>
    public bool IsDirectRegistration => Plugin is not null;

    /// <summary>
    /// 是否为程序集路径注册。
    /// </summary>
    public bool IsAssemblyRegistration => AssemblyPath is not null;

    /// <summary>
    /// 是否为包路径注册。
    /// </summary>
    public bool IsPackageRegistration => PackagePath is not null;

    /// <summary>
    /// 基于直接插件实例创建注册项。
    /// </summary>
    public static GraphEditorPluginRegistration FromPlugin(
        IGraphEditorPlugin plugin,
        GraphEditorPluginManifest? manifest = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return new GraphEditorPluginRegistration(plugin, null, null, null, manifest, provenanceEvidence, null);
    }

    /// <summary>
    /// 基于程序集路径创建注册项。
    /// </summary>
    public static GraphEditorPluginRegistration FromAssemblyPath(
        string assemblyPath,
        string? pluginTypeName = null,
        GraphEditorPluginManifest? manifest = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        if (pluginTypeName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginTypeName);
            pluginTypeName = pluginTypeName.Trim();
        }

        return new GraphEditorPluginRegistration(
            plugin: null,
            assemblyPath: Path.GetFullPath(assemblyPath),
            packagePath: null,
            pluginTypeName: pluginTypeName,
            manifest: manifest,
            provenanceEvidence: provenanceEvidence,
            stage: null);
    }

    /// <summary>
    /// 基于包路径创建注册项。
    /// </summary>
    public static GraphEditorPluginRegistration FromPackagePath(
        string packagePath,
        GraphEditorPluginManifest? manifest = null,
        GraphEditorPluginProvenanceEvidence? provenanceEvidence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);

        return new GraphEditorPluginRegistration(
            plugin: null,
            assemblyPath: null,
            packagePath: Path.GetFullPath(packagePath),
            pluginTypeName: null,
            manifest: manifest,
            provenanceEvidence: provenanceEvidence,
            stage: null);
    }

    /// <summary>
    /// 基于已验证暂存的包和主程序集路径创建注册项。
    /// </summary>
    public static GraphEditorPluginRegistration FromStagedPackage(
        string packagePath,
        string assemblyPath,
        string? pluginTypeName,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence,
        GraphEditorPluginStageSnapshot stage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyPath);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);
        ArgumentNullException.ThrowIfNull(stage);

        if (pluginTypeName is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pluginTypeName);
            pluginTypeName = pluginTypeName.Trim();
        }

        var fullPackagePath = Path.GetFullPath(packagePath);
        var fullAssemblyPath = Path.GetFullPath(assemblyPath);

        if (string.IsNullOrWhiteSpace(stage.MainAssemblyPath))
        {
            throw new ArgumentException("Stage snapshot must expose a main assembly path for staged package registrations.", nameof(stage));
        }

        if (!string.Equals(Path.GetFullPath(stage.PackagePath), fullPackagePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Stage snapshot package path must match the staged package registration path.", nameof(stage));
        }

        if (!string.Equals(Path.GetFullPath(stage.MainAssemblyPath), fullAssemblyPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Stage snapshot main assembly path must match the staged package registration assembly path.", nameof(stage));
        }

        if (pluginTypeName is not null
            && stage.PluginTypeName is not null
            && !string.Equals(stage.PluginTypeName, pluginTypeName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Stage snapshot plugin type name must match the staged package registration plugin type name.", nameof(stage));
        }

        return new GraphEditorPluginRegistration(
            plugin: null,
            assemblyPath: fullAssemblyPath,
            packagePath: fullPackagePath,
            pluginTypeName: pluginTypeName,
            manifest: manifest,
            provenanceEvidence: provenanceEvidence,
            stage: stage);
    }
}
