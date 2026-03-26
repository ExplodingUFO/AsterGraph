using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorTransactionTests
{
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
        Assert.Equal(2, session.Queries.CreateDocumentSnapshot().Nodes.Count);
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
                    "tests.transaction.node-001",
                    "Transaction Node",
                    "Tests",
                    "Runtime",
                    "Transaction test node.",
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
            "Transaction Node",
            "Tests",
            "Runtime",
            [],
            []));
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
