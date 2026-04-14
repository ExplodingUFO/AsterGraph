using System;
using System.Collections.Generic;
using System.Linq;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
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
    /// 以指定输出端口作为连线起点。
    /// </summary>
    public void StartConnection(string sourceNodeId, string sourcePortId)
        => _kernel.StartConnection(sourceNodeId, sourcePortId);

    /// <summary>
    /// 连接源输出端口与目标输入端口。
    /// </summary>
    public void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        var pendingConnection = _kernel.GetPendingConnectionSnapshot();
        if (pendingConnection.HasPendingConnection
            && string.Equals(pendingConnection.SourceNodeId, sourceNodeId, StringComparison.Ordinal)
            && string.Equals(pendingConnection.SourcePortId, sourcePortId, StringComparison.Ordinal))
        {
            _kernel.CompleteConnection(targetNodeId, targetPortId);
            return;
        }

        _kernel.StartConnection(sourceNodeId, sourcePortId);
        _kernel.CompleteConnection(targetNodeId, targetPortId);
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
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignLeft,
            minimumCount: 2,
            StatusText("editor.status.layout.alignLeft", "Aligned selection left."));

    /// <summary>
    /// 将当前选择按水平中心对齐。
    /// </summary>
    public void AlignSelectionCenter()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignCenter,
            minimumCount: 2,
            StatusText("editor.status.layout.alignCenter", "Aligned selection center."));

    /// <summary>
    /// 将当前选择按右边缘对齐。
    /// </summary>
    public void AlignSelectionRight()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignRight,
            minimumCount: 2,
            StatusText("editor.status.layout.alignRight", "Aligned selection right."));

    /// <summary>
    /// 将当前选择按上边缘对齐。
    /// </summary>
    public void AlignSelectionTop()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignTop,
            minimumCount: 2,
            StatusText("editor.status.layout.alignTop", "Aligned selection top."));

    /// <summary>
    /// 将当前选择按垂直中心对齐。
    /// </summary>
    public void AlignSelectionMiddle()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignMiddle,
            minimumCount: 2,
            StatusText("editor.status.layout.alignMiddle", "Aligned selection middle."));

    /// <summary>
    /// 将当前选择按下边缘对齐。
    /// </summary>
    public void AlignSelectionBottom()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.AlignBottom,
            minimumCount: 2,
            StatusText("editor.status.layout.alignBottom", "Aligned selection bottom."));

    /// <summary>
    /// 将当前选择按水平方向均匀分布。
    /// </summary>
    public void DistributeSelectionHorizontally()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.DistributeHorizontally,
            minimumCount: 3,
            StatusText("editor.status.layout.distributeHorizontally", "Distributed selection horizontally."));

    /// <summary>
    /// 将当前选择按垂直方向均匀分布。
    /// </summary>
    public void DistributeSelectionVertically()
        => _nodeLayoutCoordinator.ApplySelectionLayout(
            NodeSelectionLayoutService.DistributeVertically,
            minimumCount: 3,
            StatusText("editor.status.layout.distributeVertically", "Distributed selection vertically."));

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
