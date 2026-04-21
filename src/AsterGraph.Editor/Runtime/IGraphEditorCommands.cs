using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义宿主可调用的图编辑器命令入口。
/// </summary>
public interface IGraphEditorCommands
{
    /// <summary>
    /// 撤销最近一次可撤销操作。
    /// </summary>
    void Undo();

    /// <summary>
    /// 重做最近一次已撤销操作。
    /// </summary>
    void Redo();

    /// <summary>
    /// 清空当前选择。
    /// </summary>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void ClearSelection(bool updateStatus = false);

    /// <summary>
    /// 直接设置当前选择集合及主选中节点。
    /// </summary>
    /// <param name="nodeIds">新的选择节点标识集合。</param>
    /// <param name="primaryNodeId">新的主选中节点标识。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId = null, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// 基于节点定义标识添加一个节点实例。
    /// </summary>
    /// <param name="definitionId">节点定义标识。</param>
    /// <param name="preferredWorldPosition">可选的世界坐标位置。</param>
    void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition = null);

    /// <summary>
    /// 删除当前选择。
    /// </summary>
    void DeleteSelection();

    /// <summary>
    /// 批量设置节点位置。
    /// </summary>
    /// <param name="positions">目标节点位置集合。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node's persisted card width through the runtime mutation path.
    /// </summary>
    /// <param name="nodeId">Stable node identifier.</param>
    /// <param name="width">Requested card width.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the width changes.</returns>
    bool TrySetNodeWidth(string nodeId, double width, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node's persisted card size through the runtime mutation path.
    /// </summary>
    /// <param name="nodeId">Stable node identifier.</param>
    /// <param name="size">Requested persisted card size.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the size changes.</returns>
    bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node's persisted card expansion state.
    /// </summary>
    /// <param name="nodeId">Stable node identifier.</param>
    /// <param name="expansionState">Requested expansion state.</param>
    /// <returns><see langword="true"/> when the state changes.</returns>
    bool TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to create one editor-only node group from the current selection.
    /// </summary>
    /// <param name="title">Requested group title.</param>
    /// <returns>The created stable group identifier, or an empty string when creation fails.</returns>
    string TryCreateNodeGroupFromSelection(string title)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node group's collapsed state.
    /// </summary>
    /// <param name="groupId">Stable group identifier.</param>
    /// <param name="isCollapsed">Requested collapsed state.</param>
    /// <returns><see langword="true"/> when the state changes.</returns>
    bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node group's persisted position and optionally move its member nodes together.
    /// </summary>
    /// <param name="groupId">Stable group identifier.</param>
    /// <param name="position">Requested top-left world position.</param>
    /// <param name="moveMemberNodes">Whether to translate group member nodes by the same delta.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the position changes.</returns>
    bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes = true, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node group's persisted frame size.
    /// </summary>
    /// <param name="groupId">Stable group identifier.</param>
    /// <param name="size">Requested fixed frame size.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the size changes.</returns>
    bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one node group's persisted per-edge padding envelope.
    /// </summary>
    /// <param name="groupId">Stable group identifier.</param>
    /// <param name="extraPadding">Requested per-edge padding envelope.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the padding changes.</returns>
    bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one or more node-to-group memberships.
    /// </summary>
    /// <param name="changes">Requested membership changes.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when at least one membership changes.</returns>
    bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to promote one persisted node group into a composite node with a child graph scope.
    /// </summary>
    /// <param name="groupId">Stable group identifier.</param>
    /// <param name="title">Optional composite shell title override.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns>The created composite shell node identifier, or an empty string when promotion fails.</returns>
    string TryPromoteNodeGroupToComposite(string groupId, string? title = null, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to wrap the current selection into a new composite shell in one authoring step.
    /// </summary>
    /// <param name="title">Optional composite shell title override.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns>The created composite shell node identifier, or an empty string when wrapping fails.</returns>
    string TryWrapSelectionToComposite(string? title = null, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to expose one child graph port as a public composite boundary port.
    /// </summary>
    /// <param name="compositeNodeId">Composite shell node identifier.</param>
    /// <param name="childNodeId">Child graph node identifier that owns the inner port.</param>
    /// <param name="childPortId">Child graph port identifier to expose.</param>
    /// <param name="label">Optional exposed label override.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns>The created boundary port identifier, or an empty string when exposure fails.</returns>
    string TryExposeCompositePort(
        string compositeNodeId,
        string childNodeId,
        string childPortId,
        string? label = null,
        bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to remove one exposed composite boundary port.
    /// </summary>
    /// <param name="compositeNodeId">Composite shell node identifier.</param>
    /// <param name="boundaryPortId">Boundary port identifier.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the boundary port is removed.</returns>
    bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to enter the child graph scope owned by one composite node in the current active scope.
    /// </summary>
    /// <param name="compositeNodeId">Composite shell node identifier in the current active scope.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when navigation succeeds.</returns>
    bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to return from the current active scope to its parent scope.
    /// </summary>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when navigation succeeds.</returns>
    bool TryReturnToParentGraphScope(bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to set one parameter value on one specific node instance.
    /// </summary>
    /// <param name="nodeId">Stable node instance identifier.</param>
    /// <param name="parameterKey">Stable parameter key declared by the node definition.</param>
    /// <param name="value">Candidate parameter value that will be normalized against the definition metadata.</param>
    /// <returns><see langword="true"/> when the mutation succeeds and changes the runtime document.</returns>
    bool TrySetNodeParameterValue(string nodeId, string parameterKey, object? value)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to set one parameter value across the current selection when every selected node shares the same definition.
    /// </summary>
    /// <param name="parameterKey">Stable parameter key declared by the shared node definition.</param>
    /// <param name="value">Candidate parameter value that will be normalized against the definition metadata.</param>
    /// <returns><see langword="true"/> when the mutation succeeds and changes the runtime document.</returns>
    bool TrySetSelectedNodeParameterValue(string parameterKey, object? value)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to set multiple parameter values across the current selection when every selected node shares the same definition.
    /// </summary>
    /// <param name="values">Stable parameter keys mapped to candidate values that will be normalized against the definition metadata.</param>
    /// <returns><see langword="true"/> when the mutation succeeds and changes the runtime document.</returns>
    bool TrySetSelectedNodeParameterValues(IReadOnlyDictionary<string, object?> values)
        => throw new NotSupportedException();

    /// <summary>
    /// 开始一条待完成连线。
    /// </summary>
    /// <param name="sourceNodeId">源节点标识。</param>
    /// <param name="sourcePortId">源端口标识。</param>
    void BeginConnection(string sourceNodeId, string sourcePortId)
        => StartConnection(sourceNodeId, sourcePortId);

    /// <summary>
    /// 开始一条待完成连线。
    /// </summary>
    /// <param name="sourceNodeId">源节点标识。</param>
    /// <param name="sourcePortId">源端口标识。</param>
    void StartConnection(string sourceNodeId, string sourcePortId)
        => throw new NotSupportedException();

    /// <summary>
    /// 完成当前待完成连线。
    /// </summary>
    /// <param name="targetNodeId">目标节点标识。</param>
    /// <param name="targetPortId">目标端口标识。</param>
    void CompleteConnection(string targetNodeId, string targetPortId)
        => CompleteConnection(new GraphConnectionTargetRef(targetNodeId, targetPortId));

    /// <summary>
    /// Completes the current pending connection against a typed target endpoint.
    /// </summary>
    /// <param name="target">Stable target endpoint reference.</param>
    void CompleteConnection(GraphConnectionTargetRef target)
        => throw new NotSupportedException();

    /// <summary>
    /// 取消当前待完成连线。
    /// </summary>
    void CancelPendingConnection()
        => throw new NotSupportedException();

    /// <summary>
    /// 删除指定连线。
    /// </summary>
    /// <param name="connectionId">连线标识。</param>
    void DeleteConnection(string connectionId)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to disconnect one existing connection and restart authoring from its original source endpoint.
    /// </summary>
    /// <param name="connectionId">Stable connection identifier.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when reconnect mode starts and leaves a pending connection.</returns>
    bool TryReconnectConnection(string connectionId, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// Attempts to update one connection's pure display note text.
    /// </summary>
    /// <param name="connectionId">Stable connection identifier.</param>
    /// <param name="noteText">New display-only note text, or <see langword="null"/> / blank to clear it.</param>
    /// <param name="updateStatus">Whether to update status text for no-op or success cases.</param>
    /// <returns><see langword="true"/> when the note text changes.</returns>
    bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// 断开指定端口上的全部连线。
    /// </summary>
    /// <param name="nodeId">节点标识。</param>
    /// <param name="portId">端口标识。</param>
    void BreakConnectionsForPort(string nodeId, string portId)
        => throw new NotSupportedException();

    /// <summary>
    /// 平移视口。
    /// </summary>
    /// <param name="deltaX">水平位移。</param>
    /// <param name="deltaY">垂直位移。</param>
    void PanBy(double deltaX, double deltaY);

    /// <summary>
    /// 以屏幕锚点为中心缩放视口。
    /// </summary>
    /// <param name="factor">缩放因子。</param>
    /// <param name="screenAnchor">屏幕锚点。</param>
    void ZoomAt(double factor, GraphPoint screenAnchor);

    /// <summary>
    /// 更新当前视口尺寸。
    /// </summary>
    /// <param name="width">视口宽度。</param>
    /// <param name="height">视口高度。</param>
    void UpdateViewportSize(double width, double height)
        => throw new NotSupportedException();

    /// <summary>
    /// 重置视口。
    /// </summary>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void ResetView(bool updateStatus = true);

    /// <summary>
    /// 将当前图内容适配到已知视口范围。
    /// </summary>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void FitToViewport(bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// 将视口中心移动到指定节点。
    /// </summary>
    /// <param name="nodeId">目标节点标识。</param>
    void CenterViewOnNode(string nodeId)
        => throw new NotSupportedException();

    /// <summary>
    /// 将视口中心移动到指定世界坐标。
    /// </summary>
    /// <param name="worldPoint">目标世界坐标。</param>
    /// <param name="updateStatus">是否更新状态文本。</param>
    void CenterViewAt(GraphPoint worldPoint, bool updateStatus = true)
        => throw new NotSupportedException();

    /// <summary>
    /// 保存当前工作区。
    /// </summary>
    void SaveWorkspace();

    /// <summary>
    /// 加载当前工作区。
    /// </summary>
    /// <returns>加载成功时返回 <see langword="true"/>。</returns>
    bool LoadWorkspace();

    /// <summary>
    /// 使用稳定命令标识和参数执行命令。
    /// 该入口适用于宿主自动化、批处理和 descriptor-first 会话驱动。
    /// </summary>
    /// <param name="command">命令调用描述。</param>
    /// <returns>命令被识别并已分派时返回 <see langword="true"/>。</returns>
    bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
        => throw new NotSupportedException();
}
