namespace AsterGraph.Editor.Events;

/// <summary>
/// 编辑器视口变化事件参数。
/// </summary>
public sealed class GraphEditorViewportChangedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化视口变化事件参数。
    /// </summary>
    public GraphEditorViewportChangedEventArgs(
        double zoom,
        double panX,
        double panY,
        double viewportWidth,
        double viewportHeight)
    {
        Zoom = zoom;
        PanX = panX;
        PanY = panY;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
    }

    /// <summary>
    /// 当前缩放比例。
    /// </summary>
    public double Zoom { get; }

    /// <summary>
    /// 当前水平平移量。
    /// </summary>
    public double PanX { get; }

    /// <summary>
    /// 当前垂直平移量。
    /// </summary>
    public double PanY { get; }

    /// <summary>
    /// 当前视口宽度。
    /// </summary>
    public double ViewportWidth { get; }

    /// <summary>
    /// 当前视口高度。
    /// </summary>
    public double ViewportHeight { get; }
}
