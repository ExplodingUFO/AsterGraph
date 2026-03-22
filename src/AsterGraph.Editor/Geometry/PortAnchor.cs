using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

public readonly record struct PortAnchor(string NodeId, string PortId, GraphPoint Position);
