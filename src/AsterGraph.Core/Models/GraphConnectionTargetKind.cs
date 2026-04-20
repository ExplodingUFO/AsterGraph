namespace AsterGraph.Core.Models;

/// <summary>
/// Identifies the kind of target endpoint a connection terminates on.
/// </summary>
public enum GraphConnectionTargetKind
{
    /// <summary>
    /// The connection targets a node input port.
    /// </summary>
    Port = 0,

    /// <summary>
    /// The connection targets a definition-driven node parameter endpoint.
    /// </summary>
    Parameter = 1,
}
