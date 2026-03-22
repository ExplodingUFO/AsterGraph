using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.Catalog;

public sealed class NodeCatalog : INodeCatalog
{
    private readonly Dictionary<NodeDefinitionId, INodeDefinition> _definitions = [];

    public IReadOnlyCollection<INodeDefinition> Definitions => _definitions.Values.ToList().AsReadOnly();

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

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Node definition '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public bool TryGetDefinition(NodeDefinitionId id, out INodeDefinition? definition)
        => _definitions.TryGetValue(id, out definition);
}
