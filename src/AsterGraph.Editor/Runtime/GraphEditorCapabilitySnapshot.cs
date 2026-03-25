namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 表示当前运行时能力状态的不可变快照。
/// </summary>
/// <param name="CanUndo">当前是否可撤销。</param>
/// <param name="CanRedo">当前是否可重做。</param>
/// <param name="CanCopySelection">当前是否可复制选择。</param>
/// <param name="CanPaste">当前是否可粘贴。</param>
/// <param name="CanSaveWorkspace">当前是否可保存工作区。</param>
/// <param name="CanLoadWorkspace">当前是否可加载工作区。</param>
public sealed record GraphEditorCapabilitySnapshot(
    bool CanUndo,
    bool CanRedo,
    bool CanCopySelection,
    bool CanPaste,
    bool CanSaveWorkspace,
    bool CanLoadWorkspace);
