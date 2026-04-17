namespace AsterGraph.Editor.Plugins;

/// <summary>
/// Describes the decision returned by a plugin trust evaluation.
/// </summary>
public enum GraphEditorPluginTrustDecision
{
    /// <summary>
    /// Allows the plugin to continue into the loading pipeline.
    /// </summary>
    Allowed,

    /// <summary>
    /// Blocks the plugin before any contribution code executes.
    /// </summary>
    Blocked,
}

/// <summary>
/// Describes where a plugin trust evaluation came from.
/// </summary>
public enum GraphEditorPluginTrustEvaluationSource
{
    /// <summary>
    /// The plugin was implicitly allowed because no host policy was configured.
    /// </summary>
    ImplicitAllow,

    /// <summary>
    /// The result came from an explicit host-supplied policy.
    /// </summary>
    HostPolicy,
}

/// <summary>
/// Carries the context for one plugin trust evaluation.
/// </summary>
public sealed record GraphEditorPluginTrustPolicyContext
{
    /// <summary>
    /// Initializes a trust-policy context.
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
    /// Gets the current plugin registration.
    /// </summary>
    public GraphEditorPluginRegistration Registration { get; }

    /// <summary>
    /// Gets the visible plugin manifest.
    /// </summary>
    public GraphEditorPluginManifest Manifest { get; }

    /// <summary>
    /// Gets the visible provenance and signature evidence.
    /// </summary>
    public GraphEditorPluginProvenanceEvidence ProvenanceEvidence { get; }

    /// <summary>
    /// Gets the absolute local package path associated with the registration, when present.
    /// </summary>
    public string? PackagePath => Registration.PackagePath;
}

/// <summary>
/// Captures one stable plugin trust-evaluation result.
/// </summary>
public sealed record GraphEditorPluginTrustEvaluation
{
    /// <summary>
    /// Initializes a trust-evaluation result.
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
    /// Gets the trust decision.
    /// </summary>
    public GraphEditorPluginTrustDecision Decision { get; }

    /// <summary>
    /// Gets the source of the decision.
    /// </summary>
    public GraphEditorPluginTrustEvaluationSource Source { get; }

    /// <summary>
    /// Gets the optional machine-readable reason code.
    /// </summary>
    public string? ReasonCode { get; }

    /// <summary>
    /// Gets the optional host-readable reason message.
    /// </summary>
    public string? ReasonMessage { get; }

    /// <summary>
    /// Creates the default allow result used when no trust policy is configured.
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
/// Defines a host-supplied plugin trust policy.
/// </summary>
public interface IGraphEditorPluginTrustPolicy
{
    /// <summary>
    /// Evaluates a plugin before any contribution code is allowed to execute.
    /// </summary>
    /// <param name="context">The trust-evaluation context.</param>
    /// <returns>A machine-readable trust-evaluation result.</returns>
    GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context);
}
