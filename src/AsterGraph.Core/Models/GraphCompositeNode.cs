namespace AsterGraph.Core.Models;

/// <summary>
/// Composite node metadata that links a host node shell to a child graph scope.
/// </summary>
/// <param name="ChildGraphId">Target child graph scope identifier owned by the composite node.</param>
/// <param name="Inputs">Boundary input ports surfaced by the composite node.</param>
/// <param name="Outputs">Boundary output ports surfaced by the composite node.</param>
public sealed record GraphCompositeNode(
    string ChildGraphId,
    IReadOnlyList<GraphCompositeBoundaryPort>? Inputs = null,
    IReadOnlyList<GraphCompositeBoundaryPort>? Outputs = null);
