using AsterGraph.Editor.Configuration;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Menus;

/// <summary>
/// 向宿主菜单扩展器公开的只读上下文，避免宿主直接依赖编辑器内部菜单宿主接口。
/// </summary>
public sealed record GraphContextMenuExtensionContext
{
    /// <summary>
    /// 初始化一个菜单扩展上下文。
    /// </summary>
    /// <param name="targetKind">当前菜单命中的目标类型。</param>
    /// <param name="worldPosition">菜单打开时对应的世界坐标。</param>
    /// <param name="commandPermissions">当前命令权限快照。</param>
    /// <param name="selectedNodeIds">当前选中节点标识集合。</param>
    /// <param name="primarySelectedNodeId">当前主选中节点标识。</param>
    /// <param name="selectedConnectionId">当前选中连线标识。</param>
    /// <param name="clickedNode">当前点击的节点快照。</param>
    /// <param name="clickedPort">当前点击的端口快照。</param>
    /// <param name="clickedConnection">当前点击的连线快照。</param>
    public GraphContextMenuExtensionContext(
        ContextMenuTargetKind targetKind,
        GraphPoint worldPosition,
        GraphEditorCommandPermissions commandPermissions,
        IReadOnlyList<string>? selectedNodeIds = null,
        string? primarySelectedNodeId = null,
        string? selectedConnectionId = null,
        GraphContextMenuNodeInfo? clickedNode = null,
        GraphContextMenuPortInfo? clickedPort = null,
        GraphContextMenuConnectionInfo? clickedConnection = null)
    {
        TargetKind = targetKind;
        WorldPosition = worldPosition;
        CommandPermissions = commandPermissions;
        SelectedNodeIds = selectedNodeIds ?? [];
        PrimarySelectedNodeId = primarySelectedNodeId;
        SelectedConnectionId = selectedConnectionId;
        ClickedNode = clickedNode;
        ClickedPort = clickedPort;
        ClickedConnection = clickedConnection;
    }

    /// <summary>
    /// 获取当前菜单命中的目标类型。
    /// </summary>
    public ContextMenuTargetKind TargetKind { get; }

    /// <summary>
    /// 获取菜单打开位置对应的世界坐标。
    /// </summary>
    public GraphPoint WorldPosition { get; }

    /// <summary>
    /// 获取当前命令权限快照。
    /// </summary>
    public GraphEditorCommandPermissions CommandPermissions { get; }

    /// <summary>
    /// 获取当前选中的节点标识集合。
    /// </summary>
    public IReadOnlyList<string> SelectedNodeIds { get; }

    /// <summary>
    /// 获取当前主选中节点标识。
    /// </summary>
    public string? PrimarySelectedNodeId { get; }

    /// <summary>
    /// 获取当前选中连线标识。
    /// </summary>
    public string? SelectedConnectionId { get; }

    /// <summary>
    /// 获取当前命中的节点快照。
    /// </summary>
    public GraphContextMenuNodeInfo? ClickedNode { get; }

    /// <summary>
    /// 获取当前命中的端口快照。
    /// </summary>
    public GraphContextMenuPortInfo? ClickedPort { get; }

    /// <summary>
    /// 获取当前命中的连线快照。
    /// </summary>
    public GraphContextMenuConnectionInfo? ClickedConnection { get; }
}
