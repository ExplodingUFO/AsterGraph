using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Catalog;

/// <summary>
/// Registry contract for resolving node definitions by their stable IDs.
/// </summary>
public interface INodeCatalog
{
    IReadOnlyCollection<INodeDefinition> Definitions { get; }

    /// <summary>
    /// Registers all definitions from the provider.
    /// Implementations must reject duplicate <see cref="NodeDefinitionId"/> values.
    /// </summary>
    void RegisterProvider(INodeDefinitionProvider provider);

    /// <summary>
    /// Registers one definition.
    /// Implementations must reject duplicate <see cref="NodeDefinitionId"/> values.
    /// </summary>
    void RegisterDefinition(INodeDefinition definition);

    bool TryGetDefinition(NodeDefinitionId id, out INodeDefinition? definition);
}
