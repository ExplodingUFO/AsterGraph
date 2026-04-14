using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel.Internal;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.Kernel;

internal sealed class GraphEditorKernel : IGraphEditorSessionHost, IGraphEditorKernelCommandRouterHost, IGraphEditorWorkspaceSaveCoordinatorHost
{
    private const double DefaultZoom = 0.88;
    private const double DefaultPanX = 110;
    private const double DefaultPanY = 96;

    private readonly INodeCatalog _nodeCatalog;
    private readonly IPortCompatibilityService _compatibilityService;
    private readonly IGraphWorkspaceService _workspaceService;
    private readonly GraphEditorHistoryService _historyService = new();
    private readonly GraphEditorKernelViewportCoordinator _viewportCoordinator = new(DefaultZoom, DefaultPanX, DefaultPanY);
    private readonly GraphEditorKernelCompatibilityQueries _compatibilityQueries;
    private readonly GraphEditorKernelDocumentMutator _documentMutator = new();
    private readonly GraphEditorKernelCommandRouter _commandRouter;
    private readonly GraphEditorWorkspaceSaveCoordinator _workspaceSaveCoordinator;
    private GraphEditorBehaviorOptions _behaviorOptions;
    private readonly GraphEditorStyleOptions _styleOptions;
    private GraphDocument _document;
    private List<string> _selectedNodeIds = [];
    private string? _primarySelectedNodeId;
    private GraphEditorPendingConnectionSnapshot _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
    private string? _lastSavedDocumentSignature;
    private double _zoom = DefaultZoom;
    private double _panX = DefaultPanX;
    private double _panY = DefaultPanY;
    private double _viewportWidth;
    private double _viewportHeight;

    public GraphEditorKernel(
        GraphDocument document,
        INodeCatalog nodeCatalog,
        IPortCompatibilityService compatibilityService,
        IGraphWorkspaceService workspaceService,
        GraphEditorStyleOptions styleOptions,
        GraphEditorBehaviorOptions behaviorOptions)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(nodeCatalog);
        ArgumentNullException.ThrowIfNull(compatibilityService);
        ArgumentNullException.ThrowIfNull(workspaceService);

        _document = CloneDocument(document);
        _nodeCatalog = nodeCatalog;
        _compatibilityService = compatibilityService;
        _workspaceService = workspaceService;
        _compatibilityQueries = new GraphEditorKernelCompatibilityQueries(compatibilityService);
        _styleOptions = styleOptions;
        _behaviorOptions = behaviorOptions;
        _commandRouter = new GraphEditorKernelCommandRouter(this);
        _workspaceSaveCoordinator = new GraphEditorWorkspaceSaveCoordinator(this);
        _lastSavedDocumentSignature = CreateDocumentSignature(_document);
        _historyService.Reset(CaptureHistoryState());
        CurrentStatusMessage = "Ready to edit.";
    }

    public void UpdateBehaviorOptions(GraphEditorBehaviorOptions behaviorOptions)
    {
        ArgumentNullException.ThrowIfNull(behaviorOptions);

        _behaviorOptions = behaviorOptions;
        if (_pendingConnection.HasPendingConnection && !_behaviorOptions.Commands.Connections.AllowCreate)
        {
            CancelPendingConnection();
        }
    }

    public event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;
    public event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported
    {
        add { }
        remove { }
    }

    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported
    {
        add { }
        remove { }
    }
    public event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged;
    public event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailureRaised;
    public event Action<GraphEditorDiagnostic>? DiagnosticPublished;

    public string CurrentStatusMessage { get; private set; } = string.Empty;

    bool IGraphEditorWorkspaceSaveCoordinatorHost.CanSaveWorkspace
        => _behaviorOptions.Commands.Workspace.AllowSave;

    string IGraphEditorWorkspaceSaveCoordinatorHost.WorkspacePath
        => _workspaceService.WorkspacePath;

    GraphDocument IGraphEditorWorkspaceSaveCoordinatorHost.CreateWorkspaceDocumentSnapshot()
        => CloneDocument(_document);

    void IGraphEditorWorkspaceSaveCoordinatorHost.SaveWorkspaceDocument(GraphDocument document)
        => _workspaceService.Save(document);

    string IGraphEditorWorkspaceSaveCoordinatorHost.CreateWorkspaceDocumentSignature(GraphDocument document)
        => CreateDocumentSignature(document);

    void IGraphEditorWorkspaceSaveCoordinatorHost.SetWorkspaceSaveDisabledStatus()
        => CurrentStatusMessage = "Saving is disabled by host permissions.";

    void IGraphEditorWorkspaceSaveCoordinatorHost.HandleWorkspaceSaveSucceeded(string signature, string statusMessage)
    {
        _lastSavedDocumentSignature = signature;
        CurrentStatusMessage = statusMessage;
        DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
            "workspace.save.succeeded",
            "workspace.save",
            CurrentStatusMessage,
            GraphEditorDiagnosticSeverity.Info));
        DocumentChanged?.Invoke(this, new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.WorkspaceSaved, statusMessage: CurrentStatusMessage));
    }

    void IGraphEditorWorkspaceSaveCoordinatorHost.HandleWorkspaceSaveFailed(string statusMessage, Exception exception)
    {
        CurrentStatusMessage = statusMessage;
        RecoverableFailureRaised?.Invoke(
            this,
            new GraphEditorRecoverableFailureEventArgs(
                "workspace.save.failed",
                "workspace.save",
                CurrentStatusMessage,
                exception));
    }

    void IGraphEditorWorkspaceSaveCoordinatorHost.CompleteWorkspaceSaveAttempt()
    {
    }

    GraphEditorBehaviorOptions IGraphEditorKernelCommandRouterHost.BehaviorOptions => _behaviorOptions;

    GraphDocument IGraphEditorKernelCommandRouterHost.Document => _document;

    int IGraphEditorKernelCommandRouterHost.SelectedNodeCount => _selectedNodeIds.Count;

    GraphEditorPendingConnectionSnapshot IGraphEditorKernelCommandRouterHost.PendingConnection => _pendingConnection;

    double IGraphEditorKernelCommandRouterHost.ViewportWidth => _viewportWidth;

    double IGraphEditorKernelCommandRouterHost.ViewportHeight => _viewportHeight;

    bool IGraphEditorKernelCommandRouterHost.WorkspaceExists => _workspaceService.Exists();

    internal bool IsDirty
        => !string.Equals(CreateDocumentSignature(_document), _lastSavedDocumentSignature, StringComparison.Ordinal);

    public void Undo()
    {
        if (!_behaviorOptions.History.EnableUndoRedo || !_behaviorOptions.Commands.History.AllowUndo)
        {
            CurrentStatusMessage = "Undo is disabled by host permissions.";
            return;
        }

        if (!_historyService.TryUndo(out var state) || state is null)
        {
            CurrentStatusMessage = "No more undo steps.";
            return;
        }

        RestoreHistoryState(state, "Undo applied.", GraphEditorDocumentChangeKind.Undo);
    }

    public void Redo()
    {
        if (!_behaviorOptions.History.EnableUndoRedo || !_behaviorOptions.Commands.History.AllowRedo)
        {
            CurrentStatusMessage = "Redo is disabled by host permissions.";
            return;
        }

        if (!_historyService.TryRedo(out var state) || state is null)
        {
            CurrentStatusMessage = "No more redo steps.";
            return;
        }

        RestoreHistoryState(state, "Redo applied.", GraphEditorDocumentChangeKind.Redo);
    }

    public void ClearSelection(bool updateStatus)
        => SetSelection([], null, updateStatus);

    public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
    {
        var existingIds = _document.Nodes.Select(node => node.Id).ToHashSet(StringComparer.Ordinal);
        var selectedIds = nodeIds
            .Where(existingIds.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var nextPrimary = !string.IsNullOrWhiteSpace(primaryNodeId) && selectedIds.Contains(primaryNodeId, StringComparer.Ordinal)
            ? primaryNodeId
            : selectedIds.LastOrDefault();

        if (_selectedNodeIds.SequenceEqual(selectedIds, StringComparer.Ordinal)
            && string.Equals(_primarySelectedNodeId, nextPrimary, StringComparison.Ordinal))
        {
            return;
        }

        _selectedNodeIds = selectedIds;
        _primarySelectedNodeId = nextPrimary;
        if (updateStatus)
        {
            CurrentStatusMessage = selectedIds.Count == 0
                ? "Selection cleared."
                : $"Selected {selectedIds.Count} node{(selectedIds.Count == 1 ? string.Empty : "s")}.";
        }

        SelectionChanged?.Invoke(this, new GraphEditorSelectionChangedEventArgs(_selectedNodeIds.ToList(), _primarySelectedNodeId));
    }

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
    {
        ArgumentNullException.ThrowIfNull(definitionId);

        if (!_behaviorOptions.Commands.Nodes.AllowCreate)
        {
            CurrentStatusMessage = "Node creation is disabled by host permissions.";
            return;
        }

        if (!_nodeCatalog.TryGetDefinition(definitionId, out var definition) || definition is null)
        {
            throw new InvalidOperationException($"Node definition '{definitionId}' is not registered in the current editor catalog.");
        }

        var position = preferredWorldPosition ?? GetViewportCenter();
        var offset = 26 * (_document.Nodes.Count % 4);
        var node = new GraphNode(
            CreateNodeId(definitionId),
            definition.DisplayName,
            definition.Category,
            definition.Subtitle,
            definition.Description ?? definition.Subtitle,
            new GraphPoint(
                position.X - (definition.DefaultWidth / 2) + offset,
                position.Y - (definition.DefaultHeight / 2) + offset),
            new GraphSize(definition.DefaultWidth, definition.DefaultHeight),
            definition.InputPorts.Select(port => new GraphPort(port.Key, port.DisplayName, PortDirection.Input, port.TypeId.Value, port.AccentHex, port.TypeId)).ToList(),
            definition.OutputPorts.Select(port => new GraphPort(port.Key, port.DisplayName, PortDirection.Output, port.TypeId.Value, port.AccentHex, port.TypeId)).ToList(),
            definition.AccentHex,
            definition.Id,
            definition.Parameters.Select(parameter => new GraphParameterValue(parameter.Key, parameter.ValueType, parameter.DefaultValue)).ToList());

        _document = _document with
        {
            Nodes = _document.Nodes.Concat([node]).ToList(),
        };
        SetSelection([node.Id], node.Id, updateStatus: false);
        MarkDirty($"Added {node.Title}.", GraphEditorDocumentChangeKind.NodesAdded, [node.Id], null);
    }

    public void DeleteNodeById(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_behaviorOptions.Commands.Nodes.AllowDelete)
        {
            CurrentStatusMessage = "Node deletion is disabled by host permissions.";
            return;
        }

        var mutation = _documentMutator.DeleteNode(_document, nodeId);
        if (mutation.Node is null)
        {
            return;
        }

        if (mutation.RemovedConnections.Count > 0 && !CanRemoveConnectionsAsSideEffect())
        {
            CurrentStatusMessage = "Deleting a connected node requires delete or disconnect permission for the affected links.";
            return;
        }

        _document = mutation.Document;
        if (string.Equals(_pendingConnection.SourceNodeId, mutation.Node.Id, StringComparison.Ordinal))
        {
            CancelPendingConnection();
        }

        var remainingSelection = _selectedNodeIds
            .Where(selectedNodeId => !string.Equals(selectedNodeId, mutation.Node.Id, StringComparison.Ordinal))
            .ToList();
        SetSelection(remainingSelection, remainingSelection.LastOrDefault(), updateStatus: false);
        MarkDirty(
            $"Deleted {mutation.Node.Title}.",
            GraphEditorDocumentChangeKind.NodesRemoved,
            [mutation.Node.Id],
            mutation.RemovedConnections.Select(connection => connection.Id).ToList());
    }

    public void DuplicateNode(string nodeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_behaviorOptions.Commands.Nodes.AllowDuplicate)
        {
            CurrentStatusMessage = "Node duplication is disabled by host permissions.";
            return;
        }

        var node = FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        var mutation = _documentMutator.DuplicateNode(
            _document,
            nodeId,
            CreateNodeId(node.DefinitionId, node.Id),
            new GraphPoint(node.Position.X + 48, node.Position.Y + 48));
        if (mutation.Node is null)
        {
            return;
        }

        _document = mutation.Document;
        SetSelection([mutation.Node.Id], mutation.Node.Id, updateStatus: false);
        MarkDirty(
            $"Duplicated {mutation.Node.Title}.",
            GraphEditorDocumentChangeKind.NodesAdded,
            [mutation.Node.Id],
            null);
    }

    public void DeleteSelection()
    {
        if (!_behaviorOptions.Commands.Nodes.AllowDelete)
        {
            CurrentStatusMessage = "Node deletion is disabled by host permissions.";
            return;
        }

        if (_selectedNodeIds.Count == 0)
        {
            CurrentStatusMessage = "Select a node before deleting.";
            return;
        }

        var mutation = _documentMutator.DeleteSelection(_document, _selectedNodeIds);
        var removedNodes = mutation.RemovedNodes;
        var removedConnections = mutation.RemovedConnections;

        if (removedConnections.Count > 0 && !CanRemoveConnectionsAsSideEffect())
        {
            CurrentStatusMessage = "Deleting connected nodes requires delete or disconnect permission for the affected links.";
            return;
        }

        _document = mutation.Document;
        CancelPendingConnection();
        SetSelection([], null, updateStatus: false);
        var status = removedNodes.Count == 1
            ? $"Deleted {removedNodes[0].Title}."
            : $"Deleted {removedNodes.Count} nodes.";
        MarkDirty(
            status,
            GraphEditorDocumentChangeKind.NodesRemoved,
            removedNodes.Select(node => node.Id).ToList(),
            removedConnections.Select(connection => connection.Id).ToList());
    }

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
    {
        ArgumentNullException.ThrowIfNull(positions);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node movement is disabled by host permissions.";
            }

            return;
        }

        var mutation = _documentMutator.SetNodePositions(_document, positions);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = positions.Count == 0
                    ? "No node positions were provided."
                    : "No matching nodes were found for the provided positions.";
            }

            return;
        }

        _document = mutation.Document;
        if (updateStatus)
        {
            CurrentStatusMessage = mutation.ChangedNodeIds.Count == 1
                ? "Updated 1 node position."
                : $"Updated {mutation.ChangedNodeIds.Count} node positions.";
        }

        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: true);
    }

    public void StartConnection(string sourceNodeId, string sourcePortId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowCreate)
        {
            CurrentStatusMessage = "Connection creation is disabled by host permissions.";
            return;
        }

        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null)
        {
            return;
        }

        var nextPending = GraphEditorPendingConnectionSnapshot.Create(true, sourceNode.Id, sourcePort.Id);
        if (_pendingConnection == nextPending)
        {
            CancelPendingConnection();
            return;
        }

        _pendingConnection = nextPending;
        CurrentStatusMessage = $"Connecting from {sourceNode.Title}.{sourcePort.Label}.";
        PendingConnectionChanged?.Invoke(this, new GraphEditorPendingConnectionChangedEventArgs(_pendingConnection));
    }

    public void CompleteConnection(string targetNodeId, string targetPortId)
    {
        if (!_pendingConnection.HasPendingConnection || _pendingConnection.SourceNodeId is null || _pendingConnection.SourcePortId is null)
        {
            return;
        }

        ConnectPorts(_pendingConnection.SourceNodeId, _pendingConnection.SourcePortId, targetNodeId, targetPortId);
    }

    public void CancelPendingConnection()
    {
        if (!_pendingConnection.HasPendingConnection)
        {
            return;
        }

        _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
        PendingConnectionChanged?.Invoke(this, new GraphEditorPendingConnectionChangedEventArgs(_pendingConnection));
    }

    public void DeleteConnection(string connectionId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowDelete)
        {
            CurrentStatusMessage = "Connection deletion is disabled by host permissions.";
            return;
        }

        var mutation = _documentMutator.DeleteConnection(_document, connectionId);
        if (mutation.Connection is null)
        {
            return;
        }

        _document = mutation.Document;
        MarkDirty($"Deleted connection {mutation.Connection.Label}.", GraphEditorDocumentChangeKind.ConnectionsChanged, null, [mutation.Connection.Id]);
    }

    public void BreakConnectionsForPort(string nodeId, string portId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowDisconnect)
        {
            CurrentStatusMessage = "Disconnect is disabled by host permissions.";
            return;
        }

        RemoveConnections(
            connection =>
                (connection.SourceNodeId == nodeId && connection.SourcePortId == portId)
                || (connection.TargetNodeId == nodeId && connection.TargetPortId == portId),
            "Disconnected port links.");
    }

    public void DisconnectIncoming(string nodeId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowDisconnect)
        {
            CurrentStatusMessage = "Disconnect is disabled by host permissions.";
            return;
        }

        RemoveConnections(connection => connection.TargetNodeId == nodeId, "Disconnected incoming links.");
    }

    public void DisconnectOutgoing(string nodeId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowDisconnect)
        {
            CurrentStatusMessage = "Disconnect is disabled by host permissions.";
            return;
        }

        RemoveConnections(connection => connection.SourceNodeId == nodeId, "Disconnected outgoing links.");
    }

    public void DisconnectAll(string nodeId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowDisconnect)
        {
            CurrentStatusMessage = "Disconnect is disabled by host permissions.";
            return;
        }

        RemoveConnections(
            connection => connection.SourceNodeId == nodeId || connection.TargetNodeId == nodeId,
            "Disconnected all links.");
    }

    public void PanBy(double deltaX, double deltaY)
    {
        ApplyViewportSnapshot(_viewportCoordinator.PanBy(GetViewportSnapshot(), deltaX, deltaY));
    }

    public void ZoomAt(double factor, GraphPoint screenAnchor)
    {
        ApplyViewportSnapshot(_viewportCoordinator.ZoomAt(GetViewportSnapshot(), factor, screenAnchor));
    }

    public void UpdateViewportSize(double width, double height)
    {
        ApplyViewportSnapshot(_viewportCoordinator.UpdateViewportSize(GetViewportSnapshot(), width, height));
    }

    public void ResetView(bool updateStatus)
    {
        ApplyViewportSnapshot(_viewportCoordinator.ResetView(GetViewportSnapshot()));
        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport reset.";
        }
    }

    public void FitToViewport(bool updateStatus)
    {
        if (!_viewportCoordinator.TryFitToViewport(GetViewportSnapshot(), _document.Nodes, out var updatedViewport))
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Nothing to fit yet.";
            }

            return;
        }

        ApplyViewportSnapshot(updatedViewport);
        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport fit to scene.";
        }
    }

    public void CenterViewOnNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (!_viewportCoordinator.TryCenterViewOnNode(GetViewportSnapshot(), node, out var updatedViewport))
        {
            return;
        }

        ApplyViewportSnapshot(updatedViewport);
        CurrentStatusMessage = $"Centered on {node!.Title}.";
    }

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus)
    {
        if (!_viewportCoordinator.TryCenterViewAt(GetViewportSnapshot(), worldPoint, out var updatedViewport))
        {
            return;
        }

        ApplyViewportSnapshot(updatedViewport);
        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport centered from mini map.";
        }
    }

    public void SaveWorkspace()
        => _workspaceSaveCoordinator.SaveWorkspace();

    public bool LoadWorkspace()
    {
        if (!_behaviorOptions.Commands.Workspace.AllowLoad)
        {
            CurrentStatusMessage = "Loading is disabled by host permissions.";
            return false;
        }

        try
        {
            if (!_workspaceService.Exists())
            {
                CurrentStatusMessage = "No saved snapshot yet. Save once to create one.";
                DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                    "workspace.load.missing",
                    "workspace.load",
                    CurrentStatusMessage,
                    GraphEditorDiagnosticSeverity.Warning));
                return false;
            }

            var document = _workspaceService.Load();
            LoadDocument(document, "Workspace loaded from disk.", markClean: true, resetHistory: true);
            CancelPendingConnection();
            ClearSelection(updateStatus: false);
            ResetView(updateStatus: false);
            DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.load.succeeded",
                "workspace.load",
                _workspaceService.WorkspacePath,
                GraphEditorDiagnosticSeverity.Info));
            DocumentChanged?.Invoke(this, new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.WorkspaceLoaded, statusMessage: CurrentStatusMessage));
            return true;
        }
        catch (Exception exception)
        {
            CurrentStatusMessage = $"Load failed: {exception.Message}";
            RecoverableFailureRaised?.Invoke(
                this,
                new GraphEditorRecoverableFailureEventArgs(
                    "workspace.load.failed",
                    "workspace.load",
                    CurrentStatusMessage,
                    exception));
            return false;
        }
    }

    public GraphDocument CreateDocumentSnapshot()
        => CloneDocument(_document);

    public GraphEditorSelectionSnapshot GetSelectionSnapshot()
        => new(_selectedNodeIds.ToList(), _primarySelectedNodeId);

    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => new(_zoom, _panX, _panY, _viewportWidth, _viewportHeight);

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => new(
            _historyService.CanUndo && _behaviorOptions.History.EnableUndoRedo && _behaviorOptions.Commands.History.AllowUndo,
            _historyService.CanRedo && _behaviorOptions.History.EnableUndoRedo && _behaviorOptions.Commands.History.AllowRedo,
            _selectedNodeIds.Count > 0,
            false,
            _behaviorOptions.Commands.Workspace.AllowSave,
            _behaviorOptions.Commands.Workspace.AllowLoad)
        {
            CanSetSelection = true,
            CanMoveNodes = _behaviorOptions.Commands.Nodes.AllowMove,
            CanCreateConnections = _behaviorOptions.Commands.Connections.AllowCreate,
            CanDeleteConnections = _behaviorOptions.Commands.Connections.AllowDelete,
            CanBreakConnections = _behaviorOptions.Commands.Connections.AllowDisconnect,
            CanUpdateViewport = true,
            CanFitToViewport = _document.Nodes.Count > 0 && _viewportWidth > 0 && _viewportHeight > 0,
            CanCenterViewport = _viewportWidth > 0 && _viewportHeight > 0,
        };

    public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
        =>
        [
            new GraphEditorFeatureDescriptorSnapshot("query.feature-descriptors", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.document-snapshot", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.selection-snapshot", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.viewport-snapshot", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.node-positions", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.pending-connection-snapshot", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.compatible-port-target-snapshot", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("query.compatible-target-mvvm-shim", "query", true),
            new GraphEditorFeatureDescriptorSnapshot("service.workspace", "service", true),
            new GraphEditorFeatureDescriptorSnapshot("service.diagnostics", "service", true),
            new GraphEditorFeatureDescriptorSnapshot("surface.mutation.batch", "surface", true),
        ];

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => _commandRouter.GetCommandDescriptors();

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
        => _commandRouter.TryExecuteCommand(command);

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _document.Nodes
            .Select(node => new NodePositionSnapshot(node.Id, node.Position))
            .ToList();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => _pendingConnection;

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _compatibilityQueries.GetCompatiblePortTargets(_document, sourceNodeId, sourcePortId);

#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _compatibilityQueries.GetCompatibleTargets(_document, sourceNodeId, sourcePortId);
#pragma warning restore CS0618

    private void ConnectPorts(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowCreate)
        {
            CurrentStatusMessage = "Connection creation is disabled by host permissions.";
            return;
        }

        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        var targetNode = FindNode(targetNodeId);
        var targetPort = targetNode?.Inputs.FirstOrDefault(port => string.Equals(port.Id, targetPortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null || targetNode is null || targetPort is null)
        {
            return;
        }

        if (sourcePort.TypeId is null || targetPort.TypeId is null)
        {
            CurrentStatusMessage = "Connection endpoints must expose stable type identifiers.";
            return;
        }

        var compatibility = _compatibilityService.Evaluate(sourcePort.TypeId, targetPort.TypeId);
        if (!compatibility.IsCompatible)
        {
            CurrentStatusMessage = $"Incompatible connection: {sourcePort.TypeId} -> {targetPort.TypeId}.";
            return;
        }

        if (_document.Connections.Any(connection =>
                connection.SourceNodeId == sourceNode.Id
                && connection.SourcePortId == sourcePort.Id
                && connection.TargetNodeId == targetNode.Id
                && connection.TargetPortId == targetPort.Id))
        {
            CancelPendingConnection();
            CurrentStatusMessage = "That connection already exists.";
            return;
        }

        var replacedConnections = _document.Connections
            .Where(connection => connection.TargetNodeId == targetNode.Id && connection.TargetPortId == targetPort.Id)
            .ToList();
        if (replacedConnections.Count > 0 && !CanReplaceIncomingConnection())
        {
            CurrentStatusMessage = "Replacing an incoming connection requires delete or disconnect permission.";
            return;
        }

        var nextConnection = new GraphConnection(
            CreateConnectionId(),
            sourceNode.Id,
            sourcePort.Id,
            targetNode.Id,
            targetPort.Id,
            $"{sourcePort.Label} to {targetPort.Label}",
            sourcePort.AccentHex,
            compatibility.ConversionId);

        _document = _document with
        {
            Connections = _document.Connections
                .Except(replacedConnections)
                .Concat([nextConnection])
                .ToList(),
        };
        _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
        PendingConnectionChanged?.Invoke(this, new GraphEditorPendingConnectionChangedEventArgs(_pendingConnection));
        CurrentStatusMessage = compatibility.Kind == PortCompatibilityKind.ImplicitConversion
            ? $"Connected {sourceNode.Title} to {targetNode.Title} with implicit conversion."
            : $"Connected {sourceNode.Title} to {targetNode.Title}.";
        MarkDirty(
            CurrentStatusMessage,
            GraphEditorDocumentChangeKind.ConnectionsChanged,
            [sourceNode.Id, targetNode.Id],
            [nextConnection.Id],
            preserveStatus: true);
    }

    private GraphPoint GetViewportCenter()
        => _viewportCoordinator.GetViewportCenter(GetViewportSnapshot());

    private void ApplyViewportSnapshot(GraphEditorViewportSnapshot snapshot)
    {
        _zoom = snapshot.Zoom;
        _panX = snapshot.PanX;
        _panY = snapshot.PanY;
        _viewportWidth = snapshot.ViewportWidth;
        _viewportHeight = snapshot.ViewportHeight;
        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    private void MarkDirty(
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds,
        IReadOnlyList<string>? connectionIds,
        bool preserveStatus = false)
    {
        if (!preserveStatus)
        {
            CurrentStatusMessage = status;
        }

        var state = CaptureHistoryState();
        _historyService.Push(state);
        DocumentChanged?.Invoke(this, new GraphEditorDocumentChangedEventArgs(changeKind, nodeIds, connectionIds, CurrentStatusMessage));
    }

    private void LoadDocument(GraphDocument document, string status, bool markClean, bool resetHistory)
    {
        _document = CloneDocument(document);
        _selectedNodeIds = [];
        _primarySelectedNodeId = null;
        _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
        CurrentStatusMessage = status;

        var historyState = CaptureHistoryState();
        if (resetHistory)
        {
            _historyService.Reset(historyState);
        }

        if (markClean)
        {
            _lastSavedDocumentSignature = historyState.Signature;
        }
    }

    private void RestoreHistoryState(GraphEditorHistoryState state, string status, GraphEditorDocumentChangeKind changeKind)
    {
        _document = state.Document;
        _selectedNodeIds = state.SelectedNodeIds.ToList();
        _primarySelectedNodeId = state.PrimarySelectedNodeId;
        _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
        CurrentStatusMessage = status;
        SelectionChanged?.Invoke(this, new GraphEditorSelectionChangedEventArgs(_selectedNodeIds.ToList(), _primarySelectedNodeId));
        PendingConnectionChanged?.Invoke(this, new GraphEditorPendingConnectionChangedEventArgs(_pendingConnection));
        DocumentChanged?.Invoke(this, new GraphEditorDocumentChangedEventArgs(changeKind, statusMessage: CurrentStatusMessage));
    }

    private GraphEditorHistoryState CaptureHistoryState()
        => new(
            CloneDocument(_document),
            _selectedNodeIds.ToList(),
            _primarySelectedNodeId,
            CreateDocumentSignature(_document));

    private bool CanReplaceIncomingConnection()
        => _behaviorOptions.Commands.Connections.AllowDelete || _behaviorOptions.Commands.Connections.AllowDisconnect;

    private bool CanRemoveConnectionsAsSideEffect()
        => _behaviorOptions.Commands.Connections.AllowDelete || _behaviorOptions.Commands.Connections.AllowDisconnect;

    private void RemoveConnections(Func<GraphConnection, bool> predicate, string status)
    {
        var mutation = _documentMutator.RemoveConnections(_document, predicate);
        if (mutation.RemovedConnections.Count == 0)
        {
            CurrentStatusMessage = "No matching connections to remove.";
            return;
        }

        _document = mutation.Document;
        MarkDirty(status, GraphEditorDocumentChangeKind.ConnectionsChanged, null, mutation.RemovedConnections.Select(connection => connection.Id).ToList());
    }

    private GraphNode? FindNode(string nodeId)
        => _document.Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));

    private string CreateNodeId(NodeDefinitionId definitionId)
        => CreateNodeId(definitionId, definitionId.Value);

    private string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
        => CreateNodeId(
            (definitionId?.Value ?? fallbackKey)
            .Replace(".", "-", StringComparison.Ordinal));

    private string CreateNodeId(string templateKey)
        => CreateUniqueId(_document.Nodes.Select(node => node.Id), $"{templateKey}-");

    private string CreateConnectionId()
        => CreateUniqueId(_document.Connections.Select(connection => connection.Id), "connection-");

    private static string CreateUniqueId(IEnumerable<string> existingIds, string prefix)
    {
        var ids = existingIds.ToHashSet(StringComparer.Ordinal);
        var next = 1;

        foreach (var id in ids)
        {
            if (!id.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (int.TryParse(id[prefix.Length..], out var value))
            {
                next = Math.Max(next, value + 1);
            }
        }

        string candidate;
        do
        {
            candidate = $"{prefix}{next:000}";
            next++;
        }
        while (ids.Contains(candidate));

        return candidate;
    }

    private static string CreateDocumentSignature(GraphDocument document)
        => GraphDocumentSerializer.Serialize(document);

    private static GraphDocument CloneDocument(GraphDocument document)
        => new(
            document.Title,
            document.Description,
            document.Nodes.Select(CloneNode).ToList(),
            document.Connections.Select(CloneConnection).ToList());

    private static GraphNode CloneNode(GraphNode node)
        => new(
            node.Id,
            node.Title,
            node.Category,
            node.Subtitle,
            node.Description,
            node.Position,
            node.Size,
            node.Inputs.Select(ClonePort).ToList(),
            node.Outputs.Select(ClonePort).ToList(),
            node.AccentHex,
            node.DefinitionId,
            node.ParameterValues?.Select(CloneParameterValue).ToList());

    private static GraphPort ClonePort(GraphPort port)
        => new(
            port.Id,
            port.Label,
            port.Direction,
            port.DataType,
            port.AccentHex,
            port.TypeId);

    private static GraphConnection CloneConnection(GraphConnection connection)
        => new(
            connection.Id,
            connection.SourceNodeId,
            connection.SourcePortId,
            connection.TargetNodeId,
            connection.TargetPortId,
            connection.Label,
            connection.AccentHex,
            connection.ConversionId);

    private static GraphParameterValue CloneParameterValue(GraphParameterValue parameter)
        => new(parameter.Key, parameter.TypeId, parameter.Value);
}
