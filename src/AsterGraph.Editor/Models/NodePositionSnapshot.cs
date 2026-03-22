using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Models;

/// <summary>
/// Stable editor-facing snapshot of a node's persisted canvas position.
/// </summary>
public sealed record NodePositionSnapshot(string NodeId, GraphPoint Position);
