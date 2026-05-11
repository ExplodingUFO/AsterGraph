namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// Identifies why the current pending connection target was rejected.
/// </summary>
public enum GraphEditorPendingConnectionRejectionReason
{
    /// <summary>
    /// No target has been rejected.
    /// </summary>
    None = 0,

    /// <summary>
    /// One or both endpoints do not expose stable type identifiers.
    /// </summary>
    EndpointTypeMissing = 1,

    /// <summary>
    /// The source and target endpoint types are not compatible.
    /// </summary>
    IncompatibleEndpointTypes = 2,

    /// <summary>
    /// The source output has reached its configured connection limit.
    /// </summary>
    SourceConnectionLimitReached = 3,

    /// <summary>
    /// The target input would exceed its configured connection limit.
    /// </summary>
    TargetConnectionLimitExceeded = 4,

    /// <summary>
    /// Completing the connection would introduce a directed graph cycle.
    /// </summary>
    WouldCreateCycle = 5,
}
