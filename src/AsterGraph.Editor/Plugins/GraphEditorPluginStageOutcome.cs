namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 指示插件包暂存的结果状态。
/// </summary>
public enum GraphEditorPluginStageOutcome
{
    /// <summary>
    /// 已成功完成新的包暂存。
    /// </summary>
    Staged,

    /// <summary>
    /// 复用了已有的暂存缓存。
    /// </summary>
    CacheHit,

    /// <summary>
    /// 暂存在进入加载路径前被显式拒绝。
    /// </summary>
    Refused,

    /// <summary>
    /// 暂存尝试失败。
    /// </summary>
    Failed,
}
