using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// 暴露节点模板只读信息与实例化入口。
/// </summary>
public sealed class NodeTemplateViewModel
{
    /// <summary>
    /// 使用节点定义初始化节点模板视图模型。
    /// </summary>
    /// <param name="definition">要投影的节点定义。</param>
    public NodeTemplateViewModel(INodeDefinition definition)
    {
        Definition = definition;
        var snapshot = GraphEditorStencilItemSnapshot.Create(definition);
        Key = snapshot.Key;
        Title = snapshot.Title;
        Category = snapshot.Category;
        Subtitle = snapshot.Subtitle;
        Description = snapshot.Description;
        Size = snapshot.Size;
        AccentHex = snapshot.AccentHex;
    }

    /// <summary>
    /// 模板所基于的节点定义。
    /// </summary>
    public INodeDefinition Definition { get; }

    /// <summary>
    /// 基于节点定义标识生成的模板键值。
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// 模板标题。
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// 模板分类。
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// 模板副标题。
    /// </summary>
    public string Subtitle { get; }

    /// <summary>
    /// 模板描述文本。
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 由模板创建节点时使用的默认尺寸。
    /// </summary>
    public GraphSize Size { get; }

    /// <summary>
    /// 模板强调色。
    /// </summary>
    public string AccentHex { get; }

    /// <summary>
    /// 输入端口数量。
    /// </summary>
    public int InputCount => Definition.InputPorts.Count;

    /// <summary>
    /// 输出端口数量。
    /// </summary>
    public int OutputCount => Definition.OutputPorts.Count;

    /// <summary>
    /// 汇总输入与输出端口数量的文本。
    /// </summary>
    public string PortSummary => $"{InputCount} in  ·  {OutputCount} out";

    /// <summary>
    /// 基于当前模板创建一个节点视图模型实例。
    /// </summary>
    /// <param name="nodeId">新节点实例的标识。</param>
    /// <param name="position">新节点实例的放置坐标。</param>
    /// <returns>包含模板端口与元数据的新节点视图模型。</returns>
    public NodeViewModel CreateNode(string nodeId, GraphPoint position)
        => new(new GraphNode(
            nodeId,
            Definition.DisplayName,
            Definition.Category,
            Definition.Subtitle,
            Definition.Description ?? string.Empty,
            position,
            Size,
            Definition.InputPorts.Select(port => new GraphPort(
                port.Key,
                port.DisplayName,
                PortDirection.Input,
                port.TypeId.Value,
                port.AccentHex,
                port.TypeId)).ToList(),
            Definition.OutputPorts.Select(port => new GraphPort(
                port.Key,
                port.DisplayName,
                PortDirection.Output,
                port.TypeId.Value,
                port.AccentHex,
                port.TypeId)).ToList(),
            Definition.AccentHex,
            Definition.Id));
}
