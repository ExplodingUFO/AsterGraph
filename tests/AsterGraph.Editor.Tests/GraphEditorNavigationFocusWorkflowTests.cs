using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorNavigationFocusWorkflowTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.navigation.node");

    [Fact]
    public void Queries_GetCommandDescriptorsExposeBookmarkAndFocusWorkflowRoutes()
    {
        var session = CreateSession();

        var descriptors = session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var registry = session.Queries.GetCommandRegistry();

        Assert.Equal(GraphEditorCommandSourceKind.Host, descriptors["viewport.focus-node"].Source);
        Assert.True(descriptors["viewport.focus-node"].IsEnabled);
        Assert.False(descriptors["viewport.focus-issue"].IsEnabled);
        Assert.Equal("No validation issue is available to focus.", descriptors["viewport.focus-issue"].DisabledReason);
        Assert.True(descriptors["viewport.bookmark.add"].IsEnabled);
        Assert.False(descriptors["viewport.bookmark.activate"].IsEnabled);
        Assert.Contains(registry, entry => entry.CommandId == "viewport.focus-search-result");
        Assert.Contains(registry, entry => entry.CommandId == "viewport.bookmark.add");
    }

    [Fact]
    public void Commands_ViewportBookmarksCaptureActivateAndRemoveCurrentScopeViewport()
    {
        var session = CreateSession();
        session.Commands.UpdateViewportSize(1280, 720);
        session.Commands.PanBy(240, -120);

        Assert.True(session.Commands.TryAddViewportBookmark("root-home", "Root Home"));
        var bookmarks = session.Queries.GetViewportBookmarks();
        var bookmark = Assert.Single(bookmarks.Bookmarks);
        var capturedViewport = session.Queries.GetViewportSnapshot();
        Assert.Equal("graph-root", bookmark.ScopeId);
        Assert.Equal(capturedViewport.PanX, bookmark.PanX);
        Assert.Equal(capturedViewport.PanY, bookmark.PanY);

        session.Commands.PanBy(-50, 80);

        Assert.True(session.Commands.TryActivateViewportBookmark("root-home", updateStatus: false));
        var viewport = session.Queries.GetViewportSnapshot();
        Assert.Equal(bookmark.PanX, viewport.PanX, precision: 3);
        Assert.Equal(bookmark.PanY, viewport.PanY, precision: 3);

        Assert.True(session.Commands.TryRemoveViewportBookmark("root-home"));
        Assert.Empty(session.Queries.GetViewportBookmarks().Bookmarks);
    }

    [Fact]
    public void Commands_FocusNodeNavigatesToScopeSelectsNodeAndCentersViewport()
    {
        var session = CreateSession();
        session.Commands.UpdateViewportSize(1280, 720);

        Assert.True(session.Commands.TryExecuteCommand(
            CreateCommand(
                "viewport.focus-node",
                ("scopeId", "graph-child-001"),
                ("nodeId", "child-target"),
                ("updateStatus", "false"))));

        Assert.Equal("graph-child-001", session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);
        Assert.Equal(["child-target"], session.Queries.GetSelectionSnapshot().SelectedNodeIds);
        Assert.NotEqual(0, session.Queries.GetViewportSnapshot().PanX);
    }

    [Fact]
    public void Commands_FocusIssueAndSearchResultUseStableSourceBackedTargets()
    {
        var session = CreateSession(includeBrokenConnection: true);
        session.Commands.UpdateViewportSize(1280, 720);
        var issue = Assert.Single(session.Queries.GetValidationSnapshot().Issues, candidate => candidate.Code == "connection.target-node-missing");

        Assert.True(session.Commands.TryExecuteCommand(
            CreateCommand(
                "viewport.focus-issue",
                ("issueCode", issue.Code),
                ("scopeId", issue.ScopeId),
                ("connectionId", issue.ConnectionId ?? string.Empty),
                ("updateStatus", "false"))));

        Assert.Equal([issue.ConnectionId ?? string.Empty], session.Queries.GetSelectionSnapshot().SelectedConnectionIds);

        var connectionResult = Assert.Single(
            session.Queries.SearchGraphItems(new GraphEditorGraphItemSearchQuery("child-flow", GraphEditorGraphItemSearchResultKind.Connection)).Results);

        Assert.True(session.Commands.TryExecuteCommand(
            CreateCommand(
                "viewport.focus-search-result",
                ("resultId", connectionResult.Id),
                ("updateStatus", "false"))));
        Assert.Equal([connectionResult.ConnectionId ?? string.Empty], session.Queries.GetSelectionSnapshot().SelectedConnectionIds);
    }

    private static IGraphEditorSession CreateSession(bool includeBrokenConnection = false)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Navigation Node",
            "Tests",
            "Navigation",
            [new PortDefinition("in", "In", new PortTypeId("flow"), "#6AD5C4")],
            [new PortDefinition("out", "Out", new PortTypeId("flow"), "#6AD5C4")]));

        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(includeBrokenConnection),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
        });
    }

    private static GraphDocument CreateDocument(bool includeBrokenConnection)
    {
        var childConnections = new List<GraphConnection>
        {
            new("child-flow", "child-source", "out", "child-target", "in", "Child Flow", "#6AD5C4"),
        };
        if (includeBrokenConnection)
        {
            childConnections.Add(new GraphConnection("child-broken", "child-source", "out", "missing-target", "in", "Broken Child Flow", "#FF6A6A"));
        }

        return GraphDocument.CreateScoped(
            "Navigation Workflow",
            "Navigation focus workflow proof.",
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [
                        CreateNode(
                            "composite-shell",
                            "Composite Shell",
                            new GraphPoint(120, 160),
                            new GraphCompositeNode("graph-child-001", [], [])),
                        CreateNode("root-node", "Root Node", new GraphPoint(480, 180)),
                    ],
                    []),
                new GraphScope(
                    "graph-child-001",
                    [
                        CreateNode("child-source", "Child Source", new GraphPoint(80, 100)),
                        CreateNode("child-target", "Child Target", new GraphPoint(420, 160)),
                    ],
                    childConnections),
            ]);
    }

    private static GraphNode CreateNode(
        string id,
        string title,
        GraphPoint position,
        GraphCompositeNode? composite = null)
        => new(
            id,
            title,
            "Tests",
            "Navigation",
            title + " node.",
            position,
            new GraphSize(220, 140),
            [new GraphPort("in", "In", PortDirection.Input, "flow", "#6AD5C4", new PortTypeId("flow"))],
            [new GraphPort("out", "Out", PortDirection.Output, "flow", "#6AD5C4", new PortTypeId("flow"))],
            "#6AD5C4",
            DefinitionId,
            [],
            null,
            composite);

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            commandId,
            arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList());
}
