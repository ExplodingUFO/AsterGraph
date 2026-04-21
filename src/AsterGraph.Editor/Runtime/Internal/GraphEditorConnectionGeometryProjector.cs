using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace AsterGraph.Editor.Runtime;

internal static class GraphEditorConnectionGeometryProjector
{
    internal static IReadOnlyList<GraphEditorConnectionGeometrySnapshot> Create(
        GraphDocument document,
        Func<GraphNode, INodeDefinition?> definitionResolver)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(definitionResolver);

        if (document.Connections.Count == 0 || document.Nodes.Count == 0)
        {
            return [];
        }

        var nodesById = document.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var connectedParameterKeysByNodeId = document.Connections
            .Where(connection => connection.TargetKind == GraphConnectionTargetKind.Parameter)
            .GroupBy(connection => connection.TargetNodeId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(connection => connection.TargetPortId)
                    .ToHashSet(StringComparer.Ordinal),
                StringComparer.Ordinal);

        return document.Connections
            .Select(connection => CreateConnectionGeometrySnapshot(connection, nodesById, connectedParameterKeysByNodeId, definitionResolver))
            .Where(snapshot => snapshot is not null)
            .Select(snapshot => snapshot!)
            .ToList();
    }

    private static GraphEditorConnectionGeometrySnapshot? CreateConnectionGeometrySnapshot(
        GraphConnection connection,
        IReadOnlyDictionary<string, GraphNode> nodesById,
        IReadOnlyDictionary<string, HashSet<string>> connectedParameterKeysByNodeId,
        Func<GraphNode, INodeDefinition?> definitionResolver)
    {
        if (!nodesById.TryGetValue(connection.SourceNodeId, out var sourceNode)
            || !nodesById.TryGetValue(connection.TargetNodeId, out var targetNode))
        {
            return null;
        }

        var sourcePortIndex = FindPortIndex(sourceNode.Outputs, connection.SourcePortId);
        if (sourcePortIndex < 0)
        {
            return null;
        }

        var sourceAnchor = new GraphEditorConnectionEndpointGeometrySnapshot(
            sourceNode.Id,
            connection.SourcePortId,
            GraphConnectionTargetKind.Port,
            PortAnchorCalculator.GetAnchor(
                CreateNodeBounds(sourceNode),
                PortDirection.Output,
                sourcePortIndex,
                sourceNode.Outputs.Count));

        var targetAnchor = CreateTargetEndpointGeometrySnapshot(targetNode, connection.Target, connectedParameterKeysByNodeId, definitionResolver);
        if (targetAnchor is null)
        {
            return null;
        }

        return new GraphEditorConnectionGeometrySnapshot(
            connection.Id,
            sourceAnchor,
            targetAnchor,
            connection.Presentation?.Route ?? GraphConnectionRoute.Empty);
    }

    private static GraphEditorConnectionEndpointGeometrySnapshot? CreateTargetEndpointGeometrySnapshot(
        GraphNode node,
        GraphConnectionTargetRef target,
        IReadOnlyDictionary<string, HashSet<string>> connectedParameterKeysByNodeId,
        Func<GraphNode, INodeDefinition?> definitionResolver)
    {
        connectedParameterKeysByNodeId.TryGetValue(node.Id, out var connectedParameterKeys);
        connectedParameterKeys ??= new HashSet<string>(StringComparer.Ordinal);

        if (target.Kind == GraphConnectionTargetKind.Port)
        {
            var targetPortIndex = FindPortIndex(node.Inputs, target.TargetId);
            if (targetPortIndex < 0)
            {
                return null;
            }

            return new GraphEditorConnectionEndpointGeometrySnapshot(
                node.Id,
                target.TargetId,
                GraphConnectionTargetKind.Port,
                PortAnchorCalculator.GetAnchor(
                    CreateNodeBounds(node),
                    PortDirection.Input,
                    targetPortIndex,
                    node.Inputs.Count + ResolveVisibleParameterKeys(node, connectedParameterKeys, definitionResolver).Count));
        }

        var visibleParameterKeys = ResolveVisibleParameterKeys(node, connectedParameterKeys, definitionResolver);
        var parameterIndex = visibleParameterKeys.IndexOf(target.TargetId);
        if (parameterIndex < 0)
        {
            visibleParameterKeys.Add(target.TargetId);
            parameterIndex = visibleParameterKeys.Count - 1;
        }

        return new GraphEditorConnectionEndpointGeometrySnapshot(
            node.Id,
            target.TargetId,
            GraphConnectionTargetKind.Parameter,
            PortAnchorCalculator.GetAnchor(
                CreateNodeBounds(node),
                PortDirection.Input,
                node.Inputs.Count + parameterIndex,
                node.Inputs.Count + visibleParameterKeys.Count));
    }

    private static List<string> ResolveVisibleParameterKeys(
        GraphNode node,
        HashSet<string> connectedParameterKeys,
        Func<GraphNode, INodeDefinition?> definitionResolver)
    {
        var definition = definitionResolver(node);
        if (definition is null)
        {
            return connectedParameterKeys
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToList();
        }

        var measurement = GraphEditorNodeSurfaceMeasurer.Measure(GraphEditorNodeSurfacePlanner.Create(node, definition));
        var revealsOptionalParameters = node.Size.Height >= measurement.HeightToRevealAdditionalInputs;
        var visibleKeys = definition.Parameters
            .Where(parameter => parameter.IsRequired || revealsOptionalParameters || connectedParameterKeys.Contains(parameter.Key))
            .Select(parameter => parameter.Key)
            .ToList();

        foreach (var extraKey in connectedParameterKeys.OrderBy(key => key, StringComparer.Ordinal))
        {
            if (!visibleKeys.Contains(extraKey, StringComparer.Ordinal))
            {
                visibleKeys.Add(extraKey);
            }
        }

        return visibleKeys;
    }

    private static NodeBounds CreateNodeBounds(GraphNode node)
        => new(node.Position.X, node.Position.Y, node.Size.Width, node.Size.Height);

    private static int FindPortIndex(IReadOnlyList<GraphPort> ports, string portId)
    {
        for (var index = 0; index < ports.Count; index++)
        {
            if (string.Equals(ports[index].Id, portId, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }
}
