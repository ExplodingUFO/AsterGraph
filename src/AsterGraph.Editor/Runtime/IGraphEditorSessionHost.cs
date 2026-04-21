using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using System.Threading;

namespace AsterGraph.Editor.Runtime;

internal interface IGraphEditorSessionHost
{
    event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;
    event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;
    event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;
    event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported;
    event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported;
    event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged;
    event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailureRaised;
    event Action<GraphEditorDiagnostic>? DiagnosticPublished;

    string CurrentStatusMessage { get; }

    void Undo();
    void Redo();
    void ClearSelection(bool updateStatus);
    void SetSelection(IReadOnlyList<string> nodeIds, string? primaryNodeId, bool updateStatus);
    void AddNode(NodeDefinitionId definitionId, GraphPoint? preferredWorldPosition);
    void DeleteSelection();
    Task<bool> TryCopySelectionAsync(CancellationToken cancellationToken);
    Task<bool> TryPasteSelectionAsync(CancellationToken cancellationToken);
    bool TryExportSelectionFragment(string? path);
    bool TryImportFragment(string? path);
    bool TryClearWorkspaceFragment(string? path);
    string TryExportSelectionAsTemplate(string? name);
    bool TryExportSceneAsSvg(string? path);
    bool TryImportFragmentTemplate(string path);
    bool TryDeleteFragmentTemplate(string path);
    void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus);
    bool TrySetNodeWidth(string nodeId, double width, bool updateStatus);
    bool TrySetNodeSize(string nodeId, GraphSize size, bool updateStatus);
    bool TrySetNodeExpansionState(string nodeId, GraphNodeExpansionState expansionState);
    string TryCreateNodeGroupFromSelection(string title);
    bool TrySetNodeGroupCollapsed(string groupId, bool isCollapsed);
    bool TrySetNodeGroupPosition(string groupId, GraphPoint position, bool moveMemberNodes, bool updateStatus);
    bool TrySetNodeGroupSize(string groupId, GraphSize size, bool updateStatus);
    bool TrySetNodeGroupExtraPadding(string groupId, GraphPadding extraPadding, bool updateStatus);
    bool TrySetNodeGroupMemberships(IReadOnlyList<GraphEditorNodeGroupMembershipChange> changes, bool updateStatus);
    string TryPromoteNodeGroupToComposite(string groupId, string? title, bool updateStatus);
    string TryWrapSelectionToComposite(string? title, bool updateStatus);
    string TryExposeCompositePort(string compositeNodeId, string childNodeId, string childPortId, string? label, bool updateStatus);
    bool TryUnexposeCompositePort(string compositeNodeId, string boundaryPortId, bool updateStatus);
    bool TryEnterCompositeChildGraph(string compositeNodeId, bool updateStatus)
        => false;
    bool TryReturnToParentGraphScope(bool updateStatus)
        => false;
    bool TrySetNodeParameterValue(string nodeId, string parameterKey, object? value);
    bool TrySetSelectedNodeParameterValue(string parameterKey, object? value);
    bool TrySetSelectedNodeParameterValues(IReadOnlyDictionary<string, object?> values);
    void StartConnection(string sourceNodeId, string sourcePortId);
    void CompleteConnection(GraphConnectionTargetRef target);
    void CancelPendingConnection();
    void DeleteConnection(string connectionId);
    bool TryReconnectConnection(string connectionId, bool updateStatus)
        => false;
    bool TrySetConnectionNoteText(string connectionId, string? noteText, bool updateStatus)
        => false;
    void BreakConnectionsForPort(string nodeId, string portId);
    void PanBy(double deltaX, double deltaY);
    void ZoomAt(double factor, GraphPoint screenAnchor);
    void UpdateViewportSize(double width, double height);
    void ResetView(bool updateStatus);
    void FitToViewport(bool updateStatus);
    void CenterViewOnNode(string nodeId);
    void CenterViewAt(GraphPoint worldPoint, bool updateStatus);
    void SaveWorkspace();
    bool LoadWorkspace();

    GraphDocument CreateDocumentSnapshot();
    GraphDocument CreateActiveScopeDocumentSnapshot()
        => CreateDocumentSnapshot();
    GraphEditorSelectionSnapshot GetSelectionSnapshot();
    GraphEditorViewportSnapshot GetViewportSnapshot();
    GraphEditorCapabilitySnapshot GetCapabilitySnapshot();
    GraphEditorFragmentStorageSnapshot GetFragmentStorageSnapshot();
    IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors();
    IReadOnlyList<GraphEditorFragmentTemplateSnapshot> GetFragmentTemplateSnapshots();
    IReadOnlyList<GraphEditorNodeSurfaceSnapshot> GetNodeSurfaceSnapshots();
    GraphEditorHierarchyStateSnapshot GetHierarchyStateSnapshot();
    IReadOnlyList<GraphEditorCompositeNodeSnapshot> GetCompositeNodeSnapshots();
    GraphEditorScopeNavigationSnapshot GetScopeNavigationSnapshot()
        => new(CreateDocumentSnapshot().RootGraphId, null, false, []);
    IReadOnlyList<GraphNodeGroup> GetNodeGroups();
    IReadOnlyList<GraphEditorNodeGroupSnapshot> GetNodeGroupSnapshots();
    IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors();
    bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command);
    IReadOnlyList<NodePositionSnapshot> GetNodePositions();
    GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot();
    IReadOnlyList<GraphEditorCompatibleConnectionTargetSnapshot> GetCompatibleConnectionTargets(string sourceNodeId, string sourcePortId);
    IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId);
}
