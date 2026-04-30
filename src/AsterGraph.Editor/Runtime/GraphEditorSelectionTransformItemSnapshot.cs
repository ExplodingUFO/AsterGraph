using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorSelectionTransformItemSnapshot(
    string NodeId,
    GraphPoint Position,
    GraphSize Size,
    GraphPoint PreviewPosition);
