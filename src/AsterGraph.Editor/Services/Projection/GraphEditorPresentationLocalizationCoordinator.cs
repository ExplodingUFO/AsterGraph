using System.Globalization;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorPresentationLocalizationCoordinatorHost
{
    IGraphEditorSession Session { get; }

    IReadOnlyList<NodeViewModel> Nodes { get; }

    NodeViewModel? FindNode(string nodeId);

    INodePresentationProvider? CurrentNodePresentationProvider { get; }

    IGraphLocalizationProvider? CurrentLocalizationProvider { get; }

    bool TrySetNodePresentationProvider(INodePresentationProvider? provider);

    bool TrySetLocalizationProvider(IGraphLocalizationProvider? provider);

    void RefreshSelectionProjection();

    void RaiseComputedPropertyChanges();

    void NotifyFragmentLibraryCaptionChanged();
}

internal sealed class GraphEditorPresentationLocalizationCoordinator
{
    private readonly IGraphEditorPresentationLocalizationCoordinatorHost _host;

    public GraphEditorPresentationLocalizationCoordinator(IGraphEditorPresentationLocalizationCoordinatorHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public bool HasNodePresentationProvider => _host.CurrentNodePresentationProvider is not null;

    public bool HasLocalizationProvider => _host.CurrentLocalizationProvider is not null;

    public void SetLocalizationProvider(IGraphLocalizationProvider? provider)
    {
        if (!_host.TrySetLocalizationProvider(provider))
        {
            return;
        }

        _host.RefreshSelectionProjection();
        _host.RaiseComputedPropertyChanges();
        _host.NotifyFragmentLibraryCaptionChanged();
    }

    public void SetNodePresentationProvider(INodePresentationProvider? provider, bool refreshImmediately = true)
    {
        if (!_host.TrySetNodePresentationProvider(provider))
        {
            return;
        }

        if (refreshImmediately)
        {
            RefreshNodePresentations();
        }
    }

    public bool RefreshNodePresentation(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var node = _host.FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        ApplyNodePresentation(node);
        return true;
    }

    public int RefreshNodePresentations()
    {
        foreach (var node in _host.Nodes)
        {
            ApplyNodePresentation(node);
        }

        return _host.Nodes.Count;
    }

    public void ApplyNodePresentation(NodeViewModel node)
    {
        var provider = _host.CurrentNodePresentationProvider;
        if (provider is null)
        {
            node.UpdatePresentation(NodePresentationState.Empty);
            return;
        }

        var state = provider.GetNodePresentation(
            new NodePresentationContext(
                _host.Session,
                node.Id,
                node.DefinitionId,
                node.Title,
                node.Category,
                node.Subtitle,
                node.Description,
                node.AccentHex,
                node.IsSelected,
                node.InputCount,
                node.OutputCount,
                node.ParameterValues,
                node));
        ArgumentNullException.ThrowIfNull(state);
        node.UpdatePresentation(state);
    }

    public string LocalizeText(string key, string fallback)
    {
        var provider = _host.CurrentLocalizationProvider;
        if (provider is null)
        {
            return fallback;
        }

        var localized = provider.GetString(key, fallback);
        return string.IsNullOrWhiteSpace(localized) ? fallback : localized;
    }

    public string LocalizeFormat(string key, string fallback, params object?[] arguments)
        => string.Format(CultureInfo.InvariantCulture, LocalizeText(key, fallback), arguments);

    public string StatusText(string key, string fallback)
        => LocalizeText(key, fallback);

    public string StatusText(string key, string fallback, params object?[] arguments)
        => LocalizeFormat(key, fallback, arguments);
}
