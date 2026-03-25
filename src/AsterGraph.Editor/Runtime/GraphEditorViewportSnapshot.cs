namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 表示当前视口状态的不可变快照。
/// </summary>
/// <param name="Zoom">当前缩放比例。</param>
/// <param name="PanX">当前水平平移量。</param>
/// <param name="PanY">当前垂直平移量。</param>
/// <param name="ViewportWidth">当前视口宽度。</param>
/// <param name="ViewportHeight">当前视口高度。</param>
public sealed record GraphEditorViewportSnapshot(
    double Zoom,
    double PanX,
    double PanY,
    double ViewportWidth,
    double ViewportHeight);
