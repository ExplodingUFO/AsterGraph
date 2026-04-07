using System.Reflection;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
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
    private const string SourceNodeId = "tests.session.source-001";
    private const string TargetNodeId = "tests.session.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

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
    public void GraphEditorSession_PreservesConcreteBeginConnectionCompatibilityShim()
    {
        var method = typeof(GraphEditorSession).GetMethod(nameof(GraphEditorSession.BeginConnection), [typeof(string), typeof(string)]);

        Assert.NotNull(method);
        Assert.Contains(
            method!.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_NoLongerStoresGraphEditorViewModelAsItsRuntimeStateOwner()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(new NodeDefinitionId("tests.session.kernel-owner")));

        Assert.DoesNotContain(
            session.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType == typeof(GraphEditorViewModel));
    }

    [Fact]
    public void AsterGraphEditorFactory_Create_EditorSession_NoLongerStoresGraphEditorViewModelAsItsRuntimeStateOwner()
    {
        var editor = AsterGraphEditorFactory.Create(CreateOptions(new NodeDefinitionId("tests.session.editor-kernel-owner")));
        var session = editor.Session;
        var host = session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(session);

        Assert.NotNull(host);
        Assert.IsNotType<GraphEditorViewModel>(host);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_CreateDocumentSnapshot_ReturnsDetachedSnapshot()
    {
        var definitionId = new NodeDefinitionId("tests.session.detached-snapshot");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateMutableDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var snapshot = session.Queries.CreateDocumentSnapshot();
        var leakedNodes = Assert.IsType<List<GraphNode>>(snapshot.Nodes);
        leakedNodes.Add(
            new GraphNode(
                "external-node-001",
                "External Node",
                "Tests",
                string.Empty,
                string.Empty,
                new GraphPoint(0, 0),
                new GraphSize(120, 80),
                [],
                [],
                "#FFFFFF",
                definitionId,
                []));

        var after = session.Queries.CreateDocumentSnapshot();

        Assert.DoesNotContain(after.Nodes, node => node.Id == "external-node-001");
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_DoesNotTrackExternalMutationsFromTheOriginalDocument()
    {
        var definitionId = new NodeDefinitionId("tests.session.external-mutation");
        var document = CreateMutableDocument(definitionId);
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = document,
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var sourceNodes = Assert.IsType<List<GraphNode>>(document.Nodes);
        sourceNodes.Add(
            new GraphNode(
                "source-owned-node-001",
                "Source Owned Node",
                "Tests",
                string.Empty,
                string.Empty,
                new GraphPoint(32, 48),
                new GraphSize(120, 80),
                [],
                [],
                "#FFFFFF",
                definitionId,
                []));

        var snapshot = session.Queries.CreateDocumentSnapshot();

        Assert.DoesNotContain(snapshot.Nodes, node => node.Id == "source-owned-node-001");
    }

    [Fact]
    public void IGraphEditorCommands_BeginConnection_DefaultShimMatchesStartConnectionBehavior()
    {
        var definitionId = new NodeDefinitionId("tests.session.begin-shim");
        IGraphEditorCommands commands = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId)).Commands;

        commands.BeginConnection(SourceNodeId, SourcePortId);

        var session = Assert.IsAssignableFrom<IGraphEditorSession>(commands);
        var pending = session.Queries.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);
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
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SetSelection), typeof(IReadOnlyList<string>), typeof(string), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.AddNode), typeof(NodeDefinitionId), typeof(GraphPoint?));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.DeleteSelection));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SetNodePositions), typeof(IReadOnlyList<NodePositionSnapshot>), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.StartConnection), typeof(string), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CompleteConnection), typeof(string), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CancelPendingConnection));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.DeleteConnection), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.BreakConnectionsForPort), typeof(string), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.PanBy), typeof(double), typeof(double));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ZoomAt), typeof(double), typeof(GraphPoint));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.UpdateViewportSize), typeof(double), typeof(double));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ResetView), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.FitToViewport), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CenterViewOnNode), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CenterViewAt), typeof(GraphPoint), typeof(bool));
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

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetSelectionSnapshot));
        Assert.Equal(typeof(GraphEditorSelectionSnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetSelectionSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetViewportSnapshot));
        Assert.Equal(typeof(GraphEditorViewportSnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetViewportSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCapabilitySnapshot));
        Assert.Equal(typeof(GraphEditorCapabilitySnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetCapabilitySnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodePositions));
        Assert.Equal(typeof(IReadOnlyList<NodePositionSnapshot>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodePositions))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetPendingConnectionSnapshot));
        Assert.Equal(typeof(GraphEditorPendingConnectionSnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetPendingConnectionSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompatiblePortTargets), typeof(string), typeof(string));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompatiblePortTargets))!.ReturnType);

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
        AssertEvent(eventsType, nameof(IGraphEditorEvents.CommandExecuted), typeof(GraphEditorCommandExecutedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.PendingConnectionChanged), typeof(GraphEditorPendingConnectionChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.RecoverableFailure), typeof(GraphEditorRecoverableFailureEventArgs));
    }

    [Fact]
    public void GraphEditorCompatiblePortTargetSnapshot_IsRuntimeSafeAndMvvmFree()
    {
        var snapshotType = typeof(GraphEditorCompatiblePortTargetSnapshot);

        Assert.True(snapshotType.IsPublic);
        Assert.DoesNotContain(
            snapshotType.GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => property.PropertyType == typeof(NodeViewModel) || property.PropertyType == typeof(PortViewModel));
    }

    [Fact]
    public void GraphEditorCompatiblePortTargetSnapshot_NormalizesBlankDisplayMetadata()
    {
        var snapshot = new GraphEditorCompatiblePortTargetSnapshot(
            "node-1",
            "",
            "port-1",
            " ",
            new PortTypeId("float"),
            null!,
            PortCompatibilityResult.Exact());

        Assert.Equal("node-1", snapshot.NodeTitle);
        Assert.Equal("port-1", snapshot.PortLabel);
        Assert.Equal(string.Empty, snapshot.PortAccentHex);
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
    public void AsterGraphEditorFactory_CreateSession_SupportsRuntimeOnlySelectionConnectionAndViewportCommands()
    {
        var definitionId = new NodeDefinitionId("tests.session.runtime");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.UpdateViewportSize(1280, 720);
        session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot(SourceNodeId, new GraphPoint(240, 180)),
                new NodePositionSnapshot(TargetNodeId, new GraphPoint(620, 220)),
            ],
            updateStatus: false);

        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Equal([SourceNodeId], selection.SelectedNodeIds);
        Assert.Equal(SourceNodeId, selection.PrimarySelectedNodeId);

        var positions = session.Queries.GetNodePositions().ToDictionary(snapshot => snapshot.NodeId, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(240, 180), positions[SourceNodeId].Position);
        Assert.Equal(new GraphPoint(620, 220), positions[TargetNodeId].Position);

        var capabilities = session.Queries.GetCapabilitySnapshot();
        Assert.True(capabilities.CanSetSelection);
        Assert.True(capabilities.CanMoveNodes);
        Assert.True(capabilities.CanCreateConnections);
        Assert.True(capabilities.CanDeleteConnections);
        Assert.True(capabilities.CanBreakConnections);
        Assert.True(capabilities.CanUpdateViewport);
        Assert.True(capabilities.CanFitToViewport);
        Assert.True(capabilities.CanCenterViewport);

        var compatibleTargets = session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId);
        var target = Assert.Single(compatibleTargets);
        Assert.Equal(TargetNodeId, target.NodeId);
        Assert.Equal(TargetPortId, target.PortId);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);

        var pending = session.Queries.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);

        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        session.Commands.FitToViewport(updateStatus: false);
        session.Commands.CenterViewOnNode(TargetNodeId);
        session.Commands.CenterViewAt(new GraphPoint(430, 260), updateStatus: false);

        var after = session.Queries.CreateDocumentSnapshot();
        Assert.Single(after.Connections);
        Assert.False(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
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

    [Fact]
    public void GraphEditorViewModel_Session_PublishesPendingConnectionChangesForDirectEditorOperations()
    {
        var definitionId = new NodeDefinitionId("tests.session.pending-event");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var pendingSnapshots = new List<GraphEditorPendingConnectionSnapshot>();

        editor.Session.Events.PendingConnectionChanged += (_, args) => pendingSnapshots.Add(args.PendingConnection);

        editor.StartConnection(SourceNodeId, SourcePortId);
        editor.CancelPendingConnection();

        Assert.Equal(2, pendingSnapshots.Count);
        Assert.True(pendingSnapshots[0].HasPendingConnection);
        Assert.Equal(SourceNodeId, pendingSnapshots[0].SourceNodeId);
        Assert.Equal(SourcePortId, pendingSnapshots[0].SourcePortId);
        Assert.False(pendingSnapshots[1].HasPendingConnection);
    }

    [Fact]
    public void RuntimeSession_CaptureInspectionSnapshot_UsesNormalizedPendingConnectionState()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspection-pending");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.StartConnection(SourceNodeId, SourcePortId);

        var snapshot = session.Diagnostics.CaptureInspectionSnapshot();
        var pending = session.Queries.GetPendingConnectionSnapshot();

        Assert.Equal(pending.HasPendingConnection, snapshot.PendingConnection.HasPendingConnection);
        Assert.Equal(pending.SourceNodeId, snapshot.PendingConnection.SourceNodeId);
        Assert.Equal(pending.SourcePortId, snapshot.PendingConnection.SourcePortId);
    }

    [Fact]
    public void GraphEditorViewModel_InspectorConnectionSummaries_StayAlignedWithSelectionAndConnectionChanges()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspector-cache");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var session = editor.Session;

        editor.SelectSingleNode(editor.FindNode(SourceNodeId), updateStatus: false);
        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorDownstream);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        Assert.Equal("0 incoming  ·  1 outgoing", editor.InspectorConnections);
        Assert.Contains("Target Node", editor.InspectorDownstream, StringComparison.Ordinal);
        Assert.Contains("Input", editor.InspectorDownstream, StringComparison.Ordinal);

        editor.SelectSingleNode(editor.FindNode(TargetNodeId), updateStatus: false);
        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);

        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;
        session.Commands.DeleteConnection(connectionId);

        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);
    }

    [Fact]
    public void GraphEditorViewModel_InspectorConnectionSummaries_RemainAccurateAfterUndoRedoRestore()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspector-restore");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));

        editor.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
        editor.SelectSingleNode(editor.FindNode(TargetNodeId), updateStatus: false);

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);

        editor.Undo();
        editor.Redo();
        editor.SelectSingleNode(editor.FindNode(TargetNodeId), updateStatus: false);

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Single(editor.Connections);
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
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Runtime",
                    "Session source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Runtime",
                    "Session target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#6AD5C4",
                    definitionId),
            ],
            []);

    private static GraphDocument CreateMutableDocument(NodeDefinitionId definitionId)
        => new(
            "Session Graph",
            "Runtime session regression coverage.",
            new List<GraphNode>
            {
                new(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Runtime",
                    "Session source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    new List<GraphPort>(),
                    new List<GraphPort> { new(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")) },
                    "#6AD5C4",
                    definitionId),
                new(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Runtime",
                    "Session target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    new List<GraphPort> { new(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")) },
                    new List<GraphPort>(),
                    "#6AD5C4",
                    definitionId),
            },
            new List<GraphConnection>());

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Session Node",
            "Tests",
            "Runtime",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }
}
