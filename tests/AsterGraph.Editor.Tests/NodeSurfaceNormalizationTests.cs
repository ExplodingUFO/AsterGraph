using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.ViewModels;
using System.Linq;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class NodeSurfaceNormalizationTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.node-surface.normalization");

    [Fact]
    public void NodeViewModel_Constructor_NormalizesUndersizedHeightToVisiblePortRows()
    {
        var node = CreateGraphNode(height: 140d, inputCount: 3, outputCount: 0);

        var viewModel = new NodeViewModel(node);

        Assert.Equal(220d, viewModel.Width);
        Assert.Equal(GraphEditorNodeSurfaceMetrics.CalculateRequiredHeight(3, 0), viewModel.Height);
    }

    [Fact]
    public void SessionCommands_AddNode_NormalizesDefinitionDefaultSizeBeforePersisting()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Normalized Node",
            "Tests",
            "Normalization",
            [
                new PortDefinition("input-a", "Input A", new PortTypeId("float"), "#F3B36B"),
                new PortDefinition("input-b", "Input B", new PortTypeId("float"), "#F3B36B"),
                new PortDefinition("input-c", "Input C", new PortTypeId("float"), "#F3B36B"),
            ],
            [],
            defaultWidth: 220d,
            defaultHeight: 140d));

        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument("Normalization Graph", "Covers normalized add-node sizing.", [], []),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        session.Commands.AddNode(DefinitionId, new GraphPoint(320, 180));

        var created = Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes);
        Assert.Equal(220d, created.Size.Width);
        Assert.Equal(GraphEditorNodeSurfaceMetrics.CalculateRequiredHeight(3, 0), created.Size.Height);
    }

    private static GraphNode CreateGraphNode(double height, int inputCount, int outputCount)
        => new(
            "tests.node-surface.normalization-001",
            "Normalized Node",
            "Tests",
            "Normalization",
            "Node surface normalization fixture.",
            new GraphPoint(120, 80),
            new GraphSize(220d, height),
            Enumerable.Range(0, inputCount)
                .Select(index => new GraphPort(
                    $"input-{index}",
                    $"Input {index}",
                    PortDirection.Input,
                    "float",
                    "#F3B36B",
                    new PortTypeId("float")))
                .ToList(),
            Enumerable.Range(0, outputCount)
                .Select(index => new GraphPort(
                    $"output-{index}",
                    $"Output {index}",
                    PortDirection.Output,
                    "float",
                    "#6AD5C4",
                    new PortTypeId("float")))
                .ToList(),
            "#6AD5C4",
            DefinitionId);
}
