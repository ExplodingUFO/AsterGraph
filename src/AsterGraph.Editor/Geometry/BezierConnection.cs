using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Geometry;

public readonly record struct BezierConnection(
    GraphPoint Start,
    GraphPoint Control1,
    GraphPoint Control2,
    GraphPoint End);
