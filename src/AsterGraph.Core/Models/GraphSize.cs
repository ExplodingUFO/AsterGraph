namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable width and height pair in graph world space.
/// </summary>
/// <param name="Width">Width component.</param>
/// <param name="Height">Height component.</param>
public readonly record struct GraphSize(double Width, double Height);
