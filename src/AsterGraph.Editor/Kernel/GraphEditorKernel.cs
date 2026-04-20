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
    private string _activeGraphId;
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

        _document = _documentMutator.NormalizeNodeGroupBounds(CloneDocument(document));
        _activeGraphId = _document.RootGraphId;
        _nodeCatalog = nodeCatalog;
        _compatibilityService = compatibilityService;
        _workspaceService = workspaceService;
        _compatibilityQueries = new GraphEditorKernelCompatibilityQueries(compatibilityService, nodeCatalog);
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

    public bool TrySetNodeWidth(string nodeId, double width, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node surface editing is disabled by host permissions.";
            }

            return false;
        }

        if (width <= 0d)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node width must be positive.";
            }

            return false;
        }

        var mutation = _documentMutator.SetNodeWidth(CreateActiveScopeDocumentSnapshot(), nodeId, width);
        if (mutation.NodeId is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching node width change was applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Updated node width to {mutation.Size!.Value.Width:0.##}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, [mutation.NodeId], null, preserveStatus: !updateStatus);
        return true;
    }

    public bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node surface editing is disabled by host permissions.";
            }

            return false;
        }

        if (size.Width <= 0d || size.Height <= 0d)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node size must be positive.";
            }

            return false;
        }

        var mutation = _documentMutator.SetNodeSize(CreateActiveScopeDocumentSnapshot(), nodeId, size);
        if (mutation.NodeId is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching node size change was applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Updated node size to {mutation.Size!.Value.Width:0.##} × {mutation.Size.Value.Height:0.##}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, [mutation.NodeId], null, preserveStatus: !updateStatus);
        return true;
    }

    public bool TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            CurrentStatusMessage = "Node surface editing is disabled by host permissions.";
            return false;
        }

        var mutation = _documentMutator.SetNodeExpansionState(CreateActiveScopeDocumentSnapshot(), nodeId, expansionState);
        if (mutation.NodeId is null)
        {
            CurrentStatusMessage = "No matching node surface change was applied.";
            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = expansionState == GraphNodeExpansionState.Expanded
            ? "Expanded node card."
            : "Collapsed node card.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, [mutation.NodeId], null, preserveStatus: true);
        return true;
    }

    public string TryCreateNodeGroupFromSelection(string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            CurrentStatusMessage = "Node grouping is disabled by host permissions.";
            return string.Empty;
        }

        if (_selectedNodeIds.Count == 0)
        {
            CurrentStatusMessage = "Select one or more nodes before creating a group.";
            return string.Empty;
        }

        var groupId = CreateUniqueId(GetAllNodeGroups().Select(group => group.Id), "group-");
        var mutation = _documentMutator.CreateNodeGroupFromSelection(CreateActiveScopeDocumentSnapshot(), _selectedNodeIds, groupId, title);
        if (mutation.Group is null)
        {
            CurrentStatusMessage = "No node group was created.";
            return string.Empty;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Created group {mutation.Group.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: true);
        return mutation.Group.Id;
    }

    public bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            CurrentStatusMessage = "Node grouping is disabled by host permissions.";
            return false;
        }

        var mutation = _documentMutator.SetNodeGroupCollapsed(CreateActiveScopeDocumentSnapshot(), groupId, isCollapsed);
        if (mutation.Group is null)
        {
            CurrentStatusMessage = "No matching group collapse change was applied.";
            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = isCollapsed
            ? $"Collapsed group {mutation.Group.Title}."
            : $"Expanded group {mutation.Group.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: true);
        return true;
    }

    public bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node grouping is disabled by host permissions.";
            }

            return false;
        }

        var mutation = _documentMutator.SetNodeGroupPosition(CreateActiveScopeDocumentSnapshot(), groupId, position, moveMemberNodes);
        if (mutation.Group is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching group position change was applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Moved group {mutation.Group.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: !updateStatus);
        return true;
    }

    public bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node grouping is disabled by host permissions.";
            }

            return false;
        }

        if (size.Width <= 0d || size.Height <= 0d)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Group frame size must be positive.";
            }

            return false;
        }

        var mutation = _documentMutator.SetNodeGroupSize(CreateActiveScopeDocumentSnapshot(), groupId, size);
        if (mutation.Group is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching group size change was applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Resized group {mutation.Group.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: !updateStatus);
        return true;
    }

    public bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node grouping is disabled by host permissions.";
            }

            return false;
        }

        var mutation = _documentMutator.SetNodeGroupExtraPadding(CreateActiveScopeDocumentSnapshot(), groupId, extraPadding);
        if (mutation.Group is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching group padding change was applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Resized group {mutation.Group.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: !updateStatus);
        return true;
    }

    public bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus)
    {
        ArgumentNullException.ThrowIfNull(changes);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Node grouping is disabled by host permissions.";
            }

            return false;
        }

        var mutation = _documentMutator.SetNodeGroupMemberships(CreateActiveScopeDocumentSnapshot(), changes);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No node-group membership changes were applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = mutation.ChangedNodeIds.Count == 1
            ? "Updated 1 node-group membership."
            : $"Updated {mutation.ChangedNodeIds.Count} node-group memberships.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, mutation.ChangedNodeIds, null, preserveStatus: !updateStatus);
        return true;
    }

    public string TryPromoteNodeGroupToComposite(string groupId, string? title, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Composite promotion is disabled by host permissions.";
            }

            return string.Empty;
        }

        var activeDocument = CreateActiveScopeDocumentSnapshot();
        var compositeNodeId = CreateUniqueId(GetAllNodes().Select(node => node.Id), "composite-node-");
        var childGraphId = CreateUniqueId(_document.GraphScopes.Select(scope => scope.Id), "graph-composite-");
        var mutation = _documentMutator.PromoteNodeGroupToComposite(activeDocument, groupId, compositeNodeId, childGraphId, title);
        if (mutation.CompositeNode is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = mutation.FailureReason ?? "No composite node was created.";
            }

            return string.Empty;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Promoted group to composite {mutation.CompositeNode.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, [mutation.CompositeNode.Id], null, preserveStatus: !updateStatus);
        return mutation.CompositeNode.Id;
    }

    public string TryExposeCompositePort(string compositeNodeId, string childNodeId, string childPortId, string? label, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositeNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(childNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(childPortId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Composite boundary editing is disabled by host permissions.";
            }

            return string.Empty;
        }

        var activeDocument = CreateActiveScopeDocumentSnapshot();
        var compositeNode = activeDocument.Nodes.FirstOrDefault(node => string.Equals(node.Id, compositeNodeId, StringComparison.Ordinal));
        var childScope = compositeNode?.Composite is null
            ? null
            : activeDocument.GraphScopes.FirstOrDefault(scope => string.Equals(scope.Id, compositeNode.Composite.ChildGraphId, StringComparison.Ordinal));
        var childNode = childScope?.Nodes.FirstOrDefault(node => string.Equals(node.Id, childNodeId, StringComparison.Ordinal));
        var matchingPort = childNode?.Inputs.FirstOrDefault(port => string.Equals(port.Id, childPortId, StringComparison.Ordinal))
            ?? childNode?.Outputs.FirstOrDefault(port => string.Equals(port.Id, childPortId, StringComparison.Ordinal));
        var existingBoundaryIds = compositeNode?.Composite is null
            ? Enumerable.Empty<string>()
            : (compositeNode.Composite.Inputs ?? [])
                .Concat(compositeNode.Composite.Outputs ?? [])
                .Select(port => port.Id);
        var boundaryPrefix = matchingPort?.Direction == PortDirection.Input
            ? "boundary-input-"
            : "boundary-output-";
        var boundaryPortId = CreateUniqueId(existingBoundaryIds, boundaryPrefix);

        var mutation = _documentMutator.ExposeCompositePort(activeDocument, compositeNodeId, childNodeId, childPortId, boundaryPortId, label);
        if (mutation.CompositeNode is null || string.IsNullOrWhiteSpace(mutation.BoundaryPortId))
        {
            if (updateStatus)
            {
                CurrentStatusMessage = mutation.FailureReason ?? "No composite boundary port was exposed.";
            }

            return string.Empty;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Exposed composite port {mutation.BoundaryPortId}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, [mutation.CompositeNode.Id], null, preserveStatus: !updateStatus);
        return mutation.BoundaryPortId;
    }

    public bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositeNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(boundaryPortId);

        if (!_behaviorOptions.Commands.Nodes.AllowMove)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Composite boundary editing is disabled by host permissions.";
            }

            return false;
        }

        var mutation = _documentMutator.UnexposeCompositePort(CreateActiveScopeDocumentSnapshot(), compositeNodeId, boundaryPortId);
        if (mutation.CompositeNode is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = mutation.FailureReason ?? "No composite boundary port was removed.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Removed composite port {boundaryPortId}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.LayoutChanged, [mutation.CompositeNode.Id], null, preserveStatus: !updateStatus);
        return true;
    }

    public bool TrySetNodeParameterValue(string nodeId, string parameterKey, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterKey);

        if (!_behaviorOptions.Commands.Nodes.AllowEditParameters)
        {
            CurrentStatusMessage = "Parameter editing is disabled by host permissions.";
            return false;
        }

        var node = FindNode(nodeId);
        if (node is null)
        {
            CurrentStatusMessage = "No matching node was found for parameter editing.";
            return false;
        }

        if (node.DefinitionId is null || !_nodeCatalog.TryGetDefinition(node.DefinitionId, out var definition) || definition is null)
        {
            CurrentStatusMessage = "Parameter editing requires a registered node definition.";
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
            CreateActiveScopeDocumentSnapshot(),
            [nodeId],
            parameterDefinition,
            normalized.Value);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            CurrentStatusMessage = $"No parameter changes were applied for {parameterDefinition.DisplayName}.";
            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = $"Updated {parameterDefinition.DisplayName} on {node.Title}.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.ParametersChanged, mutation.ChangedNodeIds, null, preserveStatus: true);
        return true;
    }

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
            CreateActiveScopeDocumentSnapshot(),
            selectedNodes.Select(node => node.Id).ToList(),
            parameterDefinition,
            normalized.Value);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            CurrentStatusMessage = $"No parameter changes were applied for {parameterDefinition.DisplayName}.";
            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        MarkDirty(
            mutation.ChangedNodeIds.Count == 1
                ? $"Updated {parameterDefinition.DisplayName} on 1 node."
                : $"Updated {parameterDefinition.DisplayName} on {mutation.ChangedNodeIds.Count} nodes.",
            GraphEditorDocumentChangeKind.ParametersChanged,
            mutation.ChangedNodeIds,
            null);
        return true;
    }

    public bool TrySetSelectedNodeParameterValues(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
        {
            CurrentStatusMessage = "No parameter changes were supplied.";
            return false;
        }

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

        var normalizedValues = new List<GraphEditorKernelParameterValueUpdate>(values.Count);
        foreach (var pair in values)
        {
            var parameterDefinition = definition.Parameters.FirstOrDefault(parameter => string.Equals(parameter.Key, pair.Key, StringComparison.Ordinal));
            if (parameterDefinition is null)
            {
                CurrentStatusMessage = $"Parameter '{pair.Key}' is not declared by {definition.DisplayName}.";
                return false;
            }

            if (parameterDefinition.Constraints.IsReadOnly)
            {
                CurrentStatusMessage = $"{parameterDefinition.DisplayName} is read-only.";
                return false;
            }

            var normalized = NodeParameterValueAdapter.NormalizeValue(parameterDefinition, pair.Value);
            if (!normalized.IsValid)
            {
                CurrentStatusMessage = normalized.ValidationError ?? $"Parameter '{parameterDefinition.DisplayName}' could not be normalized.";
                return false;
            }

            normalizedValues.Add(new GraphEditorKernelParameterValueUpdate(parameterDefinition, normalized.Value));
        }

        var mutation = _documentMutator.SetNodeParameterValues(
            CreateActiveScopeDocumentSnapshot(),
            selectedNodes.Select(node => node.Id).ToList(),
            normalizedValues);
        if (mutation.ChangedNodeIds.Count == 0)
        {
            CurrentStatusMessage = "No parameter changes were applied.";
            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = normalizedValues.Count == 1
            ? $"Updated {normalizedValues[0].Definition.DisplayName} on {mutation.ChangedNodeIds.Count} node{(mutation.ChangedNodeIds.Count == 1 ? string.Empty : "s")}."
            : $"Updated {normalizedValues.Count} parameters on {mutation.ChangedNodeIds.Count} nodes.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.ParametersChanged, mutation.ChangedNodeIds, null, preserveStatus: true);
        return true;
    }

    public void StartConnection(string sourceNodeId, string sourcePortId)
        => _connectionMutationCoordinator.StartConnection(sourceNodeId, sourcePortId);

    public void CompleteConnection(string targetNodeId, string targetPortId)
        => CompleteConnection(new GraphConnectionTargetRef(targetNodeId, targetPortId));

    public void CompleteConnection(GraphConnectionTargetRef target)
        => _connectionMutationCoordinator.CompleteConnection(target);

    public void CancelPendingConnection()
        => _connectionMutationCoordinator.CancelPendingConnection();

    public void DeleteConnection(string connectionId)
        => _connectionMutationCoordinator.DeleteConnection(connectionId);

    public void DisconnectConnection(string connectionId)
        => _connectionMutationCoordinator.DisconnectConnection(connectionId);

    public void BreakConnectionsForPort(string nodeId, string portId)
        => _connectionMutationCoordinator.BreakConnectionsForPort(nodeId, portId);

    public void DisconnectIncoming(string nodeId)
        => _connectionMutationCoordinator.DisconnectIncoming(nodeId);

    public void DisconnectOutgoing(string nodeId)
        => _connectionMutationCoordinator.DisconnectOutgoing(nodeId);

    public void DisconnectAll(string nodeId)
        => _connectionMutationCoordinator.DisconnectAll(nodeId);

    public bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        if (!(_behaviorOptions.Commands.Connections.AllowCreate
              || _behaviorOptions.Commands.Connections.AllowDelete
              || _behaviorOptions.Commands.Connections.AllowDisconnect))
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Connection presentation editing is disabled by host permissions.";
            }

            return false;
        }

        var mutation = _documentMutator.SetConnectionNoteText(CreateActiveScopeDocumentSnapshot(), connectionId, noteText);
        if (mutation.Connection is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching connection note change was applied.";
            }

            return false;
        }

        ApplyActiveScopeDocument(mutation.Document);
        CurrentStatusMessage = string.IsNullOrWhiteSpace(mutation.Connection.Presentation?.NoteText)
            ? "Cleared connection note."
            : "Updated connection note.";
        MarkDirty(CurrentStatusMessage, GraphEditorDocumentChangeKind.ConnectionsChanged, null, [mutation.Connection.Id], preserveStatus: !updateStatus);
        return true;
    }

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
        if (!_viewportCoordinator.TryFitToViewport(GetViewportSnapshot(), GetActiveGraphScope().Nodes, out var updatedViewport))
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

    public bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(compositeNodeId);

        var activeScope = GetActiveGraphScope();
        var compositeNode = activeScope.Nodes.FirstOrDefault(node => string.Equals(node.Id, compositeNodeId, StringComparison.Ordinal));
        if (compositeNode?.Composite is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching composite node was found in the current scope.";
            }

            return false;
        }

        var childScope = TryGetGraphScope(compositeNode.Composite.ChildGraphId);
        if (childScope is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "The composite node is missing its child graph scope.";
            }

            return false;
        }

        return ApplyScopeNavigation(
            childScope.Id,
            $"Entered composite scope {compositeNode.Title}.",
            updateStatus);
    }

    public bool TryReturnToParentGraphScope(bool updateStatus)
    {
        var parent = TryGetParentScopeNavigation(_activeGraphId);
        if (parent is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "Already at the root scope.";
            }

            return false;
        }

        return ApplyScopeNavigation(
            parent.Value.ParentScopeId,
            $"Returned to parent scope {parent.Value.ParentTitle}.",
            updateStatus);
    }

    public GraphDocument CreateDocumentSnapshot()
        => CloneDocument(_document);

    public GraphDocument CreateActiveScopeDocumentSnapshot()
        => CreateScopedDocumentSnapshot(_activeGraphId);

    public GraphEditorSelectionSnapshot GetSelectionSnapshot()
        => _selectionCoordinator.GetSelectionSnapshot();

    public GraphEditorViewportSnapshot GetViewportSnapshot()
        => new(_zoom, _panX, _panY, _viewportWidth, _viewportHeight);

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot()
        => _projectionCoordinator.GetCapabilitySnapshot();

    public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors()
        => _projectionCoordinator.GetFeatureDescriptors();

    public IReadOnlyList<GraphEditorNodeSurfaceSnapshot> GetNodeSurfaceSnapshots()
        => GetActiveGraphScope().Nodes
            .Select(node =>
            {
                var surface = node.Surface ?? GraphNodeSurfaceState.Default;
                var definition = GraphEditorNodeSurfaceTierResolver.ResolveDefinition(_nodeCatalog, node.DefinitionId);
                var activeTier = GraphEditorNodeSurfaceTierResolver.ResolveActiveTier(node.Size, _behaviorOptions, definition);
                return new GraphEditorNodeSurfaceSnapshot(node.Id, node.Size, activeTier, surface.ExpansionState, surface.GroupId);
            })
            .ToList();

    public IReadOnlyList<GraphEditorCompositeNodeSnapshot> GetCompositeNodeSnapshots()
        => GetActiveGraphScope().Nodes
            .Where(node => node.Composite is not null)
            .Select(node => new GraphEditorCompositeNodeSnapshot(
                node.Id,
                node.Composite!.ChildGraphId,
                (node.Composite.Inputs ?? []).Select(CloneCompositeBoundaryPort).ToList(),
                (node.Composite.Outputs ?? []).Select(CloneCompositeBoundaryPort).ToList()))
            .ToList();

    public GraphEditorScopeNavigationSnapshot GetScopeNavigationSnapshot()
    {
        var parent = TryGetParentScopeNavigation(_activeGraphId);
        var breadcrumbs = BuildScopeBreadcrumbs(_activeGraphId);
        return new(
            _activeGraphId,
            parent?.ParentScopeId,
            parent is not null,
            breadcrumbs);
    }

    public IReadOnlyList<GraphNodeGroup> GetNodeGroups()
        => GetActiveGraphScope().Groups?.Select(CloneGroup).ToList() ?? [];

    public IReadOnlyList<GraphEditorNodeGroupSnapshot> GetNodeGroupSnapshots()
    {
        var activeScope = GetActiveGraphScope();
        if (activeScope.Groups is not { Count: > 0 })
        {
            return [];
        }

        return activeScope.Groups
            .Select(GraphEditorNodeGroupLayoutResolver.CreateSnapshot)
            .ToList();
    }

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
        => _commandRouter.GetCommandDescriptors();

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
        => _commandRouter.TryExecuteCommand(command);

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions()
        => GetActiveGraphScope().Nodes
            .Select(node => new NodePositionSnapshot(node.Id, node.Position))
            .ToList();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot()
        => _pendingConnection;

    public IReadOnlyList<GraphEditorCompatibleConnectionTargetSnapshot> GetCompatibleConnectionTargets(string sourceNodeId, string sourcePortId)
        => _compatibilityQueries.GetCompatibleConnectionTargets(CreateActiveScopeDocumentSnapshot(), sourceNodeId, sourcePortId);

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _compatibilityQueries.GetCompatiblePortTargets(CreateActiveScopeDocumentSnapshot(), sourceNodeId, sourcePortId);

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

    internal GraphDocument CreateDocumentSnapshotWithActiveScopeContents(
        IReadOnlyList<GraphNode> nodes,
        IReadOnlyList<GraphConnection> connections,
        IReadOnlyList<GraphNodeGroup>? groups = null)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(connections);

        var activeDocument = CreateActiveScopeDocumentSnapshot().WithRootGraphContents(nodes, connections, groups);
        return _documentMutator.NormalizeNodeGroupBounds(MergeActiveScopeDocument(activeDocument));
    }

    private bool ApplyRetainedDocumentSnapshot(GraphDocument document, GraphEditorSelectionSnapshot selection)
    {
        _document = _documentMutator.NormalizeNodeGroupBounds(CloneDocument(document));
        return NormalizeSessionStateAfterDocumentChange(selection);
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

        var nodesById = GetActiveGraphScope().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
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
        => GetActiveGraphScope().Nodes.FirstOrDefault(node => string.Equals(node.Id, nodeId, StringComparison.Ordinal));

    private static IReadOnlyDictionary<string, GraphEditorNodeGroupMemberBounds> CreateGroupMemberBounds(IReadOnlyList<GraphNode> nodes)
        => nodes.ToDictionary(
            node => node.Id,
            node => new GraphEditorNodeGroupMemberBounds(
                node.Position,
                node.Size),
            StringComparer.Ordinal);

    private string CreateNodeId(NodeDefinitionId definitionId)
        => CreateNodeId(definitionId, definitionId.Value);

    private string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
        => CreateNodeId(
            (definitionId?.Value ?? fallbackKey)
            .Replace(".", "-", StringComparison.Ordinal));

    private string CreateNodeId(string templateKey)
        => CreateUniqueId(GetAllNodes().Select(node => node.Id), $"{templateKey}-");

    private string CreateConnectionId()
        => CreateUniqueId(GetAllConnections().Select(connection => connection.Id), "connection-");

    private void ApplyActiveScopeDocument(GraphDocument document)
    {
        _document = _documentMutator.NormalizeNodeGroupBounds(MergeActiveScopeDocument(document));
        NormalizeSessionStateAfterDocumentChange();
    }

    private bool ApplyScopeNavigation(string targetScopeId, string status, bool updateStatus)
    {
        if (string.Equals(_activeGraphId, targetScopeId, StringComparison.Ordinal))
        {
            return false;
        }

        if (TryGetGraphScope(targetScopeId) is null)
        {
            if (updateStatus)
            {
                CurrentStatusMessage = "No matching graph scope was found.";
            }

            return false;
        }

        var selectionChanged = _selectedNodeIds.Count > 0 || !string.IsNullOrWhiteSpace(_primarySelectedNodeId);
        var pendingChanged = _pendingConnection.HasPendingConnection;

        _activeGraphId = targetScopeId;
        _selectedNodeIds = [];
        _primarySelectedNodeId = null;
        _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);

        if (updateStatus)
        {
            CurrentStatusMessage = status;
        }

        if (selectionChanged)
        {
            SelectionChanged?.Invoke(this, new GraphEditorSelectionChangedEventArgs(_selectedNodeIds.ToList(), _primarySelectedNodeId));
        }

        if (pendingChanged)
        {
            PendingConnectionChanged?.Invoke(this, new GraphEditorPendingConnectionChangedEventArgs(_pendingConnection));
        }

        DocumentChanged?.Invoke(
            this,
            new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.ScopeChanged, statusMessage: CurrentStatusMessage));
        return true;
    }

    private bool NormalizeSessionStateAfterDocumentChange(GraphEditorSelectionSnapshot? selection = null)
    {
        if (TryGetGraphScope(_activeGraphId) is null)
        {
            _activeGraphId = _document.RootGraphId;
        }

        var activeNodesById = GetActiveGraphScope().Nodes.ToDictionary(node => node.Id, StringComparer.Ordinal);
        var requestedNodeIds = selection?.SelectedNodeIds ?? _selectedNodeIds;
        var selectedNodeIds = requestedNodeIds
            .Where(activeNodesById.ContainsKey)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var requestedPrimaryNodeId = selection?.PrimarySelectedNodeId ?? _primarySelectedNodeId;
        var primaryNodeId = !string.IsNullOrWhiteSpace(requestedPrimaryNodeId)
            && selectedNodeIds.Contains(requestedPrimaryNodeId, StringComparer.Ordinal)
            ? requestedPrimaryNodeId
            : selectedNodeIds.LastOrDefault();
        var selectionChanged = !_selectedNodeIds.SequenceEqual(selectedNodeIds, StringComparer.Ordinal)
            || !string.Equals(_primarySelectedNodeId, primaryNodeId, StringComparison.Ordinal);

        _selectedNodeIds = selectedNodeIds;
        _primarySelectedNodeId = primaryNodeId;

        if (_pendingConnection.HasPendingConnection
            && (string.IsNullOrWhiteSpace(_pendingConnection.SourceNodeId)
                || string.IsNullOrWhiteSpace(_pendingConnection.SourcePortId)
                || !activeNodesById.TryGetValue(_pendingConnection.SourceNodeId, out var sourceNode)
                || !sourceNode.Outputs.Any(port => string.Equals(port.Id, _pendingConnection.SourcePortId, StringComparison.Ordinal))))
        {
            _pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
        }

        return selectionChanged;
    }

    private GraphDocument CreateScopedDocumentSnapshot(string rootGraphId)
    {
        var rootScope = TryGetGraphScope(rootGraphId)
            ?? throw new InvalidOperationException($"Graph scope '{rootGraphId}' was not found.");
        var scopesById = _document.GetGraphScopes().ToDictionary(scope => scope.Id, StringComparer.Ordinal);
        var queuedScopeIds = new Queue<string>();
        var visitedScopeIds = new HashSet<string>(StringComparer.Ordinal);
        var scopedScopes = new List<GraphScope>();

        queuedScopeIds.Enqueue(rootScope.Id);
        while (queuedScopeIds.Count > 0)
        {
            var currentScopeId = queuedScopeIds.Dequeue();
            if (!visitedScopeIds.Add(currentScopeId) || !scopesById.TryGetValue(currentScopeId, out var currentScope))
            {
                continue;
            }

            scopedScopes.Add(CloneScope(currentScope));

            foreach (var childGraphId in currentScope.Nodes
                         .Where(node => node.Composite is not null)
                         .Select(node => node.Composite!.ChildGraphId)
                         .Distinct(StringComparer.Ordinal))
            {
                if (scopesById.ContainsKey(childGraphId))
                {
                    queuedScopeIds.Enqueue(childGraphId);
                }
            }
        }

        return GraphDocument.CreateScoped(_document.Title, _document.Description, rootScope.Id, scopedScopes);
    }

    private GraphDocument MergeActiveScopeDocument(GraphDocument document)
    {
        var currentScopes = _document.GetGraphScopes();
        var replacementScopes = document.GetGraphScopes()
            .ToDictionary(scope => scope.Id, CloneScope, StringComparer.Ordinal);
        var mergedScopes = currentScopes
            .Select(scope => replacementScopes.TryGetValue(scope.Id, out var replacement)
                ? replacement
                : CloneScope(scope))
            .ToList();

        foreach (var replacementScope in replacementScopes.Values)
        {
            if (mergedScopes.Any(scope => string.Equals(scope.Id, replacementScope.Id, StringComparison.Ordinal)))
            {
                continue;
            }

            mergedScopes.Add(CloneScope(replacementScope));
        }

        return GraphDocument.CreateScoped(
            document.Title,
            document.Description,
            _document.RootGraphId,
            mergedScopes);
    }

    private GraphScope GetActiveGraphScope()
        => TryGetGraphScope(_activeGraphId)
            ?? TryGetGraphScope(_document.RootGraphId)
            ?? throw new InvalidOperationException("Root graph scope was not found.");

    private GraphScope? TryGetGraphScope(string scopeId)
        => _document.GraphScopes.FirstOrDefault(scope => string.Equals(scope.Id, scopeId, StringComparison.Ordinal));

    private IReadOnlyList<GraphEditorScopeBreadcrumbSnapshot> BuildScopeBreadcrumbs(string scopeId)
    {
        var breadcrumbs = new List<GraphEditorScopeBreadcrumbSnapshot>();
        var cursor = scopeId;

        while (true)
        {
            breadcrumbs.Add(new GraphEditorScopeBreadcrumbSnapshot(cursor, ResolveScopeTitle(cursor)));
            var parent = TryGetParentScopeNavigation(cursor);
            if (parent is null)
            {
                break;
            }

            cursor = parent.Value.ParentScopeId;
        }

        breadcrumbs.Reverse();
        return breadcrumbs;
    }

    private string ResolveScopeTitle(string scopeId)
    {
        if (string.Equals(scopeId, _document.RootGraphId, StringComparison.Ordinal))
        {
            return _document.Title;
        }

        var parent = TryGetParentScopeNavigation(scopeId);
        return parent?.ParentTitle ?? scopeId;
    }

    private ScopeParentNavigation? TryGetParentScopeNavigation(string scopeId)
    {
        foreach (var scope in _document.GraphScopes)
        {
            var parentNode = scope.Nodes.FirstOrDefault(node =>
                string.Equals(node.Composite?.ChildGraphId, scopeId, StringComparison.Ordinal));
            if (parentNode is null)
            {
                continue;
            }

            return new ScopeParentNavigation(scope.Id, parentNode.Id, parentNode.Title);
        }

        return null;
    }

    private IReadOnlyList<GraphNode> GetAllNodes()
        => _document.GraphScopes.SelectMany(scope => scope.Nodes).ToList();

    private IReadOnlyList<GraphConnection> GetAllConnections()
        => _document.GraphScopes.SelectMany(scope => scope.Connections).ToList();

    private IReadOnlyList<GraphNodeGroup> GetAllNodeGroups()
        => _document.GraphScopes.SelectMany(scope => scope.Groups ?? []).ToList();

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

    private readonly record struct ScopeParentNavigation(string ParentScopeId, string ParentCompositeNodeId, string ParentTitle);

    private static string CreateDocumentSignature(GraphDocument document)
        => GraphDocumentSerializer.Serialize(document);

    private static GraphDocument CloneDocument(GraphDocument document)
        => GraphDocument.CreateScoped(
            document.Title,
            document.Description,
            document.RootGraphId,
            document.GetGraphScopes().Select(CloneScope).ToList());

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
            node.ParameterValues?.Select(CloneParameterValue).ToList(),
            node.Surface is null ? null : node.Surface with { },
            node.Composite is null ? null : CloneComposite(node.Composite));

    private static GraphPort ClonePort(GraphPort port)
        => new(
            port.Id,
            port.Label,
            port.Direction,
            port.DataType,
            port.AccentHex,
            port.TypeId);

    private static GraphNodeGroup CloneGroup(GraphNodeGroup group)
        => new(
            group.Id,
            group.Title,
            group.Position,
            group.Size,
            group.NodeIds.ToList(),
            group.IsCollapsed,
            group.ExtraPadding);

    private static GraphConnection CloneConnection(GraphConnection connection)
        => new(
            connection.Id,
            connection.SourceNodeId,
            connection.SourcePortId,
            connection.TargetNodeId,
            connection.TargetPortId,
            connection.Label,
            connection.AccentHex,
            connection.ConversionId,
            connection.Presentation is null ? null : CloneEdgePresentation(connection.Presentation))
        {
            TargetKind = connection.TargetKind,
        };

    private static GraphScope CloneScope(GraphScope scope)
        => new(
            scope.Id,
            scope.Nodes.Select(CloneNode).ToList(),
            scope.Connections.Select(CloneConnection).ToList(),
            scope.Groups?.Select(CloneGroup).ToList() ?? []);

    private static GraphCompositeNode CloneComposite(GraphCompositeNode composite)
        => new(
            composite.ChildGraphId,
            composite.Inputs?.Select(CloneCompositeBoundaryPort).ToList() ?? [],
            composite.Outputs?.Select(CloneCompositeBoundaryPort).ToList() ?? []);

    private static GraphCompositeBoundaryPort CloneCompositeBoundaryPort(GraphCompositeBoundaryPort port)
        => new(
            port.Id,
            port.Label,
            port.Direction,
            port.DataType,
            port.AccentHex,
            port.ChildNodeId,
            port.ChildPortId,
            port.TypeId);

    private static GraphEdgePresentation CloneEdgePresentation(GraphEdgePresentation presentation)
        => new(presentation.NoteText);

    private static GraphParameterValue CloneParameterValue(GraphParameterValue parameter)
        => new(parameter.Key, parameter.TypeId, parameter.Value);
}
