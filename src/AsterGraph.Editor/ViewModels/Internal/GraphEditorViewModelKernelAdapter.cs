using System.Diagnostics.CodeAnalysis;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using System.Threading;

namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorViewModelKernelAdapter : IGraphEditorSessionHost, IDisposable
{
    private readonly GraphEditorKernel _kernel;
    private readonly GraphEditorViewModel _owner;
    private bool _suppressOwnerProjection;

    public GraphEditorViewModelKernelAdapter(GraphEditorKernel kernel, GraphEditorViewModel owner)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));

        _kernel.DocumentChanged += HandleKernelDocumentChanged;
        _kernel.SelectionChanged += HandleKernelSelectionChanged;
        _kernel.ViewportChanged += HandleKernelViewportChanged;
        _kernel.PendingConnectionChanged += HandleKernelPendingConnectionChanged;
        _kernel.RecoverableFailureRaised += HandleKernelRecoverableFailureRaised;
        _kernel.DiagnosticPublished += HandleKernelDiagnosticPublished;

        _owner.FragmentExported += HandleOwnerFragmentExported;
        _owner.FragmentImported += HandleOwnerFragmentImported;
        _owner.RecoverableFailureRaised += HandleOwnerRecoverableFailureRaised;
        _owner.DiagnosticPublished += HandleOwnerDiagnosticPublished;
    }

    public event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;
    public event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;
    public event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported;
    public event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported;
    public event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged;
    public event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailureRaised;
    public event Action<GraphEditorDiagnostic>? DiagnosticPublished;

    public string CurrentStatusMessage => _kernel.CurrentStatusMessage;

    public void Initialize()
    {
        ApplyOwnerDocumentProjection(markClean: !_kernel.IsDirty);
        _owner.ApplyKernelViewport(_kernel.GetViewportSnapshot());
        ApplyOwnerStatusProjection();
    }

    public void Undo() => _kernel.Undo();

    public void Redo() => _kernel.Redo();

    public void ClearSelection(bool updateStatus) => _kernel.ClearSelection(updateStatus);

    public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
        => _kernel.SetSelection(nodeIds, primaryNodeId, updateStatus);

    public void SetConnectionSelection(IReadOnlyList<string> connectionIds, string? primaryConnectionId, bool updateStatus)
        => _kernel.SetConnectionSelection(connectionIds, primaryConnectionId, updateStatus);

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
        => _kernel.AddNode(definitionId, preferredWorldPosition);

    public bool TryInsertNodeIntoConnection(
        string connectionId,
        NodeDefinitionId definitionId,
        string inputTargetId,
        GraphConnectionTargetKind inputTargetKind,
        string outputPortId,
        GraphPoint? preferredWorldPosition)
        => _kernel.TryInsertNodeIntoConnection(
            connectionId,
            definitionId,
            inputTargetId,
            inputTargetKind,
            outputPortId,
            preferredWorldPosition);

    public bool TryDeleteSelectionAndReconnect()
        => _kernel.TryDeleteSelectionAndReconnect();

    public bool TryDetachSelectionFromConnections()
        => _kernel.TryDetachSelectionFromConnections();

    public bool TryDeleteSelectedConnections()
        => _kernel.TryDeleteSelectedConnections();

    public bool TrySliceConnections(GraphPoint start, GraphPoint end)
        => _kernel.TrySliceConnections(start, end);

    public void DeleteSelection() => _kernel.DeleteSelection();

    public Task<bool> TryCopySelectionAsync(CancellationToken cancellationToken)
        => _kernel.TryCopySelectionAsync(cancellationToken);

    public Task<bool> TryPasteSelectionAsync(CancellationToken cancellationToken)
        => _kernel.TryPasteSelectionAsync(cancellationToken);

    public bool TryExportSelectionFragment(string? path)
        => _kernel.TryExportSelectionFragment(path);

    public bool TryImportFragment(string? path)
        => _kernel.TryImportFragment(path);

    public bool TryClearWorkspaceFragment(string? path)
        => _kernel.TryClearWorkspaceFragment(path);

    public string TryExportSelectionAsTemplate(string? name)
        => _kernel.TryExportSelectionAsTemplate(name);

    public bool TryExportSceneAsSvg(string? path)
        => _kernel.TryExportSceneAsSvg(path);

    public bool TryExportSceneAsImage(
        GraphEditorSceneImageExportFormat format,
        string? path,
        GraphEditorSceneImageExportOptions? options)
        => _kernel.TryExportSceneAsImage(format, path, options);

    public bool TryImportFragmentTemplate(string path)
        => _kernel.TryImportFragmentTemplate(path);

    public bool TryDeleteFragmentTemplate(string path)
        => _kernel.TryDeleteFragmentTemplate(path);

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
        => _kernel.SetNodePositions(positions, updateStatus);

    public bool TrySetNodeWidth(string nodeId, double width, bool updateStatus)
        => _kernel.TrySetNodeWidth(nodeId, width, updateStatus);

    public bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus)
        => _kernel.TrySetNodeSize(nodeId, size, updateStatus);

    public bool TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState)
        => _kernel.TrySetNodeExpansionState(nodeId, expansionState);

    public string TryCreateNodeGroupFromSelection(string title)
        => _kernel.TryCreateNodeGroupFromSelection(title);

    public bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed)
        => _kernel.TrySetNodeGroupCollapsed(groupId, isCollapsed);

    public bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus)
        => _kernel.TrySetNodeGroupPosition(groupId, position, moveMemberNodes, updateStatus);

    public bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus)
        => _kernel.TrySetNodeGroupSize(groupId, size, updateStatus);

    public bool TrySetNodeGroupFrame(string groupId, GraphPoint position, GraphSize size, bool updateStatus)
        => _kernel.TrySetNodeGroupFrame(groupId, position, size, updateStatus);

    public bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus)
        => _kernel.TrySetNodeGroupExtraPadding(groupId, extraPadding, updateStatus);

    public bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus)
        => _kernel.TrySetNodeGroupMemberships(changes, updateStatus);

    public string TryPromoteNodeGroupToComposite(string groupId, string? title, bool updateStatus)
        => _kernel.TryPromoteNodeGroupToComposite(groupId, title, updateStatus);

    public string TryWrapSelectionToComposite(string? title, bool updateStatus)
        => _kernel.TryWrapSelectionToComposite(title, updateStatus);

    public string TryExposeCompositePort(string compositeNodeId, string childNodeId, string childPortId, string? label, bool updateStatus)
        => _kernel.TryExposeCompositePort(compositeNodeId, childNodeId, childPortId, label, updateStatus);

    public bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus)
        => _kernel.TryUnexposeCompositePort(compositeNodeId, boundaryPortId, updateStatus);

    public bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus)
        => _kernel.TryEnterCompositeChildGraph(compositeNodeId, updateStatus);

    public bool TryReturnToParentGraphScope(bool updateStatus)
        => _kernel.TryReturnToParentGraphScope(updateStatus);

    public bool TrySetNodeParameterValue(string nodeId, string parameterKey, object? value)
        => _kernel.TrySetNodeParameterValue(nodeId, parameterKey, value);

    public bool TrySetSelectedNodeParameterValue(string parameterKey, object? value)
        => _kernel.TrySetSelectedNodeParameterValue(parameterKey, value);

    public bool TrySetSelectedNodeParameterValues(IReadOnlyDictionary<string, object?> values)
        => _kernel.TrySetSelectedNodeParameterValues(values);

    public void StartConnection(string sourceNodeId, string sourcePortId)
        => _kernel.StartConnection(sourceNodeId, sourcePortId);

    public void CompleteConnection(GraphConnectionTargetRef target)
        => _kernel.CompleteConnection(target);

    public void CancelPendingConnection() => _kernel.CancelPendingConnection();

    public void DeleteConnection(string connectionId) => _kernel.DeleteConnection(connectionId);

    public bool TryReconnectConnection(string connectionId, bool updateStatus)
        => _kernel.TryReconnectConnection(connectionId, updateStatus);

    public bool TrySetConnectionLabel(string connectionId, string? label, bool updateStatus)
        => _kernel.TrySetConnectionLabel(connectionId, label, updateStatus);

    public bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus)
        => _kernel.TrySetConnectionNoteText(connectionId, noteText, updateStatus);

    public bool TryInsertConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus)
        => _kernel.TryInsertConnectionRouteVertex(connectionId, vertexIndex, position, updateStatus);

    public bool TryMoveConnectionRouteVertex(string connectionId, int vertexIndex, GraphPoint position, bool updateStatus)
        => _kernel.TryMoveConnectionRouteVertex(connectionId, vertexIndex, position, updateStatus);

    public bool TryRemoveConnectionRouteVertex(string connectionId, int vertexIndex, bool updateStatus)
        => _kernel.TryRemoveConnectionRouteVertex(connectionId, vertexIndex, updateStatus);

    public void BreakConnectionsForPort(string nodeId, string portId)
        => _kernel.BreakConnectionsForPort(nodeId, portId);

    public void PanBy(double deltaX, double deltaY) => _kernel.PanBy(deltaX, deltaY);

    public void ZoomAt(double factor, GraphPoint screenAnchor) => _kernel.ZoomAt(factor, screenAnchor);

    public void UpdateViewportSize(double width, double height) => _kernel.UpdateViewportSize(width, height);

    public void ResetView(bool updateStatus) => _kernel.ResetView(updateStatus);

    public void FitToViewport(bool updateStatus) => _kernel.FitToViewport(updateStatus);

    public void FitSelectionToViewport(bool updateStatus) => _kernel.FitSelectionToViewport(updateStatus);

    public void FocusSelection(bool updateStatus) => _kernel.FocusSelection(updateStatus);

    public void FocusCurrentScope(bool updateStatus) => _kernel.FocusCurrentScope(updateStatus);

    public void CenterViewOnNode(string nodeId) => _kernel.CenterViewOnNode(nodeId);

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus) => _kernel.CenterViewAt(worldPoint, updateStatus);

    public void SaveWorkspace() => _kernel.SaveWorkspace();

    public bool LoadWorkspace() => _kernel.LoadWorkspace();

    public GraphDocument CreateDocumentSnapshot() => _kernel.CreateDocumentSnapshot();

    public GraphDocument CreateActiveScopeDocumentSnapshot() => _kernel.CreateActiveScopeDocumentSnapshot();

    public GraphEditorSelectionSnapshot GetSelectionSnapshot() => _kernel.GetSelectionSnapshot();

    public GraphEditorViewportSnapshot GetViewportSnapshot() => _kernel.GetViewportSnapshot();

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot() => _kernel.GetCapabilitySnapshot();

    public GraphEditorFragmentStorageSnapshot GetFragmentStorageSnapshot() => _kernel.GetFragmentStorageSnapshot();

    public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors() => _kernel.GetFeatureDescriptors();

    public IReadOnlyList<GraphEditorFragmentTemplateSnapshot> GetFragmentTemplateSnapshots()
        => _kernel.GetFragmentTemplateSnapshots();

    public IReadOnlyList<GraphEditorNodeSurfaceSnapshot> GetNodeSurfaceSnapshots() => _kernel.GetNodeSurfaceSnapshots();

    public GraphEditorHierarchyStateSnapshot GetHierarchyStateSnapshot() => _kernel.GetHierarchyStateSnapshot();

    public IReadOnlyList<GraphEditorCompositeNodeSnapshot> GetCompositeNodeSnapshots() => _kernel.GetCompositeNodeSnapshots();

    public GraphEditorScopeNavigationSnapshot GetScopeNavigationSnapshot() => _kernel.GetScopeNavigationSnapshot();

    public IReadOnlyList<GraphNodeGroup> GetNodeGroups() => _kernel.GetNodeGroups();

    public IReadOnlyList<GraphEditorNodeGroupSnapshot> GetNodeGroupSnapshots()
        => _kernel.GetNodeGroupSnapshots();

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
    {
        var descriptors = _kernel.GetCommandDescriptors()
            .ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        foreach (var retainedDescriptor in CreateRetainedCommandDescriptors())
        {
            descriptors[retainedDescriptor.Id] = descriptors.TryGetValue(retainedDescriptor.Id, out var kernelDescriptor)
                && !retainedDescriptor.IsEnabled
                && retainedDescriptor.DisabledReason is null
                && kernelDescriptor.DisabledReason is not null
                    ? GraphEditorCommandDescriptorCatalog.Create(
                        retainedDescriptor.Id,
                        retainedDescriptor.Source,
                        retainedDescriptor.IsEnabled,
                        kernelDescriptor.DisabledReason)
                    : retainedDescriptor;
        }

        return descriptors.Values
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
    }

    private IReadOnlyList<GraphEditorCommandDescriptorSnapshot> CreateRetainedCommandDescriptors()
        =>
        [
            GraphEditorCommandDescriptorCatalog.Create("nodes.inspect", GraphEditorCommandSourceKind.Retained, true),
            GraphEditorCommandDescriptorCatalog.Create("nodes.delete-by-id", GraphEditorCommandSourceKind.Retained, _owner.CommandPermissions.Nodes.AllowDelete),
            GraphEditorCommandDescriptorCatalog.Create("nodes.duplicate", GraphEditorCommandSourceKind.Retained, _owner.CommandPermissions.Nodes.AllowDuplicate),
            GraphEditorCommandDescriptorCatalog.Create("clipboard.copy", GraphEditorCommandSourceKind.Retained, _owner.CanCopySelection),
            GraphEditorCommandDescriptorCatalog.Create("clipboard.paste", GraphEditorCommandSourceKind.Retained, _owner.CanPaste),
            GraphEditorCommandDescriptorCatalog.Create("connections.disconnect", GraphEditorCommandSourceKind.Retained, _owner.CommandPermissions.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create("connections.disconnect-incoming", GraphEditorCommandSourceKind.Retained, _owner.CommandPermissions.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create("connections.disconnect-outgoing", GraphEditorCommandSourceKind.Retained, _owner.CommandPermissions.Connections.AllowDisconnect),
            GraphEditorCommandDescriptorCatalog.Create("connections.disconnect-all", GraphEditorCommandSourceKind.Retained, _owner.CommandPermissions.Connections.AllowDisconnect),
        ];

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        switch (command.CommandId)
        {
            case "nodes.inspect":
                if (!TryGetRequiredArgument(command, "nodeId", out var inspectNodeId))
                {
                    return false;
                }

                _owner.SelectSingleNode(_owner.FindNode(inspectNodeId));
                return true;

            case "nodes.delete-by-id":
                if (!TryGetRequiredArgument(command, "nodeId", out var deleteNodeId))
                {
                    return false;
                }

                _owner.DeleteNodeById(deleteNodeId);
                return true;

            case "nodes.duplicate":
                if (!TryGetRequiredArgument(command, "nodeId", out var duplicateNodeId))
                {
                    return false;
                }

                _owner.DuplicateNode(duplicateNodeId);
                return true;

            case "clipboard.copy":
                if (_owner.CopySelectionCommand.CanExecute(null))
                {
                    _owner.CopySelectionCommand.Execute(null);
                }

                return true;

            case "clipboard.paste":
                if (_owner.PasteCommand.CanExecute(null))
                {
                    _owner.PasteCommand.Execute(null);
                }

                return true;

            case "connections.disconnect-incoming":
                if (!TryGetRequiredArgument(command, "nodeId", out var incomingNodeId))
                {
                    return false;
                }

                _kernel.DisconnectIncoming(incomingNodeId);
                return true;

            case "connections.disconnect":
                if (!TryGetRequiredArgument(command, "connectionId", out var disconnectConnectionId))
                {
                    return false;
                }

                _kernel.DisconnectConnection(disconnectConnectionId);
                return true;

            case "connections.disconnect-outgoing":
                if (!TryGetRequiredArgument(command, "nodeId", out var outgoingNodeId))
                {
                    return false;
                }

                _kernel.DisconnectOutgoing(outgoingNodeId);
                return true;

            case "connections.disconnect-all":
                if (!TryGetRequiredArgument(command, "nodeId", out var disconnectNodeId))
                {
                    return false;
                }

                _kernel.DisconnectAll(disconnectNodeId);
                return true;

            default:
                return _kernel.TryExecuteCommand(command);
        }
    }

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions() => _kernel.GetNodePositions();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot() => _kernel.GetPendingConnectionSnapshot();

    public IReadOnlyList<GraphEditorEdgeTemplateSnapshot> GetEdgeTemplateSnapshots(string sourceNodeId, string sourcePortId)
        => _kernel.GetEdgeTemplateSnapshots(sourceNodeId, sourcePortId);

    internal void CommitRetainedMutation(
        GraphDocument document,
        GraphEditorSelectionSnapshot selection,
        string status,
        GraphEditorDocumentChangeKind changeKind,
        IReadOnlyList<string>? nodeIds = null,
        IReadOnlyList<string>? connectionIds = null)
    {
        _suppressOwnerProjection = true;
        try
        {
            _kernel.CommitRetainedMutation(document, selection, status, changeKind, nodeIds, connectionIds);
        }
        finally
        {
            _suppressOwnerProjection = false;
        }
    }

    internal void SaveRetainedWorkspace(GraphDocument document, GraphEditorSelectionSnapshot selection)
    {
        _suppressOwnerProjection = true;
        try
        {
            _kernel.SaveRetainedWorkspace(document, selection);
        }
        finally
        {
            _suppressOwnerProjection = false;
        }
    }

    private static bool TryGetRequiredArgument(
        GraphEditorCommandInvocationSnapshot command,
        string name,
        [NotNullWhen(true)] out string? value)
    {
        if (!command.TryGetArgument(name, out value) || string.IsNullOrWhiteSpace(value))
        {
            value = null;
            return false;
        }

        return true;
    }

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _kernel.GetCompatiblePortTargets(sourceNodeId, sourcePortId);

    public void Dispose()
    {
        _kernel.DocumentChanged -= HandleKernelDocumentChanged;
        _kernel.SelectionChanged -= HandleKernelSelectionChanged;
        _kernel.ViewportChanged -= HandleKernelViewportChanged;
        _kernel.PendingConnectionChanged -= HandleKernelPendingConnectionChanged;
        _kernel.RecoverableFailureRaised -= HandleKernelRecoverableFailureRaised;
        _kernel.DiagnosticPublished -= HandleKernelDiagnosticPublished;

        _owner.FragmentExported -= HandleOwnerFragmentExported;
        _owner.FragmentImported -= HandleOwnerFragmentImported;
        _owner.RecoverableFailureRaised -= HandleOwnerRecoverableFailureRaised;
        _owner.DiagnosticPublished -= HandleOwnerDiagnosticPublished;
    }

    private void HandleKernelDocumentChanged(object? sender, GraphEditorDocumentChangedEventArgs args)
    {
        var markClean = args.ChangeKind is GraphEditorDocumentChangeKind.WorkspaceLoaded or GraphEditorDocumentChangeKind.WorkspaceSaved;
        if (!_suppressOwnerProjection)
        {
            ApplyOwnerDocumentProjection(markClean);
        }

        ApplyOwnerStatusProjection();
        if (!_suppressOwnerProjection)
        {
            _owner.NotifyKernelProjectedDocumentChanged(args);
        }

        DocumentChanged?.Invoke(this, args);
    }

    private void HandleKernelSelectionChanged(object? sender, GraphEditorSelectionChangedEventArgs args)
    {
        if (!_suppressOwnerProjection)
        {
            _owner.ApplyKernelSelection(args.SelectedNodeIds, args.PrimarySelectedNodeId);
        }

        ApplyOwnerStatusProjection();
        SelectionChanged?.Invoke(this, args);
    }

    private void HandleKernelViewportChanged(object? sender, GraphEditorViewportChangedEventArgs args)
    {
        _owner.ApplyKernelViewport(new GraphEditorViewportSnapshot(args.Zoom, args.PanX, args.PanY, args.ViewportWidth, args.ViewportHeight));
        ViewportChanged?.Invoke(this, args);
    }

    private void HandleKernelPendingConnectionChanged(object? sender, GraphEditorPendingConnectionChangedEventArgs args)
    {
        _owner.ApplyKernelPendingConnection(args.PendingConnection);
        ApplyOwnerStatusProjection();
        PendingConnectionChanged?.Invoke(this, args);
    }

    private void HandleKernelRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
    {
        ApplyOwnerStatusProjection();
        _owner.ApplyKernelRecoverableFailure(args);
    }

    private void HandleKernelDiagnosticPublished(GraphEditorDiagnostic diagnostic)
    {
        _owner.ApplyKernelDiagnostic(diagnostic);
    }

    private void HandleOwnerFragmentExported(object? sender, GraphEditorFragmentEventArgs args)
        => FragmentExported?.Invoke(this, args);

    private void HandleOwnerFragmentImported(object? sender, GraphEditorFragmentEventArgs args)
        => FragmentImported?.Invoke(this, args);

    private void HandleOwnerRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
        => RecoverableFailureRaised?.Invoke(this, args);

    private void HandleOwnerDiagnosticPublished(GraphEditorDiagnostic diagnostic)
        => DiagnosticPublished?.Invoke(diagnostic);

    private void ApplyOwnerDocumentProjection(bool markClean)
    {
        _owner.ApplyKernelDocument(_kernel.CreateActiveScopeDocumentSnapshot(), _kernel.CurrentStatusMessage, markClean);
        _owner.ApplyKernelSelection(_kernel.GetSelectionSnapshot());
        _owner.ApplyKernelPendingConnection(_kernel.GetPendingConnectionSnapshot());
    }

    private void ApplyOwnerStatusProjection()
    {
        _owner.ApplyKernelStatus(_kernel.CurrentStatusMessage);
        _owner.ApplyKernelDirtyState(_kernel.IsDirty);
    }
}
