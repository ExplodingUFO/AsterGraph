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
    bool CanLoadWorkspace)
{
    /// <summary>
    /// 初始化包含扩展能力字段的兼容快照。
    /// </summary>
    [Obsolete("Use the six-parameter constructor plus init properties instead.")]
    public GraphEditorCapabilitySnapshot(
        bool canUndo,
        bool canRedo,
        bool canCopySelection,
        bool canPaste,
        bool canSaveWorkspace,
        bool canLoadWorkspace,
        bool canSetSelection,
        bool canMoveNodes,
        bool canCreateConnections,
        bool canDeleteConnections,
        bool canBreakConnections,
        bool canUpdateViewport,
        bool canFitToViewport,
        bool canCenterViewport)
        : this(
            canUndo,
            canRedo,
            canCopySelection,
            canPaste,
            canSaveWorkspace,
            canLoadWorkspace)
    {
        CanSetSelection = canSetSelection;
        CanMoveNodes = canMoveNodes;
        CanCreateConnections = canCreateConnections;
        CanDeleteConnections = canDeleteConnections;
        CanBreakConnections = canBreakConnections;
        CanUpdateViewport = canUpdateViewport;
        CanFitToViewport = canFitToViewport;
        CanCenterViewport = canCenterViewport;
    }

    /// <summary>
    /// 当前是否允许设置选择。
    /// </summary>
    public bool CanSetSelection { get; init; }

    /// <summary>
    /// 当前是否允许移动节点。
    /// </summary>
    public bool CanMoveNodes { get; init; }

    /// <summary>
    /// 当前是否允许对齐当前选择。
    /// </summary>
    public bool CanAlignSelection { get; init; }

    /// <summary>
    /// 当前是否允许分布当前选择。
    /// </summary>
    public bool CanDistributeSelection { get; init; }

    /// <summary>
    /// Current whether node parameter editing is allowed for the active selection.
    /// </summary>
    public bool CanEditNodeParameters { get; init; }

    /// <summary>
    /// 当前是否允许创建连线。
    /// </summary>
    public bool CanCreateConnections { get; init; }

    /// <summary>
    /// 当前是否允许删除连线。
    /// </summary>
    public bool CanDeleteConnections { get; init; }

    /// <summary>
    /// 当前是否允许断开端口连线。
    /// </summary>
    public bool CanBreakConnections { get; init; }

    /// <summary>
    /// 当前是否允许更新视口尺寸。
    /// </summary>
    public bool CanUpdateViewport { get; init; }

    /// <summary>
    /// 当前是否允许根据视口尺寸适配内容。
    /// </summary>
    public bool CanFitToViewport { get; init; }

    /// <summary>
    /// 当前是否允许居中视口。
    /// </summary>
    public bool CanCenterViewport { get; init; }

    /// <summary>
    /// 兼容旧的 14 字段解构形式。
    /// </summary>
    [Obsolete("Use direct property access instead.")]
    public void Deconstruct(
        out bool canUndo,
        out bool canRedo,
        out bool canCopySelection,
        out bool canPaste,
        out bool canSaveWorkspace,
        out bool canLoadWorkspace,
        out bool canSetSelection,
        out bool canMoveNodes,
        out bool canCreateConnections,
        out bool canDeleteConnections,
        out bool canBreakConnections,
        out bool canUpdateViewport,
        out bool canFitToViewport,
        out bool canCenterViewport)
    {
        canUndo = CanUndo;
        canRedo = CanRedo;
        canCopySelection = CanCopySelection;
        canPaste = CanPaste;
        canSaveWorkspace = CanSaveWorkspace;
        canLoadWorkspace = CanLoadWorkspace;
        canSetSelection = CanSetSelection;
        canMoveNodes = CanMoveNodes;
        canCreateConnections = CanCreateConnections;
        canDeleteConnections = CanDeleteConnections;
        canBreakConnections = CanBreakConnections;
        canUpdateViewport = CanUpdateViewport;
        canFitToViewport = CanFitToViewport;
        canCenterViewport = CanCenterViewport;
    }
}
