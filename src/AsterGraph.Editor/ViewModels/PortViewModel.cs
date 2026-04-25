using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Mutable runtime projection of one node port.
/// </summary>
public sealed class PortViewModel
{
    /// <summary>
    /// 初始化端口视图模型。
    /// </summary>
    /// <param name="model">端口模型快照。</param>
    /// <param name="index">端口在所属边上的索引。</param>
    /// <param name="total">所属边上的端口总数。</param>
    public PortViewModel(GraphPort model, int index, int total)
    {
        Id = model.Id;
        Label = model.Label;
        Direction = model.Direction;
        DataType = model.DataType;
        TypeId = model.TypeId ?? new PortTypeId(model.DataType);
        AccentHex = model.AccentHex;
        GroupName = model.GroupName;
        MinConnections = model.MinConnections;
        MaxConnections = model.MaxConnections;
        Index = index;
        Total = total;
    }

    /// <summary>
    /// 端口标识。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 端口显示名称。
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// 端口方向。
    /// </summary>
    public PortDirection Direction { get; }

    /// <summary>
    /// 端口数据类型文本。
    /// </summary>
    public string DataType { get; }

    /// <summary>
    /// 稳定端口类型标识。
    /// </summary>
    public PortTypeId TypeId { get; }

    /// <summary>
    /// 端口强调色。
    /// </summary>
    public string AccentHex { get; }

    /// <summary>
    /// 端口分组名称。
    /// </summary>
    public string? GroupName { get; }

    /// <summary>
    /// 端口最小连接数。
    /// </summary>
    public int MinConnections { get; }

    /// <summary>
    /// 端口最大连接数。
    /// </summary>
    public int MaxConnections { get; }

    /// <summary>
    /// 端口在所属边上的索引。
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// 所属边上的端口总数。
    /// </summary>
    public int Total { get; }

    /// <summary>
    /// 转回不可变模型快照。
    /// </summary>
    /// <returns>对应的不可变端口模型。</returns>
    public GraphPort ToModel()
        => new(Id, Label, Direction, DataType, AccentHex, TypeId, GroupName, MinConnections, MaxConnections);
}
