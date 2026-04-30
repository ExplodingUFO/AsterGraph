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

        var nodeBoundsById = document.Nodes.ToDictionary(node => node.Id, CreateNodeBounds, StringComparer.Ordinal);
        var projected = document.Connections
            .Select(connection => CreateConnectionGeometrySnapshot(connection, nodesById, connectedParameterKeysByNodeId, definitionResolver))
            .Where(snapshot => snapshot is not null)
            .Select(snapshot => snapshot!)
            .ToList();
        if (projected.Count == 0)
        {
            return projected;
        }

        var pathsByConnectionId = projected.ToDictionary(
            snapshot => snapshot.ConnectionId,
            snapshot => ConnectionPathBuilder.BuildRoutePoints(snapshot.Source.Position, snapshot.Route, snapshot.Target.Position, snapshot.RouteStyle),
            StringComparer.Ordinal);
        return projected
            .Select(snapshot => snapshot with
            {
                RoutingEvidence = CreateRoutingEvidence(snapshot, projected, pathsByConnectionId, nodeBoundsById),
            })
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

        var route = connection.Presentation?.Route ?? GraphConnectionRoute.Empty;
        return new GraphEditorConnectionGeometrySnapshot(
            connection.Id,
            sourceAnchor,
            targetAnchor,
            route,
            ResolveRouteStyle(route));
    }

    private static GraphEditorConnectionRouteStyle ResolveRouteStyle(GraphConnectionRoute route)
        => route.IsEmpty
            ? GraphEditorConnectionRouteStyle.Bezier
            : GraphEditorConnectionRouteStyle.Orthogonal;

    private static GraphEditorConnectionRouteEvidenceSnapshot CreateRoutingEvidence(
        GraphEditorConnectionGeometrySnapshot snapshot,
        IReadOnlyList<GraphEditorConnectionGeometrySnapshot> allSnapshots,
        IReadOnlyDictionary<string, IReadOnlyList<GraphPoint>> pathsByConnectionId,
        IReadOnlyDictionary<string, NodeBounds> nodeBoundsById)
    {
        var path = pathsByConnectionId[snapshot.ConnectionId];
        var obstacleNodeIds = nodeBoundsById
            .Where(item =>
                !string.Equals(item.Key, snapshot.Source.NodeId, StringComparison.Ordinal)
                && !string.Equals(item.Key, snapshot.Target.NodeId, StringComparison.Ordinal)
                && PathIntersectsBounds(path, item.Value))
            .Select(item => item.Key)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToList();
        var crossingCount = allSnapshots
            .Where(other => !string.Equals(other.ConnectionId, snapshot.ConnectionId, StringComparison.Ordinal))
            .Where(other => !SharesEndpoint(snapshot, other))
            .Count(other => PathsIntersect(path, pathsByConnectionId[other.ConnectionId]));

        return new GraphEditorConnectionRouteEvidenceSnapshot(obstacleNodeIds, crossingCount, path);
    }

    private static bool SharesEndpoint(
        GraphEditorConnectionGeometrySnapshot first,
        GraphEditorConnectionGeometrySnapshot second)
        => string.Equals(first.Source.NodeId, second.Source.NodeId, StringComparison.Ordinal)
           || string.Equals(first.Source.NodeId, second.Target.NodeId, StringComparison.Ordinal)
           || string.Equals(first.Target.NodeId, second.Source.NodeId, StringComparison.Ordinal)
           || string.Equals(first.Target.NodeId, second.Target.NodeId, StringComparison.Ordinal);

    private static bool PathIntersectsBounds(IReadOnlyList<GraphPoint> path, NodeBounds bounds)
    {
        for (var index = 0; index < path.Count - 1; index++)
        {
            if (SegmentIntersectsBounds(path[index], path[index + 1], bounds))
            {
                return true;
            }
        }

        return false;
    }

    private static bool PathsIntersect(IReadOnlyList<GraphPoint> first, IReadOnlyList<GraphPoint> second)
    {
        for (var firstIndex = 0; firstIndex < first.Count - 1; firstIndex++)
        {
            for (var secondIndex = 0; secondIndex < second.Count - 1; secondIndex++)
            {
                if (SegmentsIntersect(first[firstIndex], first[firstIndex + 1], second[secondIndex], second[secondIndex + 1]))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool SegmentIntersectsBounds(GraphPoint start, GraphPoint end, NodeBounds bounds)
    {
        if (PointInBounds(start, bounds) || PointInBounds(end, bounds))
        {
            return true;
        }

        var topLeft = new GraphPoint(bounds.X, bounds.Y);
        var topRight = new GraphPoint(bounds.X + bounds.Width, bounds.Y);
        var bottomRight = new GraphPoint(bounds.X + bounds.Width, bounds.Y + bounds.Height);
        var bottomLeft = new GraphPoint(bounds.X, bounds.Y + bounds.Height);
        return SegmentsIntersect(start, end, topLeft, topRight)
               || SegmentsIntersect(start, end, topRight, bottomRight)
               || SegmentsIntersect(start, end, bottomRight, bottomLeft)
               || SegmentsIntersect(start, end, bottomLeft, topLeft);
    }

    private static bool PointInBounds(GraphPoint point, NodeBounds bounds)
        => point.X >= bounds.X
           && point.X <= bounds.X + bounds.Width
           && point.Y >= bounds.Y
           && point.Y <= bounds.Y + bounds.Height;

    private static bool SegmentsIntersect(GraphPoint a, GraphPoint b, GraphPoint c, GraphPoint d)
    {
        var abX = b.X - a.X;
        var abY = b.Y - a.Y;
        var acX = c.X - a.X;
        var acY = c.Y - a.Y;
        var adX = d.X - a.X;
        var adY = d.Y - a.Y;
        var cdX = d.X - c.X;
        var cdY = d.Y - c.Y;
        var caX = a.X - c.X;
        var caY = a.Y - c.Y;
        var cbX = b.X - c.X;
        var cbY = b.Y - c.Y;
        var first = Cross(abX, abY, acX, acY);
        var second = Cross(abX, abY, adX, adY);
        var third = Cross(cdX, cdY, caX, caY);
        var fourth = Cross(cdX, cdY, cbX, cbY);

        if (NearlyZero(first) && PointOnSegment(c, a, b))
        {
            return true;
        }

        if (NearlyZero(second) && PointOnSegment(d, a, b))
        {
            return true;
        }

        if (NearlyZero(third) && PointOnSegment(a, c, d))
        {
            return true;
        }

        if (NearlyZero(fourth) && PointOnSegment(b, c, d))
        {
            return true;
        }

        return (first > 0d) != (second > 0d)
               && (third > 0d) != (fourth > 0d);
    }

    private static double Cross(double leftX, double leftY, double rightX, double rightY)
        => (leftX * rightY) - (leftY * rightX);

    private static bool PointOnSegment(GraphPoint point, GraphPoint start, GraphPoint end)
        => point.X >= Math.Min(start.X, end.X) - 0.001d
           && point.X <= Math.Max(start.X, end.X) + 0.001d
           && point.Y >= Math.Min(start.Y, end.Y) - 0.001d
           && point.Y <= Math.Max(start.Y, end.Y) + 0.001d;

    private static bool NearlyZero(double value)
        => Math.Abs(value) < 0.001d;

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
