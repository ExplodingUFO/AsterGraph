using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

/// <summary>
/// Immutable persisted node snapshot inside a graph document.
/// </summary>
/// <param name="Id">Stable node identifier within the document.</param>
/// <param name="Title">Display title shown for the node.</param>
/// <param name="Category">Host-facing grouping category for the node.</param>
/// <param name="Subtitle">Secondary short text shown under the title.</param>
/// <param name="Description">Longer descriptive text for the node.</param>
/// <param name="Position">Node world position in the canvas.</param>
/// <param name="Size">Node world-space size.</param>
/// <param name="Inputs">Declared input ports on the node.</param>
/// <param name="Outputs">Declared output ports on the node.</param>
/// <param name="AccentHex">Primary accent color for the node card.</param>
/// <param name="DefinitionId">Optional stable definition identifier used to resolve catalog metadata.</param>
/// <param name="ParameterValues">Optional persisted parameter values for the node.</param>
/// <param name="Surface">Optional persisted host/runtime node surface state.</param>
public sealed record GraphNode(
    string Id,
    string Title,
    string Category,
    string Subtitle,
    string Description,
    GraphPoint Position,
    GraphSize Size,
    IReadOnlyList<GraphPort> Inputs,
    IReadOnlyList<GraphPort> Outputs,
    string AccentHex,
    NodeDefinitionId? DefinitionId = null,
    IReadOnlyList<GraphParameterValue>? ParameterValues = null,
    GraphNodeSurfaceState? Surface = null);
