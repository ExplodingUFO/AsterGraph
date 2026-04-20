using AsterGraph.Abstractions.Catalog;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Collects runtime contributions declared by plugins.
/// </summary>
public sealed class GraphEditorPluginBuilder
{
    private readonly List<INodeDefinitionProvider> _nodeDefinitionProviders = [];
    private readonly List<IGraphEditorPluginCommandContributor> _commandContributors = [];
    private readonly List<IGraphEditorPluginNodePresentationProvider> _nodePresentationProviders = [];
    private readonly List<IGraphEditorPluginLocalizationProvider> _localizationProviders = [];

    /// <summary>
    /// Gets the registered node-definition providers.
    /// </summary>
    public IReadOnlyList<INodeDefinitionProvider> NodeDefinitionProviders => _nodeDefinitionProviders;

    /// <summary>
    /// Gets the registered command contributors.
    /// </summary>
    public IReadOnlyList<IGraphEditorPluginCommandContributor> CommandContributors => _commandContributors;

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
    /// Adds one stable command contributor.
    /// </summary>
    public GraphEditorPluginBuilder AddCommandContributor(IGraphEditorPluginCommandContributor contributor)
    {
        ArgumentNullException.ThrowIfNull(contributor);
        _commandContributors.Add(contributor);
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
        _commandContributors.AddRange(contributions.CommandContributors);
        _nodePresentationProviders.AddRange(contributions.NodePresentationProviders);
        _localizationProviders.AddRange(contributions.LocalizationProviders);
    }

    internal GraphEditorPluginContributionSet Build()
        => new(
            _nodeDefinitionProviders.ToList(),
            _commandContributors.ToList(),
            _nodePresentationProviders.ToList(),
            _localizationProviders.ToList());
}

/// <summary>
/// Describes one plugin command-descriptor query.
/// </summary>
public sealed record GraphEditorPluginCommandContext
{
    /// <summary>
    /// Initializes a plugin command context.
    /// </summary>
    public GraphEditorPluginCommandContext(IGraphEditorSession session)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// Gets the current runtime session.
    /// </summary>
    public IGraphEditorSession Session { get; }
}

/// <summary>
/// Describes one plugin command execution request.
/// </summary>
public sealed record GraphEditorPluginCommandExecutionContext
{
    /// <summary>
    /// Initializes a plugin command execution context.
    /// </summary>
    public GraphEditorPluginCommandExecutionContext(
        IGraphEditorSession session,
        GraphEditorCommandInvocationSnapshot command)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        Command = command ?? throw new ArgumentNullException(nameof(command));
    }

    /// <summary>
    /// Gets the current runtime session.
    /// </summary>
    public IGraphEditorSession Session { get; }

    /// <summary>
    /// Gets the command being executed.
    /// </summary>
    public GraphEditorCommandInvocationSnapshot Command { get; }
}

/// <summary>
/// Defines the stable plugin command contribution contract.
/// </summary>
public interface IGraphEditorPluginCommandContributor
{
    /// <summary>
    /// Returns the current contributed command descriptors.
    /// </summary>
    IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors(GraphEditorPluginCommandContext context);

    /// <summary>
    /// Attempts to execute one contributed command.
    /// </summary>
    bool TryExecuteCommand(GraphEditorPluginCommandExecutionContext context);
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
    IReadOnlyList<IGraphEditorPluginCommandContributor> CommandContributors,
    IReadOnlyList<IGraphEditorPluginNodePresentationProvider> NodePresentationProviders,
    IReadOnlyList<IGraphEditorPluginLocalizationProvider> LocalizationProviders)
{
    public static GraphEditorPluginContributionSet Empty { get; } = new([], [], [], []);
}
