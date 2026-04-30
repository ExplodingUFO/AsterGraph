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

public sealed class GraphEditorLayoutProviderSeamTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.layout.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.layout.target");

    [Fact]
    public void Queries_ReturnEmptyLayoutPlanWhenHostDoesNotProvideProvider()
    {
        var session = CreateSession(null);
        var request = new GraphLayoutRequest
        {
            Mode = GraphLayoutRequestMode.Selection,
            SelectedNodeIds = ["source-001", "target-001"],
        };

        var plan = session.Queries.CreateLayoutPlan(request);

        Assert.False(plan.IsAvailable);
        Assert.Same(request, plan.Request);
        Assert.Empty(plan.NodePositions);
        Assert.Equal("No layout provider is configured.", plan.EmptyReason);
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "query.layout-plan" && descriptor.IsAvailable);
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "integration.layout-provider" && !descriptor.IsAvailable);
    }

    [Fact]
    public void Queries_ReturnHostOwnedLayoutPlanWithoutMutatingDocument()
    {
        var provider = new TestLayoutProvider(request => new GraphLayoutPlan(
            true,
            request,
            [
                new GraphLayoutNodePosition("source-001", new GraphPoint(100, 80)),
                new GraphLayoutNodePosition("target-001", new GraphPoint(420, 80)),
            ],
            ResetManualRoutes: true));
        var session = CreateSession(provider);
        var before = session.Queries.CreateDocumentSnapshot();
        var request = new GraphLayoutRequest
        {
            Mode = GraphLayoutRequestMode.All,
            Orientation = GraphLayoutOrientation.LeftToRight,
            HorizontalSpacing = 320,
            VerticalSpacing = 160,
            PinnedNodeIds = ["source-001"],
        };

        var plan = session.Queries.CreateLayoutPlan(request);
        var after = session.Queries.CreateDocumentSnapshot();

        Assert.True(plan.IsAvailable);
        Assert.Same(request, plan.Request);
        Assert.True(plan.ResetManualRoutes);
        Assert.Contains(plan.NodePositions, node => node.NodeId == "target-001" && node.Position == new GraphPoint(420, 80));
        Assert.Equal(before.Nodes.Select(node => node.Position), after.Nodes.Select(node => node.Position));
        Assert.Contains(
            session.Queries.GetFeatureDescriptors(),
            descriptor => descriptor.Id == "integration.layout-provider" && descriptor.IsAvailable);
    }

    [Fact]
    public void Commands_ApplyLayoutPlanMutatesThroughOneUndoableOperation()
    {
        var session = CreateSession(null);
        var before = session.Queries.CreateDocumentSnapshot();
        var plan = new GraphLayoutPlan(
            true,
            new GraphLayoutRequest { Mode = GraphLayoutRequestMode.All },
            [
                new GraphLayoutNodePosition("source-001", new GraphPoint(96, 64)),
                new GraphLayoutNodePosition("target-001", new GraphPoint(384, 64)),
            ],
            ResetManualRoutes: true);

        Assert.True(session.Commands.TryApplyLayoutPlan(plan));

        var applied = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(new GraphPoint(96, 64), applied.Nodes.Single(node => node.Id == "source-001").Position);
        Assert.Equal(new GraphPoint(384, 64), applied.Nodes.Single(node => node.Id == "target-001").Position);
        Assert.Null(Assert.Single(applied.Connections).Presentation?.Route);
        Assert.True(session.Queries.GetCapabilitySnapshot().CanUndo);

        session.Commands.Undo();

        var undone = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(before.Nodes.Select(node => node.Position), undone.Nodes.Select(node => node.Position));
        Assert.NotNull(Assert.Single(undone.Connections).Presentation?.Route);
    }

    [Fact]
    public void Commands_ApplyLayoutRequestPreviewsThenAppliesProviderPlan()
    {
        var provider = new TestLayoutProvider(request => new GraphLayoutPlan(
            true,
            request,
            [
                new GraphLayoutNodePosition("target-001", new GraphPoint(512, 192)),
            ]));
        var session = CreateSession(provider);
        var request = new GraphLayoutRequest
        {
            Mode = GraphLayoutRequestMode.Selection,
            SelectedNodeIds = ["target-001"],
        };

        Assert.True(session.Commands.TryApplyLayoutRequest(request));

        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(new GraphPoint(512, 192), document.Nodes.Single(node => node.Id == "target-001").Position);
    }

    [Fact]
    public void Commands_SelectionAlignAndDistributeRemainSupported()
    {
        var session = CreateSession(null);
        session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
            "nodes.move",
            [
                new GraphEditorCommandArgumentSnapshot("position", "source-001|120|120"),
                new GraphEditorCommandArgumentSnapshot("position", "target-001|430|200"),
            ]));
        session.Commands.AddNode(TargetDefinitionId, new GraphPoint(760, 280));
        var thirdNodeId = session.Queries.CreateDocumentSnapshot()
            .Nodes
            .Single(node => node.Id is not "source-001" and not "target-001")
            .Id;
        session.Commands.SetSelection(["source-001", "target-001", thirdNodeId], "source-001", updateStatus: false);

        Assert.True(session.Commands.TryApplySelectionLayout(GraphSelectionLayoutOperation.AlignTop));

        var aligned = session.Queries.CreateDocumentSnapshot().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(120, aligned["source-001"].Position.Y);
        Assert.Equal(120, aligned["target-001"].Position.Y);
        Assert.Equal(120, aligned[thirdNodeId].Position.Y);

        Assert.True(session.Commands.TryApplySelectionLayout(GraphSelectionLayoutOperation.DistributeHorizontally));

        var distributed = session.Queries.CreateDocumentSnapshot().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var centers = new[]
        {
            distributed["source-001"].Position.X + distributed["source-001"].Size.Width / 2,
            distributed["target-001"].Position.X + distributed["target-001"].Size.Width / 2,
            distributed[thirdNodeId].Position.X + distributed[thirdNodeId].Size.Width / 2,
        }.Order().ToArray();
        Assert.Equal(centers[1] - centers[0], centers[2] - centers[1], precision: 6);
    }

    [Fact]
    public void Commands_SnapSelectionAndAllNodesToGridDeterministically()
    {
        var session = CreateSession(null);
        session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot("source-001", new GraphPoint(13, 37)),
                new NodePositionSnapshot("target-001", new GraphPoint(51, 69)),
            ],
            updateStatus: false);
        session.Commands.SetSelection(["source-001"], "source-001", updateStatus: false);

        Assert.True(session.Commands.TrySnapSelectedNodesToGrid(20));

        var selectedSnap = session.Queries.CreateDocumentSnapshot().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(20, 40), selectedSnap["source-001"].Position);
        Assert.Equal(new GraphPoint(51, 69), selectedSnap["target-001"].Position);

        Assert.True(session.Commands.TrySnapAllNodesToGrid(20));

        var allSnap = session.Queries.CreateDocumentSnapshot().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(20, 40), allSnap["source-001"].Position);
        Assert.Equal(new GraphPoint(60, 60), allSnap["target-001"].Position);
    }

    [Fact]
    public void Commands_RouteLayoutApplyPlanThroughStableCommandSurface()
    {
        var session = CreateSession(null);

        Assert.True(session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
            "layout.apply-plan",
            [
                new GraphEditorCommandArgumentSnapshot("position", "source-001|160|96"),
                new GraphEditorCommandArgumentSnapshot("position", "target-001|480|96"),
                new GraphEditorCommandArgumentSnapshot("resetManualRoutes", "true"),
            ])));

        var document = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(new GraphPoint(160, 96), document.Nodes.Single(node => node.Id == "source-001").Position);
        Assert.Equal(new GraphPoint(480, 96), document.Nodes.Single(node => node.Id == "target-001").Position);
        Assert.Null(Assert.Single(document.Connections).Presentation?.Route);
    }

    private static IGraphEditorSession CreateSession(IGraphLayoutProvider? layoutProvider)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Layout",
            [],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Layout",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Layout Provider",
                "Layout provider proof.",
                [
                    new GraphNode(
                        "source-001",
                        "Source",
                        "Tests",
                        "Layout",
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
                        "Layout",
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
                        "#6AD5C4",
                        Presentation: new GraphEdgePresentation(Route: new GraphConnectionRoute([new GraphPoint(300, 180)]))),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            LayoutProvider = layoutProvider,
        });
    }

    private sealed class TestLayoutProvider(Func<GraphLayoutRequest, GraphLayoutPlan> createPlan) : IGraphLayoutProvider
    {
        public GraphLayoutPlan CreateLayoutPlan(GraphLayoutRequest request)
            => createPlan(request);
    }
}
