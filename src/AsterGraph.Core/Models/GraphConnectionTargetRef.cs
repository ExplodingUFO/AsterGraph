namespace AsterGraph.Core.Models;

/// <summary>
/// Stable reference to a connection target endpoint on a node.
/// </summary>
public readonly record struct GraphConnectionTargetRef(
    string NodeId,
    string TargetId,
    GraphConnectionTargetKind Kind = GraphConnectionTargetKind.Port)
{
    /// <summary>
    /// Whether the target resolves to a node input port.
    /// </summary>
    public bool IsPort => Kind == GraphConnectionTargetKind.Port;

    /// <summary>
    /// Whether the target resolves to a parameter endpoint.
    /// </summary>
    public bool IsParameter => Kind == GraphConnectionTargetKind.Parameter;
}
