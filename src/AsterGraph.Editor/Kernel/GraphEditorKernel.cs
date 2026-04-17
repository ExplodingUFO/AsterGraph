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
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Parameters;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Viewport;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel : IGraphEditorSessionHost
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
    private readonly GraphEditorKernelNodeMutationHost _nodeMutationHost;
    private readonly GraphEditorKernelNodeMutationCoordinator _nodeMutationCoordinator;
    private readonly GraphEditorKernelConnectionMutationHost _connectionMutationHost;
    private readonly GraphEditorKernelConnectionMutationCoordinator _connectionMutationCoordinator;
    private readonly GraphEditorKernelCommandRouterHost _commandRouterHost;
    private readonly GraphEditorKernelCommandRouter _commandRouter;
    private readonly GraphEditorKernelHistoryCoordinator _historyCoordinator;
    private readonly GraphEditorKernelWorkspaceSaveCoordinatorHost _workspaceSaveCoordinatorHost;
    private readonly GraphEditorKernelWorkspaceLoadCoordinatorHost _workspaceLoadCoordinatorHost;
    private readonly GraphEditorWorkspaceSaveCoordinator _workspaceSaveCoordinator;
    private readonly GraphEditorWorkspaceLoadCoordinator _workspaceLoadCoordinator;
    private readonly GraphEditorKernelSelectionCoordinator _selectionCoordinator;
    private readonly GraphEditorKernelProjectionCoordinator _projectionCoordinator;
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
        _nodeMutationHost = new GraphEditorKernelNodeMutationHost(this);
        _nodeMutationCoordinator = new GraphEditorKernelNodeMutationCoordinator(_nodeMutationHost, _documentMutator);
        _connectionMutationHost = new GraphEditorKernelConnectionMutationHost(this);
        _connectionMutationCoordinator = new GraphEditorKernelConnectionMutationCoordinator(_connectionMutationHost, _documentMutator);
        _commandRouterHost = new GraphEditorKernelCommandRouterHost(this);
        _commandRouter = new GraphEditorKernelCommandRouter(_commandRouterHost);
        _historyCoordinator = new GraphEditorKernelHistoryCoordinator(this);
        _selectionCoordinator = new GraphEditorKernelSelectionCoordinator(this);
        _projectionCoordinator = new GraphEditorKernelProjectionCoordinator(this);
        _workspaceSaveCoordinatorHost = new GraphEditorKernelWorkspaceSaveCoordinatorHost(this);
        _workspaceLoadCoordinatorHost = new GraphEditorKernelWorkspaceLoadCoordinatorHost(this);
        _workspaceSaveCoordinator = new GraphEditorWorkspaceSaveCoordinator(_workspaceSaveCoordinatorHost);
        _workspaceLoadCoordinator = new GraphEditorWorkspaceLoadCoordinator(_workspaceLoadCoordinatorHost);
        _lastSavedDocumentSignature = CreateDocumentSignature(_document);
        _historyService.Reset(_historyCoordinator.CaptureHistoryState());
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

    internal bool IsDirty
        => _historyCoordinator.IsDirty(_lastSavedDocumentSignature);

    public void Undo()
        => _historyCoordinator.Undo();

    public void Redo()
        => _historyCoordinator.Redo();

    public void ClearSelection(bool updateStatus)
        => _selectionCoordinator.ClearSelection(updateStatus);

    public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
        => _selectionCoordinator.SetSelection(nodeIds, primaryNodeId, updateStatus);

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
        => _nodeMutationCoordinator.AddNode(definitionId, preferredWorldPosition);

    public void DeleteNodeById(string nodeId)
        => _nodeMutationCoordinator.DeleteNodeById(nodeId);

    public void DuplicateNode(string nodeId)
        => _nodeMutationCoordinator.DuplicateNode(nodeId);

    public void DeleteSelection()
        => _nodeMutationCoordinator.DeleteSelection();

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
        => _nodeMutationCoordinator.SetNodePositions(positions, updateStatus);

    public bool TrySetSelectedNodeParameterValue(string parameterKey, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

        if (!_behaviorOptions.Commands.Nodes.AllowEditParameters)
        {
            CurrentStatusMessage = "Parameter editing is disabled by host permissions.";
            return false;
        }

        if (!TryGetSharedSelectionDefinition(out var definition, out var selectedNodes) || definition is null)
        {
            CurrentStatusMessage = _selectedNodeIds.Count == 0
                ? "Select a node before editing parameters."
                : "Parameter editing requires selected nodes to share one definition.";
            return false;
        }

        var parameterDefinition = definition.Parameters.FirstOrDefault(parameter => string.Equals(parameter.Key, parameterKey, StringComparison.Ordinal));
        if (parameterDefinition is null)
        {
            CurrentStatusMessage = $"Parameter '{parameterKey}' is not declared by {definition.DisplayName}.";
            return false;
        }

        if (parameterDefinition.Constraints.IsReadOnly)
        {
            CurrentStatusMessage = $"{parameterDefinition.DisplayName} is read-only.";
            return false;
        }

        var normalized = NodeParameterValueAdapter.NormalizeValue(parameterDefinition, value);
        if (!normalized.IsValid)
        {
            CurrentStatusMessage = normalized.ValidationError ?? $"Parameter '{parameterDefinition.DisplayName}' could not be normalized.";
            return false;
        }

        var mutation = _documentMutator.SetNodeParameterValue(
            _document,
            selectedNodes.Select(node => node.Id).ToList(),
            parameterDefinition,
            normalized.Value);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            CurrentStatusMessage = $"No parameter changes were applied for {parameterDefinition.DisplayName}.";
            return false;
        }

        _document = mutation.Document;
        MarkDirty(
            mutation.ChangedNodeIds.Count == 1
                ? $"Updated {parameterDefinition.DisplayName} on 1 node."
                : $"Updated {parameterDefinition.DisplayName} on {mutation.ChangedNodeIds.Count} nodes.",
            GraphEditorDocumentChangeKind.ParametersChanged,
            mutation.ChangedNodeIds,
            null);
        return true;
    }

    public void StartConnection(string sourceNodeId, string sourcePortId)
        => _connectionMutationCoordinator.StartConnection(sourceNodeId, sourcePortId);

    public void CompleteConnection(string targetNodeId, string targetPortId)
        => _connectionMutationCoordinator.CompleteConnection(targetNodeId, targetPortId);

    public void CancelPendingConnection()
        => _connectionMutationCoordinator.CancelPendingConnection();

    public void DeleteConnection(string connectionId)
        => _connectionMutationCoordinator.DeleteConnection(connectionId);

    public void BreakConnectionsForPort(string nodeId, string portId)
        => _connectionMutationCoordinator.BreakConnectionsForPort(nodeId, portId);

    public void DisconnectIncoming(string nodeId)
        => _connectionMutationCoordinator.DisconnectIncoming(nodeId);

    public void DisconnectOutgoing(string nodeId)
        => _connectionMutationCoordinator.DisconnectOutgoing(nodeId);

    public void DisconnectAll(string nodeId)
        => _connectionMutationCoordinator.DisconnectAll(nodeId);

    public void PanBy(double deltaX, double deltaY)
    {
        _historyCoordinator.ApplyViewportSnapshot(_viewportCoordinator.PanBy(GetViewportSnapshot(), deltaX, deltaY));
    }

    public void ZoomAt(double factor, GraphPoint screenAnchor)
    {
        _historyCoordinator.ApplyViewportSnapshot(_viewportCoordinator.ZoomAt(GetViewportSnapshot(), factor, screenAnchor));
    }

    public void UpdateViewportSize(double width, double height)
    {
        _historyCoordinator.ApplyViewportSnapshot(_viewportCoordinator.UpdateViewportSize(GetViewportSnapshot(), width, height));
    }

    public void ResetView(bool updateStatus)
    {
        _historyCoordinator.ApplyViewportSnapshot(_viewportCoordinator.ResetView(GetViewportSnapshot()));
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

        _historyCoordinator.ApplyViewportSnapshot(updatedViewport);
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

        _historyCoordinator.ApplyViewportSnapshot(updatedViewport);
        CurrentStatusMessage = $"Centered on {node!.Title}.";
    }

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus)
    {
        if (!_viewportCoordinator.TryCenterViewAt(GetViewportSnapshot(), worldPoint, out var updatedViewport))
        {
            return;
        }

        _historyCoordinator.ApplyViewportSnapshot(updatedViewport);
        if (updateStatus)
        {
            CurrentStatusMessage = "Viewport centered from mini map.";
        }
    }

    public void SaveWorkspace()
        => _workspaceSaveCoordinator.SaveWorkspace();

    public bool LoadWorkspace()
        => _workspaceLoadCoordinator.LoadWorkspace();

    public GraphDocument CreateDocumentSnapshot()
        => CloneDocument(_document);

    public GraphEditorSelectionSnapshot GetSelectionSnapshot()
        => _selectionCoordinator.GetSelectionSnapshot();

    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => new(_zoom, _panX, _panY, _viewportWidth, _viewportHeight);

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => _projectionCoordinator.GetCapabilitySnapshot();

    public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
        => _projectionCoordinator.GetFeatureDescriptors();

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

    internal void CommitRetainedMutation(
        GraphDocument document,
        GraphEditorSelectionSnapshot selection,
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selection);

        var selectionChanged = ApplyRetainedDocumentSnapshot(document, selection);
        CurrentStatusMessage = status;
        _historyService.Push(CaptureHistoryState());

        if (selectionChanged)
        {
            SelectionChanged?.Invoke(
                this,
                new GraphEditorSelectionChangedEventArgs(_selectedNodeIds.ToList(), _primarySelectedNodeId));
        }

        DocumentChanged?.Invoke(
            this,
                new GraphEditorDocumentChangedEventArgs(changeKind, nodeIds, connectionIds, CurrentStatusMessage));
    }

    internal void SaveRetainedWorkspace(GraphDocument document, GraphEditorSelectionSnapshot selection)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selection);

        var selectionChanged = ApplyRetainedDocumentSnapshot(document, selection);

        if (!_behaviorOptions.Commands.Workspace.AllowSave)
        {
            CurrentStatusMessage = "Saving is disabled by host permissions.";
            return;
        }

        try
        {
            _workspaceService.Save(document);
            _lastSavedDocumentSignature = CreateDocumentSignature(document);
            _historyService.ReplaceCurrent(CaptureHistoryState());
            CurrentStatusMessage = $"Saved snapshot to {_workspaceService.WorkspacePath}.";
            DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.save.succeeded",
                "workspace.save",
                CurrentStatusMessage,
                GraphEditorDiagnosticSeverity.Info));
            if (selectionChanged)
            {
                SelectionChanged?.Invoke(this, new GraphEditorSelectionChangedEventArgs(_selectedNodeIds.ToList(), _primarySelectedNodeId));
            }

            DocumentChanged?.Invoke(
                this,
                new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.WorkspaceSaved, statusMessage: CurrentStatusMessage));
        }
        catch (Exception exception)
        {
            CurrentStatusMessage = $"Save failed: {exception.Message}";
            DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.save.failed",
                "workspace.save",
                CurrentStatusMessage,
                GraphEditorDiagnosticSeverity.Warning,
                exception));
            RecoverableFailureRaised?.Invoke(
                this,
                new GraphEditorRecoverableFailureEventArgs(
                    "workspace.save.failed",
                    "workspace.save",
                    CurrentStatusMessage,
                    exception));
        }
    }

    private GraphPoint GetViewportCenter()
        => _viewportCoordinator.GetViewportCenter(GetViewportSnapshot());

    private bool ApplyRetainedDocumentSnapshot(GraphDocument document, GraphEditorSelectionSnapshot selection)
    {
        _document = CloneDocument(document);

        var existingIds = _document.Nodes
            .Select(node => node.Id)
            .ToHashSet(StringComparer.Ordinal);
        var selectedNodeIds = selection.SelectedNodeIds
            .Where(existingIds.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var primaryNodeId = !string.IsNullOrWhiteSpace(selection.PrimarySelectedNodeId)
            && selectedNodeIds.Contains(selection.PrimarySelectedNodeId, StringComparer.Ordinal)
            ? selection.PrimarySelectedNodeId
            : selectedNodeIds.LastOrDefault();
        var selectionChanged = !_selectedNodeIds.SequenceEqual(selectedNodeIds, StringComparer.Ordinal)
            || !string.Equals(_primarySelectedNodeId, primaryNodeId, StringComparison.Ordinal);

        _selectedNodeIds = selectedNodeIds;
        _primarySelectedNodeId = primaryNodeId;
        return selectionChanged;
    }

    private void MarkDirty(
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds,
        IReadOnlyList<string>? connectionIds,
        bool preserveStatus = false)
        => _historyCoordinator.MarkDirty(status, changeKind, nodeIds, connectionIds, preserveStatus);

    private void LoadDocument(GraphDocument document, string status, bool markClean, bool resetHistory)
        => _historyCoordinator.LoadDocument(document, status, markClean, resetHistory);

    private GraphEditorHistoryState CaptureHistoryState()
        => _historyCoordinator.CaptureHistoryState();

    private bool CanReplaceIncomingConnection()
        => _behaviorOptions.Commands.Connections.AllowDelete || _behaviorOptions.Commands.Connections.AllowDisconnect;

    private bool CanRemoveConnectionsAsSideEffect()
        => _behaviorOptions.Commands.Connections.AllowDelete || _behaviorOptions.Commands.Connections.AllowDisconnect;

    private bool HasSharedSelectionDefinitionWithParameters()
        => TryGetSharedSelectionDefinition(out var definition, out _) && definition is { Parameters.Count: > 0 };

    private bool TryGetSharedSelectionDefinition(
        out INodeDefinition? definition,
        out IReadOnlyList<GraphNode> selectedNodes)
    {
        definition = null;
        selectedNodes = [];

        if (_selectedNodeIds.Count == 0)
        {
            return false;
        }

        var nodesById = _document.Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var resolvedNodes = _selectedNodeIds
            .Where(nodeId => nodesById.ContainsKey(nodeId))
            .Select(nodeId => nodesById[nodeId])
            .ToList();
        if (resolvedNodes.Count == 0)
        {
            return false;
        }

        var sharedDefinitionId = resolvedNodes[0].DefinitionId;
        if (sharedDefinitionId is null || resolvedNodes.Any(node => node.DefinitionId != sharedDefinitionId))
        {
            return false;
        }

        if (!_nodeCatalog.TryGetDefinition(sharedDefinitionId, out definition) || definition is null)
        {
            return false;
        }

        selectedNodes = resolvedNodes;
        return true;
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
