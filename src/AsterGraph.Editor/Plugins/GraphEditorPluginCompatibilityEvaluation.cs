namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 指示插件兼容性评估的稳定状态。
/// </summary>
public enum GraphEditorPluginCompatibilityStatus
{
    /// <summary>
    /// 没有足够信息判断兼容性。
    /// </summary>
    Unknown,

    /// <summary>
    /// 当前宿主与清单声明兼容。
    /// </summary>
    Compatible,

    /// <summary>
    /// 当前宿主与清单声明不兼容。
    /// </summary>
    Incompatible,
}

/// <summary>
/// 描述一次插件兼容性评估结果。
/// </summary>
public sealed record GraphEditorPluginCompatibilityEvaluation
{
    /// <summary>
    /// 初始化兼容性评估结果。
    /// </summary>
    public GraphEditorPluginCompatibilityEvaluation(
        GraphEditorPluginCompatibilityStatus status,
        string? reasonCode = null,
        string? reasonMessage = null)
    {
        Status = status;
        ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode.Trim();
        ReasonMessage = string.IsNullOrWhiteSpace(reasonMessage) ? null : reasonMessage.Trim();
    }

    /// <summary>
    /// 兼容性状态。
    /// </summary>
    public GraphEditorPluginCompatibilityStatus Status { get; }

    /// <summary>
    /// 可选的稳定原因代码。
    /// </summary>
    public string? ReasonCode { get; }

    /// <summary>
    /// 可选的宿主可读原因文本。
    /// </summary>
    public string? ReasonMessage { get; }
}
