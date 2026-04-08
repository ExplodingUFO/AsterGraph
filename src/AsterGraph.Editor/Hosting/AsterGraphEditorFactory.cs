using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Kernel;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Plugins.Internal;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// 提供 AsterGraph 编辑器运行时的规范宿主组合入口。
/// </summary>
/// <remarks>
/// <see cref="CreateSession(AsterGraphEditorOptions)"/> 是当前的规范 runtime-first 组合入口，
/// 适用于宿主自定义 UI 或直接围绕 <see cref="IGraphEditorSession"/> 进行集成。
/// <see cref="Create(AsterGraphEditorOptions)"/> 是当前的规范 hosted-UI 组合入口，
/// 返回保留的 <see cref="GraphEditorViewModel"/> 兼容立面，便于宿主在迁移窗口内继续接入默认 Avalonia 外壳。
/// 直接构造 <see cref="GraphEditorViewModel"/> 仍然受支持，但应视为兼容路径而不是新的首选组合方式。
/// </remarks>
public static class AsterGraphEditorFactory
{
    /// <summary>
    /// 使用宿主提供的选项创建一个 <see cref="GraphEditorViewModel"/>。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的图编辑器视图模型。</returns>
    /// <remarks>
    /// 新的默认宿主 UI 组合代码应优先使用此工厂入口，然后搭配
    /// <c>AsterGraphAvaloniaViewFactory.Create(...)</c> 或其他保留的 UI 表面工厂。
    /// 该返回值仍然是兼容立面，但其 <see cref="GraphEditorViewModel.Session"/> 现在建立在共享 runtime 边界之上。
    /// </remarks>
    public static GraphEditorViewModel Create(AsterGraphEditorOptions options)
    {
        var resolved = Resolve(options);

        var editor = new GraphEditorViewModel(
            resolved.Options.Document!,
            resolved.NodeCatalog,
            resolved.Options.CompatibilityService!,
            resolved.WorkspaceService,
            resolved.FragmentWorkspaceService,
            resolved.StyleOptions,
            resolved.BehaviorOptions,
            resolved.FragmentLibraryService,
            resolved.Options.ContextMenuAugmentor,
            resolved.NodePresentationProvider,
            resolved.LocalizationProvider,
            resolved.ClipboardPayloadSerializer,
            resolved.Options.DiagnosticsSink);

        if (editor.Session is GraphEditorSession runtimeSession)
        {
            runtimeSession.ConfigureInstrumentation(resolved.Options.Instrumentation);
            runtimeSession.SetPluginContextMenuAugmentors(resolved.PluginContextMenuAugmentors);
            runtimeSession.SetPluginLoadSnapshots(resolved.PluginLoadResult.Snapshots);
            PublishDiagnostics(runtimeSession, resolved.PluginLoadResult.Diagnostics);
        }

        return editor;
    }

    /// <summary>
    /// 使用宿主提供的选项创建一个 <see cref="IGraphEditorSession"/>。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的图编辑器运行时会话。</returns>
    /// <remarks>
    /// 这是当前自定义 UI 宿主和自动化/插件前置集成的规范入口。
    /// 与 <see cref="Create(AsterGraphEditorOptions)"/> 返回的保留兼容立面相比，
    /// 该路径不会携带 <see cref="GraphEditorViewModel"/> 特有的兼容命令或 MVVM 表面。
    /// </remarks>
    public static IGraphEditorSession CreateSession(AsterGraphEditorOptions options)
    {
        var resolved = Resolve(options);
        var kernel = new GraphEditorKernel(
            resolved.Options.Document!,
            resolved.NodeCatalog,
            resolved.Options.CompatibilityService!,
            resolved.WorkspaceService,
            resolved.StyleOptions,
            resolved.BehaviorOptions);
        var session = new GraphEditorSession(
            kernel,
            resolved.Options.DiagnosticsSink,
            new GraphEditorSessionDescriptorSupport(
                resolved.NodeCatalog,
                (key, fallback) => resolved.LocalizationProvider?.GetString(key, fallback) ?? fallback,
                hasFragmentWorkspaceService: true,
                hasFragmentLibraryService: true,
                hasClipboardPayloadSerializer: true,
                hasPluginLoader: true,
                hasContextMenuAugmentor: resolved.Options.ContextMenuAugmentor is not null || resolved.PluginContextMenuAugmentors.Count > 0,
                hasNodePresentationProvider: resolved.NodePresentationProvider is not null,
                hasLocalizationProvider: resolved.LocalizationProvider is not null));
        session.ConfigureInstrumentation(resolved.Options.Instrumentation);
        session.SetPluginContextMenuAugmentors(resolved.PluginContextMenuAugmentors);
        session.SetPluginLoadSnapshots(resolved.PluginLoadResult.Snapshots);
        PublishDiagnostics(session, resolved.PluginLoadResult.Diagnostics);
        return session;
    }

    private static ResolvedEditorOptions Resolve(AsterGraphEditorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Document);
        ArgumentNullException.ThrowIfNull(options.NodeCatalog);
        ArgumentNullException.ThrowIfNull(options.CompatibilityService);

        var clipboardPayloadSerializer = options.ClipboardPayloadSerializer ?? new GraphClipboardPayloadSerializer();
        var workspaceService = options.WorkspaceService ?? new GraphWorkspaceService(GraphEditorStorageDefaults.GetWorkspacePath(options.StorageRootPath));
        var fragmentWorkspaceService = options.FragmentWorkspaceService ?? new GraphFragmentWorkspaceService(
            GraphEditorStorageDefaults.GetFragmentPath(options.StorageRootPath),
            clipboardPayloadSerializer);
        var fragmentLibraryService = options.FragmentLibraryService ?? new GraphFragmentLibraryService(
            GraphEditorStorageDefaults.GetFragmentLibraryPath(options.StorageRootPath),
            clipboardPayloadSerializer);
        var styleOptions = options.StyleOptions ?? GraphEditorStyleOptions.Default;
        var behaviorOptions = ResolveBehaviorOptions(options.BehaviorOptions, styleOptions);
        var pluginLoadResult = AsterGraphPluginLoader.Load(options.PluginRegistrations);
        var nodeCatalog = ComposeNodeCatalog(options.NodeCatalog, pluginLoadResult);
        var localizationProvider = ComposeLocalizationProvider(options.LocalizationProvider, pluginLoadResult);
        var nodePresentationProvider = ComposeNodePresentationProvider(options.NodePresentationProvider, pluginLoadResult);

        return new ResolvedEditorOptions(
            options,
            clipboardPayloadSerializer,
            workspaceService,
            fragmentWorkspaceService,
            fragmentLibraryService,
            styleOptions,
            behaviorOptions,
            pluginLoadResult,
            nodeCatalog,
            nodePresentationProvider,
            localizationProvider,
            pluginLoadResult.Contributions.ContextMenuAugmentors.ToList());
    }

    private static GraphEditorBehaviorOptions ResolveBehaviorOptions(
        GraphEditorBehaviorOptions? behaviorOptions,
        GraphEditorStyleOptions styleOptions)
    {
        if (behaviorOptions is not null)
        {
            return behaviorOptions;
        }

        return GraphEditorBehaviorOptions.Default with
        {
            DragAssist = GraphEditorBehaviorOptions.Default.DragAssist with
            {
                EnableGridSnapping = styleOptions.Canvas.EnableGridSnapping,
                EnableAlignmentGuides = styleOptions.Canvas.EnableAlignmentGuides,
                SnapTolerance = styleOptions.Canvas.SnapTolerance,
            },
        };
    }

    private static void PublishDiagnostics(GraphEditorSession session, IReadOnlyList<GraphEditorDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(diagnostics);

        foreach (var diagnostic in diagnostics)
        {
            session.PublishDiagnostic(diagnostic);
        }
    }

    private static AsterGraph.Abstractions.Catalog.INodeCatalog ComposeNodeCatalog(
        AsterGraph.Abstractions.Catalog.INodeCatalog baseCatalog,
        GraphEditorPluginLoadResult pluginLoadResult)
    {
        ArgumentNullException.ThrowIfNull(baseCatalog);
        ArgumentNullException.ThrowIfNull(pluginLoadResult);

        return pluginLoadResult.Contributions.NodeDefinitionProviders.Count == 0
            ? baseCatalog
            : new PluginComposedNodeCatalog(baseCatalog, pluginLoadResult.Contributions.NodeDefinitionProviders);
    }

    private static IGraphLocalizationProvider? ComposeLocalizationProvider(
        IGraphLocalizationProvider? hostProvider,
        GraphEditorPluginLoadResult pluginLoadResult)
    {
        ArgumentNullException.ThrowIfNull(pluginLoadResult);
        return CompositePluginLocalizationProvider.Compose(pluginLoadResult.Contributions.LocalizationProviders, hostProvider);
    }

    private static INodePresentationProvider? ComposeNodePresentationProvider(
        INodePresentationProvider? hostProvider,
        GraphEditorPluginLoadResult pluginLoadResult)
    {
        ArgumentNullException.ThrowIfNull(pluginLoadResult);
        return CompositePluginNodePresentationProvider.Compose(pluginLoadResult.Contributions.NodePresentationProviders, hostProvider);
    }

    private sealed record ResolvedEditorOptions(
        AsterGraphEditorOptions Options,
        IGraphClipboardPayloadSerializer ClipboardPayloadSerializer,
        IGraphWorkspaceService WorkspaceService,
        IGraphFragmentWorkspaceService FragmentWorkspaceService,
        IGraphFragmentLibraryService FragmentLibraryService,
        GraphEditorStyleOptions StyleOptions,
        GraphEditorBehaviorOptions BehaviorOptions,
        GraphEditorPluginLoadResult PluginLoadResult,
        AsterGraph.Abstractions.Catalog.INodeCatalog NodeCatalog,
        INodePresentationProvider? NodePresentationProvider,
        IGraphLocalizationProvider? LocalizationProvider,
        IReadOnlyList<AsterGraph.Editor.Plugins.IGraphEditorPluginContextMenuAugmentor> PluginContextMenuAugmentors);
}
