using AsterGraph.Editor.Localization;

namespace AsterGraph.Editor.Plugins.Internal;

internal sealed class CompositePluginLocalizationProvider : IGraphLocalizationProvider
{
    private readonly IReadOnlyList<IGraphEditorPluginLocalizationProvider> _pluginProviders;
    private readonly IGraphLocalizationProvider? _hostProvider;

    private CompositePluginLocalizationProvider(
        IReadOnlyList<IGraphEditorPluginLocalizationProvider> pluginProviders,
        IGraphLocalizationProvider? hostProvider)
    {
        _pluginProviders = pluginProviders;
        _hostProvider = hostProvider;
    }

    public static IGraphLocalizationProvider? Compose(
        IReadOnlyList<IGraphEditorPluginLocalizationProvider> pluginProviders,
        IGraphLocalizationProvider? hostProvider)
    {
        ArgumentNullException.ThrowIfNull(pluginProviders);

        return pluginProviders.Count == 0
            ? hostProvider
            : new CompositePluginLocalizationProvider(pluginProviders.ToList(), hostProvider);
    }

    public string GetString(string key, string fallback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(fallback);

        var current = fallback;
        foreach (var provider in _pluginProviders)
        {
            var localized = provider.GetString(key, current);
            if (!string.IsNullOrWhiteSpace(localized))
            {
                current = localized;
            }
        }

        if (_hostProvider is not null)
        {
            var localized = _hostProvider.GetString(key, current);
            if (!string.IsNullOrWhiteSpace(localized))
            {
                current = localized;
            }
        }

        return current;
    }
}
