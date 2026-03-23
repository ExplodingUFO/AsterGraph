using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 描述右键菜单命中节点的只读快照。
/// </summary>
/// <param name="Id">节点标识。</param>
/// <param name="Title">节点标题。</param>
/// <param name="Category">节点分类。</param>
/// <param name="DefinitionId">节点定义标识。</param>
/// <param name="Position">节点位置。</param>
/// <param name="Size">节点大小。</param>
/// <param name="InputCount">输入端口数量。</param>
/// <param name="OutputCount">输出端口数量。</param>
public sealed record GraphContextMenuNodeInfo(
    string Id,
    string Title,
    string Category,
    NodeDefinitionId? DefinitionId,
    GraphPoint Position,
    GraphSize Size,
    int InputCount,
    int OutputCount);
