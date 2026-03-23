using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 描述一次右键菜单请求的基础上下文。
/// </summary>
public sealed record ContextMenuContext
{
    /// <summary>
    /// 初始化一份右键菜单上下文。
    /// </summary>
    /// <param name="targetKind">当前菜单命中的目标类型。</param>
    /// <param name="worldPosition">当前命中的世界坐标。</param>
    /// <param name="selectedNodeId">当前主选中节点标识。</param>
    /// <param name="selectedNodeIds">当前完整选中节点标识集合。</param>
    /// <param name="selectedConnectionId">当前选中连线标识。</param>
    /// <param name="selectedConnectionIds">当前完整选中连线标识集合。</param>
    /// <param name="clickedNodeId">当前点击节点标识。</param>
    /// <param name="clickedPortNodeId">当前点击端口所属节点标识。</param>
    /// <param name="clickedPortId">当前点击端口标识。</param>
    /// <param name="clickedConnectionId">当前点击连线标识。</param>
    /// <param name="availableNodeDefinitions">当前可用节点定义集合。</param>
    public ContextMenuContext(
        ContextMenuTargetKind targetKind,
        GraphPoint worldPosition,
        string? selectedNodeId = null,
        IReadOnlyList<string>? selectedNodeIds = null,
        string? selectedConnectionId = null,
        IReadOnlyList<string>? selectedConnectionIds = null,
        string? clickedNodeId = null,
        string? clickedPortNodeId = null,
        string? clickedPortId = null,
        string? clickedConnectionId = null,
        IReadOnlyList<INodeDefinition>? availableNodeDefinitions = null)
    {
        if ((clickedPortNodeId is null) != (clickedPortId is null))
        {
            throw new ArgumentException("Clicked port node and port IDs must be provided together.");
        }

        TargetKind = targetKind;
        WorldPosition = worldPosition;
        SelectedNodeId = selectedNodeId;
        SelectedNodeIds = NormalizeIdentifiers(selectedNodeIds, selectedNodeId);
        SelectedConnectionId = selectedConnectionId;
        SelectedConnectionIds = NormalizeIdentifiers(selectedConnectionIds, selectedConnectionId);
        ClickedNodeId = clickedNodeId;
        ClickedPortNodeId = clickedPortNodeId;
        ClickedPortId = clickedPortId;
        ClickedConnectionId = clickedConnectionId;
        AvailableNodeDefinitions = availableNodeDefinitions ?? [];
    }

    /// <summary>
    /// 获取当前菜单命中的目标类型。
    /// </summary>
    public ContextMenuTargetKind TargetKind { get; }

    /// <summary>
    /// 获取当前命中的世界坐标。
    /// </summary>
    public GraphPoint WorldPosition { get; }

    /// <summary>
    /// 获取当前主选中节点标识。
    /// </summary>
    public string? SelectedNodeId { get; }

    /// <summary>
    /// 获取当前完整选中节点标识集合。
    /// </summary>
    public IReadOnlyList<string> SelectedNodeIds { get; }

    /// <summary>
    /// 获取当前选中连线标识。
    /// </summary>
    public string? SelectedConnectionId { get; }

    /// <summary>
    /// 获取当前完整选中连线标识集合。
    /// </summary>
    public IReadOnlyList<string> SelectedConnectionIds { get; }

    /// <summary>
    /// 获取当前点击节点标识。
    /// </summary>
    public string? ClickedNodeId { get; }

    /// <summary>
    /// 获取当前点击端口所属节点标识。
    /// </summary>
    public string? ClickedPortNodeId { get; }

    /// <summary>
    /// 获取当前点击端口标识。
    /// </summary>
    public string? ClickedPortId { get; }

    /// <summary>
    /// 获取当前点击连线标识。
    /// </summary>
    public string? ClickedConnectionId { get; }

    /// <summary>
    /// 获取当前可用节点定义集合。
    /// </summary>
    public IReadOnlyList<INodeDefinition> AvailableNodeDefinitions { get; }

    private static IReadOnlyList<string> NormalizeIdentifiers(IReadOnlyList<string>? identifiers, string? singleIdentifier)
    {
        var normalized = (identifiers ?? [])
            .Where(identifier => !string.IsNullOrWhiteSpace(identifier))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (!string.IsNullOrWhiteSpace(singleIdentifier) && !normalized.Contains(singleIdentifier, StringComparer.Ordinal))
        {
            normalized.Add(singleIdentifier);
        }

        return normalized;
    }
}
