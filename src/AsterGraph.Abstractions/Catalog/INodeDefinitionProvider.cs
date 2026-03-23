using AsterGraph.Abstractions.Definitions;

namespace AsterGraph.Abstractions.Catalog;

/// <summary>
/// Extension point for assemblies that contribute one or more node definitions.
/// </summary>
public interface INodeDefinitionProvider
{
    /// <summary>
    /// 返回当前程序集提供的全部节点定义。
    /// </summary>
    /// <returns>节点定义集合。</returns>
    IReadOnlyList<INodeDefinition> GetNodeDefinitions();
}
