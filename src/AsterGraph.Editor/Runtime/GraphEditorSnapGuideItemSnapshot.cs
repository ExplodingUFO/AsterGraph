using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorSnapGuideItemSnapshot(
    string NodeId,
    GraphPoint CurrentPosition,
    GraphPoint SnappedPosition,
    GraphPoint Offset,
    double GridSize);
