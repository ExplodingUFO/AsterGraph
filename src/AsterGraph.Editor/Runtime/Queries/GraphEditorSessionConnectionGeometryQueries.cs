using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

public sealed partial class GraphEditorSession
{
    private int _connectionGeometryCacheRevision = -1;
    private IReadOnlyList<GraphEditorConnectionGeometrySnapshot>? _connectionGeometryCache;

    public IReadOnlyList<GraphEditorConnectionGeometrySnapshot> GetConnectionGeometrySnapshots()
        => CreateConnectionGeometrySnapshots(CreateDocumentSnapshot());

    private IReadOnlyList<GraphEditorConnectionGeometrySnapshot> CreateConnectionGeometrySnapshots(GraphDocument document)
    {
        if (_connectionGeometryCacheRevision == _documentRevision
            && _connectionGeometryCache is not null)
        {
            return _connectionGeometryCache;
        }

        _connectionGeometryCache = GraphEditorConnectionGeometryProjector.Create(document, ResolveNodeDefinition);
        _connectionGeometryCacheRevision = _documentRevision;
        return _connectionGeometryCache;
    }

    private INodeDefinition? ResolveNodeDefinition(GraphNode node)
    {
        if (node.DefinitionId is null || _descriptorSupport is null)
        {
            return null;
        }

        return _descriptorSupport.Definitions.FirstOrDefault(definition => definition.Id == node.DefinitionId);
    }
}
