using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 描述右键菜单命中连线的只读快照。
/// </summary>
/// <param name="Id">连线标识。</param>
/// <param name="SourceNodeId">源节点标识。</param>
/// <param name="SourcePortId">源端口标识。</param>
/// <param name="TargetNodeId">目标节点标识。</param>
/// <param name="TargetPortId">目标端口标识。</param>
/// <param name="Label">连线显示文本。</param>
/// <param name="ConversionId">隐式转换标识。</param>
public sealed record GraphContextMenuConnectionInfo(
    string Id,
    string SourceNodeId,
    string SourcePortId,
    string TargetNodeId,
    string TargetPortId,
    string Label,
    ConversionId? ConversionId);
