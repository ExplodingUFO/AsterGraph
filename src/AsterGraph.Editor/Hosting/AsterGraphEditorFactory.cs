using AsterGraph.Abstractions.Styling;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Kernel;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// 提供 AsterGraph 编辑器运行时的规范宿主组合入口。
/// </summary>
/// <remarks>
/// 该工厂只负责校验输入并委托到现有 <see cref="GraphEditorViewModel"/> 兼容立面；
/// 直接构造 <see cref="GraphEditorViewModel"/> 仍然受支持，便于宿主分阶段迁移。
/// </remarks>
public static class AsterGraphEditorFactory
{
    /// <summary>
    /// 使用宿主提供的选项创建一个 <see cref="GraphEditorViewModel"/>。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的图编辑器视图模型。</returns>
    public static GraphEditorViewModel Create(AsterGraphEditorOptions options)
    {
        var resolved = Resolve(options);

        var editor = new GraphEditorViewModel(
            resolved.Options.Document!,
            resolved.Options.NodeCatalog!,
            resolved.Options.CompatibilityService!,
            resolved.WorkspaceService,
            resolved.FragmentWorkspaceService,
            resolved.StyleOptions,
            resolved.BehaviorOptions,
            resolved.FragmentLibraryService,
            resolved.Options.ContextMenuAugmentor,
            resolved.Options.NodePresentationProvider,
            resolved.Options.LocalizationProvider,
            resolved.ClipboardPayloadSerializer,
            resolved.Options.DiagnosticsSink);

        if (editor.Session is GraphEditorSession runtimeSession)
        {
            runtimeSession.ConfigureInstrumentation(resolved.Options.Instrumentation);
        }

        return editor;
    }

    /// <summary>
    /// 使用宿主提供的选项创建一个 <see cref="IGraphEditorSession"/>。
    /// </summary>
    /// <param name="options">宿主组合选项。</param>
    /// <returns>新的图编辑器运行时会话。</returns>
    public static IGraphEditorSession CreateSession(AsterGraphEditorOptions options)
    {
        var resolved = Resolve(options);
        var kernel = new GraphEditorKernel(
            resolved.Options.Document!,
            resolved.Options.NodeCatalog!,
            resolved.Options.CompatibilityService!,
            resolved.WorkspaceService,
            resolved.StyleOptions,
            resolved.BehaviorOptions);
        var session = new GraphEditorSession(kernel, resolved.Options.DiagnosticsSink);
        session.ConfigureInstrumentation(resolved.Options.Instrumentation);
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

        return new ResolvedEditorOptions(
            options,
            clipboardPayloadSerializer,
            workspaceService,
            fragmentWorkspaceService,
            fragmentLibraryService,
            styleOptions,
            behaviorOptions);
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

    private sealed record ResolvedEditorOptions(
        AsterGraphEditorOptions Options,
        IGraphClipboardPayloadSerializer ClipboardPayloadSerializer,
        IGraphWorkspaceService WorkspaceService,
        IGraphFragmentWorkspaceService FragmentWorkspaceService,
        IGraphFragmentLibraryService FragmentLibraryService,
        GraphEditorStyleOptions StyleOptions,
        GraphEditorBehaviorOptions BehaviorOptions);
}
