using Avalonia.Controls;
using AsterGraph.Avalonia.Presentation;

namespace AsterGraph.Avalonia.Controls.Internal;

internal readonly record struct NodeCanvasResizeFeedbackHit(
    Control Surface,
    GraphResizeFeedbackContext Context,
    GraphNodeResizeHandleKind? NodeHandle,
    NodeCanvasGroupResizeEdge? GroupEdge);
