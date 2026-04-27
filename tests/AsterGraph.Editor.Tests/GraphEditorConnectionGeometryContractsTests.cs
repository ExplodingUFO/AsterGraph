using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Geometry;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorConnectionGeometryContractsTests
{
    private const string SourceNodeId = "tests.geometry.source-001";
    private const string TargetNodeId = "tests.geometry.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const string TargetParameterKey = "gain";

    [Fact]
    public void IGraphEditorQueries_ExposeConnectionGeometrySnapshots()
    {
        var queriesType = typeof(IGraphEditorQueries);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetConnectionGeometrySnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorConnectionGeometrySnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetConnectionGeometrySnapshots))!.ReturnType);

        Assert.NotNull(typeof(GraphEditorSceneSnapshot).GetProperty(nameof(GraphEditorSceneSnapshot.ConnectionGeometries)));
        Assert.NotNull(typeof(GraphEditorConnectionGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionGeometrySnapshot.ConnectionId)));
        Assert.NotNull(typeof(GraphEditorConnectionGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionGeometrySnapshot.Source)));
        Assert.NotNull(typeof(GraphEditorConnectionGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionGeometrySnapshot.Target)));
        Assert.NotNull(typeof(GraphEditorConnectionGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionGeometrySnapshot.Route)));
        Assert.NotNull(typeof(GraphEditorConnectionEndpointGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionEndpointGeometrySnapshot.NodeId)));
        Assert.NotNull(typeof(GraphEditorConnectionEndpointGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionEndpointGeometrySnapshot.EndpointId)));
        Assert.NotNull(typeof(GraphEditorConnectionEndpointGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionEndpointGeometrySnapshot.EndpointKind)));
        Assert.NotNull(typeof(GraphEditorConnectionEndpointGeometrySnapshot).GetProperty(nameof(GraphEditorConnectionEndpointGeometrySnapshot.Position)));
    }

    [Fact]
    public void SessionQueries_GetConnectionGeometrySnapshots_ProjectPortConnections_IntoSceneContracts()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions());

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        var geometry = Assert.Single(session.Queries.GetConnectionGeometrySnapshots());
        var sceneGeometry = Assert.Single(session.Queries.GetSceneSnapshot().ConnectionGeometries);
        var expectedSource = PortAnchorCalculator.GetAnchor(
            new NodeBounds(120d, 160d, 240d, 160d),
            PortDirection.Output,
            index: 0,
            total: 1);
        var expectedTarget = PortAnchorCalculator.GetAnchor(
            new NodeBounds(520d, 180d, 240d, 160d),
            PortDirection.Input,
            index: 0,
            total: 1);

        Assert.Equal(sceneGeometry, geometry);
        Assert.Equal(SourceNodeId, geometry.Source.NodeId);
        Assert.Equal(SourcePortId, geometry.Source.EndpointId);
        Assert.Equal(GraphConnectionTargetKind.Port, geometry.Source.EndpointKind);
        Assert.Equal(expectedSource, geometry.Source.Position);
        Assert.Equal(TargetNodeId, geometry.Target.NodeId);
        Assert.Equal(TargetPortId, geometry.Target.EndpointId);
        Assert.Equal(GraphConnectionTargetKind.Port, geometry.Target.EndpointKind);
        Assert.Equal(expectedTarget, geometry.Target.Position);
        Assert.Equal(GraphConnectionRoute.Empty, geometry.Route);
    }

    [Fact]
    public void SessionQueries_GetConnectionGeometrySnapshots_ProjectParameterEndpointTargets_UsingVisibleInputRows()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateParameterEndpointOptions());

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(new GraphConnectionTargetRef(TargetNodeId, TargetParameterKey, GraphConnectionTargetKind.Parameter));

        var geometry = Assert.Single(session.Queries.GetConnectionGeometrySnapshots());
        var expectedSource = PortAnchorCalculator.GetAnchor(
            new NodeBounds(120d, 160d, 240d, 160d),
            PortDirection.Output,
            index: 0,
            total: 1);
        var expectedTarget = PortAnchorCalculator.GetAnchor(
            new NodeBounds(520d, 180d, 240d, 160d),
            PortDirection.Input,
            index: 1,
            total: 2);

        Assert.Equal(GraphConnectionTargetKind.Parameter, geometry.Target.EndpointKind);
        Assert.Equal(TargetParameterKey, geometry.Target.EndpointId);
        Assert.Equal(expectedSource, geometry.Source.Position);
        Assert.Equal(expectedTarget, geometry.Target.Position);
        Assert.Equal(GraphConnectionRoute.Empty, geometry.Route);
    }

    [Fact]
    public void SessionQueries_GetConnectionGeometrySnapshots_ProjectPersistedRouteVertices_IntoSceneContracts()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateRoutedOptions());

        var geometry = Assert.Single(session.Queries.GetConnectionGeometrySnapshots());
        var sceneGeometry = Assert.Single(session.Queries.GetSceneSnapshot().ConnectionGeometries);

        Assert.Equal(sceneGeometry, geometry);
        Assert.Equal(
            new GraphConnectionRoute(
            [
                new GraphPoint(360d, 120d),
                new GraphPoint(420d, 300d),
            ]),
            geometry.Route);
        Assert.Equal(
            [new GraphPoint(360d, 120d), new GraphPoint(420d, 300d)],
            geometry.Route.Vertices);
    }

    [Fact]
    public void SessionQueries_GetConnectionGeometrySnapshots_ReusesCacheUntilDocumentChanges()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateRoutedOptions());

        var first = session.Queries.GetConnectionGeometrySnapshots();
        var second = session.Queries.GetSceneSnapshot().ConnectionGeometries;
        session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        var afterSelectionOnly = session.Queries.GetConnectionGeometrySnapshots();

        Assert.Same(first, second);
        Assert.Same(first, afterSelectionOnly);

        var moved = session.Commands.TryMoveConnectionRouteVertex(
            "connection-route-001",
            vertexIndex: 0,
            new GraphPoint(380d, 140d),
            updateStatus: false);
        var afterRouteMove = session.Queries.GetConnectionGeometrySnapshots();

        Assert.True(moved);
        Assert.NotSame(first, afterRouteMove);
        Assert.Equal(new GraphPoint(380d, 140d), Assert.Single(afterRouteMove).Route.Vertices[0]);
    }

    [Fact]
    public void SessionFeatureDescriptors_AdvertiseConnectionGeometrySnapshots()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions());

        var descriptor = Assert.Single(
            session.Queries.GetFeatureDescriptors(),
            item => string.Equals(item.Id, "query.connection-geometry-snapshots", StringComparison.Ordinal));

        Assert.Equal("query", descriptor.Category);
        Assert.True(descriptor.IsAvailable);
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
        => Assert.NotNull(declaringType.GetMethod(methodName, parameterTypes));

    private static AsterGraphEditorOptions CreateOptions()
    {
        var definitionId = new NodeDefinitionId("tests.geometry.contracts");
        return new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };
    }

    private static AsterGraphEditorOptions CreateParameterEndpointOptions()
    {
        var definitionId = new NodeDefinitionId("tests.geometry.parameter-targets");
        return new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateParameterEndpointCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };
    }

    private static AsterGraphEditorOptions CreateRoutedOptions()
    {
        var definitionId = new NodeDefinitionId("tests.geometry.routed");
        return new AsterGraphEditorOptions
        {
            Document = CreateDocument(
                definitionId,
                [
                    new GraphConnection(
                        "connection-route-001",
                        SourceNodeId,
                        SourcePortId,
                        TargetNodeId,
                        TargetPortId,
                        "Routed Flow",
                        "#6AD5C4",
                        Presentation: new GraphEdgePresentation(
                            Route: new GraphConnectionRoute(
                            [
                                new GraphPoint(360d, 120d),
                                new GraphPoint(420d, 300d),
                            ]))),
                ]),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };
    }

    private static GraphDocument CreateDocument(
        NodeDefinitionId definitionId,
        IReadOnlyList<GraphConnection>? connections = null)
        => new(
            "Geometry Graph",
            "Runtime connection geometry coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Geometry",
                    "Source node for connection geometry tests.",
                    new GraphPoint(120d, 160d),
                    new GraphSize(240d, 160d),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Geometry",
                    "Target node for connection geometry tests.",
                    new GraphPoint(520d, 180d),
                    new GraphSize(240d, 160d),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    definitionId),
            ],
            connections ?? []);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Geometry Node",
            "Tests",
            "Runtime connection geometry coverage.",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private static NodeCatalog CreateParameterEndpointCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Geometry Node",
            "Tests",
            "Runtime connection geometry coverage.",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")],
            [
                new NodeParameterDefinition(
                    TargetParameterKey,
                    "Gain",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 1.0d),
            ]));
        return catalog;
    }
}
