using AsterGraph.Core.Models;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Node-local parameter endpoint projection used by hosted node-side authoring surfaces.
/// </summary>
public sealed class NodeParameterEndpointViewModel
{
    public NodeParameterEndpointViewModel(
        NodeParameterViewModel parameter,
        GraphConnectionTargetRef target,
        string? connectedConnectionId = null,
        string? connectedDisplayText = null)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        Parameter = parameter;
        Target = target;
        ConnectedConnectionId = string.IsNullOrWhiteSpace(connectedConnectionId) ? null : connectedConnectionId;
        ConnectedDisplayText = string.IsNullOrWhiteSpace(connectedDisplayText) ? null : connectedDisplayText.Trim();
    }

    /// <summary>
    /// Parameter editor/view state for this endpoint.
    /// </summary>
    public NodeParameterViewModel Parameter { get; }

    /// <summary>
    /// Stable connection target reference exposed by this endpoint.
    /// </summary>
    public GraphConnectionTargetRef Target { get; }

    /// <summary>
    /// Current connection identifier bound to this endpoint, when present.
    /// </summary>
    public string? ConnectedConnectionId { get; }

    /// <summary>
    /// Human-readable connected-source summary shown instead of the editor while connected.
    /// </summary>
    public string? ConnectedDisplayText { get; }

    /// <summary>
    /// Whether the endpoint currently receives an upstream value.
    /// </summary>
    public bool IsConnected => !string.IsNullOrWhiteSpace(ConnectedConnectionId);
}
