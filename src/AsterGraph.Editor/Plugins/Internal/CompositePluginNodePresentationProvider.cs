using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Plugins.Internal;

internal sealed class CompositePluginNodePresentationProvider : INodePresentationProvider
{
    private readonly IReadOnlyList<IGraphEditorPluginNodePresentationProvider> _pluginProviders;
    private readonly INodePresentationProvider? _hostProvider;

    private CompositePluginNodePresentationProvider(
        IReadOnlyList<IGraphEditorPluginNodePresentationProvider> pluginProviders,
        INodePresentationProvider? hostProvider)
    {
        _pluginProviders = pluginProviders;
        _hostProvider = hostProvider;
    }

    public static INodePresentationProvider? Compose(
        IReadOnlyList<IGraphEditorPluginNodePresentationProvider> pluginProviders,
        INodePresentationProvider? hostProvider)
    {
        ArgumentNullException.ThrowIfNull(pluginProviders);

        return pluginProviders.Count == 0
            ? hostProvider
            : new CompositePluginNodePresentationProvider(pluginProviders.ToList(), hostProvider);
    }

    public NodePresentationState GetNodePresentation(NodePresentationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var states = new List<NodePresentationState>();
        var pluginContext = new GraphEditorPluginNodePresentationContext(
            context.Session,
            context.CompatibilityNode.ToModel(),
            context.IsSelected);

        foreach (var provider in _pluginProviders)
        {
            var state = provider.GetNodePresentation(pluginContext);
            ArgumentNullException.ThrowIfNull(state);
            states.Add(state);
        }

        if (_hostProvider is not null)
        {
            var hostState = _hostProvider.GetNodePresentation(context);
            ArgumentNullException.ThrowIfNull(hostState);
            states.Add(hostState);
        }

        return Merge(states);
    }

    public NodePresentationState GetNodePresentation(NodeViewModel node)
    {
#pragma warning disable CS0618
        return _hostProvider?.GetNodePresentation(node) ?? NodePresentationState.Empty;
#pragma warning restore CS0618
    }

    private static NodePresentationState Merge(IReadOnlyList<NodePresentationState> states)
    {
        ArgumentNullException.ThrowIfNull(states);

        if (states.Count == 0)
        {
            return NodePresentationState.Empty;
        }

        string? subtitle = null;
        string? description = null;
        NodeStatusBarDescriptor? statusBar = null;
        var badges = new List<NodeAdornmentDescriptor>();

        foreach (var state in states)
        {
            if (!string.IsNullOrWhiteSpace(state.SubtitleOverride))
            {
                subtitle = state.SubtitleOverride;
            }

            if (!string.IsNullOrWhiteSpace(state.DescriptionOverride))
            {
                description = state.DescriptionOverride;
            }

            if (state.TopRightBadges.Count > 0)
            {
                badges.AddRange(state.TopRightBadges);
            }

            if (state.StatusBar is not null)
            {
                statusBar = state.StatusBar;
            }
        }

        return new NodePresentationState(
            subtitle,
            description,
            badges,
            statusBar);
    }
}
