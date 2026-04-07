using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Kernel;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;

namespace AsterGraph.Editor.ViewModels;

internal sealed class GraphEditorViewModelKernelAdapter : IGraphEditorSessionHost, IDisposable
{
    private readonly GraphEditorKernel _kernel;
    private readonly GraphEditorViewModel _owner;

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
        _owner.ApplyKernelDocument(_kernel.CreateDocumentSnapshot(), _kernel.CurrentStatusMessage, markClean: !_kernel.IsDirty);
        _owner.ApplyKernelSelection(_kernel.GetSelectionSnapshot());
        _owner.ApplyKernelViewport(_kernel.GetViewportSnapshot());
        _owner.ApplyKernelPendingConnection(_kernel.GetPendingConnectionSnapshot());
        _owner.ApplyKernelStatus(_kernel.CurrentStatusMessage);
        _owner.ApplyKernelDirtyState(_kernel.IsDirty);
    }

    public void Undo() => _kernel.Undo();

    public void Redo() => _kernel.Redo();

    public void ClearSelection(bool updateStatus) => _kernel.ClearSelection(updateStatus);

    public void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus)
        => _kernel.SetSelection(nodeIds, primaryNodeId, updateStatus);

    public void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition)
        => _kernel.AddNode(definitionId, preferredWorldPosition);

    public void DeleteSelection() => _kernel.DeleteSelection();

    public void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus)
        => _kernel.SetNodePositions(positions, updateStatus);

    public void StartConnection(string sourceNodeId, string sourcePortId)
        => _kernel.StartConnection(sourceNodeId, sourcePortId);

    public void CompleteConnection(string targetNodeId, string targetPortId)
        => _kernel.CompleteConnection(targetNodeId, targetPortId);

    public void CancelPendingConnection() => _kernel.CancelPendingConnection();

    public void DeleteConnection(string connectionId) => _kernel.DeleteConnection(connectionId);

    public void BreakConnectionsForPort(string nodeId, string portId)
        => _kernel.BreakConnectionsForPort(nodeId, portId);

    public void PanBy(double deltaX, double deltaY) => _kernel.PanBy(deltaX, deltaY);

    public void ZoomAt(double factor, GraphPoint screenAnchor) => _kernel.ZoomAt(factor, screenAnchor);

    public void UpdateViewportSize(double width, double height) => _kernel.UpdateViewportSize(width, height);

    public void ResetView(bool updateStatus) => _kernel.ResetView(updateStatus);

    public void FitToViewport(bool updateStatus) => _kernel.FitToViewport(updateStatus);

    public void CenterViewOnNode(string nodeId) => _kernel.CenterViewOnNode(nodeId);

    public void CenterViewAt(GraphPoint worldPoint, bool updateStatus) => _kernel.CenterViewAt(worldPoint, updateStatus);

    public void SaveWorkspace() => _kernel.SaveWorkspace();

    public bool LoadWorkspace() => _kernel.LoadWorkspace();

    public GraphDocument CreateDocumentSnapshot() => _kernel.CreateDocumentSnapshot();

    public GraphEditorSelectionSnapshot GetSelectionSnapshot() => _kernel.GetSelectionSnapshot();

    public GraphEditorViewportSnapshot GetViewportSnapshot() => _kernel.GetViewportSnapshot();

    public GraphEditorCapabilitySnapshot GetCapabilitySnapshot() => _kernel.GetCapabilitySnapshot();

    public IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors() => _kernel.GetFeatureDescriptors();

    public IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors()
    {
        var descriptors = _kernel.GetCommandDescriptors()
            .Concat(
            [
                new GraphEditorCommandDescriptorSnapshot("fragments.export-selection", _owner.CanExportSelectionFragment),
                new GraphEditorCommandDescriptorSnapshot("fragments.import", _owner.CanImportFragment),
                new GraphEditorCommandDescriptorSnapshot("layout.align-left", _owner.CanAlignSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.align-center", _owner.CanAlignSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.align-right", _owner.CanAlignSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.align-top", _owner.CanAlignSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.align-middle", _owner.CanAlignSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.align-bottom", _owner.CanAlignSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.distribute-horizontal", _owner.CanDistributeSelection),
                new GraphEditorCommandDescriptorSnapshot("layout.distribute-vertical", _owner.CanDistributeSelection),
                new GraphEditorCommandDescriptorSnapshot("nodes.inspect", true),
                new GraphEditorCommandDescriptorSnapshot("nodes.delete-by-id", _owner.CommandPermissions.Nodes.AllowDelete),
                new GraphEditorCommandDescriptorSnapshot("nodes.duplicate", _owner.CommandPermissions.Nodes.AllowDuplicate),
                new GraphEditorCommandDescriptorSnapshot("connections.disconnect-incoming", _owner.CommandPermissions.Connections.AllowDisconnect),
                new GraphEditorCommandDescriptorSnapshot("connections.disconnect-outgoing", _owner.CommandPermissions.Connections.AllowDisconnect),
                new GraphEditorCommandDescriptorSnapshot("connections.disconnect-all", _owner.CommandPermissions.Connections.AllowDisconnect),
            ])
            .GroupBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

        return descriptors;
    }

    public bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command)
    {
        ArgumentNullException.ThrowIfNull(command);

        switch (command.CommandId)
        {
            case "fragments.export-selection":
                _owner.ExportSelectionFragment();
                return true;

            case "fragments.import":
                _owner.ImportFragment();
                return true;

            case "layout.align-left":
                _owner.AlignSelectionLeft();
                return true;

            case "layout.align-center":
                _owner.AlignSelectionCenter();
                return true;

            case "layout.align-right":
                _owner.AlignSelectionRight();
                return true;

            case "layout.align-top":
                _owner.AlignSelectionTop();
                return true;

            case "layout.align-middle":
                _owner.AlignSelectionMiddle();
                return true;

            case "layout.align-bottom":
                _owner.AlignSelectionBottom();
                return true;

            case "layout.distribute-horizontal":
                _owner.DistributeSelectionHorizontally();
                return true;

            case "layout.distribute-vertical":
                _owner.DistributeSelectionVertically();
                return true;

            case "nodes.inspect":
                if (!command.TryGetArgument("nodeId", out var inspectNodeId))
                {
                    return false;
                }

                _owner.SelectSingleNode(_owner.FindNode(inspectNodeId));
                return true;

            case "nodes.delete-by-id":
                if (!command.TryGetArgument("nodeId", out var deleteNodeId))
                {
                    return false;
                }

                _owner.DeleteNodeById(deleteNodeId);
                return true;

            case "nodes.duplicate":
                if (!command.TryGetArgument("nodeId", out var duplicateNodeId))
                {
                    return false;
                }

                _owner.DuplicateNode(duplicateNodeId);
                return true;

            case "connections.disconnect-incoming":
                if (!command.TryGetArgument("nodeId", out var incomingNodeId))
                {
                    return false;
                }

                _owner.DisconnectIncoming(incomingNodeId);
                return true;

            case "connections.disconnect-outgoing":
                if (!command.TryGetArgument("nodeId", out var outgoingNodeId))
                {
                    return false;
                }

                _owner.DisconnectOutgoing(outgoingNodeId);
                return true;

            case "connections.disconnect-all":
                if (!command.TryGetArgument("nodeId", out var disconnectNodeId))
                {
                    return false;
                }

                _owner.DisconnectAll(disconnectNodeId);
                return true;

            default:
                return _kernel.TryExecuteCommand(command);
        }
    }

    public IReadOnlyList<NodePositionSnapshot> GetNodePositions() => _kernel.GetNodePositions();

    public GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot() => _kernel.GetPendingConnectionSnapshot();

    public IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId)
        => _kernel.GetCompatiblePortTargets(sourceNodeId, sourcePortId);

#pragma warning disable CS0618
    public IReadOnlyList<CompatiblePortTarget> GetCompatibleTargets(string sourceNodeId, string sourcePortId)
        => _kernel.GetCompatibleTargets(sourceNodeId, sourcePortId);
#pragma warning restore CS0618

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
        _owner.ApplyKernelDocument(_kernel.CreateDocumentSnapshot(), _kernel.CurrentStatusMessage, markClean);
        _owner.ApplyKernelSelection(_kernel.GetSelectionSnapshot());
        _owner.ApplyKernelPendingConnection(_kernel.GetPendingConnectionSnapshot());
        _owner.ApplyKernelStatus(_kernel.CurrentStatusMessage);
        _owner.ApplyKernelDirtyState(_kernel.IsDirty);
        DocumentChanged?.Invoke(this, args);
    }

    private void HandleKernelSelectionChanged(object? sender, GraphEditorSelectionChangedEventArgs args)
    {
        _owner.ApplyKernelSelection(args.SelectedNodeIds, args.PrimarySelectedNodeId);
        _owner.ApplyKernelStatus(_kernel.CurrentStatusMessage);
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
        _owner.ApplyKernelStatus(_kernel.CurrentStatusMessage);
        PendingConnectionChanged?.Invoke(this, args);
    }

    private void HandleKernelRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
    {
        _owner.ApplyKernelStatus(_kernel.CurrentStatusMessage);
        RecoverableFailureRaised?.Invoke(this, args);
    }

    private void HandleKernelDiagnosticPublished(GraphEditorDiagnostic diagnostic)
        => DiagnosticPublished?.Invoke(diagnostic);

    private void HandleOwnerFragmentExported(object? sender, GraphEditorFragmentEventArgs args)
        => FragmentExported?.Invoke(this, args);

    private void HandleOwnerFragmentImported(object? sender, GraphEditorFragmentEventArgs args)
        => FragmentImported?.Invoke(this, args);

    private void HandleOwnerRecoverableFailureRaised(object? sender, GraphEditorRecoverableFailureEventArgs args)
        => RecoverableFailureRaised?.Invoke(this, args);

    private void HandleOwnerDiagnosticPublished(GraphEditorDiagnostic diagnostic)
        => DiagnosticPublished?.Invoke(diagnostic);
}
