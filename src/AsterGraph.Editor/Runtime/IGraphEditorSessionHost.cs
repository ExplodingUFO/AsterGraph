using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;

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
    void SetNodePositions(IReadOnlyList<NodePositionSnapshot> positions, bool updateStatus);
    void StartConnection(string sourceNodeId, string sourcePortId);
    void CompleteConnection(string targetNodeId, string targetPortId);
    void CancelPendingConnection();
    void DeleteConnection(string connectionId);
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
    GraphEditorSelectionSnapshot GetSelectionSnapshot();
    GraphEditorViewportSnapshot GetViewportSnapshot();
    GraphEditorCapabilitySnapshot GetCapabilitySnapshot();
    IReadOnlyList<GraphEditorFeatureDescriptorSnapshot> GetFeatureDescriptors();
    IReadOnlyList<GraphEditorCommandDescriptorSnapshot> GetCommandDescriptors();
    bool TryExecuteCommand(GraphEditorCommandInvocationSnapshot command);
    IReadOnlyList<NodePositionSnapshot> GetNodePositions();
    GraphEditorPendingConnectionSnapshot GetPendingConnectionSnapshot();
    IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot> GetCompatiblePortTargets(string sourceNodeId, string sourcePortId);
}
