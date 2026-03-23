using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 描述右键菜单命中端口的只读快照。
/// </summary>
/// <param name="NodeId">所属节点标识。</param>
/// <param name="Id">端口标识。</param>
/// <param name="Label">端口显示名称。</param>
/// <param name="Direction">端口方向。</param>
/// <param name="TypeId">端口类型标识。</param>
/// <param name="DataType">端口数据类型显示文本。</param>
/// <param name="Index">端口在所属方向中的索引。</param>
/// <param name="Total">所属方向端口总数。</param>
public sealed record GraphContextMenuPortInfo(
    string NodeId,
    string Id,
    string Label,
    PortDirection Direction,
    PortTypeId TypeId,
    string DataType,
    int Index,
    int Total);
