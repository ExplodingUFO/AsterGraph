using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorGraphItemSearchProjectionTests
{
    private static readonly NodeDefinitionId SourceDefinitionId = new("tests.search.source");
    private static readonly NodeDefinitionId TargetDefinitionId = new("tests.search.target");

    [Fact]
    public void Queries_SearchGraphItemsReturnsStableNodeGroupConnectionScopeAndIssueResults()
    {
        var session = CreateSession();

        var all = session.Queries.SearchGraphItems();

        Assert.Contains(all.Results, result => result.Kind == GraphEditorGraphItemSearchResultKind.Scope && result.SourceId == "graph-root");
        Assert.Contains(all.Results, result => result.Kind == GraphEditorGraphItemSearchResultKind.Scope && result.SourceId == "graph-child-001");
        Assert.Contains(all.Results, result => result.Kind == GraphEditorGraphItemSearchResultKind.Node && result.NodeId == "source-001");
        Assert.Contains(all.Results, result => result.Kind == GraphEditorGraphItemSearchResultKind.Group && result.GroupId == "group-001");
        Assert.Contains(all.Results, result => result.Kind == GraphEditorGraphItemSearchResultKind.Connection && result.ConnectionId == "connection-001");
        Assert.Contains(all.Results, result => result.Kind == GraphEditorGraphItemSearchResultKind.Issue && result.IssueCode == "connection.target-node-missing");
        Assert.Null(all.EmptyReason);
    }

    [Fact]
    public void Queries_SearchGraphItemsFiltersByTextKindScopeAndLimit()
    {
        var session = CreateSession();

        var target = session.Queries.SearchGraphItems(new GraphEditorGraphItemSearchQuery("target", GraphEditorGraphItemSearchResultKind.Node));
        Assert.All(target.Results, result => Assert.Equal(GraphEditorGraphItemSearchResultKind.Node, result.Kind));
        Assert.Contains(target.Results, result => result.NodeId == "target-001");

        var childScope = session.Queries.SearchGraphItems(new GraphEditorGraphItemSearchQuery(ScopeId: "graph-child-001"));
        Assert.All(childScope.Results, result => Assert.Equal("graph-child-001", result.ScopeId));
        Assert.Contains(childScope.Results, result => result.NodeId == "child-001");

        var limited = session.Queries.SearchGraphItems(new GraphEditorGraphItemSearchQuery(Limit: 2));
        Assert.Equal(2, limited.Results.Count);
        Assert.Equal(2, limited.Limit);
    }

    [Fact]
    public void Queries_SearchGraphItemsReportsEmptyMatches()
    {
        var session = CreateSession();

        var snapshot = session.Queries.SearchGraphItems(new GraphEditorGraphItemSearchQuery("not-present"));

        Assert.Empty(snapshot.Results);
        Assert.Equal("No graph items matched the search text.", snapshot.EmptyReason);
    }

    private static IGraphEditorSession CreateSession()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            SourceDefinitionId,
            "Source",
            "Tests",
            "Search",
            [],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));
        catalog.RegisterDefinition(new NodeDefinition(
            TargetDefinitionId,
            "Target",
            "Tests",
            "Search",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            []));

        var source = new GraphNode(
            "source-001",
            "Source Node",
            "Tests",
            "Search",
            "Source node.",
            new GraphPoint(120, 120),
            new GraphSize(220, 120),
            [],
            [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"))],
            "#6AD5C4",
            SourceDefinitionId);
        var target = new GraphNode(
            "target-001",
            "Target Node",
            "Tests",
            "Search",
            "Target node.",
            new GraphPoint(420, 120),
            new GraphSize(220, 120),
            [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
            [],
            "#8B7BFF",
            TargetDefinitionId);
        var child = target with { Id = "child-001", Title = "Child Scope Node" };

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = GraphDocument.CreateScoped(
                "Graph Item Search",
                "Search projection proof.",
                "graph-root",
                [
                    new GraphScope(
                        "graph-root",
                        [source, target],
                        [
                            new GraphConnection("connection-001", "source-001", "out", "target-001", "in", "Signal Path", "#6AD5C4"),
                            new GraphConnection("broken-connection", "source-001", "out", "missing-target", "in", "Broken Path", "#FF6A6A"),
                        ],
                        [
                            new GraphNodeGroup(
                                "group-001",
                                "Pipeline Group",
                                new GraphPoint(96, 96),
                                new GraphSize(584, 180),
                                ["source-001", "target-001"]),
                        ]),
                    new GraphScope("graph-child-001", [child], []),
                ]),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }
}
