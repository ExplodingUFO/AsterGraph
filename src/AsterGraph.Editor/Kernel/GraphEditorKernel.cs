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
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.Kernel;

internal sealed class GraphEditorKernel : IGraphEditorSessionHost
{
    private const double DefaultZoom = 0.88;
    private const double DefaultPanX = 110;
    private const double DefaultPanY = 96;

    private readonly INodeCatalog _nodeCatalog;
    private readonly IPortCompatibilityService _compatibilityService;
    private readonly IGraphWorkspaceService _workspaceService;
    private readonly GraphEditorHistoryService _historyService = new();
    private readonly GraphEditorBehaviorOptions _behaviorOptions;
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
        _styleOptions = styleOptions;
        _behaviorOptions = behaviorOptions;
        _lastSavedDocumentSignature = CreateDocumentSignature(_document);
        _historyService.Reset(CaptureHistoryState());
        CurrentStatusMessage = "Ready to edit.";
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

        var removedNodeIds = _selectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var removedNodes = _document.Nodes.Where(node => removedNodeIds.Contains(node.Id)).ToList();
        var removedConnections = _document.Connections
            .Where(connection => removedNodeIds.Contains(connection.SourceNodeId) || removedNodeIds.Contains(connection.TargetNodeId))
            .ToList();

        if (removedConnections.Count > 0 && !CanRemoveConnectionsAsSideEffect())
        {
            CurrentStatusMessage = "Deleting connected nodes requires delete or disconnect permission for the affected links.";
            return;
        }

        _document = _document with
        {
            Nodes = _document.Nodes.Where(node => !removedNodeIds.Contains(node.Id)).ToList(),
            Connections = _document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
        };
        CancelPendingConnection();
        SetSelection([], null, updateStatus: false);
        var status = removedNodes.Count == 1
            ? $"Deleted {removedNodes[0].Title}."
            : $"Deleted {removedNodes.Count} nodes.";
        MarkDirty(
            status,
            GraphEditorDocumentChangeKind.NodesRemoved,
            removedNodeIds.ToList(),
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

        var requested = positions
            .GroupBy(snapshot => snapshot.NodeId, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToDictionary(snapshot => snapshot.NodeId, snapshot => snapshot.Position, StringComparer.Ordinal);

        if (requested.Count == 0)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No node positions were provided.";
            }

            return;
        }

        var changedNodeIds = new List<string>();
        var updatedNodes = _document.Nodes
            .Select(node =>
            {
                if (!requested.TryGetValue(node.Id, out var position) || node.Position == position)
                {
                    return node;
                }

                changedNodeIds.Add(node.Id);
                return node with { Position = position };
            })
            .ToList();

        if (changedNodeIds.Count == 0)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching nodes were found for the provided positions.";
            }

            return;
        }

        _document = _document with { Nodes = updatedNodes };
        if (updateStatus)
        {
            CurrentStatusMessage = changedNodeIds.Count == 1
                ? "Updated 1 node position."
                : $"Updated {changedNodeIds.Count} node positions.";
        }

        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, changedNodeIds, null, preserveStatus: true);
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

        var connection = _document.Connections.FirstOrDefault(candidate => string.Equals(candidate.Id, connectionId, StringComparison.Ordinal));
        if (connection is null)
        {
            return;
        }

        _document = _document with
        {
            Connections = _document.Connections.Where(candidate => !ReferenceEquals(candidate, connection)).ToList(),
        };
        MarkDirty($"Deleted connection {connection.Label}.", GraphEditorDocumentChangeKind.ConnectionsChanged, null, [connection.Id]);
    }

    public void BreakConnectionsForPort(string nodeId, string portId)
    {
        if (!_behaviorOptions.Commands.Connections.AllowDisconnect)
        {
            CurrentStatusMessage = "Disconnect is disabled by host permissions.";
            return;
        }

        var removedConnections = _document.Connections
            .Where(connection =>
                (connection.SourceNodeId == nodeId && connection.SourcePortId == portId)
                || (connection.TargetNodeId == nodeId && connection.TargetPortId == portId))
            .ToList();

        if (removedConnections.Count == 0)
        {
            CurrentStatusMessage = "No matching connections to remove.";
            return;
        }

        _document = _document with
        {
            Connections = _document.Connections.Where(connection => !removedConnections.Contains(connection)).ToList(),
        };
        MarkDirty("Disconnected port links.", GraphEditorDocumentChangeKind.ConnectionsChanged, null, removedConnections.Select(connection => connection.Id).ToList());
    }

    public void PanBy(double deltaX, double deltaY)
    {
        _panX += deltaX;
        _panY += deltaY;
        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void ZoomAt(double factor, GraphPoint screenAnchor)
    {
        var updated = ViewportMath.ZoomAround(
            new ViewportState(_zoom, _panX, _panY),
            factor,
            screenAnchor,
            minimumZoom: 0.35,
            maximumZoom: 1.9);
        _zoom = updated.Zoom;
        _panX = updated.PanX;
        _panY = updated.PanY;
        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void UpdateViewportSize(double width, double height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void ResetView(bool updateStatus)
    {
        _zoom = DefaultZoom;
        _panX = DefaultPanX;
        _panY = DefaultPanY;
        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport reset.";
        }

        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void FitToViewport(bool updateStatus)
    {
        if (_document.Nodes.Count == 0 || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Nothing to fit yet.";
            }

            return;
        }

        var minX = _document.Nodes.Min(node => node.Position.X);
        var minY = _document.Nodes.Min(node => node.Position.Y);
        var maxX = _document.Nodes.Max(node => node.Position.X + node.Size.Width);
        var maxY = _document.Nodes.Max(node => node.Position.Y + node.Size.Height);
        var graphWidth = Math.Max(maxX - minX, 1);
        var graphHeight = Math.Max(maxY - minY, 1);
        const double padding = 120;

        var zoomX = _viewportWidth / (graphWidth + (padding * 2));
        var zoomY = _viewportHeight / (graphHeight + (padding * 2));
        _zoom = Math.Clamp(Math.Min(zoomX, zoomY), 0.32, 1.4);
        _panX = ((_viewportWidth - (graphWidth * _zoom)) / 2) - (minX * _zoom);
        _panY = ((_viewportHeight - (graphHeight * _zoom)) / 2) - (minY * _zoom);

        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport fit to scene.";
        }

        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void CenterViewOnNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node is null || _viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        _panX = (_viewportWidth / 2) - ((node.Position.X + (node.Size.Width / 2)) * _zoom);
        _panY = (_viewportHeight / 2) - ((node.Position.Y + (node.Size.Height / 2)) * _zoom);
        CurrentStatusMessage = $"Centered on {node.Title}.";
        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus)
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return;
        }

        _panX = (_viewportWidth / 2) - (worldPoint.X * _zoom);
        _panY = (_viewportHeight / 2) - (worldPoint.Y * _zoom);
        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport centered from mini map.";
        }

        ViewportChanged?.Invoke(this, new GraphEditorViewportChangedEventArgs(_zoom, _panX, _panY, _viewportWidth, _viewportHeight));
    }

    public void SaveWorkspace()
    {
        if (!_behaviorOptions.Commands.Workspace.AllowSave)
        {
            CurrentStatusMessage = "Saving is disabled by host permissions.";
            return;
        }

        try
        {
            _workspaceService.Save(_document);
            _lastSavedDocumentSignature = CreateDocumentSignature(_document);
            CurrentStatusMessage = $"Saved snapshot to {_workspaceService.WorkspacePath}.";
            DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.save.succeeded",
                "workspace.save",
                CurrentStatusMessage,
                GraphEditorDiagnosticSeverity.Info));
            DocumentChanged?.Invoke(this, new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.WorkspaceSaved, statusMessage: CurrentStatusMessage));
        }
        catch (Exception exception)
        {
            CurrentStatusMessage = $"Save failed: {exception.Message}";
            RecoverableFailureRaised?.Invoke(
                this,
                new GraphEditorRecoverableFailureEventArgs(
                    "workspace.save.failed",
                    "workspace.save",
                    CurrentStatusMessage,
                    exception));
        }
    }

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

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => _document.Nodes
            .Select(node => new NodePositionSnapshot(node.Id, node.Position))
            .ToList();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => _pendingConnection;

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => GetCompatiblePortTargetsCore(sourceNodeId, sourcePortId)
            .Select(target => new GraphEditorCompatiblePortTargetSnapshot(
                target.Node.Id,
                target.Node.Title,
                target.Port.Id,
                target.Port.Label,
                target.Port.TypeId!,
                target.Port.AccentHex,
                target.Compatibility))
            .ToList();

#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => GetCompatiblePortTargetsCore(sourceNodeId, sourcePortId)
            .Select(target =>
            {
                var node = new NodeViewModel(target.Node);
                var port = node.GetPort(target.Port.Id)
                    ?? throw new InvalidOperationException($"Port '{target.Port.Id}' was not found on compatibility bridge node '{target.Node.Id}'.");
                return new CompatiblePortTarget(node, port, target.Compatibility);
            })
            .ToList();
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

    private IReadOnlyList<CompatibleTargetState> GetCompatiblePortTargetsCore(string sourceNodeId, string sourcePortId)
    {
        var sourceNode = FindNode(sourceNodeId);
        var sourcePort = sourceNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, sourcePortId, StringComparison.Ordinal));
        if (sourceNode is null || sourcePort is null)
        {
            return [];
        }

        return _document.Nodes
            .SelectMany(node => node.Inputs.Select(port => (node, port)))
            .Where(target => !(target.node.Id == sourceNodeId && target.port.Id == sourcePortId))
            .Where(target => sourcePort.TypeId is not null && target.port.TypeId is not null)
            .Select(target => new CompatibleTargetState(
                target.node,
                target.port,
                _compatibilityService.Evaluate(sourcePort.TypeId!, target.port.TypeId!)))
            .Where(target => target.Compatibility.IsCompatible)
            .ToList();
    }

    private GraphPoint GetViewportCenter()
    {
        if (_viewportWidth <= 0 || _viewportHeight <= 0)
        {
            return ViewportMath.ScreenToWorld(new ViewportState(_zoom, _panX, _panY), new GraphPoint(820, 440));
        }

        return ViewportMath.ScreenToWorld(
            new ViewportState(_zoom, _panX, _panY),
            new GraphPoint(_viewportWidth / 2, _viewportHeight / 2));
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

    private GraphNode? FindNode(string nodeId)
        => _document.Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));

    private string CreateNodeId(NodeDefinitionId definitionId)
        => CreateUniqueId(_document.Nodes.Select(node => node.Id), $"{definitionId.Value.Replace(".", "-", StringComparison.Ordinal)}-");

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

    private sealed record CompatibleTargetState(
        GraphNode Node,
        GraphPort Port,
        PortCompatibilityResult Compatibility);
}
