namespace AsterGraph.Editor.Plugins;

/// <summary>
/// 描述插件信任评估的决策结果。
/// </summary>
public enum GraphEditorPluginTrustDecision
{
    /// <summary>
    /// 允许继续进入加载流程。
    /// </summary>
    Allowed,

    /// <summary>
    /// 在执行插件贡献代码前阻止加载。
    /// </summary>
    Blocked,
}

/// <summary>
/// 描述插件信任评估结果的来源。
/// </summary>
public enum GraphEditorPluginTrustEvaluationSource
{
    /// <summary>
    /// 未配置宿主策略时的隐式允许。
    /// </summary>
    ImplicitAllow,

    /// <summary>
    /// 来自宿主提供的显式策略。
    /// </summary>
    HostPolicy,
}

/// <summary>
/// 表示一次插件信任评估的上下文。
/// </summary>
public sealed record GraphEditorPluginTrustPolicyContext
{
    /// <summary>
    /// 初始化信任评估上下文。
    /// </summary>
    public GraphEditorPluginTrustPolicyContext(
        GraphEditorPluginRegistration registration,
        GraphEditorPluginManifest manifest,
        GraphEditorPluginProvenanceEvidence provenanceEvidence)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provenanceEvidence);

        Registration = registration;
        Manifest = manifest;
        ProvenanceEvidence = provenanceEvidence;
    }

    /// <summary>
    /// 当前插件注册项。
    /// </summary>
    public GraphEditorPluginRegistration Registration { get; }

    /// <summary>
    /// 当前可见的插件清单。
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// 当前可见的来源和签名证据。
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }

    /// <summary>
    /// 当前注册关联的本地包归档绝对路径。
    /// </summary>
    public string? PackagePath => Registration.PackagePath;
}

/// <summary>
/// 描述一次稳定的插件信任评估结果。
/// </summary>
public sealed record GraphEditorPluginTrustEvaluation
{
    /// <summary>
    /// 初始化信任评估结果。
    /// </summary>
    public GraphEditorPluginTrustEvaluation(
        GraphEditorPluginTrustDecision decision,
        GraphEditorPluginTrustEvaluationSource source,
        string? reasonCode = null,
        string? reasonMessage = null)
    {
        Decision = decision;
        Source = source;
        ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? null : reasonCode.Trim();
        ReasonMessage = string.IsNullOrWhiteSpace(reasonMessage) ? null : reasonMessage.Trim();
    }

    /// <summary>
    /// 决策结果。
    /// </summary>
    public GraphEditorPluginTrustDecision Decision { get; }

    /// <summary>
    /// 决策来源。
    /// </summary>
    public GraphEditorPluginTrustEvaluationSource Source { get; }

    /// <summary>
    /// 可选的稳定原因代码。
    /// </summary>
    public string? ReasonCode { get; }

    /// <summary>
    /// 可选的宿主可读原因文本。
    /// </summary>
    public string? ReasonMessage { get; }

    /// <summary>
    /// 创建未配置策略时的默认允许结果。
    /// </summary>
    public static GraphEditorPluginTrustEvaluation ImplicitAllow(
        string? reasonCode = "trust.policy.not-configured",
        string? reasonMessage = "No plugin trust policy was configured.")
        => new(
            GraphEditorPluginTrustDecision.Allowed,
            GraphEditorPluginTrustEvaluationSource.ImplicitAllow,
            reasonCode,
            reasonMessage);
}

/// <summary>
/// 定义宿主可插入的插件信任策略。
/// </summary>
public interface IGraphEditorPluginTrustPolicy
{
    /// <summary>
    /// 对当前插件清单执行一次加载前评估。
    /// </summary>
    /// <param name="context">评估上下文。</param>
    /// <returns>机器可读的评估结果。</returns>
    GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context);
}
