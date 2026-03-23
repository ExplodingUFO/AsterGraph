namespace AsterGraph.Editor.Menus;

/// <summary>
/// 标识右键菜单命中的编辑器对象类型。
/// </summary>
public enum ContextMenuTargetKind
{
    /// <summary>
    /// 画布空白区域。
    /// </summary>
    Canvas,

    /// <summary>
    /// 当前多选集合。
    /// </summary>
    Selection,

    /// <summary>
    /// 单个节点。
    /// </summary>
    Node,

    /// <summary>
    /// 单个端口。
    /// </summary>
    Port,

    /// <summary>
    /// 单条连线。
    /// </summary>
    Connection
}
