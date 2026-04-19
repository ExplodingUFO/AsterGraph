using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;

namespace AsterGraph.Demo;

public static class DemoGraphFactory
{
    public static GraphDocument CreateDefault(INodeCatalog catalog)
        => new(
            "Terrain Shader Graph",
            "An extensible AsterGraph demo with compile-time node definitions and a reusable graph editor shell.",
            [
                CreateNode(catalog, "time", new NodeDefinitionId("aster.demo.time-driver"), new GraphPoint(60, 80)),
                CreateNode(catalog, "noise", new NodeDefinitionId("aster.demo.noise-field"), new GraphPoint(360, 40), groupId: "terrain-authoring"),
                CreateNode(catalog, "gradient", new NodeDefinitionId("aster.demo.palette-ramp"), new GraphPoint(340, 300), groupId: "terrain-authoring"),
                CreateNode(catalog, "slope", new NodeDefinitionId("aster.demo.slope-blend"), new GraphPoint(700, 170)),
                CreateNode(catalog, "light", new NodeDefinitionId("aster.demo.lighting-mix"), new GraphPoint(1080, 110), surface: new GraphNodeSurfaceState(GraphNodeExpansionState.Expanded)),
                CreateNode(catalog, "output", new NodeDefinitionId("aster.demo.viewport-output"), new GraphPoint(1460, 180)),
            ],
            [
                Connect("time", "phase", "noise", "phase", "time to detail", "#78F0E5"),
                Connect("time", "pulse", "gradient", "pulse", "pulse to ramp", "#FFD56A"),
                Connect("noise", "height", "slope", "height", "height field", "#FFD56A"),
                Connect("noise", "mask", "slope", "mask", "breakup mask", "#4AD6FF"),
                Connect("gradient", "tint", "slope", "tint", "palette feed", "#78F0E5"),
                Connect("slope", "albedo", "light", "albedo", "base surface", "#79E28A"),
                Connect("slope", "rough", "light", "rough", "roughness", "#FFD56A"),
                Connect("time", "pulse", "light", "pulse", "animated rim", "#78F0E5"),
                Connect("light", "lit", "output", "surface", "preview", "#FFB866"),
            ],
            [
                new GraphNodeGroup(
                    "terrain-authoring",
                    "Terrain Authoring",
                    new GraphPoint(280, 10),
                    new GraphSize(360, 440),
                    ["gradient", "noise"]),
            ]);

    private static GraphNode CreateNode(
        INodeCatalog catalog,
        string instanceId,
        NodeDefinitionId definitionId,
        GraphPoint position,
        string? groupId = null,
        GraphNodeSurfaceState? surface = null)
    {
        if (!catalog.TryGetDefinition(definitionId, out var definition) || definition is null)
        {
            throw new InvalidOperationException($"Missing demo node definition '{definitionId}'.");
        }

        return new GraphNode(
            instanceId,
            definition.DisplayName,
            definition.Category,
            definition.Subtitle,
            definition.Description ?? string.Empty,
            position,
            new GraphSize(definition.DefaultWidth, definition.DefaultHeight),
            definition.InputPorts.Select(port => CreatePort(port, PortDirection.Input)).ToList(),
            definition.OutputPorts.Select(port => CreatePort(port, PortDirection.Output)).ToList(),
            definition.AccentHex,
            definition.Id,
            definition.Parameters
                .Select(parameter => new GraphParameterValue(parameter.Key, parameter.ValueType, parameter.DefaultValue))
                .ToList(),
            surface ?? new GraphNodeSurfaceState(GraphNodeExpansionState.Collapsed, groupId));
    }

    private static GraphPort CreatePort(PortDefinition definition, PortDirection direction)
        => new(
            definition.Key,
            definition.DisplayName,
            direction,
            definition.TypeId.Value,
            definition.AccentHex,
            definition.TypeId,
            definition.InlineParameterKey);

    private static GraphConnection Connect(
        string sourceNodeId,
        string sourcePortId,
        string targetNodeId,
        string targetPortId,
        string label,
        string accentHex)
        => new(
            $"{sourceNodeId}.{sourcePortId}->{targetNodeId}.{targetPortId}",
            sourceNodeId,
            sourcePortId,
            targetNodeId,
            targetPortId,
            label,
            accentHex);
}
