using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorFacadeRefactorTests
{
    [Fact]
    public void EditorAssembly_ContainsDedicatedSelectionCoordinator()
    {
        var coordinatorType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorSelectionCoordinator");

        Assert.NotNull(coordinatorType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedSelectionStateSynchronizer()
    {
        var synchronizerType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorSelectionStateSynchronizer");

        Assert.NotNull(synchronizerType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedSelectionProjectionApplier()
    {
        var applierType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorSelectionProjectionApplier");

        Assert.NotNull(applierType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedDocumentCollectionSynchronizer()
    {
        var synchronizerType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorDocumentCollectionSynchronizer");

        Assert.NotNull(synchronizerType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedNodePositionDirtyTracker()
    {
        var trackerType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorNodePositionDirtyTracker");

        Assert.NotNull(trackerType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedRetainedEventPublisher()
    {
        var publisherType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorRetainedEventPublisher");

        Assert.NotNull(publisherType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedNodeLayoutCoordinator()
    {
        var coordinatorType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorNodeLayoutCoordinator");

        Assert.NotNull(coordinatorType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedPresentationLocalizationCoordinator()
    {
        var coordinatorType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorPresentationLocalizationCoordinator");

        Assert.NotNull(coordinatorType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedPresentationLocalizationCoordinatorHost()
    {
        var hostType = typeof(GraphEditorViewModel).GetNestedType(
            "GraphEditorViewModelPresentationLocalizationCoordinatorHost",
            System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(hostType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedWorkspaceSaveCoordinator()
    {
        var coordinatorType = typeof(GraphEditorViewModel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorWorkspaceSaveCoordinator");

        Assert.NotNull(coordinatorType);
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedPersistenceCoordinatorHost()
    {
        var hostType = typeof(GraphEditorViewModel).GetNestedType(
            "GraphEditorViewModelPersistenceCoordinatorHost",
            System.Reflection.BindingFlags.NonPublic);

        Assert.NotNull(hostType);
    }

    [Fact]
    public void GraphEditorViewModel_DoesNotImplementPersistenceHostInterfaces()
    {
        var viewModelType = typeof(GraphEditorViewModel);
        var implementedInterfaces = viewModelType.GetInterfaces();

        Assert.DoesNotContain(
            implementedInterfaces,
            iface => iface.FullName == "AsterGraph.Editor.Services.IGraphEditorDocumentLoadCoordinatorHost");

        Assert.DoesNotContain(
            implementedInterfaces,
            iface => iface.FullName == "AsterGraph.Editor.Services.IGraphEditorWorkspaceSaveCoordinatorHost");
    }

    [Fact]
    public void GraphEditorViewModel_DoesNotImplementPresentationLocalizationHostInterface()
    {
        var viewModelType = typeof(GraphEditorViewModel);
        var implementedInterfaces = viewModelType.GetInterfaces();

        Assert.DoesNotContain(
            implementedInterfaces,
            iface => iface.FullName == "AsterGraph.Editor.Services.IGraphEditorPresentationLocalizationCoordinatorHost");
    }

    [Fact]
    public void GraphEditorViewModel_RebuildsMixedParametersThroughPublicSelectionPath()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.public-path");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);

        editor.SetSelection([editor.Nodes[0], editor.Nodes[1]], editor.Nodes[0], status: null);

        Assert.Equal(2, editor.SelectedNodeParameters.Count);
        Assert.Contains(editor.SelectedNodeParameters, parameter => parameter.Key == "threshold" && parameter.HasMixedValues);
        Assert.Contains(editor.SelectedNodeParameters, parameter => parameter.Key == "enabled" && !parameter.HasMixedValues);
    }

    [Fact]
    public void BuildSelectedNodeParameters_PreservesMixedValuesForSharedDefinitionSelection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.parameter-node");
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Parameter Node",
            "Tests",
            "Batch Edit",
            [],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "threshold",
                    "Threshold",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 0.5),
                new NodeParameterDefinition(
                    "enabled",
                    "Enabled",
                    new PortTypeId("bool"),
                    ParameterEditorKind.Boolean,
                    defaultValue: true),
            ]));

        var selectedNodes = new[]
        {
            CreateNode("node-001", definitionId, threshold: 0.25, enabled: true),
            CreateNode("node-002", definitionId, threshold: 0.75, enabled: true),
        };

        var projection = new GraphEditorSelectionProjection();
        var parameters = projection.BuildSelectedNodeParameters(
            selectedNodes,
            catalog,
            enableBatchParameterEditing: true,
            canEditNodeParameters: true,
            (_, _) => { });

        Assert.Equal(2, parameters.Count);
        Assert.Contains(parameters, parameter => parameter.Key == "threshold" && parameter.HasMixedValues);
        Assert.Contains(parameters, parameter => parameter.Key == "enabled" && !parameter.HasMixedValues);
    }

    [Fact]
    public void FormatRelatedNodes_ReturnsDistinctReadableLinesAndNoneFallback()
    {
        var outputNode = CreateNode("node-out", new NodeDefinitionId("tests.editor.facade.out"), 1, true, outputLabel: "Color");
        var inputNode = CreateNode("node-in", new NodeDefinitionId("tests.editor.facade.in"), 1, true, inputLabel: "Tint");
        var byId = new Dictionary<string, NodeViewModel>(StringComparer.Ordinal)
        {
            [outputNode.Id] = outputNode,
            [inputNode.Id] = inputNode,
        };

        var projection = new GraphEditorSelectionProjection();
        var related = projection.FormatRelatedNodes(
            [
                new ConnectionViewModel("c-001", "node-out", "out", "node-in", "in", "link", "#55D8C1"),
                new ConnectionViewModel("c-002", "node-out", "out", "node-in", "in", "link", "#55D8C1"),
            ],
            useSource: true,
            byId.GetValueOrDefault);

        Assert.Equal("Output node  ·  Color", related);

        var none = projection.FormatRelatedNodes([], useSource: false, byId.GetValueOrDefault);
        Assert.Equal("None", none);
    }

    [Fact]
    public void SetHostContext_RaisesHostContextPropertyChangedOnlyWhenValueChanges()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.host-context");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var changedProperties = new List<string?>();
        editor.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        var hostContextA = new TestGraphHostContext(new object(), null);
        var hostContextB = new TestGraphHostContext(new object(), null);

        editor.SetHostContext(hostContextA);
        editor.SetHostContext(hostContextA);
        editor.SetHostContext(hostContextB);

        Assert.Equal(
            2,
            changedProperties.Count(propertyName => propertyName == nameof(GraphEditorViewModel.HostContext)));
    }

    [Fact]
    public void SelectedNodesCollectionChange_ReconcilesPrimarySelectionAndNodeFlags()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.selection-collection");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var first = editor.Nodes[0];
        var second = editor.Nodes[1];

        editor.SelectedNodes.Add(first);
        editor.SelectedNodes.Add(second);

        Assert.True(first.IsSelected);
        Assert.True(second.IsSelected);
        Assert.Same(first, editor.SelectedNode);

        editor.SelectedNodes.Remove(second);

        Assert.True(first.IsSelected);
        Assert.False(second.IsSelected);
        Assert.Same(first, editor.SelectedNode);
    }

    [Fact]
    public void NodesCollectionChange_CoercesSelectionToExistingNodes()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.selection-coerce");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var first = editor.Nodes[0];
        var second = editor.Nodes[1];

        editor.SetSelection([first, second], second, status: null);
        editor.Nodes.Remove(second);

        Assert.Equal([first], editor.SelectedNodes);
        Assert.Same(first, editor.SelectedNode);
        Assert.True(first.IsSelected);
        Assert.Equal("1 inputs  ·  1 outputs", editor.SelectionCaption);
        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);
        Assert.Equal("None", editor.InspectorDownstream);
    }

    [Fact]
    public void SelectedNodeChange_RebuildsInspectorProjectionAndBatchParameterProjection()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.selection-primary-refresh");
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            firstTitle: "Source node",
            secondTitle: "Target node");
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];

        editor.Connections.Add(new ConnectionViewModel("c-001", source.Id, "out", target.Id, "in", "link", "#55D8C1"));
        editor.SetSelection([source, target], source, status: null);

        Assert.Equal("2 nodes selected  ·  primary Source node", editor.SelectionCaption);
        Assert.Equal("0 incoming  ·  1 outgoing", editor.InspectorConnections);
        Assert.Contains("Target node", editor.InspectorDownstream, StringComparison.Ordinal);
        Assert.Contains("Input", editor.InspectorDownstream, StringComparison.Ordinal);
        Assert.Equal(2, editor.SelectedNodeParameters.Count);

        editor.SelectedNode = target;

        Assert.Equal("2 nodes selected  ·  primary Target node", editor.SelectionCaption);
        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Equal(2, editor.SelectedNodeParameters.Count);
    }

    [Fact]
    public void ConnectionsCollectionChange_RefreshesInspectorProjectionForSelectedNode()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.connection-refresh");
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            firstTitle: "Source node",
            secondTitle: "Target node");
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];
        var connection = new ConnectionViewModel("c-001", source.Id, "out", target.Id, "in", "link", "#55D8C1");

        editor.SelectSingleNode(target, updateStatus: false);

        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);

        editor.Connections.Add(connection);

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);

        editor.Connections.Remove(connection);

        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);
    }

    [Fact]
    public void SelectedNodeChange_RaisesEditableParameterProjectionPropertyChanged()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.parameter-projection-notify");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var first = editor.Nodes[0];
        var second = editor.Nodes[1];
        var changedProperties = new List<string?>();

        editor.SetSelection([first, second], first, status: null);
        editor.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        editor.SelectedNode = second;

        Assert.Contains(nameof(GraphEditorViewModel.HasEditableParameters), changedProperties);
        Assert.Contains(nameof(GraphEditorViewModel.HasBatchEditableParameters), changedProperties);
    }

    [Fact]
    public void NodesCollectionChange_RefreshesFitViewCommandAvailability()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.fit-view-refresh");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);

        editor.UpdateViewportSize(1280, 720);
        Assert.True(editor.FitViewCommand.CanExecute(null));

        editor.Nodes.Clear();

        Assert.False(editor.FitViewCommand.CanExecute(null));
    }

    [Fact]
    public void NodePositionChange_MarksEditorDirtyAndRefreshesWorkspaceCaption()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.node-position-dirty");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var node = editor.Nodes[0];
        var changedProperties = new List<string?>();

        Assert.False(editor.IsDirty);
        editor.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        node.X += 24;

        Assert.True(editor.IsDirty);
        Assert.Contains("Unsaved changes", editor.WorkspaceCaption, StringComparison.Ordinal);
        Assert.Contains(nameof(GraphEditorViewModel.WorkspaceCaption), changedProperties);
        Assert.Equal(1, changedProperties.Count(propertyName => propertyName == nameof(GraphEditorViewModel.WorkspaceCaption)));
    }

    [Fact]
    public void GraphEditorViewModel_PanBy_PublishesRetainedViewportChanged()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.retained-viewport");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var viewportChanges = new List<GraphEditorViewportChangedEventArgs>();

        editor.UpdateViewportSize(1280, 720);
        editor.ViewportChanged += (_, args) => viewportChanges.Add(args);

        editor.PanBy(16, 12);

        var viewportChanged = Assert.Single(viewportChanges);
        Assert.Equal(0.88, viewportChanged.Zoom);
        Assert.Equal(126, viewportChanged.PanX);
        Assert.Equal(108, viewportChanged.PanY);
        Assert.Equal(1280, viewportChanged.ViewportWidth);
        Assert.Equal(720, viewportChanged.ViewportHeight);
    }

    [Fact]
    public void GraphEditorViewModel_PanBy_PublishesRetainedAndSessionViewportEvents()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.retained-session-viewport");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var retainedViewportChanges = new List<GraphEditorViewportChangedEventArgs>();
        var sessionViewportChanges = new List<GraphEditorViewportChangedEventArgs>();

        editor.UpdateViewportSize(1280, 720);
        editor.ViewportChanged += (_, args) => retainedViewportChanges.Add(args);
        editor.Session.Events.ViewportChanged += (_, args) => sessionViewportChanges.Add(args);

        editor.PanBy(16, 12);

        var retainedViewportChanged = Assert.Single(retainedViewportChanges);
        var sessionViewportChanged = Assert.Single(sessionViewportChanges);
        Assert.Equal(sessionViewportChanged.Zoom, retainedViewportChanged.Zoom);
        Assert.Equal(sessionViewportChanged.PanX, retainedViewportChanged.PanX);
        Assert.Equal(sessionViewportChanged.PanY, retainedViewportChanged.PanY);
        Assert.Equal(sessionViewportChanged.ViewportWidth, retainedViewportChanged.ViewportWidth);
        Assert.Equal(sessionViewportChanged.ViewportHeight, retainedViewportChanged.ViewportHeight);
    }

    [Fact]
    public void GraphEditorViewModel_SelectSingleNode_PublishesRetainedSelectionChangedOnce()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.retained-selection");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var selectionChanges = new List<GraphEditorSelectionChangedEventArgs>();
        var target = editor.Nodes[1];

        editor.SelectionChanged += (_, args) => selectionChanges.Add(args);

        editor.SelectSingleNode(target, updateStatus: false);

        var selectionChanged = Assert.Single(selectionChanges);
        Assert.Equal([target.Id], selectionChanged.SelectedNodeIds);
        Assert.Equal(target.Id, selectionChanged.PrimarySelectedNodeId);
    }

    [Fact]
    public void GraphEditorViewModel_CancelPendingConnection_PublishesRetainedPendingConnectionChangedOnce()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.retained-pending");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var pendingChanges = new List<GraphEditorPendingConnectionChangedEventArgs>();
        var source = editor.Nodes[0];

        editor.StartConnection(source.Id, "out");
        editor.PendingConnectionChanged += (_, args) => pendingChanges.Add(args);

        editor.CancelPendingConnection(status: null);

        var pendingChanged = Assert.Single(pendingChanges);
        Assert.False(pendingChanged.PendingConnection.HasPendingConnection);
        Assert.Null(pendingChanged.PendingConnection.SourceNodeId);
        Assert.Null(pendingChanged.PendingConnection.SourcePortId);
    }

    [Fact]
    public void GraphEditorViewModel_AlignSelectionLeft_PublishesRetainedDocumentChangedOnce()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.retained-document");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];
        var documentChanges = new List<GraphEditorDocumentChangedEventArgs>();

        editor.SetSelection([source, target], source, status: null);
        editor.DocumentChanged += (_, args) => documentChanges.Add(args);

        editor.AlignSelectionLeft();

        var documentChanged = Assert.Single(documentChanges);
        Assert.Equal(GraphEditorDocumentChangeKind.LayoutChanged, documentChanged.ChangeKind);
        Assert.Equal([source.Id, target.Id], documentChanged.NodeIds);
        Assert.Equal(editor.StatusMessage, documentChanged.StatusMessage);
    }

    [Fact]
    public void GraphEditorViewModel_AlignSelectionLeft_PublishesDirtyStateBeforeRetainedDocumentChanged()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-event-order");
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            secondPosition: new GraphPoint(420, 220));
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];
        var dirtyStatesDuringEvents = new List<bool>();

        editor.SetSelection([source, target], source, status: null);
        editor.DocumentChanged += (_, _) => dirtyStatesDuringEvents.Add(editor.IsDirty);

        editor.AlignSelectionLeft();

        Assert.Equal([true], dirtyStatesDuringEvents);
        Assert.True(editor.IsDirty);
    }

    [Fact]
    public void GraphEditorViewModel_ApplyDragOffset_WhenMoveDisabled_DoesNotChangeNodePositions()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-drag-permissions");
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            behaviorOptions: GraphEditorBehaviorOptions.Default with
            {
                Commands = GraphEditorCommandPermissions.ReadOnly,
            });
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];
        var sourceOrigin = new GraphPoint(source.X, source.Y);
        var targetOrigin = new GraphPoint(target.X, target.Y);

        editor.ApplyDragOffset(
            new Dictionary<string, GraphPoint>(StringComparer.Ordinal)
            {
                [source.Id] = sourceOrigin,
                [target.Id] = targetOrigin,
            },
            64,
            32);

        Assert.Equal(sourceOrigin.X, source.X);
        Assert.Equal(sourceOrigin.Y, source.Y);
        Assert.Equal(targetOrigin.X, target.X);
        Assert.Equal(targetOrigin.Y, target.Y);
    }

    [Fact]
    public void GraphEditorViewModel_GetNodesInRectangle_NormalizesCornerOrder()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-rectangle");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];

        source.X = 120;
        source.Y = 80;
        target.X = 420;
        target.Y = 260;

        var selectedNodes = editor.GetNodesInRectangle(
            new GraphPoint(target.X + target.Width + 20, target.Y + target.Height + 20),
            new GraphPoint(target.X - 20, target.Y - 20));

        Assert.Equal([target], selectedNodes);
    }

    [Fact]
    public void GraphEditorViewModel_AlignSelectionLeft_WithSingleSelection_DoesNotPublishDocumentChanged()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-align-single");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var source = editor.Nodes[0];
        var documentChanges = new List<GraphEditorDocumentChangedEventArgs>();

        editor.SelectSingleNode(source, updateStatus: false);
        editor.DocumentChanged += (_, args) => documentChanges.Add(args);

        editor.AlignSelectionLeft();

        Assert.Empty(documentChanges);
        Assert.Contains("Select at least two", editor.StatusMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphEditorViewModel_TrySetNodePosition_WhenPositionUnchanged_ReturnsTrueWithoutChangingState()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-position-noop");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var source = editor.Nodes[0];
        var origin = new GraphPoint(source.X, source.Y);

        editor.StatusMessage = "Keep existing status.";

        var result = editor.TrySetNodePosition(source.Id, origin, updateStatus: false);

        Assert.True(result);
        Assert.Equal(origin.X, source.X);
        Assert.Equal(origin.Y, source.Y);
        Assert.Equal("Keep existing status.", editor.StatusMessage);
    }

    [Fact]
    public void GraphEditorViewModel_TrySetNodePosition_WhenPositionChangesAndUpdateStatusDisabled_PreservesKernelStatusMessage()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-position-update-no-status");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var source = editor.Nodes[0];
        var firstPosition = new GraphPoint(source.X + 16, source.Y + 12);
        var secondPosition = new GraphPoint(source.X + 64, source.Y + 28);

        Assert.True(editor.TrySetNodePosition(source.Id, firstPosition, updateStatus: true));
        var preservedStatus = editor.StatusMessage;

        var result = editor.TrySetNodePosition(source.Id, secondPosition, updateStatus: false);
        var movedNode = Assert.IsType<NodeViewModel>(editor.FindNode(source.Id));

        Assert.True(result);
        Assert.Equal(secondPosition.X, movedNode.X);
        Assert.Equal(secondPosition.Y, movedNode.Y);
        Assert.Equal(preservedStatus, editor.StatusMessage);
    }

    [Fact]
    public void GraphEditorViewModel_MoveNode_WithMultiSelectionMovesOnlySelectedNodes()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-move-multi-selection");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);
        var source = editor.Nodes[0];
        var target = editor.Nodes[1];
        var unaffected = CreateNode("node-003", definitionId, threshold: 0.5, enabled: true, title: "Unaffected node");
        var sourceOrigin = new GraphPoint(source.X, source.Y);
        var targetOrigin = new GraphPoint(target.X, target.Y);
        var unaffectedOrigin = new GraphPoint(unaffected.X, unaffected.Y);

        unaffected.X = 720;
        unaffected.Y = 420;
        unaffectedOrigin = new GraphPoint(unaffected.X, unaffected.Y);
        editor.Nodes.Add(unaffected);

        editor.SetSelection([source, target], source, status: null);

        editor.MoveNode(source, 36, 18);

        Assert.Equal(sourceOrigin.X + 36, source.X);
        Assert.Equal(sourceOrigin.Y + 18, source.Y);
        Assert.Equal(targetOrigin.X + 36, target.X);
        Assert.Equal(targetOrigin.Y + 18, target.Y);
        Assert.Equal(unaffectedOrigin.X, unaffected.X);
        Assert.Equal(unaffectedOrigin.Y, unaffected.Y);
    }

    [Fact]
    public void GraphEditorViewModel_SetNodePositions_WhenNoMatchingNodes_ReturnsZeroAndSetsStatus()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.layout-position-no-match");
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId);

        var appliedCount = editor.SetNodePositions(
            [
                new NodePositionSnapshot("missing-node", new GraphPoint(480, 320)),
            ],
            updateStatus: true);

        Assert.Equal(0, appliedCount);
        Assert.Contains("No matching nodes", editor.StatusMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphEditorViewModel_SaveWorkspace_WhenSaveDisabled_DoesNotWriteWorkspace()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.workspace-save-disabled");
        var workspace = new RecordingWorkspaceService();
        var editor = CreateEditorWithSharedDefinitionNodes(
            definitionId,
            behaviorOptions: GraphEditorBehaviorOptions.Default with
            {
                Commands = GraphEditorCommandPermissions.Default with
                {
                    Workspace = new WorkspaceCommandPermissions
                    {
                        AllowSave = false,
                        AllowLoad = true,
                    },
                },
            },
            workspaceService: workspace);

        editor.SaveWorkspace();

        Assert.Equal(0, workspace.SaveCalls);
        Assert.Contains("Saving is disabled", editor.StatusMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphEditorViewModel_SaveWorkspace_Failure_PublishesRecoverableFailureAndPreservesDirtyState()
    {
        var definitionId = new NodeDefinitionId("tests.editor.facade.workspace-save-failure");
        var workspace = new ThrowingWorkspaceService();
        var editor = CreateEditorWithSharedDefinitionNodes(definitionId, workspaceService: workspace);
        var diagnostics = new List<GraphEditorDiagnostic>();
        GraphEditorRecoverableFailureEventArgs? failure = null;

        editor.Nodes[0].X += 24;
        Assert.True(editor.IsDirty);

        editor.DiagnosticPublished += diagnostics.Add;
        editor.RecoverableFailureRaised += (_, args) => failure = args;

        editor.SaveWorkspace();

        Assert.True(editor.IsDirty);
        Assert.NotNull(failure);
        Assert.Equal("workspace.save.failed", failure!.Code);
        Assert.Single(diagnostics);
        Assert.Equal("workspace.save.failed", diagnostics[0].Code);
        Assert.Equal(GraphEditorDiagnosticSeverity.Warning, diagnostics[0].Severity);
    }

    private static NodeViewModel CreateNode(
        string nodeId,
        NodeDefinitionId definitionId,
        double threshold,
        bool enabled,
        string? title = null,
        string inputLabel = "Input",
        string outputLabel = "Output")
    {
        return new NodeViewModel(new GraphNode(
            nodeId,
            title ?? (nodeId == "node-out" ? "Output node" : "Input node"),
            "Tests",
            "Facade",
            "Node for projection tests.",
            new GraphPoint(120, 80),
            new GraphSize(220, 160),
            [
                new GraphPort("in", inputLabel, PortDirection.Input, "float", "#55D8C1", new PortTypeId("float")),
            ],
            [
                new GraphPort("out", outputLabel, PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
            ],
            "#55D8C1",
            definitionId,
            [
                new GraphParameterValue("threshold", new PortTypeId("float"), threshold),
                new GraphParameterValue("enabled", new PortTypeId("bool"), enabled),
            ]));
    }

    private static GraphEditorViewModel CreateEditorWithSharedDefinitionNodes(
        NodeDefinitionId definitionId,
        string firstTitle = "Input node",
        string secondTitle = "Input node",
        GraphEditorBehaviorOptions? behaviorOptions = null,
        GraphPoint? firstPosition = null,
        GraphPoint? secondPosition = null,
        IGraphWorkspaceService? workspaceService = null)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Parameter Node",
            "Tests",
            "Public Path",
            [],
            [],
            parameters:
            [
                new NodeParameterDefinition(
                    "threshold",
                    "Threshold",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 0.5),
                new NodeParameterDefinition(
                    "enabled",
                    "Enabled",
                    new PortTypeId("bool"),
                    ParameterEditorKind.Boolean,
                    defaultValue: true),
            ]));

        var document = new GraphDocument(
            "Facade Test",
            "Regression coverage for editor facade projection path.",
            [
                CreateConfiguredNode("node-001", definitionId, firstTitle, threshold: 0.25, enabled: true, firstPosition).ToModel(),
                CreateConfiguredNode("node-002", definitionId, secondTitle, threshold: 0.75, enabled: true, secondPosition).ToModel(),
            ],
            []);

        return new GraphEditorViewModel(
            document,
            catalog,
            new DefaultPortCompatibilityService(),
            workspaceService: workspaceService,
            behaviorOptions: behaviorOptions);
    }

    private static NodeViewModel CreateConfiguredNode(
        string nodeId,
        NodeDefinitionId definitionId,
        string title,
        double threshold,
        bool enabled,
        GraphPoint? position)
    {
        var node = CreateNode(nodeId, definitionId, threshold, enabled, title: title);
        if (position is { } resolvedPosition)
        {
            node.X = resolvedPosition.X;
            node.Y = resolvedPosition.Y;
        }

        return node;
    }

    private sealed record TestGraphHostContext(object Owner, object? TopLevel) : IGraphHostContext
    {
        public IServiceProvider? Services => null;
    }

    private sealed class RecordingWorkspaceService : IGraphWorkspaceService
    {
        public int SaveCalls { get; private set; }

        public string WorkspacePath { get; } = "workspace://facade-refactor";

        public bool Exists() => false;

        public GraphDocument Load() => throw new InvalidOperationException("Load should not be called in this test.");

        public void Save(GraphDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            SaveCalls++;
        }
    }

    private sealed class ThrowingWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath { get; } = "workspace://facade-refactor-failure";

        public bool Exists() => false;

        public GraphDocument Load() => throw new InvalidOperationException("Load should not be called in this test.");

        public void Save(GraphDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            throw new InvalidOperationException("save failed on purpose");
        }
    }
}
