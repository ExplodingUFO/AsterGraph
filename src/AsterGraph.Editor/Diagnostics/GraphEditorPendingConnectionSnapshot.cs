using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 表示当前待完成连线预览状态的不可变快照。
/// </summary>
public sealed record GraphEditorPendingConnectionSnapshot
{
    /// <summary>
    /// 基于原始待完成连线状态创建一个经过归一化的快照。
    /// </summary>
    public static GraphEditorPendingConnectionSnapshot Create(
        bool hasPendingConnection,
        string? sourceNodeId,
        string? sourcePortId)
        => Create(
            hasPendingConnection,
            sourceNodeId,
            sourcePortId,
            null,
            null,
            null,
            null,
            null);

    /// <summary>
    /// 基于原始待完成连线和当前悬停目标状态创建一个经过归一化的快照。
    /// </summary>
    public static GraphEditorPendingConnectionSnapshot Create(
        bool hasPendingConnection,
        string? sourceNodeId,
        string? sourcePortId,
        string? targetNodeId,
        string? targetPortId,
        GraphConnectionTargetKind? targetKind,
        bool? isTargetCompatible,
        string? validationMessage)
        => new(
            hasPendingConnection,
            hasPendingConnection ? sourceNodeId : null,
            hasPendingConnection ? sourcePortId : null,
            hasPendingConnection ? targetNodeId : null,
            hasPendingConnection ? targetPortId : null,
            hasPendingConnection ? targetKind : null,
            hasPendingConnection ? isTargetCompatible : null,
            hasPendingConnection ? validationMessage : null);

    /// <summary>
    /// 初始化待完成连线快照。
    /// </summary>
    /// <param name="hasPendingConnection">是否存在待完成连线。</param>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    public GraphEditorPendingConnectionSnapshot(
        bool hasPendingConnection,
        string? sourceNodeId,
        string? sourcePortId)
        : this(
            hasPendingConnection,
            sourceNodeId,
            sourcePortId,
            null,
            null,
            null,
            null,
            null)
    {
    }

    /// <summary>
    /// 初始化待完成连线快照。
    /// </summary>
    /// <param name="hasPendingConnection">是否存在待完成连线。</param>
    /// <param name="sourceNodeId">源节点实例标识。</param>
    /// <param name="sourcePortId">源端口实例标识。</param>
    /// <param name="targetNodeId">当前悬停目标节点实例标识。</param>
    /// <param name="targetPortId">当前悬停目标端口或参数标识。</param>
    /// <param name="targetKind">当前悬停目标类型。</param>
    /// <param name="isTargetCompatible">当前悬停目标是否兼容。</param>
    /// <param name="validationMessage">当前悬停目标的连接校验说明。</param>
    public GraphEditorPendingConnectionSnapshot(
        bool hasPendingConnection,
        string? sourceNodeId,
        string? sourcePortId,
        string? targetNodeId,
        string? targetPortId,
        GraphConnectionTargetKind? targetKind,
        bool? isTargetCompatible,
        string? validationMessage)
    {
        HasPendingConnection = hasPendingConnection;
        SourceNodeId = hasPendingConnection ? NormalizeOptional(sourceNodeId) : null;
        SourcePortId = hasPendingConnection ? NormalizeOptional(sourcePortId) : null;
        TargetNodeId = hasPendingConnection ? NormalizeOptional(targetNodeId) : null;
        TargetPortId = hasPendingConnection ? NormalizeOptional(targetPortId) : null;
        TargetKind = hasPendingConnection ? targetKind : null;
        IsTargetCompatible = hasPendingConnection ? isTargetCompatible : null;
        ValidationMessage = hasPendingConnection ? NormalizeOptional(validationMessage) : null;
    }

    /// <summary>
    /// 当前是否存在待完成连线。
    /// </summary>
    public bool HasPendingConnection { get; }

    /// <summary>
    /// 待完成连线的源节点实例标识。
    /// </summary>
    public string? SourceNodeId { get; }

    /// <summary>
    /// 待完成连线的源端口实例标识。
    /// </summary>
    public string? SourcePortId { get; }

    /// <summary>
    /// 当前悬停目标节点实例标识。
    /// </summary>
    public string? TargetNodeId { get; }

    /// <summary>
    /// 当前悬停目标端口或参数标识。
    /// </summary>
    public string? TargetPortId { get; }

    /// <summary>
    /// 当前悬停目标类型。
    /// </summary>
    public GraphConnectionTargetKind? TargetKind { get; }

    /// <summary>
    /// 当前悬停目标是否兼容。
    /// </summary>
    public bool? IsTargetCompatible { get; }

    /// <summary>
    /// 当前悬停目标的连接校验说明。
    /// </summary>
    public string? ValidationMessage { get; }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
