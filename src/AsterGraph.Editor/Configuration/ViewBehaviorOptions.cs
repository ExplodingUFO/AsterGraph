namespace AsterGraph.Editor.Configuration;

/// <summary>
/// 视图层辅助功能配置。
/// </summary>
public sealed record ViewBehaviorOptions
{
    /// <summary>
    /// 是否显示小地图。
    /// </summary>
    public bool ShowMiniMap { get; init; } = true;
}
