using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// Host-facing composite node projection that avoids direct child-scope traversal.
/// </summary>
/// <param name="NodeId">Composite shell node identifier in the active root graph.</param>
/// <param name="ChildGraphId">Promoted child graph scope identifier.</param>
/// <param name="Inputs">Exposed composite input boundary ports.</param>
/// <param name="Outputs">Exposed composite output boundary ports.</param>
public sealed record GraphEditorCompositeNodeSnapshot(
    string NodeId,
    string ChildGraphId,
    IReadOnlyList<GraphCompositeBoundaryPort> Inputs,
    IReadOnlyList<GraphCompositeBoundaryPort> Outputs);
