using AsterGraph.Abstractions.Definitions;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    internal void ApplyNodeViewProjection(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        ApplyNodeSurfaceTier(node);
        _presentationLocalizationCoordinator.ApplyNodePresentation(node);
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
        }
    }

    internal GraphEditorNodeSurfaceTierSnapshot ResolveNodeSurfaceTier(NodeViewModel node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var definition = node.DefinitionId is not null && _nodeCatalog.TryGetDefinition(node.DefinitionId, out var resolvedDefinition)
            ? resolvedDefinition
            : null;
        return GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new Core.Models.GraphSize(node.Width, node.Height),
            BehaviorOptions,
            definition);
    }

    private void ApplyNodeSurfaceTier(NodeViewModel node)
        => node.UpdateActiveSurfaceTier(ResolveNodeSurfaceTier(node));

    private void ApplyNodeParameterEndpoints(NodeViewModel node)
    {
        var definition = node.DefinitionId is not null && _nodeCatalog.TryGetDefinition(node.DefinitionId, out var resolvedDefinition)
            ? resolvedDefinition
            : null;
        if (definition is null || definition.Parameters.Count == 0)
        {
            node.UpdateParameterEndpoints([]);
            return;
        }

        var endpoints = definition.Parameters
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

    private string ResolveConnectionSourceDisplay(ConnectionViewModel connection)
    {
        var sourceNode = FindNode(connection.SourceNodeId);
        var sourcePort = sourceNode?.GetPort(connection.SourcePortId);
        return sourceNode is null
            ? connection.Label
            : $"{sourceNode.Title} · {sourcePort?.Label ?? connection.SourcePortId}";
    }
}
