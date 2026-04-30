using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSnapGuideProjectionTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.snap.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.snap.target");

    [Fact]
    public void Queries_ProjectSelectedNodeGridSnapGuides()
    {
        var session = CreateSession();
        session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot("source-001", new GraphPoint(13, 37)),
                new NodePositionSnapshot("target-001", new GraphPoint(51, 69)),
            ],
            updateStatus: false);
        session.Commands.SetSelection(["source-001"], "source-001", updateStatus: false);

        var snapshot = session.Queries.GetSnapGuideSnapshot(new GraphEditorSnapGuideQuery(20));

        Assert.True(snapshot.IsAvailable);
        Assert.Equal(20, snapshot.GridSize);
        Assert.True(snapshot.SelectionOnly);
        var item = Assert.Single(snapshot.Items);
        Assert.Equal("source-001", item.NodeId);
        Assert.Equal(new GraphPoint(13, 37), item.CurrentPosition);
        Assert.Equal(new GraphPoint(20, 40), item.SnappedPosition);
        Assert.Equal(new GraphPoint(7, 3), item.Offset);
        Assert.Null(snapshot.EmptyReason);
    }

    [Fact]
    public void Queries_ProjectAllNodeSnapGuidesWhenSelectionOnlyIsFalse()
    {
        var session = CreateSession();
        session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot("source-001", new GraphPoint(13, 37)),
                new NodePositionSnapshot("target-001", new GraphPoint(51, 69)),
            ],
            updateStatus: false);

        var snapshot = session.Queries.GetSnapGuideSnapshot(new GraphEditorSnapGuideQuery(20, SelectionOnly: false));

        Assert.True(snapshot.IsAvailable);
        Assert.Equal(["source-001", "target-001"], snapshot.Items.Select(item => item.NodeId));
        Assert.Contains(snapshot.Items, item => item.NodeId == "target-001" && item.SnappedPosition == new GraphPoint(60, 60));
    }

    [Fact]
    public void Queries_RejectInvalidOrEmptySnapGuideRequests()
    {
        var session = CreateSession();

        var invalidGrid = session.Queries.GetSnapGuideSnapshot(new GraphEditorSnapGuideQuery(0));
        Assert.False(invalidGrid.IsAvailable);
        Assert.Equal("Grid size must be positive.", invalidGrid.EmptyReason);

        var emptySelection = session.Queries.GetSnapGuideSnapshot();
        Assert.False(emptySelection.IsAvailable);
        Assert.Equal("Select one or more nodes before projecting snap guides.", emptySelection.EmptyReason);
    }

    private static IGraphEditorSession CreateSession()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Snap",
            [],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Snap",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Snap Guides",
                "Snap guide proof.",
                [
                    new GraphNode(
                        "source-001",
                        "Source",
                        "Tests",
                        "Snap",
                        "Source node.",
                        new GraphPoint(120, 120),
                        new GraphSize(220, 120),
                        [],
                        [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        "#6AD5C4",
                        SourceDefinitionId),
                    new GraphNode(
                        "target-001",
                        "Target",
                        "Tests",
                        "Snap",
                        "Target node.",
                        new GraphPoint(420, 120),
                        new GraphSize(220, 120),
                        [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
                        [],
                        "#8B7BFF",
                        TargetDefinitionId),
                ],
                [
                    new GraphConnection(
                        "connection-001",
                        "source-001",
                        "out",
                        "target-001",
                        "in",
                        "Source To Target",
                        "#6AD5C4"),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }
}
