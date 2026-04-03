namespace AsterGraph.Editor.Geometry;

/// <summary>
/// Immutable node bounds in world space.
/// </summary>
/// <param name="X">Left coordinate.</param>
/// <param name="Y">Top coordinate.</param>
/// <param name="Width">Node width.</param>
/// <param name="Height">Node height.</param>
public readonly record struct NodeBounds(double X, double Y, double Width, double Height);
