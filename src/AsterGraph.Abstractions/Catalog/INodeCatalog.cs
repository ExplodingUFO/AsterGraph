using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Abstractions.Catalog;

/// <summary>
/// Registry contract for resolving node definitions by their stable IDs.
/// </summary>
public interface INodeCatalog
{
    /// <summary>
    /// 当前已注册的全部节点定义。
    /// </summary>
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

    /// <summary>
    /// 按定义标识查找节点定义。
    /// </summary>
    /// <param name="id">节点定义标识。</param>
    /// <param name="definition">查找到的节点定义。</param>
    /// <returns>找到时返回 <see langword="true"/>。</returns>
    bool TryGetDefinition(NodeDefinitionId id, out INodeDefinition? definition);
}
