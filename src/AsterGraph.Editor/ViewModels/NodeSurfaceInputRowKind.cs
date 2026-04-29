using System.ComponentModel;

namespace AsterGraph.Editor.ViewModels;

/// <summary>
/// Distinguishes graph input ports from parameter-backed input endpoints in the node-local surface projection.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public enum NodeSurfaceInputRowKind
{
    Port = 0,
    ParameterEndpoint = 1,
}
