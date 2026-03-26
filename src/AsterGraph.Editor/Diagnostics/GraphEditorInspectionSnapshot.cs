using AsterGraph.Core.Models;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 表示当前图编辑器集成检查状态的不可变快照。
/// </summary>
public sealed record GraphEditorInspectionSnapshot
{
    /// <summary>
    /// 初始化检查快照。
    /// </summary>
    /// <param name="document">当前文档快照。</param>
    /// <param name="selection">当前选择快照。</param>
    /// <param name="viewport">当前视口快照。</param>
    /// <param name="capabilities">当前能力快照。</param>
    /// <param name="pendingConnection">当前待完成连线快照。</param>
    /// <param name="status">当前状态快照。</param>
    /// <param name="nodePositions">当前节点位置快照集合。</param>
    /// <param name="recentDiagnostics">最近诊断集合。</param>
    public GraphEditorInspectionSnapshot(
        GraphDocument document,
        GraphEditorSelectionSnapshot selection,
        GraphEditorViewportSnapshot viewport,
        GraphEditorCapabilitySnapshot capabilities,
        GraphEditorPendingConnectionSnapshot pendingConnection,
        GraphEditorStatusSnapshot status,
        IReadOnlyList<NodePositionSnapshot> nodePositions,
        IReadOnlyList<GraphEditorDiagnostic> recentDiagnostics)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(viewport);
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(pendingConnection);
        ArgumentNullException.ThrowIfNull(status);
        ArgumentNullException.ThrowIfNull(nodePositions);
        ArgumentNullException.ThrowIfNull(recentDiagnostics);

        Document = document;
        Selection = selection;
        Viewport = viewport;
        Capabilities = capabilities;
        PendingConnection = pendingConnection;
        Status = status;
        NodePositions = nodePositions.ToList();
        RecentDiagnostics = recentDiagnostics.ToList();
    }

    /// <summary>
    /// 当前文档快照。
    /// </summary>
    public GraphDocument Document { get; }

    /// <summary>
    /// 当前选择快照。
    /// </summary>
    public GraphEditorSelectionSnapshot Selection { get; }

    /// <summary>
    /// 当前视口快照。
    /// </summary>
    public GraphEditorViewportSnapshot Viewport { get; }

    /// <summary>
    /// 当前能力快照。
    /// </summary>
    public GraphEditorCapabilitySnapshot Capabilities { get; }

    /// <summary>
    /// 当前待完成连线快照。
    /// </summary>
    public GraphEditorPendingConnectionSnapshot PendingConnection { get; }

    /// <summary>
    /// 当前状态快照。
    /// </summary>
    public GraphEditorStatusSnapshot Status { get; }

    /// <summary>
    /// 当前节点位置快照集合。
    /// </summary>
    public IReadOnlyList<NodePositionSnapshot> NodePositions { get; }

    /// <summary>
    /// 最近诊断集合。
    /// </summary>
    public IReadOnlyList<GraphEditorDiagnostic> RecentDiagnostics { get; }
}
