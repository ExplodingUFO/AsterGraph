namespace AsterGraph.Avalonia.Controls;

/// <summary>
/// 指定 <see cref="GraphEditorView"/> 的外壳显示模式。
/// </summary>
public enum GraphEditorViewChromeMode
{
    /// <summary>
    /// 使用默认完整外壳，保留头部、节点库、检查器与状态栏。
    /// </summary>
    Default = 0,

    /// <summary>
    /// 仅保留中间画布区域，隐藏外壳级辅助面板。
    /// </summary>
    CanvasOnly = 1,
}
