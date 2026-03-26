using System.Reflection;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSessionTests
{
    [Fact]
    public void IGraphEditorSession_ExposesCommandsQueriesAndEventsProperties()
    {
        var sessionType = typeof(IGraphEditorSession);

        Assert.True(sessionType.IsPublic);
        Assert.True(sessionType.IsInterface);

        AssertProperty(sessionType, nameof(IGraphEditorSession.Commands), typeof(IGraphEditorCommands));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Queries), typeof(IGraphEditorQueries));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Events), typeof(IGraphEditorEvents));
    }

    [Fact]
    public void IGraphEditorCommands_DefinesHostFacingEditAndViewportActions()
    {
        var commandsType = typeof(IGraphEditorCommands);

        Assert.True(commandsType.IsPublic);
        Assert.True(commandsType.IsInterface);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.Undo));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.Redo));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ClearSelection), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.AddNode), typeof(NodeDefinitionId), typeof(GraphPoint?));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.DeleteSelection));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.PanBy), typeof(double), typeof(double));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ZoomAt), typeof(double), typeof(GraphPoint));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ResetView), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SaveWorkspace));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.LoadWorkspace));
    }

    [Fact]
    public void IGraphEditorQueries_DefinesHostFacingSnapshotAndDiscoveryReads()
    {
        var queriesType = typeof(IGraphEditorQueries);

        Assert.True(queriesType.IsPublic);
        Assert.True(queriesType.IsInterface);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.CreateDocumentSnapshot));
        Assert.Equal(typeof(GraphDocument), queriesType.GetMethod(nameof(IGraphEditorQueries.CreateDocumentSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodePositions));
        Assert.Equal(typeof(IReadOnlyList<NodePositionSnapshot>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodePositions))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompatibleTargets), typeof(string), typeof(string));
        Assert.Equal(typeof(IReadOnlyList<CompatiblePortTarget>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets))!.ReturnType);
    }

    [Fact]
    public void IGraphEditorEvents_ReusesExistingTypedEventArgs()
    {
        var eventsType = typeof(IGraphEditorEvents);

        Assert.True(eventsType.IsPublic);
        Assert.True(eventsType.IsInterface);

        AssertEvent(eventsType, nameof(IGraphEditorEvents.DocumentChanged), typeof(GraphEditorDocumentChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.SelectionChanged), typeof(GraphEditorSelectionChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.ViewportChanged), typeof(GraphEditorViewportChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.FragmentExported), typeof(GraphEditorFragmentEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.FragmentImported), typeof(GraphEditorFragmentEventArgs));
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_ExecutesCommandsQueriesAndEvents()
    {
        var definitionId = new NodeDefinitionId("tests.session.node");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        GraphEditorDocumentChangedEventArgs? documentChanged = null;
        GraphEditorViewportChangedEventArgs? viewportChanged = null;

        session.Events.DocumentChanged += (_, args) => documentChanged = args;
        session.Events.ViewportChanged += (_, args) => viewportChanged = args;

        var before = session.Queries.CreateDocumentSnapshot();

        session.Commands.AddNode(definitionId, new GraphPoint(420, 220));
        session.Commands.PanBy(12, 18);

        var after = session.Queries.CreateDocumentSnapshot();

        Assert.Equal(before.Nodes.Count + 1, after.Nodes.Count);
        Assert.NotNull(documentChanged);
        Assert.Equal(GraphEditorDocumentChangeKind.NodesAdded, documentChanged!.ChangeKind);
        Assert.NotNull(viewportChanged);
        Assert.Equal(122, viewportChanged!.PanX);
        Assert.Equal(114, viewportChanged.PanY);
    }

    [Fact]
    public void GraphEditorViewModel_Session_ExposesSharedRuntimeSurface()
    {
        var definitionId = new NodeDefinitionId("tests.session.compat");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var session = editor.Session;

        Assert.Same(session, editor.Session);

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        Assert.Single(editor.SelectedNodes);

        session.Commands.ClearSelection();

        Assert.Empty(editor.SelectedNodes);
        var editorSnapshot = editor.CreateDocumentSnapshot();
        var sessionSnapshot = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(editorSnapshot.Title, sessionSnapshot.Title);
        Assert.Equal(editorSnapshot.Description, sessionSnapshot.Description);
        Assert.Equal(editorSnapshot.Nodes.Count, sessionSnapshot.Nodes.Count);
        Assert.Equal(editorSnapshot.Connections.Count, sessionSnapshot.Connections.Count);
    }

    private static void AssertProperty(Type declaringType, string propertyName, Type propertyType)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property!.PropertyType);
        Assert.Null(property.SetMethod);
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, parameterTypes);

        Assert.NotNull(method);
    }

    private static void AssertEvent(Type declaringType, string eventName, Type eventArgsType)
    {
        var eventInfo = declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(eventInfo);

        var handlerType = eventInfo!.EventHandlerType!;
        Assert.True(handlerType.IsGenericType);
        Assert.Equal(typeof(EventHandler<>), handlerType.GetGenericTypeDefinition());
        Assert.Equal(eventArgsType, handlerType.GetGenericArguments()[0]);
    }

    private static AsterGraphEditorOptions CreateOptions(NodeDefinitionId definitionId)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Session Graph",
            "Runtime session regression coverage.",
            [
                new GraphNode(
                    "tests.session.node-001",
                    "Session Node",
                    "Tests",
                    "Runtime",
                    "Session test node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [],
                    "#6AD5C4",
                    definitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Session Node",
            "Tests",
            "Runtime",
            [],
            []));
        return catalog;
    }
}
