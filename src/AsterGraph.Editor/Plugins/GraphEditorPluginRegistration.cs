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
        GraphEditorPluginProvenanceEvidence? provenanceEvidence)
    {
        Plugin = plugin;
        AssemblyPath = assemblyPath;
        PackagePath = packagePath;
        PluginTypeName = pluginTypeName;
        Manifest = manifest;
        ProvenanceEvidence = provenanceEvidence ?? GraphEditorPluginProvenanceEvidence.NotProvided;
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
        return new GraphEditorPluginRegistration(plugin, null, null, null, manifest, provenanceEvidence);
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
            provenanceEvidence: provenanceEvidence);
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
            provenanceEvidence: provenanceEvidence);
    }
}
