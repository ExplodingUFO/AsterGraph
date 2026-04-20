using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorNodeSurfaceContractsTests
{
    private static readonly NodeDefinitionId TieredDefinitionId = new("tests.node-surface.tiered");
    private static readonly NodeDefinitionId DefaultDefinitionId = new("tests.node-surface.default");
    private static readonly NodeDefinitionId AdaptiveDefinitionId = new("tests.node-surface.adaptive");
    private const string NodeId = "tests.node-surface.node-001";
    private const string SiblingNodeId = "tests.node-surface.node-002";
    private const string OutputPortId = "output-001";
    private const string InputPortId = "input-001";

    [Fact]
    public void RuntimeContracts_ExposeNodeSurfaceQueriesAndCommands()
    {
        var queriesType = typeof(IGraphEditorQueries);
        var commandsType = typeof(IGraphEditorCommands);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodeSurfaceSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorNodeSurfaceSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodeSurfaceSnapshots))!.ReturnType);

        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.NodeId)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.Size)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.ActiveTier)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.ExpansionState)));
        Assert.NotNull(typeof(GraphEditorNodeSurfaceSnapshot).GetProperty(nameof(GraphEditorNodeSurfaceSnapshot.GroupId)));
        Assert.NotNull(typeof(NodeViewModel).GetProperty("ExpansionState"));
        Assert.NotNull(typeof(NodeViewModel).GetProperty("IsExpanded"));

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeWidth), typeof(string), typeof(double), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeWidth), [typeof(string), typeof(double), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeSize), typeof(string), typeof(GraphSize), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeSize), [typeof(string), typeof(GraphSize), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeExpansionState), typeof(string), typeof(GraphNodeExpansionState));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeExpansionState), [typeof(string), typeof(GraphNodeExpansionState)])!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodeGroups));
        Assert.Equal(
            typeof(IReadOnlyList<GraphNodeGroup>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodeGroups))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompositeNodeSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorCompositeNodeSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompositeNodeSnapshots))!.ReturnType);
        Assert.NotNull(typeof(GraphEditorCompositeNodeSnapshot).GetProperty(nameof(GraphEditorCompositeNodeSnapshot.NodeId)));
        Assert.NotNull(typeof(GraphEditorCompositeNodeSnapshot).GetProperty(nameof(GraphEditorCompositeNodeSnapshot.ChildGraphId)));
        Assert.NotNull(typeof(GraphEditorCompositeNodeSnapshot).GetProperty(nameof(GraphEditorCompositeNodeSnapshot.Inputs)));
        Assert.NotNull(typeof(GraphEditorCompositeNodeSnapshot).GetProperty(nameof(GraphEditorCompositeNodeSnapshot.Outputs)));

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodeGroupSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorNodeGroupSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodeGroupSnapshots))!.ReturnType);

        Assert.NotNull(typeof(GraphEditorNodeGroupSnapshot).GetProperty(nameof(GraphEditorNodeGroupSnapshot.Position)));
        Assert.NotNull(typeof(GraphEditorNodeGroupSnapshot).GetProperty(nameof(GraphEditorNodeGroupSnapshot.Size)));
        Assert.NotNull(typeof(GraphEditorNodeGroupSnapshot).GetProperty(nameof(GraphEditorNodeGroupSnapshot.ContentPosition)));
        Assert.NotNull(typeof(GraphEditorNodeGroupSnapshot).GetProperty(nameof(GraphEditorNodeGroupSnapshot.ContentSize)));

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryCreateNodeGroupFromSelection), typeof(string));
        Assert.Equal(
            typeof(string),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryCreateNodeGroupFromSelection), [typeof(string)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeGroupCollapsed), typeof(string), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeGroupCollapsed), [typeof(string), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeGroupPosition), typeof(string), typeof(GraphPoint), typeof(bool), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeGroupPosition), [typeof(string), typeof(GraphPoint), typeof(bool), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeGroupSize), typeof(string), typeof(GraphSize), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeGroupSize), [typeof(string), typeof(GraphSize), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetNodeGroupMemberships), typeof(IReadOnlyList<GraphEditorNodeGroupMembershipChange>), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetNodeGroupMemberships), [typeof(IReadOnlyList<GraphEditorNodeGroupMembershipChange>), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryPromoteNodeGroupToComposite), typeof(string), typeof(string), typeof(bool));
        Assert.Equal(
            typeof(string),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryPromoteNodeGroupToComposite), [typeof(string), typeof(string), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryExposeCompositePort), typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool));
        Assert.Equal(
            typeof(string),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryExposeCompositePort), [typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool)])!.ReturnType);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryUnexposeCompositePort), typeof(string), typeof(string), typeof(bool));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryUnexposeCompositePort), [typeof(string), typeof(string), typeof(bool)])!.ReturnType);
    }

    [Fact]
    public void PortDefinition_LegacyInlineParameterKeyContract_RemainsAvailable()
    {
        var port = new PortDefinition(
            "input",
            "Input",
            new PortTypeId("float"),
            "#F3B36B",
            "Legacy compatibility check",
            " gain ");

        Assert.Equal("gain", port.InlineParameterKey);
        Assert.Equal("gain", new PortViewModel(new GraphPort("input", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"), "gain"), 0, 1).InlineParameterKey);
    }

    [Fact]
    public void GraphEditorNodeSurfaceSnapshot_LegacyPositionalShape_RemainsAvailable()
    {
        var snapshot = new GraphEditorNodeSurfaceSnapshot(
            NodeId,
            new GraphSize(240d, 160d),
            GraphNodeExpansionState.Collapsed,
            "group-001");

        var (nodeId, size, expansionState, groupId) = snapshot;

        Assert.Equal(NodeId, nodeId);
        Assert.Equal(new GraphSize(240d, 160d), size);
        Assert.Equal(GraphNodeExpansionState.Collapsed, expansionState);
        Assert.Equal("group-001", groupId);
        Assert.False(string.IsNullOrWhiteSpace(snapshot.ActiveTier.Key));

        var explicitTier = new GraphEditorNodeSurfaceTierSnapshot(
            "parameter-editors",
            420d,
            250d,
            [NodeSurfaceSectionKeys.Description, NodeSurfaceSectionKeys.ParameterRail, NodeSurfaceSectionKeys.ParameterEditors]);
        var snapshotWithTier = new GraphEditorNodeSurfaceSnapshot(
            NodeId,
            new GraphSize(430d, 260d),
            explicitTier,
            GraphNodeExpansionState.Collapsed,
            "group-002");

        Assert.Equal("parameter-editors", snapshotWithTier.ActiveTier.Key);
    }

    [Fact]
    public void GraphNodeVisualContext_LegacyPresenterConstructor_RemainsAvailable()
    {
        var constructor = typeof(GraphNodeVisualContext).GetConstructor(
        [
            typeof(GraphEditorViewModel),
            typeof(NodeViewModel),
            typeof(AsterGraph.Abstractions.Styling.GraphEditorStyleOptions),
            typeof(Action),
            typeof(Action<NodeViewModel, PointerPressedEventArgs>),
            typeof(Func<NodeViewModel, double, bool, bool>),
            typeof(Func<NodeViewModel, GraphNodeExpansionState, bool>),
            typeof(Func<NodeViewModel, PortViewModel, bool>),
            typeof(Func<NodeViewModel, PortViewModel, NodeParameterViewModel?>),
            typeof(Action<NodeViewModel, PortViewModel>),
            typeof(Func<Control, NodeViewModel, ContextRequestedEventArgs, bool>),
            typeof(Func<Control, NodeViewModel, PortViewModel, ContextRequestedEventArgs, bool>),
        ]);

        Assert.NotNull(constructor);
        Assert.NotNull(typeof(GraphNodeVisualContext).GetProperty(nameof(GraphNodeVisualContext.ResolveInlineParameter)));
    }

    [Fact]
    public void AdaptiveSurfaceMeasurement_UsesRequiredParametersForBaselineAndOptionalParametersForDisclosure()
    {
        var definition = CreateAdaptiveDefinition();

        var plan = GraphEditorNodeSurfacePlanner.Create(definition);
        var measurement = GraphEditorNodeSurfaceMeasurer.Measure(plan);

        Assert.Equal(1, measurement.RequiredParameterCount);
        Assert.Equal(1, measurement.OptionalParameterCount);
        Assert.True(measurement.BaselineSize.Height < measurement.HeightToRevealAdditionalInputs);
        Assert.True(measurement.BaselineSize.Width < measurement.WidthToRevealParameterSummaries);
        Assert.True(measurement.WidthToRevealParameterSummaries < measurement.WidthToRevealInlineEditors);
    }

    [Fact]
    public void AdaptiveSurfaceTierResolver_UsesMeasuredThresholdsForDefaultProfile()
    {
        var definition = CreateAdaptiveDefinition();
        var measurement = GraphEditorNodeSurfaceMeasurer.Measure(GraphEditorNodeSurfacePlanner.Create(definition));

        var baselineTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            measurement.BaselineSize,
            GraphEditorBehaviorOptions.Default,
            definition,
            measurement);
        var summaryTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new GraphSize(measurement.WidthToRevealParameterSummaries, measurement.HeightToRevealAdditionalInputs),
            GraphEditorBehaviorOptions.Default,
            definition,
            measurement);
        var editorTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new GraphSize(measurement.WidthToRevealInlineEditors, measurement.HeightToRevealAdditionalInputs),
            GraphEditorBehaviorOptions.Default,
            definition,
            measurement);

        Assert.Equal("details", baselineTier.Key);
        Assert.Equal("parameter-rail", summaryTier.Key);
        Assert.Equal("parameter-editors", editorTier.Key);
    }

    [Fact]
    public void AdaptiveSurfaceTierResolver_RevealsDescriptionOnlyAtEditorTierByDefault()
    {
        var definition = CreateAdaptiveDefinition();
        var measurement = GraphEditorNodeSurfaceMeasurer.Measure(GraphEditorNodeSurfacePlanner.Create(definition));

        var baselineTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            measurement.BaselineSize,
            GraphEditorBehaviorOptions.Default,
            definition,
            measurement);
        var summaryTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new GraphSize(measurement.WidthToRevealParameterSummaries, measurement.HeightToRevealAdditionalInputs),
            GraphEditorBehaviorOptions.Default,
            definition,
            measurement);
        var editorTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(
            new GraphSize(measurement.WidthToRevealInlineEditors, measurement.HeightToRevealAdditionalInputs),
            GraphEditorBehaviorOptions.Default,
            definition,
            measurement);

        Assert.False(baselineTier.ShowsSection(NodeSurfaceSectionKeys.Description));
        Assert.False(summaryTier.ShowsSection(NodeSurfaceSectionKeys.Description));
        Assert.True(editorTier.ShowsSection(NodeSurfaceSectionKeys.Description));
    }

    [Fact]
    public void SessionCommands_TrySetNodeWidth_PersistsUndoableSurfaceMutation()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var changed = session.Commands.TrySetNodeWidth(NodeId, 360d, updateStatus: false);

        Assert.True(changed);
        Assert.Contains("nodes.resize-width", commandIds);

        var node = Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, node => node.Id == NodeId);
        Assert.Equal(360d, node.Size.Width);

        var surface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == NodeId);
        Assert.Equal(360d, surface.Size.Width);

        session.Commands.Undo();
        Assert.Equal(240d, Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, node => node.Id == NodeId).Size.Width);

        session.Commands.Redo();
        Assert.Equal(360d, Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, node => node.Id == NodeId).Size.Width);
    }

    [Fact]
    public void SessionCommands_TrySetNodeSize_PersistsUndoableTierDrivenMutation()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        var changed = session.Commands.TrySetNodeSize(NodeId, new GraphSize(420d, 260d), updateStatus: false);

        Assert.True(changed);
        Assert.Contains("nodes.resize", commandIds);

        var node = Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, candidate => candidate.Id == NodeId);
        Assert.Equal(new GraphSize(420d, 260d), node.Size);

        var surface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == NodeId);
        Assert.Equal(new GraphSize(420d, 260d), surface.Size);
        Assert.Equal("parameter-editors", surface.ActiveTier.Key);

        session.Commands.Undo();
        Assert.Equal(new GraphSize(240d, 160d), Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, candidate => candidate.Id == NodeId).Size);

        session.Commands.Redo();
        Assert.Equal(new GraphSize(420d, 260d), Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes, candidate => candidate.Id == NodeId).Size);
    }

    [Fact]
    public void SessionCommands_TrySetNodeExpansionState_PersistsAcrossSerialization()
    {
        var session = CreateSession();

        var changed = session.Commands.TrySetNodeExpansionState(NodeId, GraphNodeExpansionState.Expanded);

        Assert.True(changed);

        var surface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == NodeId);
        Assert.Equal(GraphNodeExpansionState.Expanded, surface.ExpansionState);

        var document = session.Queries.CreateDocumentSnapshot();
        var json = GraphDocumentSerializer.Serialize(document);
        var restored = GraphDocumentSerializer.Deserialize(json);
        var restoredNode = Assert.Single(restored.Nodes, node => node.Id == NodeId);

        Assert.NotNull(restoredNode.Surface);
        Assert.Equal(GraphNodeExpansionState.Expanded, restoredNode.Surface!.ExpansionState);
    }

    [Fact]
    public void SessionQueries_GetNodeSurfaceSnapshots_ResolveActiveTier_FromBehaviorDefaults_AndDefinitionOverrides()
    {
        var session = CreateSession();

        var surfaces = session.Queries.GetNodeSurfaceSnapshots();
        var overrideSurface = Assert.Single(surfaces, snapshot => snapshot.NodeId == NodeId);
        var defaultSurface = Assert.Single(surfaces, snapshot => snapshot.NodeId == SiblingNodeId);

        Assert.Equal("details", overrideSurface.ActiveTier.Key);
        Assert.Equal("project-parameter-rail", defaultSurface.ActiveTier.Key);

        Assert.True(session.Commands.TrySetNodeSize(SiblingNodeId, new GraphSize(420d, 260d), updateStatus: false));
        defaultSurface = Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), snapshot => snapshot.NodeId == SiblingNodeId);
        Assert.Equal("project-parameter-editors", defaultSurface.ActiveTier.Key);
    }

    [Fact]
    public void SessionCommands_NodeGroups_ExposeFixedFrames_AndIgnoreMemberGeometryChanges()
    {
        var session = CreateSession();

        session.Commands.SetSelection([NodeId, SiblingNodeId], SiblingNodeId, updateStatus: false);
        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Surface Cluster");

        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var createdGroup = Assert.Single(session.Queries.GetNodeGroups());
        var createdSnapshot = Assert.Single(session.Queries.GetNodeGroupSnapshots());
        Assert.Equal(groupId, createdGroup.Id);
        Assert.Equal("Surface Cluster", createdGroup.Title);
        Assert.Equal(
            [NodeId, SiblingNodeId],
            createdGroup.NodeIds.OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(createdGroup.Position, createdSnapshot.Position);
        Assert.Equal(createdGroup.Size, createdSnapshot.Size);
        Assert.True(createdSnapshot.ContentPosition.X >= createdGroup.Position.X);
        Assert.True(createdSnapshot.ContentPosition.Y >= createdGroup.Position.Y);
        Assert.True(createdSnapshot.ContentSize.Width > 0d);
        Assert.True(createdSnapshot.ContentSize.Height > 0d);
        Assert.True(createdSnapshot.ContentPosition.X + createdSnapshot.ContentSize.Width <= createdGroup.Position.X + createdGroup.Size.Width);
        Assert.True(createdSnapshot.ContentPosition.Y + createdSnapshot.ContentSize.Height <= createdGroup.Position.Y + createdGroup.Size.Height);

        var initialFramePosition = createdSnapshot.Position;
        var initialFrameSize = createdSnapshot.Size;
        var initialContentPosition = createdSnapshot.ContentPosition;
        var initialContentSize = createdSnapshot.ContentSize;

        Assert.True(session.Commands.TrySetNodeExpansionState(NodeId, GraphNodeExpansionState.Expanded));
        var expandedSnapshot = Assert.Single(session.Queries.GetNodeGroupSnapshots());
        Assert.Equal(initialFramePosition, expandedSnapshot.Position);
        Assert.Equal(initialFrameSize, expandedSnapshot.Size);
        Assert.Equal(initialContentPosition, expandedSnapshot.ContentPosition);
        Assert.Equal(initialContentSize, expandedSnapshot.ContentSize);

        Assert.True(session.Commands.TrySetNodeWidth(SiblingNodeId, 320d, updateStatus: false));
        var resizedSnapshot = Assert.Single(session.Queries.GetNodeGroupSnapshots());
        Assert.Equal(initialFramePosition, resizedSnapshot.Position);
        Assert.Equal(initialFrameSize, resizedSnapshot.Size);

        Assert.True(session.Commands.TrySetNodeGroupSize(groupId!, new GraphSize(640d, 300d), updateStatus: false));
        var resizedGroup = Assert.Single(session.Queries.GetNodeGroups());
        var resizedFrameSnapshot = Assert.Single(session.Queries.GetNodeGroupSnapshots());
        Assert.Equal(new GraphSize(640d, 300d), resizedGroup.Size);
        Assert.Equal(new GraphSize(640d, 300d), resizedFrameSnapshot.Size);
        Assert.Equal(
            initialContentPosition.X - initialFramePosition.X,
            resizedFrameSnapshot.ContentPosition.X - resizedGroup.Position.X);
        Assert.Equal(
            initialContentPosition.Y - initialFramePosition.Y,
            resizedFrameSnapshot.ContentPosition.Y - resizedGroup.Position.Y);
        Assert.Equal(
            initialContentSize.Width + (640d - initialFrameSize.Width),
            resizedFrameSnapshot.ContentSize.Width);
        Assert.Equal(
            initialContentSize.Height + (300d - initialFrameSize.Height),
            resizedFrameSnapshot.ContentSize.Height);

        Assert.True(session.Commands.TrySetNodeGroupCollapsed(groupId!, isCollapsed: true));
        Assert.True(session.Commands.TrySetNodeGroupPosition(groupId, new GraphPoint(200, 120), moveMemberNodes: true, updateStatus: false));

        var movedGroup = Assert.Single(session.Queries.GetNodeGroups());
        var movedSnapshot = Assert.Single(session.Queries.GetNodeGroupSnapshots());
        Assert.True(movedGroup.IsCollapsed);
        Assert.Equal(new GraphPoint(200, 120), movedGroup.Position);
        Assert.Equal(movedGroup.Position, movedSnapshot.Position);
        Assert.Equal(movedGroup.Size, movedSnapshot.Size);

        var json = GraphDocumentSerializer.Serialize(session.Queries.CreateDocumentSnapshot());
        var restored = GraphDocumentSerializer.Deserialize(json);
        var restoredGroup = Assert.Single(restored.Groups!);
        Assert.Equal(groupId, restoredGroup.Id);
        Assert.True(restoredGroup.IsCollapsed);
        Assert.Equal(new GraphPoint(200, 120), restoredGroup.Position);
        Assert.Equal(new GraphSize(640d, 300d), restoredGroup.Size);
    }

    [Fact]
    public void SessionCommands_NodeGroups_PreserveEmptyGroupsAcrossMembershipMutations()
    {
        var session = CreateSession();

        session.Commands.SetSelection([NodeId], NodeId, updateStatus: false);
        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Surface Cluster");

        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(
            session.Commands.TrySetNodeGroupMemberships(
                [
                    new GraphEditorNodeGroupMembershipChange(NodeId, null),
                ],
                updateStatus: false));

        var detachedGroup = Assert.Single(session.Queries.GetNodeGroups());
        Assert.Equal(groupId, detachedGroup.Id);
        Assert.Empty(detachedGroup.NodeIds);
        Assert.All(session.Queries.CreateDocumentSnapshot().Nodes, node => Assert.Null(node.Surface?.GroupId));

        var json = GraphDocumentSerializer.Serialize(session.Queries.CreateDocumentSnapshot());
        var restored = GraphDocumentSerializer.Deserialize(json);
        var restoredGroup = Assert.Single(restored.Groups!);
        Assert.Equal(groupId, restoredGroup.Id);
        Assert.Empty(restoredGroup.NodeIds);
    }

    [Fact]
    public void SessionQueries_CreateDocumentSnapshot_PreservesChildScopesAndCompositeMetadata()
    {
        var session = CreateSession(CreateScopedDocument());

        Assert.True(session.Commands.TrySetNodeSize(NodeId, new GraphSize(420d, 260d), updateStatus: false));

        var snapshot = session.Queries.CreateDocumentSnapshot();

        Assert.Equal("graph-root", snapshot.RootGraphId);
        Assert.Equal(2, snapshot.GraphScopes!.Count);

        var rootScope = Assert.Single(snapshot.GraphScopes, scope => scope.Id == "graph-root");
        Assert.Equal(new GraphSize(420d, 260d), Assert.Single(rootScope.Nodes, node => node.Id == NodeId).Size);

        var compositeNode = Assert.Single(rootScope.Nodes, node => node.Id == "tests.node-surface.composite-001");
        Assert.NotNull(compositeNode.Composite);
        Assert.Equal("graph-composite-001", compositeNode.Composite!.ChildGraphId);

        var childScope = Assert.Single(snapshot.GraphScopes, scope => scope.Id == "graph-composite-001");
        var childConnection = Assert.Single(childScope.Connections);
        Assert.NotNull(childConnection.Presentation);
        Assert.Equal("Preview branch", childConnection.Presentation!.NoteText);
    }

    [Fact]
    public void SessionCommands_NodeGroups_PromoteToComposite_AndExposeBoundaryPorts()
    {
        var session = CreateSession(CreatePromotionDocument(), CreatePromotionCatalog());

        session.Commands.SetSelection([NodeId, SiblingNodeId], SiblingNodeId, updateStatus: false);
        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Composite Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var compositeNodeId = session.Commands.TryPromoteNodeGroupToComposite(groupId, "Composite Cluster", updateStatus: false);
        Assert.False(string.IsNullOrWhiteSpace(compositeNodeId));

        var snapshot = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(2, snapshot.GraphScopes.Count);

        var rootScope = Assert.Single(snapshot.GraphScopes, scope => scope.Id == snapshot.RootGraphId);
        var compositeNode = Assert.Single(rootScope.Nodes, node => node.Id == compositeNodeId);
        Assert.NotNull(compositeNode.Composite);
        Assert.Empty(compositeNode.Composite!.Outputs ?? []);

        var compositeSnapshot = Assert.Single(session.Queries.GetCompositeNodeSnapshots());
        Assert.Equal(compositeNodeId, compositeSnapshot.NodeId);
        Assert.Equal(compositeNode.Composite.ChildGraphId, compositeSnapshot.ChildGraphId);
        Assert.Empty(compositeSnapshot.Outputs);

        var boundaryPortId = session.Commands.TryExposeCompositePort(
            compositeNodeId,
            NodeId,
            OutputPortId,
            "Composite Output",
            updateStatus: false);
        Assert.False(string.IsNullOrWhiteSpace(boundaryPortId));

        compositeSnapshot = Assert.Single(session.Queries.GetCompositeNodeSnapshots());
        Assert.Equal(boundaryPortId, Assert.Single(compositeSnapshot.Outputs).Id);

        Assert.True(session.Commands.TryUnexposeCompositePort(compositeNodeId, boundaryPortId, updateStatus: false));
        compositeSnapshot = Assert.Single(session.Queries.GetCompositeNodeSnapshots());
        Assert.Empty(compositeSnapshot.Outputs);
    }

    private static IGraphEditorSession CreateSession(GraphDocument? document = null, INodeCatalog? catalog = null)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = document ?? CreateRootDocument(),
            NodeCatalog = catalog ?? CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            BehaviorOptions = new GraphEditorBehaviorOptions
            {
                View = new ViewBehaviorOptions
                {
                    NodeSurfaceTierProfile = new NodeSurfaceTierProfile(
                    [
                        new NodeSurfaceTierDefinition("project-compact"),
                        new NodeSurfaceTierDefinition(
                            "project-parameter-rail",
                            minWidth: 200d,
                            minHeight: 140d,
                            visibleSectionKeys:
                            [
                                NodeSurfaceSectionKeys.Description,
                                NodeSurfaceSectionKeys.ParameterRail,
                            ]),
                        new NodeSurfaceTierDefinition(
                            "project-parameter-editors",
                            minWidth: 360d,
                            minHeight: 220d,
                            visibleSectionKeys:
                            [
                                NodeSurfaceSectionKeys.Description,
                                NodeSurfaceSectionKeys.ParameterRail,
                                NodeSurfaceSectionKeys.ParameterEditors,
                            ]),
                    ]),
                },
            },
        });

    private static GraphDocument CreateRootDocument()
        => new(
            "Node Surface Graph",
            "Covers runtime node surface contracts.",
            [
                new GraphNode(
                    NodeId,
                    "Node Surface",
                    "Tests",
                    "Contracts",
                    "Covers width and expanded-state mutations.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [],
                    "#6AD5C4",
                    TieredDefinitionId,
                    []),
                new GraphNode(
                    SiblingNodeId,
                    "Node Surface Sibling",
                    "Tests",
                    "Contracts",
                    "Covers grouped node-surface mutations.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 150),
                    [],
                    [],
                    "#F3B36B",
                    DefaultDefinitionId,
                    []),
            ],
            []);

    private static GraphDocument CreatePromotionDocument()
        => new(
            "Composite Promotion Graph",
            "Covers group promotion and boundary-port exposure contracts.",
            [
                new GraphNode(
                    NodeId,
                    "Promote Source",
                    "Tests",
                    "Contracts",
                    "Source node promoted into child scope.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort(OutputPortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefaultDefinitionId,
                    []),
                new GraphNode(
                    SiblingNodeId,
                    "Promote Target",
                    "Tests",
                    "Contracts",
                    "Target node promoted into child scope.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 150),
                    [new GraphPort(InputPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefaultDefinitionId,
                    []),
            ],
            [
                new GraphConnection(
                    "connection-root-001",
                    NodeId,
                    OutputPortId,
                    SiblingNodeId,
                    InputPortId,
                    "Promoted Flow",
                    "#6AD5C4"),
            ]);

    private static GraphDocument CreateScopedDocument()
        => GraphDocument.CreateScoped(
            "Node Surface Graph",
            "Covers runtime node surface contracts.",
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [
                        new GraphNode(
                            NodeId,
                            "Node Surface",
                            "Tests",
                            "Contracts",
                            "Covers width and expanded-state mutations.",
                            new GraphPoint(120, 160),
                            new GraphSize(240, 160),
                            [],
                            [],
                            "#6AD5C4",
                            TieredDefinitionId,
                            []),
                        new GraphNode(
                            "tests.node-surface.composite-001",
                            "Composite",
                            "Tests",
                            "Contracts",
                            "Composite shell.",
                            new GraphPoint(520, 160),
                            new GraphSize(280, 180),
                            [],
                            [],
                            "#A67CF5",
                            null,
                            [],
                            null,
                            new GraphCompositeNode(
                                "graph-composite-001",
                                [],
                                [
                                    new GraphCompositeBoundaryPort(
                                        "boundary-output-001",
                                        "Composite Output",
                                        PortDirection.Output,
                                        "float",
                                        "#A67CF5",
                                        "tests.node-surface.child-001",
                                        "output-001",
                                        new PortTypeId("float")),
                                ])),
                    ],
                    []),
                new GraphScope(
                    "graph-composite-001",
                    [
                        new GraphNode(
                            "tests.node-surface.child-001",
                            "Child Source",
                            "Tests",
                            "Contracts",
                            "Child scope source.",
                            new GraphPoint(40, 40),
                            new GraphSize(220, 150),
                            [],
                            [],
                            "#6AD5C4",
                            DefaultDefinitionId,
                            []),
                        new GraphNode(
                            "tests.node-surface.child-002",
                            "Child Target",
                            "Tests",
                            "Contracts",
                            "Child scope target.",
                            new GraphPoint(320, 40),
                            new GraphSize(220, 150),
                            [],
                            [],
                            "#F3B36B",
                            DefaultDefinitionId,
                            []),
                    ],
                    [
                        new GraphConnection(
                            "connection-child-001",
                            "tests.node-surface.child-001",
                            "output-001",
                            "tests.node-surface.child-002",
                            "input-001",
                            "Child Flow",
                            "#6AD5C4",
                            null,
                            new GraphEdgePresentation("Preview branch")),
                    ]),
            ]);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                TieredDefinitionId,
                "Node Surface Tiered",
                "Tests",
                "Contracts",
                [],
                [],
                description: "Node surface regression definition.",
                defaultWidth: 240d,
                defaultHeight: 160d,
                surfaceTierProfile: new NodeSurfaceTierProfile(
                [
                    new NodeSurfaceTierDefinition("compact"),
                    new NodeSurfaceTierDefinition(
                        "details",
                        minWidth: 220d,
                        minHeight: 150d,
                        visibleSectionKeys:
                        [
                            NodeSurfaceSectionKeys.Description,
                        ]),
                    new NodeSurfaceTierDefinition(
                        "parameter-editors",
                        minWidth: 400d,
                        minHeight: 240d,
                        visibleSectionKeys:
                        [
                            NodeSurfaceSectionKeys.Description,
                            NodeSurfaceSectionKeys.ParameterRail,
                            NodeSurfaceSectionKeys.ParameterEditors,
                        ]),
                ])));
        catalog.RegisterDefinition(
            new NodeDefinition(
                DefaultDefinitionId,
                "Node Surface Default",
                "Tests",
                "Contracts",
                [],
                [],
                description: "Node surface behavior-default definition.",
                defaultWidth: 220d,
                defaultHeight: 150d));
        return catalog;
    }

    private static NodeDefinition CreateAdaptiveDefinition()
        => new(
            AdaptiveDefinitionId,
            "Adaptive Surface",
            "Tests",
            "Contracts",
            [],
            [new PortDefinition(OutputPortId, "Result", new PortTypeId("float"), "#6AD5C4")],
            parameters:
            [
                new NodeParameterDefinition(
                    "required-input",
                    "Required Input",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    isRequired: true),
                new NodeParameterDefinition(
                    "optional-gain",
                    "Optional Gain",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 0.5d),
            ]);

    private static INodeCatalog CreatePromotionCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                DefaultDefinitionId,
                "Composite Promotion",
                "Tests",
                "Contracts",
                [new PortDefinition(InputPortId, "Input", new PortTypeId("float"), "#F3B36B")],
                [new PortDefinition(OutputPortId, "Output", new PortTypeId("float"), "#6AD5C4")],
                description: "Composite promotion regression definition.",
                defaultWidth: 240d,
                defaultHeight: 160d));
        return catalog;
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(methodName, parameterTypes);
        Assert.NotNull(method);
    }
}
