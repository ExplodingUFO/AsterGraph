using System.ComponentModel;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

/// <summary>
/// World-space anchor information for one node port.
/// </summary>
/// <param name="NodeId">Owning node identifier.</param>
/// <param name="PortId">Owning port identifier.</param>
/// <param name="Position">Anchor world position.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct PortAnchor(string NodeId, string PortId, GraphPoint Position);
