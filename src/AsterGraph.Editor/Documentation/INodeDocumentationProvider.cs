using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Documentation;

/// <summary>
/// Projects node, port, parameter, and edge documentation from registered definition metadata.
/// </summary>
public interface INodeDocumentationProvider
{
    NodeDocumentationSnapshot? GetNodeDocumentation(NodeDefinitionId? definitionId);

    PortDocumentationSnapshot? GetPortDocumentation(NodeDefinitionId? definitionId, string portId, PortDirection direction);

    ParameterDocumentationSnapshot? GetParameterDocumentation(NodeDefinitionId? definitionId, string parameterKey);

    EdgeDocumentationSnapshot? GetEdgeDocumentation(GraphConnection connection, GraphDocument document);
}

