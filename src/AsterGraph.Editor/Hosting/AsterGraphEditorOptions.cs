using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Hosting;

/// <summary>
/// 定义通过 <see cref="AsterGraphEditorFactory"/> 组合图编辑器运行时所需的宿主输入。
/// </summary>
/// <remarks>
/// 该选项契约提供 Phase 1 的规范宿主入口；直接使用 <c>new GraphEditorViewModel(...)</c>
/// 仍然是受支持的兼容路径，便于现有宿主逐步迁移。
/// </remarks>
public sealed record AsterGraphEditorOptions
{
    /// <summary>
    /// 要加载到编辑器中的初始图文档。
    /// </summary>
    public GraphDocument? Document { get; init; }

    /// <summary>
    /// 节点目录。
    /// </summary>
    public INodeCatalog? NodeCatalog { get; init; }

    /// <summary>
    /// 端口兼容性服务。
    /// </summary>
    public IPortCompatibilityService? CompatibilityService { get; init; }

    /// <summary>
    /// 可选的整图工作区服务。
    /// </summary>
    public IGraphWorkspaceService? WorkspaceService { get; init; }

    /// <summary>
    /// 可选的片段工作区服务。
    /// </summary>
    public IGraphFragmentWorkspaceService? FragmentWorkspaceService { get; init; }

    /// <summary>
    /// 可选的显式存储根目录；未提供时回退到包中立默认路径。
    /// </summary>
    public string? StorageRootPath { get; init; }

    /// <summary>
    /// 可选的样式配置。
    /// </summary>
    public GraphEditorStyleOptions? StyleOptions { get; init; }

    /// <summary>
    /// 可选的行为配置。
    /// </summary>
    public GraphEditorBehaviorOptions? BehaviorOptions { get; init; }

    /// <summary>
    /// 可选的片段模板库服务。
    /// </summary>
    public IGraphFragmentLibraryService? FragmentLibraryService { get; init; }

    /// <summary>
    /// 可选的片段剪贴板负载序列化器。
    /// </summary>
    public IGraphClipboardPayloadSerializer? ClipboardPayloadSerializer { get; init; }

    /// <summary>
    /// 可选的宿主右键菜单增强器。
    /// </summary>
    public IGraphContextMenuAugmentor? ContextMenuAugmentor { get; init; }

    /// <summary>
    /// 可选的节点展示状态提供器。
    /// </summary>
    public INodePresentationProvider? NodePresentationProvider { get; init; }

    /// <summary>
    /// 可选的图编辑器内置文案本地化提供器。
    /// </summary>
    public IGraphLocalizationProvider? LocalizationProvider { get; init; }

    /// <summary>
    /// 可选的运行时诊断发布器。
    /// </summary>
    public IGraphEditorDiagnosticsSink? DiagnosticsSink { get; init; }
}
