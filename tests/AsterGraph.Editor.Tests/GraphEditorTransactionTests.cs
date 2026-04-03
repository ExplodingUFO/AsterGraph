using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorTransactionTests
{
    private const string SourceNodeId = "tests.transaction.source-001";
    private const string TargetNodeId = "tests.transaction.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void RuntimeSession_BeginMutation_DefersRuntimeNotificationsUntilDisposed()
    {
        var definitionId = new NodeDefinitionId("tests.transaction.node");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var documentChanges = 0;
        var viewportChanges = 0;
        var commandIds = new List<string>();

        session.Events.DocumentChanged += (_, _) => documentChanges++;
        session.Events.ViewportChanged += (_, _) => viewportChanges++;
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);

        using (session.BeginMutation("batch-add"))
        {
            session.Commands.AddNode(definitionId, new GraphPoint(320, 180));
            session.Commands.PanBy(10, 15);

            Assert.Equal(0, documentChanges);
            Assert.Equal(0, viewportChanges);
            Assert.Empty(commandIds);
        }

        Assert.Equal(1, documentChanges);
        Assert.Equal(1, viewportChanges);
        Assert.Equal(new[] { "nodes.add", "viewport.pan" }, commandIds);
        Assert.Equal(3, session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [Fact]
    public void RuntimeSession_BeginMutation_BatchesSelectionAndConnectionCommandsUntilDisposed()
    {
        var definitionId = new NodeDefinitionId("tests.transaction.connection-batch");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var documentChanges = 0;
        var selectionChanges = 0;
        var pendingConnectionChanges = 0;
        var commandEvents = new List<GraphEditorCommandExecutedEventArgs>();

        session.Events.DocumentChanged += (_, _) => documentChanges++;
        session.Events.SelectionChanged += (_, _) => selectionChanges++;
        session.Events.PendingConnectionChanged += (_, _) => pendingConnectionChanges++;
        session.Events.CommandExecuted += (_, args) => commandEvents.Add(args);

        using (session.BeginMutation("connect-and-select"))
        {
            session.Commands.StartConnection(SourceNodeId, SourcePortId);
            Assert.True(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);

            session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
            session.Commands.SetSelection([TargetNodeId], TargetNodeId, updateStatus: false);

            Assert.Equal(0, documentChanges);
            Assert.Equal(0, selectionChanges);
            Assert.Equal(0, pendingConnectionChanges);
            Assert.Empty(commandEvents);
        }

        Assert.Equal(1, documentChanges);
        Assert.Equal(1, selectionChanges);
        Assert.Equal(0, pendingConnectionChanges);
        Assert.Equal(
            ["connections.begin", "connections.complete", "selection.set"],
            commandEvents.Select(args => args.CommandId).ToArray());
        Assert.All(commandEvents, args =>
        {
            Assert.True(args.IsInMutationScope);
            Assert.Equal("connect-and-select", args.MutationLabel);
        });
        Assert.False(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
        Assert.Single(session.Queries.CreateDocumentSnapshot().Connections);
    }

    [Fact]
    public void RuntimeSession_BeginMutation_PublishesPendingConnectionChangeWhenBatchEndsWithActivePendingConnection()
    {
        var definitionId = new NodeDefinitionId("tests.transaction.pending-batch");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var pendingSnapshots = new List<GraphEditorPendingConnectionSnapshot>();

        session.Events.PendingConnectionChanged += (_, args) => pendingSnapshots.Add(args.PendingConnection);

        using (session.BeginMutation("start-only"))
        {
            session.Commands.StartConnection(SourceNodeId, SourcePortId);
            Assert.True(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
        }

        var pending = Assert.Single(pendingSnapshots);
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);
    }

    [Fact]
    public void RuntimeSession_QueriesExposeSelectionViewportAndCapabilities()
    {
        var definitionId = new NodeDefinitionId("tests.transaction.queries");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var session = editor.Session;

        editor.UpdateViewportSize(1280, 720);
        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);

        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Single(selection.SelectedNodeIds);
        Assert.Equal(editor.Nodes[0].Id, selection.PrimarySelectedNodeId);

        var viewport = session.Queries.GetViewportSnapshot();
        Assert.Equal(0.88, viewport.Zoom);
        Assert.Equal(110, viewport.PanX);
        Assert.Equal(96, viewport.PanY);
        Assert.Equal(1280, viewport.ViewportWidth);
        Assert.Equal(720, viewport.ViewportHeight);

        var capabilities = session.Queries.GetCapabilitySnapshot();
        Assert.False(capabilities.CanUndo);
        Assert.False(capabilities.CanRedo);
        Assert.True(capabilities.CanCopySelection);
        Assert.False(capabilities.CanPaste);
        Assert.True(capabilities.CanSaveWorkspace);
        Assert.True(capabilities.CanLoadWorkspace);
        Assert.True(capabilities.CanSetSelection);
        Assert.True(capabilities.CanMoveNodes);
        Assert.True(capabilities.CanCreateConnections);
        Assert.True(capabilities.CanDeleteConnections);
        Assert.True(capabilities.CanBreakConnections);
        Assert.True(capabilities.CanUpdateViewport);
        Assert.True(capabilities.CanFitToViewport);
        Assert.True(capabilities.CanCenterViewport);
    }

    [Fact]
    public void RuntimeSession_CenterViewOnNode_PublishesViewportChanged()
    {
        var definitionId = new NodeDefinitionId("tests.transaction.center-node");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        GraphEditorViewportChangedEventArgs? viewportChanged = null;

        session.Commands.UpdateViewportSize(1280, 720);
        session.Events.ViewportChanged += (_, args) => viewportChanged = args;

        session.Commands.CenterViewOnNode(SourceNodeId);

        Assert.NotNull(viewportChanged);
    }

    [Fact]
    public void RuntimeSession_PublishesRecoverableFailuresThroughSharedFacade()
    {
        var definitionId = new NodeDefinitionId("tests.transaction.failure");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId, new ThrowingAugmentor()));
        var session = editor.Session;
        GraphEditorRecoverableFailureEventArgs? failure = null;

        session.Events.RecoverableFailure += (_, args) => failure = args;

        var menu = editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90)));

        Assert.NotEmpty(menu);
        Assert.NotNull(failure);
        Assert.Equal("contextmenu.augment.failed", failure!.Code);
        Assert.Equal("contextmenu.augment", failure.Operation);
        Assert.Contains("augmentor", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AsterGraphEditorOptions CreateOptions(NodeDefinitionId definitionId, IGraphContextMenuAugmentor? augmentor = null)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
            ContextMenuAugmentor = augmentor,
        };

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Transaction Graph",
            "Runtime batching regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Transaction Source",
                    "Tests",
                    "Runtime",
                    "Transaction source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Transaction Target",
                    "Tests",
                    "Runtime",
                    "Transaction target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
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
            "Transaction Node",
            "Tests",
            "Runtime",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private sealed class ThrowingAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => throw new InvalidOperationException("augmentor exploded");
    }
}
