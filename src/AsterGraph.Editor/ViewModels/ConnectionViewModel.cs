using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Mutable runtime projection of one graph connection.
/// </summary>
public sealed class ConnectionViewModel
{
    /// <summary>
    /// 初始化连线视图模型。
    /// </summary>
    /// <param name="id">连线标识。</param>
    /// <param name="sourceNodeId">源节点标识。</param>
    /// <param name="sourcePortId">源端口标识。</param>
    /// <param name="targetNodeId">目标节点标识。</param>
    /// <param name="targetPortId">目标端口标识。</param>
    /// <param name="label">连线标签。</param>
    /// <param name="accentHex">连线强调色。</param>
    /// <param name="conversionId">可选的隐式转换标识。</param>
    /// <param name="noteText">可选的纯展示注释文本。</param>
    /// <param name="targetKind">目标端点类型。</param>
    public ConnectionViewModel(
        string id,
        string sourceNodeId,
        string sourcePortId,
        string targetNodeId,
        string targetPortId,
        string label,
        string accentHex,
        ConversionId? conversionId = null,
        string? noteText = null,
        GraphConnectionTargetKind targetKind = GraphConnectionTargetKind.Port)
    {
        Id = id;
        SourceNodeId = sourceNodeId;
        SourcePortId = sourcePortId;
        TargetNodeId = targetNodeId;
        TargetPortId = targetPortId;
        Label = label;
        AccentHex = accentHex;
        ConversionId = conversionId;
        NoteText = string.IsNullOrWhiteSpace(noteText) ? null : noteText.Trim();
        TargetKind = targetKind;
    }

    /// <summary>
    /// 连线标识。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 源节点标识。
    /// </summary>
    public string SourceNodeId { get; }

    /// <summary>
    /// 源端口标识。
    /// </summary>
    public string SourcePortId { get; }

    /// <summary>
    /// 目标节点标识。
    /// </summary>
    public string TargetNodeId { get; }

    /// <summary>
    /// 目标端口标识。
    /// </summary>
    public string TargetPortId { get; }

    /// <summary>
    /// 目标端点类型。
    /// </summary>
    public GraphConnectionTargetKind TargetKind { get; }

    /// <summary>
    /// 连线标签。
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// 连线强调色。
    /// </summary>
    public string AccentHex { get; }

    /// <summary>
    /// 可选的隐式转换标识。
    /// </summary>
    public ConversionId? ConversionId { get; }

    /// <summary>
    /// 可选的纯展示注释文本。
    /// </summary>
    public string? NoteText { get; }

    /// <summary>
    /// 强类型目标端点引用。
    /// </summary>
    public GraphConnectionTargetRef Target => new(TargetNodeId, TargetPortId, TargetKind);

    /// <summary>
    /// 转回不可变模型快照。
    /// </summary>
    /// <returns>对应的不可变连线模型。</returns>
    public GraphConnection ToModel()
        => new(
            Id,
            SourceNodeId,
            SourcePortId,
            TargetNodeId,
            TargetPortId,
            Label,
            AccentHex,
            ConversionId,
            string.IsNullOrWhiteSpace(NoteText) ? null : new GraphEdgePresentation(NoteText))
        {
            TargetKind = TargetKind,
        };
}
