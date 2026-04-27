using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Stable host-facing validation feedback for one graph node, connection, or endpoint.
/// </summary>
public sealed record GraphEditorValidationIssueSnapshot
{
    /// <summary>
    /// Initializes one validation issue snapshot.
    /// </summary>
    public GraphEditorValidationIssueSnapshot(
        string code,
        GraphEditorValidationIssueSeverity severity,
        string message,
        string scopeId,
        string? nodeId = null,
        string? connectionId = null,
        string? endpointId = null,
        GraphConnectionTargetKind? targetKind = null,
        string? parameterKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeId);

        Code = code.Trim();
        Severity = severity;
        Message = string.IsNullOrWhiteSpace(message) ? Code : message.Trim();
        ScopeId = scopeId.Trim();
        NodeId = NormalizeOptional(nodeId);
        ConnectionId = NormalizeOptional(connectionId);
        EndpointId = NormalizeOptional(endpointId);
        TargetKind = targetKind;
        ParameterKey = NormalizeOptional(parameterKey);
    }

    /// <summary>
    /// Stable machine-readable issue code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Readiness severity.
    /// </summary>
    public GraphEditorValidationIssueSeverity Severity { get; }

    /// <summary>
    /// Human-readable feedback summary.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Graph scope that owns the affected item.
    /// </summary>
    public string ScopeId { get; }

    /// <summary>
    /// Optional affected node identifier.
    /// </summary>
    public string? NodeId { get; }

    /// <summary>
    /// Optional affected connection identifier.
    /// </summary>
    public string? ConnectionId { get; }

    /// <summary>
    /// Optional affected endpoint identifier.
    /// </summary>
    public string? EndpointId { get; }

    /// <summary>
    /// Optional affected target endpoint kind.
    /// </summary>
    public GraphConnectionTargetKind? TargetKind { get; }

    /// <summary>
    /// Optional affected parameter key.
    /// </summary>
    public string? ParameterKey { get; }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
