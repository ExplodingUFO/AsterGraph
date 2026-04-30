using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

public sealed record GraphEditorSelectionTransformQuery(
    GraphPoint? SelectionRectanglePosition = null,
    GraphSize? SelectionRectangleSize = null,
    GraphPoint? PreviewDelta = null,
    bool ConstrainPreviewToPrimaryAxis = false);
