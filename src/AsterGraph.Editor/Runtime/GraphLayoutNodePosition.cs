using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

public sealed record GraphLayoutNodePosition(
    string NodeId,
    GraphPoint Position,
    bool IsPinned = false);
