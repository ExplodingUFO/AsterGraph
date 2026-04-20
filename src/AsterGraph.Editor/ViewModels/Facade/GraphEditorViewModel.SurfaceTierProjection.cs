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

        ApplyNodeInputProjection(node);
    }

    internal void RefreshNodeSurfaceTiers()
    {
        foreach (var node in Nodes)
        {
            ApplyNodeSurfaceTier(node);
            ApplyNodeInputProjection(node);
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

    private void ApplyNodeInputProjection(NodeViewModel node)
    {
        var (definition, _, measurement) = ResolveNodeSurfacePlan(node);
        var visibleInputs = new List<NodeSurfaceInputRowViewModel>(node.Inputs.Count + Math.Max(definition?.Parameters.Count ?? 0, 0));

        foreach (var port in node.Inputs)
        {
            visibleInputs.Add(new NodeSurfaceInputRowViewModel(
                NodeSurfaceInputRowKind.Port,
                port.Label,
                port.TypeId,
                port: port));
        }

        if (definition is null || definition.Parameters.Count == 0)
        {
            node.UpdateParameterEndpoints([]);
            node.UpdateInputRows(visibleInputs);
            return;
        }

        var revealsOptionalParameters = node.Height >= measurement.HeightToRevealAdditionalInputs;
        var parameterConnectionsByKey = Connections
            .Where(candidate =>
                candidate.TargetKind == GraphConnectionTargetKind.Parameter
                && string.Equals(candidate.TargetNodeId, node.Id, StringComparison.Ordinal))
            .ToDictionary(connection => connection.TargetPortId, StringComparer.Ordinal);
        var endpoints = definition.Parameters
            .Where(parameter =>
            {
                parameterConnectionsByKey.TryGetValue(parameter.Key, out var connection);
                return parameter.IsRequired || revealsOptionalParameters || connection is not null;
            })
            .Select(parameter =>
            {
                parameterConnectionsByKey.TryGetValue(parameter.Key, out var connection);
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
        foreach (var endpoint in endpoints)
        {
            var inlineContentKind = ResolveInlineContentKind(node.ActiveSurfaceTier, endpoint);
            visibleInputs.Add(new NodeSurfaceInputRowViewModel(
                NodeSurfaceInputRowKind.ParameterEndpoint,
                endpoint.Parameter.DisplayName,
                endpoint.Parameter.TypeId,
                parameterEndpoint: endpoint,
                inlineContentKind: inlineContentKind,
                inlineDisplayText: ResolveInlineDisplayText(endpoint, inlineContentKind)));
        }

        node.UpdateInputRows(visibleInputs);
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

    private static NodeSurfaceInlineContentKind ResolveInlineContentKind(
        GraphEditorNodeSurfaceTierSnapshot tier,
        NodeParameterEndpointViewModel endpoint)
    {
        if (endpoint.IsConnected)
        {
            return NodeSurfaceInlineContentKind.Connection;
        }

        if (tier.ShowsSection(NodeSurfaceSectionKeys.InputEditors))
        {
            return NodeSurfaceInlineContentKind.Editor;
        }

        return tier.ShowsSection(NodeSurfaceSectionKeys.InputSummaries)
            ? NodeSurfaceInlineContentKind.Summary
            : NodeSurfaceInlineContentKind.None;
    }

    private static string? ResolveInlineDisplayText(
        NodeParameterEndpointViewModel endpoint,
        NodeSurfaceInlineContentKind inlineContentKind)
        => inlineContentKind switch
        {
            NodeSurfaceInlineContentKind.Connection => endpoint.ConnectedDisplayText ?? "Connected",
            NodeSurfaceInlineContentKind.Summary => DescribeParameterValue(endpoint.Parameter),
            _ => null,
        };

    private static string DescribeParameterValue(NodeParameterViewModel parameter)
    {
        if (parameter.HasMixedValues)
        {
            return parameter.MixedValueHint;
        }

        if (parameter.IsBoolean)
        {
            return parameter.BoolValue is true ? "Enabled" : "Disabled";
        }

        if (parameter.IsEnum)
        {
            return parameter.SelectedOption?.Label ?? parameter.StringValue;
        }

        return string.IsNullOrWhiteSpace(parameter.StringValue)
            ? parameter.TypeId.Value
            : parameter.StringValue;
    }
}
