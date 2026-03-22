using AsterGraph.Abstractions.Identifiers;

namespace AsterGraph.Core.Models;

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
    IReadOnlyList<GraphParameterValue>? ParameterValues = null);
