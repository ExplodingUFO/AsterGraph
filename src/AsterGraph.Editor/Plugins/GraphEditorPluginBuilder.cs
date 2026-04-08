using AsterGraph.Abstractions.Catalog;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 收集插件声明的运行时贡献。
/// </summary>
public sealed class GraphEditorPluginBuilder
{
    private readonly List<INodeDefinitionProvider> _nodeDefinitionProviders = [];
    private readonly List<IGraphEditorPluginContextMenuAugmentor> _contextMenuAugmentors = [];
    private readonly List<IGraphEditorPluginNodePresentationProvider> _nodePresentationProviders = [];
    private readonly List<IGraphEditorPluginLocalizationProvider> _localizationProviders = [];

    /// <summary>
    /// 节点定义提供器集合。
    /// </summary>
    public IReadOnlyList<INodeDefinitionProvider> NodeDefinitionProviders => _nodeDefinitionProviders;

    /// <summary>
    /// 右键菜单增强器集合。
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> ContextMenuAugmentors => _contextMenuAugmentors;

    /// <summary>
    /// 节点展示提供器集合。
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginNodePresentationProvider> NodePresentationProviders => _nodePresentationProviders;

    /// <summary>
    /// 本地化提供器集合。
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginLocalizationProvider> LocalizationProviders => _localizationProviders;

    /// <summary>
    /// 添加一个节点定义提供器。
    /// </summary>
    public GraphEditorPluginBuilder AddNodeDefinitionProvider(INodeDefinitionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _nodeDefinitionProviders.Add(provider);
        return this;
    }

    /// <summary>
    /// 批量添加节点定义提供器。
    /// </summary>
    public GraphEditorPluginBuilder AddNodeDefinitionProviders(IEnumerable<INodeDefinitionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        foreach (var provider in providers)
        {
            AddNodeDefinitionProvider(provider);
        }

        return this;
    }

    /// <summary>
    /// 添加一个稳定菜单增强器。
    /// </summary>
    public GraphEditorPluginBuilder AddContextMenuAugmentor(IGraphEditorPluginContextMenuAugmentor augmentor)
    {
        ArgumentNullException.ThrowIfNull(augmentor);
        _contextMenuAugmentors.Add(augmentor);
        return this;
    }

    /// <summary>
    /// 添加一个稳定节点展示提供器。
    /// </summary>
    public GraphEditorPluginBuilder AddNodePresentationProvider(IGraphEditorPluginNodePresentationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _nodePresentationProviders.Add(provider);
        return this;
    }

    /// <summary>
    /// 添加一个稳定本地化提供器。
    /// </summary>
    public GraphEditorPluginBuilder AddLocalizationProvider(IGraphEditorPluginLocalizationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _localizationProviders.Add(provider);
        return this;
    }

    internal void Merge(GraphEditorPluginContributionSet contributions)
    {
        ArgumentNullException.ThrowIfNull(contributions);

        _nodeDefinitionProviders.AddRange(contributions.NodeDefinitionProviders);
        _contextMenuAugmentors.AddRange(contributions.ContextMenuAugmentors);
        _nodePresentationProviders.AddRange(contributions.NodePresentationProviders);
        _localizationProviders.AddRange(contributions.LocalizationProviders);
    }

    internal GraphEditorPluginContributionSet Build()
        => new(
            _nodeDefinitionProviders.ToList(),
            _contextMenuAugmentors.ToList(),
            _nodePresentationProviders.ToList(),
            _localizationProviders.ToList());
}

/// <summary>
/// 描述一次插件菜单增强请求。
/// </summary>
public sealed record GraphEditorPluginMenuAugmentationContext
{
    /// <summary>
    /// 初始化插件菜单增强上下文。
    /// </summary>
    public GraphEditorPluginMenuAugmentationContext(
        IGraphEditorSession session,
        ContextMenuContext context,
        IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> stockItems)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        StockItems = stockItems ?? throw new ArgumentNullException(nameof(stockItems));
    }

    /// <summary>
    /// 当前运行时会话。
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// 当前命中上下文。
    /// </summary>
    public ContextMenuContext Context { get; }

    /// <summary>
    /// 编辑器默认生成的稳定菜单描述。
    /// </summary>
    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> StockItems { get; }
}

/// <summary>
/// 定义稳定的插件菜单增强接口。
/// </summary>
public interface IGraphEditorPluginContextMenuAugmentor
{
    /// <summary>
    /// 返回增强后的稳定菜单描述集合。
    /// </summary>
    IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context);
}

/// <summary>
/// 描述一次插件节点展示计算请求。
/// </summary>
public sealed record GraphEditorPluginNodePresentationContext
{
    /// <summary>
    /// 初始化插件节点展示上下文。
    /// </summary>
    public GraphEditorPluginNodePresentationContext(
        IGraphEditorSession session,
        GraphNode node,
        bool isSelected)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Node = node ?? throw new ArgumentNullException(nameof(node));
        IsSelected = isSelected;
    }

    /// <summary>
    /// 当前运行时会话。
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// 当前节点快照。
    /// </summary>
    public GraphNode Node { get; }

    /// <summary>
    /// 节点当前是否选中。
    /// </summary>
    public bool IsSelected { get; }
}

/// <summary>
/// 定义稳定的插件节点展示接口。
/// </summary>
public interface IGraphEditorPluginNodePresentationProvider
{
    /// <summary>
    /// 计算节点展示状态。
    /// </summary>
    NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context);
}

/// <summary>
/// 定义稳定的插件本地化接口。
/// </summary>
public interface IGraphEditorPluginLocalizationProvider
{
    /// <summary>
    /// 按键查询本地化文本。
    /// </summary>
    string GetString(string key, string fallback);
}

internal sealed record GraphEditorPluginContributionSet(
    IReadOnlyList<INodeDefinitionProvider> NodeDefinitionProviders,
    IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> ContextMenuAugmentors,
    IReadOnlyList<IGraphEditorPluginNodePresentationProvider> NodePresentationProviders,
    IReadOnlyList<IGraphEditorPluginLocalizationProvider> LocalizationProviders)
{
    public static GraphEditorPluginContributionSet Empty { get; } = new([], [], [], []);
}
