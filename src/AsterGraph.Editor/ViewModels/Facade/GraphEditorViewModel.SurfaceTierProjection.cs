using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal void ApplyNodeViewProjection(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        _presentationLocalizationCoordinator.ApplyNodePresentation(node);
        ApplyNodeSurfaceTier(node);
    }

    internal void FinalizeNodeViewProjection(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        ApplyNodeParameterEndpoints(node);
    }

    internal void RefreshNodeSurfaceTiers()
    {
        foreach (var node in Nodes)
        {
            ApplyNodeSurfaceTier(node);
            ApplyNodeParameterEndpoints(node);
        }
    }

    internal GraphEditorNodeSurfaceTierSnapshot ResolveNodeSurfaceTier(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var (definition, _, measurement) = ResolveNodeSurfacePlan(node);
        return GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new Core.Models.GraphSize(node.Width, node.Height),
            BehaviorOptions,
            definition,
            measurement);
    }

    private void ApplyNodeSurfaceTier(NodeViewModel node)
    {
        var (_, _, measurement) = ResolveNodeSurfacePlan(node);
        node.UpdateSurfaceMeasurement(measurement);
        node.UpdateActiveSurfaceTier(ResolveNodeSurfaceTier(node));
    }

    private void ApplyNodeParameterEndpoints(NodeViewModel node)
    {
        var (definition, _, measurement) = ResolveNodeSurfacePlan(node);
        if (definition is null || definition.Parameters.Count == 0)
        {
            node.UpdateParameterEndpoints([]);
            return;
        }

        var revealsOptionalParameters = node.Height >= measurement.HeightToRevealAdditionalInputs;
        var endpoints = definition.Parameters
            .Where(parameter =>
            {
                var connection = Connections.FirstOrDefault(candidate =>
                    candidate.TargetKind == GraphConnectionTargetKind.Parameter
                    && string.Equals(candidate.TargetNodeId, node.Id, StringComparison.Ordinal)
                    && string.Equals(candidate.TargetPortId, parameter.Key, StringComparison.Ordinal));
                return parameter.IsRequired || revealsOptionalParameters || connection is not null;
            })
            .Select(parameter =>
            {
                var connection = Connections.FirstOrDefault(candidate =>
                    candidate.TargetKind == GraphConnectionTargetKind.Parameter
                    && string.Equals(candidate.TargetNodeId, node.Id, StringComparison.Ordinal)
                    && string.Equals(candidate.TargetPortId, parameter.Key, StringComparison.Ordinal));
                var currentValue = node.GetParameterValue(parameter.Key) ?? parameter.DefaultValue;
                var parameterViewModel = new NodeParameterViewModel(
                    parameter,
                    [currentValue],
                    (_, value) => Session.Commands.TrySetNodeParameterValue(node.Id, parameter.Key, value),
                    isHostReadOnly: !CanEditNodeParameters || connection is not null);

                return new NodeParameterEndpointViewModel(
                    parameterViewModel,
                    new GraphConnectionTargetRef(node.Id, parameter.Key, GraphConnectionTargetKind.Parameter),
                    connection?.Id,
                    connection is null ? null : ResolveConnectionSourceDisplay(connection));
            })
            .ToList();

        node.UpdateParameterEndpoints(endpoints);
    }

    private (INodeDefinition? Definition, GraphEditorNodeSurfaceContentPlan Plan, GraphEditorNodeSurfaceMeasurement Measurement) ResolveNodeSurfacePlan(NodeViewModel node)
    {
        var definition = node.DefinitionId is not null && _nodeCatalog.TryGetDefinition(node.DefinitionId, out var resolvedDefinition)
            ? resolvedDefinition
            : null;
        var plan = GraphEditorNodeSurfacePlanner.Create(node, definition);
        var measurement = GraphEditorNodeSurfaceMeasurer.Measure(plan);
        return (definition, plan, measurement);
    }

    private string ResolveConnectionSourceDisplay(ConnectionViewModel connection)
    {
        var sourceNode = FindNode(connection.SourceNodeId);
        var sourcePort = sourceNode?.GetPort(connection.SourcePortId);
        return sourceNode is null
            ? connection.Label
            : $"{sourceNode.Title} · {sourcePort?.Label ?? connection.SourcePortId}";
    }
}
