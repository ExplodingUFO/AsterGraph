using System;
using System.Collections.Generic;
using System.Linq;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    /// <summary>
    /// 获取与指定矩形相交的节点集合。
    /// </summary>
    /// <param name="firstCorner">矩形第一个角点。</param>
    /// <param name="secondCorner">矩形第二个角点。</param>
    /// <returns>命中的节点集合。</returns>
    public IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner)
        => _nodeLayoutCoordinator.GetNodesInRectangle(firstCorner, secondCorner);

    /// <summary>
    /// 获取当前所有节点位置的不可变快照，供宿主持久化使用。
    /// </summary>
    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _nodeLayoutCoordinator.GetNodePositions();

    /// <summary>
    /// 按节点实例标识读取当前位置。
    /// </summary>
    public bool TryGetNodePosition(string nodeId, out NodePositionSnapshot? snapshot)
        => _nodeLayoutCoordinator.TryGetNodePosition(nodeId, out snapshot);

    /// <summary>
    /// 按节点实例标识更新单个节点的位置。
    /// </summary>
    public bool TrySetNodePosition(string nodeId, GraphPoint position, bool updateStatus = true)
        => _nodeLayoutCoordinator.TrySetNodePosition(nodeId, position, updateStatus);

    /// <summary>
    /// 批量应用节点位置并返回实际更新的节点数量。
    /// </summary>
    public int SetNodePositions(IEnumerable<NodePositionSnapshot> positions, bool updateStatus = true)
        => _nodeLayoutCoordinator.SetNodePositions(positions, updateStatus);

    /// <summary>
    /// Attempts to persist one node width through the session runtime surface path.
    /// </summary>
    public bool TrySetNodeWidth(NodeViewModel node, double width, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _sessionHost.TrySetNodeWidth(node.Id, width, updateStatus);
    }

    /// <summary>
    /// Attempts to persist one node size through the session runtime surface path.
    /// </summary>
    public bool TrySetNodeSize(NodeViewModel node, GraphSize size, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _sessionHost.TrySetNodeSize(node.Id, size, updateStatus);
    }

    /// <summary>
    /// Attempts to persist one node expansion-state change through the session runtime surface path.
    /// </summary>
    [Obsolete("Compatibility-only retained helper. Use Session.Commands.TrySetNodeExpansionState(node.Id, ...) for canonical runtime updates, drive size-based disclosure with Session.Commands.TrySetNodeSize(...), and inspect Session.Queries.GetNodeSurfaceSnapshots() for persisted node-surface state.")]
    public bool TrySetNodeExpansionState(NodeViewModel node, GraphNodeExpansionState expansionState)
    {
        ArgumentNullException.ThrowIfNull(node);
        return _sessionHost.TrySetNodeExpansionState(node.Id, expansionState);
    }

    /// <summary>
    /// Gets persisted editor-only node groups from the shared runtime session.
    /// </summary>
    public IReadOnlyList<GraphNodeGroup> GetNodeGroups()
        => _sessionHost.GetNodeGroups();

    /// <summary>
    /// Gets resolved editor-only node-group boundary snapshots from the shared runtime session.
    /// </summary>
    public IReadOnlyList<GraphEditorNodeGroupSnapshot> GetNodeGroupSnapshots()
        => _sessionHost.GetNodeGroupSnapshots();

    /// <summary>
    /// Attempts to create one editor-only group from the current selection.
    /// </summary>
    public string TryCreateNodeGroupFromSelection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        return _sessionHost.TryCreateNodeGroupFromSelection(title);
    }

    /// <summary>
    /// Attempts to update one persisted group's collapsed state.
    /// </summary>
    public bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        return _sessionHost.TrySetNodeGroupCollapsed(groupId, isCollapsed);
    }

    /// <summary>
    /// Attempts to move one persisted group and optionally its member nodes.
    /// </summary>
    public bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes = true, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        return _sessionHost.TrySetNodeGroupPosition(groupId, position, moveMemberNodes, updateStatus);
    }

    /// <summary>
    /// Attempts to update one group's fixed frame size.
    /// </summary>
    public bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        return _sessionHost.TrySetNodeGroupSize(groupId, size, updateStatus);
    }

    /// <summary>
    /// Attempts to persist one group's fixed frame position and size as a single layout mutation.
    /// </summary>
    public bool TrySetNodeGroupFrame(string groupId, GraphPoint position, GraphSize size, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        return _sessionHost.TrySetNodeGroupFrame(groupId, position, size, updateStatus);
    }

    /// <summary>
    /// Attempts to update one group's persisted per-edge padding envelope.
    /// </summary>
    [Obsolete("Compatibility-only retained helper. Prefer fixed-frame group edits through Session.Commands.TrySetNodeGroupSize(...) and Session.Commands.TrySetNodeGroupPosition(...), then inspect Session.Queries.GetNodeGroupSnapshots() for canonical persisted bounds.")]
    public bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);
        return _sessionHost.TrySetNodeGroupExtraPadding(groupId, extraPadding, updateStatus);
    }

    /// <summary>
    /// Attempts to apply one or more node-group membership changes.
    /// </summary>
    public bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus = true)
    {
        ArgumentNullException.ThrowIfNull(changes);
        return _sessionHost.TrySetNodeGroupMemberships(changes, updateStatus);
    }

    /// <summary>
    /// Determines whether an input port currently receives an upstream connection.
    /// </summary>
    public bool HasIncomingConnection(NodeViewModel node, PortViewModel port)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(port);

        return Connections.Any(connection =>
            connection.TargetKind == GraphConnectionTargetKind.Port
            && string.Equals(connection.TargetNodeId, node.Id, StringComparison.Ordinal)
            && string.Equals(connection.TargetPortId, port.Id, StringComparison.Ordinal));
    }

    /// <summary>
    /// 将屏幕坐标转换为当前视口下的世界坐标。
    /// </summary>
    /// <param name="screen">待转换的屏幕坐标。</param>
    /// <returns>应用当前缩放和平移后的世界坐标。</returns>
    public GraphPoint ScreenToWorld(GraphPoint screen)
        => ViewportMath.ScreenToWorld(new ViewportState(Zoom, PanX, PanY), screen);

    /// <summary>
    /// 从节点模板创建一个新节点。
    /// </summary>
    /// <param name="template">节点模板。</param>
    /// <param name="preferredWorldPosition">可选的首选世界坐标。</param>
    public void AddNode(NodeTemplateViewModel template, GraphPoint? preferredWorldPosition = null)
    {
        if (!CommandPermissions.Nodes.AllowCreate)
        {
            SetStatus("editor.status.node.create.disabledByPermissions", "Node creation is disabled by host permissions.");
            return;
        }

        _kernel.AddNode(template.Definition.Id, preferredWorldPosition);
    }

    /// <summary>
    /// 按指定偏移移动节点；如果节点属于当前多选，则移动整个选择集。
    /// </summary>
    /// <param name="node">拖动源节点。</param>
    /// <param name="deltaX">水平偏移。</param>
    /// <param name="deltaY">垂直偏移。</param>
    public void MoveNode(NodeViewModel node, double deltaX, double deltaY)
        => _nodeLayoutCoordinator.MoveNode(node, deltaX, deltaY);

    /// <summary>
    /// 按屏幕偏移平移当前视口。
    /// </summary>
    public void PanBy(double deltaX, double deltaY)
        => _kernel.PanBy(deltaX, deltaY);

    /// <summary>
    /// 围绕指定屏幕锚点缩放当前视口。
    /// </summary>
    /// <param name="factor">缩放系数。</param>
    /// <param name="screenAnchor">屏幕锚点。</param>
    public void ZoomAt(double factor, GraphPoint screenAnchor)
        => _kernel.ZoomAt(factor, screenAnchor);

    /// <summary>
    /// 重置缩放和平移到默认视口。
    /// </summary>
    public void ResetView(bool updateStatus = true)
        => _kernel.ResetView(updateStatus);

    /// <summary>
    /// 将当前图内容适配到指定视口范围。
    /// </summary>
    public void FitToViewport(double viewportWidth, double viewportHeight, bool updateStatus = true)
    {
        _kernel.UpdateViewportSize(viewportWidth, viewportHeight);
        _kernel.FitToViewport(updateStatus);
    }

    /// <summary>
    /// 激活指定端口，按方向决定是开始连线还是完成连线。
    /// </summary>
    public void ActivatePort(NodeViewModel node, PortViewModel port)
    {
        SelectSingleNode(node);

        if (port.Direction == PortDirection.Output)
        {
            StartConnection(node.Id, port.Id);
            return;
        }

        if (!HasPendingConnection)
        {
            SetStatus("editor.status.connection.selectOutputPortFirst", "Select an output port first.");
            return;
        }

        ConnectPorts(PendingSourceNode!.Id, PendingSourcePort!.Id, node.Id, port.Id);
    }

    /// <summary>
    /// Activates a typed connection target exposed by node-local hosted UI.
    /// </summary>
    public void ActivateConnectionTarget(NodeViewModel node, GraphConnectionTargetRef target)
    {
        ArgumentNullException.ThrowIfNull(node);

        SelectSingleNode(node);
        if (!HasPendingConnection)
        {
            SetStatus("editor.status.connection.selectOutputPortFirst", "Select an output port first.");
            return;
        }

        ConnectToTarget(PendingSourceNode!.Id, PendingSourcePort!.Id, target);
    }

    /// <summary>
    /// 以指定输出端口作为连线起点。
    /// </summary>
    public void StartConnection(string sourceNodeId, string sourcePortId)
        => _kernel.StartConnection(sourceNodeId, sourcePortId);

    /// <summary>
    /// 连接源输出端口与目标输入端口。
    /// </summary>
    public void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
        => ConnectToTarget(sourceNodeId, sourcePortId, new GraphConnectionTargetRef(targetNodeId, targetPortId));

    public void ConnectToTarget(string sourceNodeId, string sourcePortId, GraphConnectionTargetRef target)
    {
        var pendingConnection = _kernel.GetPendingConnectionSnapshot();
        if (pendingConnection.HasPendingConnection
            && string.Equals(pendingConnection.SourceNodeId, sourceNodeId, StringComparison.Ordinal)
            && string.Equals(pendingConnection.SourcePortId, sourcePortId, StringComparison.Ordinal))
        {
            _kernel.CompleteConnection(target);
            return;
        }

        _kernel.StartConnection(sourceNodeId, sourcePortId);
        _kernel.CompleteConnection(target);
    }

    /// <summary>
    /// 取消当前待完成的连线预览。
    /// </summary>
    public void CancelPendingConnection(string? status = null)
    {
        _kernel.CancelPendingConnection();

        if (!string.IsNullOrWhiteSpace(status))
        {
            StatusMessage = status;
        }
    }

    /// <summary>
    /// 删除当前选择，保留旧的单节点删除入口以兼容宿主调用。
    /// </summary>
    public void DeleteSelectedNode()
        => DeleteSelection();

    /// <summary>
    /// 删除当前选择集及其相关连线。
    /// </summary>
    public void DeleteSelection()
    {
        if (!CommandPermissions.Nodes.AllowDelete)
        {
            SetStatus("editor.status.node.delete.disabledByPermissions", "Node deletion is disabled by host permissions.");
            return;
        }

        if (SelectedNodes.Count == 0)
        {
            SetStatus("editor.status.node.delete.selectNodeFirst", "Select a node before deleting.");
            return;
        }

        _kernel.DeleteSelection();
    }

    /// <summary>
    /// 将当前选择按左边缘对齐。
    /// </summary>
    public void AlignSelectionLeft()
        => ExecuteLayoutCommand("layout.align-left");

    /// <summary>
    /// 将当前选择按水平中心对齐。
    /// </summary>
    public void AlignSelectionCenter()
        => ExecuteLayoutCommand("layout.align-center");

    /// <summary>
    /// 将当前选择按右边缘对齐。
    /// </summary>
    public void AlignSelectionRight()
        => ExecuteLayoutCommand("layout.align-right");

    /// <summary>
    /// 将当前选择按上边缘对齐。
    /// </summary>
    public void AlignSelectionTop()
        => ExecuteLayoutCommand("layout.align-top");

    /// <summary>
    /// 将当前选择按垂直中心对齐。
    /// </summary>
    public void AlignSelectionMiddle()
        => ExecuteLayoutCommand("layout.align-middle");

    /// <summary>
    /// 将当前选择按下边缘对齐。
    /// </summary>
    public void AlignSelectionBottom()
        => ExecuteLayoutCommand("layout.align-bottom");

    /// <summary>
    /// 将当前选择按水平方向均匀分布。
    /// </summary>
    public void DistributeSelectionHorizontally()
        => ExecuteLayoutCommand("layout.distribute-horizontal");

    /// <summary>
    /// 将当前选择按垂直方向均匀分布。
    /// </summary>
    public void DistributeSelectionVertically()
        => ExecuteLayoutCommand("layout.distribute-vertical");

    /// <summary>
    /// 按实例标识删除单个节点。
    /// </summary>
    public void DeleteNodeById(string nodeId)
        => _compatibilityCommands.DeleteNodeById(nodeId);

    /// <summary>
    /// 复制单个节点并自动偏移生成副本。
    /// </summary>
    public void DuplicateNode(string nodeId)
        => _compatibilityCommands.DuplicateNode(nodeId);

    /// <summary>
    /// 断开指定节点的所有入边。
    /// </summary>
    public void DisconnectIncoming(string nodeId)
        => _compatibilityCommands.DisconnectIncoming(nodeId);

    /// <summary>
    /// 断开指定节点的所有出边。
    /// </summary>
    public void DisconnectOutgoing(string nodeId)
        => _compatibilityCommands.DisconnectOutgoing(nodeId);

    private void ExecuteLayoutCommand(string commandId)
    {
        Session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(commandId));
        StatusMessage = _sessionHost.CurrentStatusMessage;
    }

    /// <summary>
    /// 断开指定节点的全部连线。
    /// </summary>
    public void DisconnectAll(string nodeId)
        => _compatibilityCommands.DisconnectAll(nodeId);

    /// <summary>
    /// 断开指定端口上的全部连线。
    /// </summary>
    public void BreakConnectionsForPort(string nodeId, string portId)
        => _compatibilityCommands.BreakConnectionsForPort(nodeId, portId);

    /// <summary>
    /// 删除指定连线。
    /// </summary>
    public void DeleteConnection(string connectionId)
        => _compatibilityCommands.DeleteConnection(connectionId);

    /// <summary>
    /// 断开指定连线。
    /// </summary>
    public void DisconnectConnection(string connectionId)
        => _compatibilityCommands.DisconnectConnection(connectionId);

    /// <summary>
    /// 更新指定连线的纯展示注释文本。
    /// </summary>
    public bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus = true)
        => _kernel.TrySetConnectionNoteText(connectionId, noteText, updateStatus);

    /// <summary>
    /// 按实例标识查找连线视图模型。
    /// </summary>
    public ConnectionViewModel? FindConnection(string connectionId)
        => _documentProjectionApplier.FindConnection(connectionId);

    /// <summary>
    /// 将视口中心移动到指定节点。
    /// </summary>
    public void CenterViewOnNode(string nodeId)
        => _kernel.CenterViewOnNode(nodeId);

    /// <summary>
    /// 将视口中心移动到指定世界坐标。
    /// </summary>
    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus = true)
        => _kernel.CenterViewAt(worldPoint, updateStatus);

    /// <summary>
    /// 查询指定输出端口可连接的兼容输入端口。
    /// </summary>
    /// <remarks>
    /// 此兼容立面仅保留在 retained host 边界，
    /// 并将运行时 snapshot 目标重新映射回当前 <see cref="GraphEditorViewModel"/> 持有的
    /// <see cref="NodeViewModel"/> / <see cref="PortViewModel"/> 实例。
    /// </remarks>
#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _kernel.GetCompatiblePortTargets(sourceNodeId, sourcePortId)
            .Select(target =>
            {
                var node = FindNode(target.NodeId);
                var port = node?.GetPort(target.PortId);
                return node is null || port is null
                    ? null
                    : new CompatiblePortTarget(node, port, target.Compatibility);
            })
            .Where(target => target is not null)
            .Select(target => target!)
            .ToList();
#pragma warning restore CS0618
}
