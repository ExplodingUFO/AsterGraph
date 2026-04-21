using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using System.Linq;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorHierarchyStateContractsTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.hierarchy.node");
    private const string CompositeNodeId = "tests.hierarchy.composite-001";
    private const string RootStandaloneNodeId = "tests.hierarchy.root-001";
    private const string ChildGraphId = "graph-child-001";
    private const string ChildSourceNodeId = "tests.hierarchy.child-source-001";
    private const string ChildTargetNodeId = "tests.hierarchy.child-target-001";
    private const string SourcePortId = "output-001";
    private const string TargetPortId = "input-001";

    [Fact]
    public void RuntimeContracts_ExposeHierarchyStateQuery()
    {
        var queriesType = typeof(IGraphEditorQueries);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetHierarchyStateSnapshot));
        Assert.Equal(
            typeof(GraphEditorHierarchyStateSnapshot),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetHierarchyStateSnapshot))!.ReturnType);

        Assert.NotNull(typeof(GraphEditorHierarchyStateSnapshot).GetProperty(nameof(GraphEditorHierarchyStateSnapshot.ScopeNavigation)));
        Assert.NotNull(typeof(GraphEditorHierarchyStateSnapshot).GetProperty(nameof(GraphEditorHierarchyStateSnapshot.ParentCompositeNodeId)));
        Assert.NotNull(typeof(GraphEditorHierarchyStateSnapshot).GetProperty(nameof(GraphEditorHierarchyStateSnapshot.CompositeNodes)));
        Assert.NotNull(typeof(GraphEditorHierarchyStateSnapshot).GetProperty(nameof(GraphEditorHierarchyStateSnapshot.NodeGroups)));
        Assert.NotNull(typeof(GraphEditorHierarchyStateSnapshot).GetProperty(nameof(GraphEditorHierarchyStateSnapshot.Nodes)));
        Assert.NotNull(typeof(GraphEditorHierarchyStateSnapshot).GetProperty(nameof(GraphEditorHierarchyStateSnapshot.GroupMoveConstraints)));

        Assert.NotNull(typeof(GraphEditorHierarchyNodeSnapshot).GetProperty(nameof(GraphEditorHierarchyNodeSnapshot.NodeId)));
        Assert.NotNull(typeof(GraphEditorHierarchyNodeSnapshot).GetProperty(nameof(GraphEditorHierarchyNodeSnapshot.ParentGroupId)));
        Assert.NotNull(typeof(GraphEditorHierarchyNodeSnapshot).GetProperty(nameof(GraphEditorHierarchyNodeSnapshot.CollapsedByGroupId)));
        Assert.NotNull(typeof(GraphEditorHierarchyNodeSnapshot).GetProperty(nameof(GraphEditorHierarchyNodeSnapshot.IsVisibleInActiveScope)));

        Assert.NotNull(typeof(GraphEditorGroupMoveConstraintsSnapshot).GetProperty(nameof(GraphEditorGroupMoveConstraintsSnapshot.CanMoveFrameIndependently)));
        Assert.NotNull(typeof(GraphEditorGroupMoveConstraintsSnapshot).GetProperty(nameof(GraphEditorGroupMoveConstraintsSnapshot.CanMoveFrameWithMembers)));
    }

    [Fact]
    public void SessionQueries_GetHierarchyStateSnapshot_ExposesRootAndChildScopeOwnership()
    {
        var session = CreateSession(CreateScopedDocument());

        var rootState = session.Queries.GetHierarchyStateSnapshot();

        Assert.Equal("graph-root", rootState.ScopeNavigation.CurrentScopeId);
        Assert.Null(rootState.ScopeNavigation.ParentScopeId);
        Assert.Null(rootState.ParentCompositeNodeId);
        Assert.Empty(rootState.NodeGroups);
        Assert.Equal(
            [CompositeNodeId, RootStandaloneNodeId],
            rootState.Nodes.Select(snapshot => snapshot.NodeId).OrderBy(id => id, StringComparer.Ordinal));
        Assert.All(rootState.Nodes, snapshot =>
        {
            Assert.Null(snapshot.ParentGroupId);
            Assert.Null(snapshot.CollapsedByGroupId);
            Assert.True(snapshot.IsVisibleInActiveScope);
        });

        var compositeNode = Assert.Single(rootState.CompositeNodes);
        Assert.Equal(CompositeNodeId, compositeNode.NodeId);
        Assert.Equal(ChildGraphId, compositeNode.ChildGraphId);

        Assert.True(session.Commands.TryEnterCompositeChildGraph(CompositeNodeId, updateStatus: false));

        var childState = session.Queries.GetHierarchyStateSnapshot();

        Assert.Equal(ChildGraphId, childState.ScopeNavigation.CurrentScopeId);
        Assert.Equal("graph-root", childState.ScopeNavigation.ParentScopeId);
        Assert.True(childState.ScopeNavigation.CanNavigateToParent);
        Assert.Equal(CompositeNodeId, childState.ParentCompositeNodeId);
        Assert.Empty(childState.CompositeNodes);
        Assert.Equal(
            [ChildSourceNodeId, ChildTargetNodeId],
            childState.Nodes.Select(snapshot => snapshot.NodeId).OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public void SessionQueries_GetHierarchyStateSnapshot_ExposesCollapsedGroupMembershipAndMoveConstraints()
    {
        var session = CreateSession(CreateScopedDocument());

        Assert.True(session.Commands.TryEnterCompositeChildGraph(CompositeNodeId, updateStatus: false));
        session.Commands.SetSelection([ChildSourceNodeId, ChildTargetNodeId], ChildTargetNodeId, updateStatus: false);

        var groupId = session.Commands.TryCreateNodeGroupFromSelection("Child Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));
        Assert.True(session.Commands.TrySetNodeGroupCollapsed(groupId, isCollapsed: true));

        var originalPositions = session.Queries.GetNodePositions()
            .ToDictionary(snapshot => snapshot.NodeId, snapshot => snapshot.Position, StringComparer.Ordinal);

        Assert.True(
            session.Commands.TrySetNodeGroupPosition(
                groupId,
                new GraphPoint(420d, 260d),
                moveMemberNodes: false,
                updateStatus: false));

        var hierarchy = session.Queries.GetHierarchyStateSnapshot();
        var group = Assert.Single(hierarchy.NodeGroups);
        var sourceNode = Assert.Single(hierarchy.Nodes, snapshot => snapshot.NodeId == ChildSourceNodeId);
        var targetNode = Assert.Single(hierarchy.Nodes, snapshot => snapshot.NodeId == ChildTargetNodeId);
        var currentPositions = session.Queries.GetNodePositions()
            .ToDictionary(snapshot => snapshot.NodeId, snapshot => snapshot.Position, StringComparer.Ordinal);

        Assert.True(hierarchy.GroupMoveConstraints.CanMoveFrameIndependently);
        Assert.True(hierarchy.GroupMoveConstraints.CanMoveFrameWithMembers);
        Assert.Equal(groupId, group.Id);
        Assert.True(group.IsCollapsed);
        Assert.Equal(
            [ChildSourceNodeId, ChildTargetNodeId],
            group.NodeIds.OrderBy(id => id, StringComparer.Ordinal));

        Assert.Equal(groupId, sourceNode.ParentGroupId);
        Assert.Equal(groupId, targetNode.ParentGroupId);
        Assert.Equal(groupId, sourceNode.CollapsedByGroupId);
        Assert.Equal(groupId, targetNode.CollapsedByGroupId);
        Assert.False(sourceNode.IsVisibleInActiveScope);
        Assert.False(targetNode.IsVisibleInActiveScope);

        Assert.Equal(originalPositions[ChildSourceNodeId], currentPositions[ChildSourceNodeId]);
        Assert.Equal(originalPositions[ChildTargetNodeId], currentPositions[ChildTargetNodeId]);
    }

    private static IGraphEditorSession CreateSession(GraphDocument document)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = document,
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateScopedDocument()
        => GraphDocument.CreateScoped(
            "Hierarchy Graph",
            "Covers canonical hierarchy-state contracts.",
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [
                        new GraphNode(
                            CompositeNodeId,
                            "Composite Node",
                            "Tests",
                            "Hierarchy",
                            "Composite shell for hierarchy-state tests.",
                            new GraphPoint(120d, 160d),
                            new GraphSize(260d, 180d),
                            [],
                            [],
                            "#A67CF5",
                            null,
                            [],
                            null,
                            new GraphCompositeNode(ChildGraphId, [], [])),
                        new GraphNode(
                            RootStandaloneNodeId,
                            "Root Standalone",
                            "Tests",
                            "Hierarchy",
                            "Root scope node that remains outside the composite.",
                            new GraphPoint(460d, 180d),
                            new GraphSize(240d, 160d),
                            [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                            [],
                            "#F3B36B",
                            DefinitionId),
                    ],
                    []),
                new GraphScope(
                    ChildGraphId,
                    [
                        new GraphNode(
                            ChildSourceNodeId,
                            "Child Source",
                            "Tests",
                            "Hierarchy",
                            "Child scope source node.",
                            new GraphPoint(80d, 100d),
                            new GraphSize(220d, 150d),
                            [],
                            [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                            "#6AD5C4",
                            DefinitionId),
                        new GraphNode(
                            ChildTargetNodeId,
                            "Child Target",
                            "Tests",
                            "Hierarchy",
                            "Child scope target node.",
                            new GraphPoint(360d, 140d),
                            new GraphSize(220d, 150d),
                            [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                            [],
                            "#F3B36B",
                            DefinitionId),
                    ],
                    [
                        new GraphConnection(
                            "connection-child-001",
                            ChildSourceNodeId,
                            SourcePortId,
                            ChildTargetNodeId,
                            TargetPortId,
                            "Child Flow",
                            "#6AD5C4"),
                    ]),
            ]);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Hierarchy Node",
            "Tests",
            "Hierarchy",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private static void AssertMethod(Type declaringType, string name, params Type[] parameterTypes)
        => Assert.NotNull(declaringType.GetMethod(name, parameterTypes));
}
