namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 表示当前选择状态的不可变快照。
/// </summary>
/// <param name="SelectedNodeIds">当前选中的节点实例标识集合。</param>
/// <param name="PrimarySelectedNodeId">当前主选中节点实例标识。</param>
public sealed record GraphEditorSelectionSnapshot(
    IReadOnlyList<string> SelectedNodeIds,
    string? PrimarySelectedNodeId)
{
    public IReadOnlyList<string> SelectedConnectionIds { get; init; } = [];

    public string? PrimarySelectedConnectionId { get; init; }
}
