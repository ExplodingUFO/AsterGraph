using AsterGraph.Abstractions.Definitions;

namespace AsterGraph.Abstractions.Catalog;

/// <summary>
/// Extension point for assemblies that contribute one or more node definitions.
/// </summary>
public interface INodeDefinitionProvider
{
    IReadOnlyList<INodeDefinition> GetNodeDefinitions();
}
