using AsterGraph.Abstractions.Catalog;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Collects runtime contributions declared by plugins.
/// </summary>
public sealed class GraphEditorPluginBuilder
{
    private readonly List<INodeDefinitionProvider> _nodeDefinitionProviders = [];
    private readonly List<IGraphEditorPluginContextMenuAugmentor> _contextMenuAugmentors = [];
    private readonly List<IGraphEditorPluginNodePresentationProvider> _nodePresentationProviders = [];
    private readonly List<IGraphEditorPluginLocalizationProvider> _localizationProviders = [];

    /// <summary>
    /// Gets the registered node-definition providers.
    /// </summary>
    public IReadOnlyList<INodeDefinitionProvider> NodeDefinitionProviders => _nodeDefinitionProviders;

    /// <summary>
    /// Gets the registered context-menu augmentors.
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginContextMenuAugmentor> ContextMenuAugmentors => _contextMenuAugmentors;

    /// <summary>
    /// Gets the registered node-presentation providers.
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginNodePresentationProvider> NodePresentationProviders => _nodePresentationProviders;

    /// <summary>
    /// Gets the registered localization providers.
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginLocalizationProvider> LocalizationProviders => _localizationProviders;

    /// <summary>
    /// Adds one node-definition provider.
    /// </summary>
    public GraphEditorPluginBuilder AddNodeDefinitionProvider(INodeDefinitionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _nodeDefinitionProviders.Add(provider);
        return this;
    }

    /// <summary>
    /// Adds multiple node-definition providers.
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
    /// Adds one stable menu augmentor.
    /// </summary>
    public GraphEditorPluginBuilder AddContextMenuAugmentor(IGraphEditorPluginContextMenuAugmentor augmentor)
    {
        ArgumentNullException.ThrowIfNull(augmentor);
        _contextMenuAugmentors.Add(augmentor);
        return this;
    }

    /// <summary>
    /// Adds one stable node-presentation provider.
    /// </summary>
    public GraphEditorPluginBuilder AddNodePresentationProvider(IGraphEditorPluginNodePresentationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _nodePresentationProviders.Add(provider);
        return this;
    }

    /// <summary>
    /// Adds one stable localization provider.
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
/// Describes one plugin menu-augmentation request.
/// </summary>
public sealed record GraphEditorPluginMenuAugmentationContext
{
    /// <summary>
    /// Initializes a plugin menu-augmentation context.
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
    /// Gets the current runtime session.
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// Gets the current hit-test context.
    /// </summary>
    public ContextMenuContext Context { get; }

    /// <summary>
    /// Gets the stock stable menu descriptors produced by the editor before plugin augmentation.
    /// </summary>
    public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> StockItems { get; }
}

/// <summary>
/// Defines the stable plugin menu-augmentation contract.
/// </summary>
public interface IGraphEditorPluginContextMenuAugmentor
{
    /// <summary>
    /// Returns the augmented stable menu descriptor set.
    /// </summary>
    IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context);
}

/// <summary>
/// Describes one plugin node-presentation request.
/// </summary>
public sealed record GraphEditorPluginNodePresentationContext
{
    /// <summary>
    /// Initializes a plugin node-presentation context.
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
    /// Gets the current runtime session.
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// Gets the current node snapshot.
    /// </summary>
    public GraphNode Node { get; }

    /// <summary>
    /// Gets whether the node is currently selected.
    /// </summary>
    public bool IsSelected { get; }
}

/// <summary>
/// Defines the stable plugin node-presentation contract.
/// </summary>
public interface IGraphEditorPluginNodePresentationProvider
{
    /// <summary>
    /// Computes the node presentation state.
    /// </summary>
    NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context);
}

/// <summary>
/// Defines the stable plugin localization contract.
/// </summary>
public interface IGraphEditorPluginLocalizationProvider
{
    /// <summary>
    /// Resolves a localized string by key.
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
