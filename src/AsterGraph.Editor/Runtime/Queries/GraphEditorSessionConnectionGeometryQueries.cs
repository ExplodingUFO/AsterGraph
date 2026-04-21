using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    public IReadOnlyList<GraphEditorConnectionGeometrySnapshot> GetConnectionGeometrySnapshots()
        => CreateConnectionGeometrySnapshots(CreateDocumentSnapshot());

    private IReadOnlyList<GraphEditorConnectionGeometrySnapshot> CreateConnectionGeometrySnapshots(GraphDocument document)
        => GraphEditorConnectionGeometryProjector.Create(document, ResolveNodeDefinition);

    private INodeDefinition? ResolveNodeDefinition(GraphNode node)
    {
        if (node.DefinitionId is null || _descriptorSupport is null)
        {
            return null;
        }

        return _descriptorSupport.Definitions.FirstOrDefault(definition => definition.Id == node.DefinitionId);
    }
}
