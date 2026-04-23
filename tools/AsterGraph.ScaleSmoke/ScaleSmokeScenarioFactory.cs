using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;

namespace AsterGraph.ScaleSmoke;

public static class ScaleSmokeScenarioFactory
{
    private const string InputPortId = "in";
    private const string OutputPortId = "out";

    public static NodeCatalog CreateCatalog(NodeDefinitionId primaryDefinitionId)
    {
        ArgumentNullException.ThrowIfNull(primaryDefinitionId);

        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(CreateDefinition(
            primaryDefinitionId,
            "Scale Node",
            "Scale",
            "Repeatable larger-graph validation node.",
            "#6AD5C4"));
        catalog.RegisterDefinition(CreateDefinition(
            new NodeDefinitionId("scale.authoring.node"),
            "Authoring Probe Node",
            "Authoring",
            "Template used to validate stencil filtering and command surface projection.",
            "#7BC9FF"));
        catalog.RegisterDefinition(CreateDefinition(
            new NodeDefinitionId("scale.plugins.audit"),
            "Plugin Audit Node",
            "Plugins",
            "Template used to validate widened host-owned authoring surfaces.",
            "#E88973"));
        return catalog;
    }

    public static GraphDocument CreateDocument(ScaleSmokeTier tier, NodeDefinitionId primaryDefinitionId)
    {
        ArgumentNullException.ThrowIfNull(tier);
        ArgumentNullException.ThrowIfNull(primaryDefinitionId);

        var nodes = new List<GraphNode>(tier.NodeCount);
        var connections = new List<GraphConnection>(tier.NodeCount - 1);

        for (var index = 0; index < tier.NodeCount; index++)
        {
            var nodeId = $"scale-node-{index:000}";
            nodes.Add(new GraphNode(
                nodeId,
                $"Scale Node {index:000}",
                "Scale",
                "Large Graph",
                "Used to validate repeatable scale, history, and reload scenarios.",
                new GraphPoint(120 + ((index % 12) * 280), 120 + ((index / 12) * 180)),
                new GraphSize(220, 160),
                [
                    new GraphPort(InputPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")),
                ],
                [
                    new GraphPort(OutputPortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")),
                ],
                index % 2 == 0 ? "#6AD5C4" : "#F3B36B",
                primaryDefinitionId));

            if (index == 0)
            {
                continue;
            }

            connections.Add(new GraphConnection(
                $"scale-connection-{index - 1:000}",
                $"scale-node-{index - 1:000}",
                OutputPortId,
                nodeId,
                InputPortId,
                $"Scale Edge {index - 1:000}->{index:000}",
                "#6AD5C4"));
        }

        return new GraphDocument(
            $"Scale Smoke Graph ({tier.Id})",
            "Repeatable larger-graph validation scenario for the public alpha proof ring.",
            nodes,
            connections);
    }

    private static NodeDefinition CreateDefinition(
        NodeDefinitionId definitionId,
        string displayName,
        string category,
        string description,
        string accentHex)
        => new(
            definitionId,
            displayName,
            category,
            "Scale Smoke",
            [new PortDefinition(InputPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(OutputPortId, "Output", new PortTypeId("float"), "#6AD5C4")],
            description: description,
            accentHex: accentHex);
}
