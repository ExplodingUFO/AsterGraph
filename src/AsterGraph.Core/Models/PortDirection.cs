namespace AsterGraph.Core.Models;

/// <summary>
/// Declares whether a port consumes or produces values.
/// </summary>
public enum PortDirection
{
    /// <summary>
    /// Input port.
    /// </summary>
    Input = 0,

    /// <summary>
    /// Output port.
    /// </summary>
    Output = 1,
}
