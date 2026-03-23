using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.Catalog;

/// <summary>
/// 默认节点目录实现，供宿主注册编译期节点定义。
/// </summary>
public sealed class NodeCatalog : INodeCatalog
{
    private readonly Dictionary<NodeDefinitionId, INodeDefinition> _definitions = [];

    /// <summary>
    /// 当前已注册的全部节点定义。
    /// </summary>
    public IReadOnlyCollection<INodeDefinition> Definitions => _definitions.Values.ToList().AsReadOnly();

    /// <summary>
    /// 从节点定义提供程序批量注册节点定义。
    /// </summary>
    /// <param name="provider">节点定义提供程序。</param>
    public void RegisterProvider(INodeDefinitionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        foreach (var definition in provider.GetNodeDefinitions())
        {
            RegisterDefinition(definition);
        }
    }

    /// <summary>
    /// 注册单个节点定义。
    /// </summary>
    /// <param name="definition">节点定义。</param>
    public void RegisterDefinition(INodeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Node definition '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    /// <summary>
    /// 按定义标识查找节点定义。
    /// </summary>
    /// <param name="id">节点定义标识。</param>
    /// <param name="definition">查找到的节点定义。</param>
    /// <returns>找到时返回 <see langword="true"/>。</returns>
    public bool TryGetDefinition(NodeDefinitionId id, out INodeDefinition? definition)
        => _definitions.TryGetValue(id, out definition);
}
