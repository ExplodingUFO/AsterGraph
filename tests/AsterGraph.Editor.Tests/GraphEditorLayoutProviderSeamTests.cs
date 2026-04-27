using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
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
                    new GraphConnection("connection-001", "source-001", "out", "target-001", "in", "Source To Target", "#6AD5C4"),
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
