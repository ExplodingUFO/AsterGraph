using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.Plugins.Internal;

internal sealed class PluginComposedNodeCatalog : INodeCatalog
{
    private readonly INodeCatalog _baseCatalog;
    private readonly Dictionary<NodeDefinitionId, INodeDefinition> _pluginDefinitions = [];

    public PluginComposedNodeCatalog(INodeCatalog baseCatalog, IReadOnlyList<INodeDefinitionProvider> pluginProviders)
    {
        _baseCatalog = baseCatalog ?? throw new ArgumentNullException(nameof(baseCatalog));
        ArgumentNullException.ThrowIfNull(pluginProviders);

        foreach (var provider in pluginProviders)
        {
            RegisterProvider(provider);
        }
    }

    public IReadOnlyCollection<INodeDefinition> Definitions
        => _baseCatalog.Definitions
            .Concat(_pluginDefinitions.Values)
            .ToList()
            .AsReadOnly();

    public void RegisterProvider(INodeDefinitionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        foreach (var definition in provider.GetNodeDefinitions())
        {
            RegisterDefinition(definition);
        }
    }

    public void RegisterDefinition(INodeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_baseCatalog.TryGetDefinition(definition.Id, out _)
            || _pluginDefinitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Node definition '{definition.Id}' is already registered.");
        }

        _pluginDefinitions[definition.Id] = definition;
    }

    public bool TryGetDefinition(NodeDefinitionId id, out INodeDefinition? definition)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _baseCatalog.TryGetDefinition(id, out definition)
            || _pluginDefinitions.TryGetValue(id, out definition);
    }
}
