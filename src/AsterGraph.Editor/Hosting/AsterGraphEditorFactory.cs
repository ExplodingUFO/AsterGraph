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
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Document);
        ArgumentNullException.ThrowIfNull(options.NodeCatalog);
        ArgumentNullException.ThrowIfNull(options.CompatibilityService);

        return new GraphEditorViewModel(
            options.Document,
            options.NodeCatalog,
            options.CompatibilityService,
            options.WorkspaceService,
            options.FragmentWorkspaceService,
            options.StyleOptions,
            options.BehaviorOptions,
            options.FragmentLibraryService,
            options.ContextMenuAugmentor,
            options.NodePresentationProvider,
            options.LocalizationProvider);
    }
}
