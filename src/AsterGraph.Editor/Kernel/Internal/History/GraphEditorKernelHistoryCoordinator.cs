using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelHistoryCoordinator
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelHistoryCoordinator(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public bool IsDirty(string? lastSavedDocumentSignature)
            => !string.Equals(CreateDocumentSignature(_owner._document), lastSavedDocumentSignature, StringComparison.Ordinal);

        public void Undo()
        {
            if (!_owner._behaviorOptions.History.EnableUndoRedo || !_owner._behaviorOptions.Commands.History.AllowUndo)
            {
                _owner.CurrentStatusMessage = "Undo is disabled by host permissions.";
                return;
            }

            if (!_owner._historyService.TryUndo(out var state) || state is null)
            {
                _owner.CurrentStatusMessage = "No more undo steps.";
                return;
            }

            RestoreHistoryState(state, "Undo applied.", GraphEditorDocumentChangeKind.Undo);
        }

        public void Redo()
        {
            if (!_owner._behaviorOptions.History.EnableUndoRedo || !_owner._behaviorOptions.Commands.History.AllowRedo)
            {
                _owner.CurrentStatusMessage = "Redo is disabled by host permissions.";
                return;
            }

            if (!_owner._historyService.TryRedo(out var state) || state is null)
            {
                _owner.CurrentStatusMessage = "No more redo steps.";
                return;
            }

            RestoreHistoryState(state, "Redo applied.", GraphEditorDocumentChangeKind.Redo);
        }

        public void ApplyViewportSnapshot(GraphEditorViewportSnapshot snapshot)
        {
            _owner._zoom = snapshot.Zoom;
            _owner._panX = snapshot.PanX;
            _owner._panY = snapshot.PanY;
            _owner._viewportWidth = snapshot.ViewportWidth;
            _owner._viewportHeight = snapshot.ViewportHeight;
            _owner.ViewportChanged?.Invoke(
                _owner,
                new GraphEditorViewportChangedEventArgs(_owner._zoom, _owner._panX, _owner._panY, _owner._viewportWidth, _owner._viewportHeight));
        }

        public void MarkDirty(
            string status,
            GraphEditorDocumentChangeKind changeKind,
            IReadOnlyList<string>? nodeIds,
            IReadOnlyList<string>? connectionIds,
            bool preserveStatus = false)
        {
            if (!preserveStatus)
            {
                _owner.CurrentStatusMessage = status;
            }

            var state = CaptureHistoryState();
            _owner._historyService.Push(state);
            _owner.DocumentChanged?.Invoke(
                _owner,
                new GraphEditorDocumentChangedEventArgs(changeKind, nodeIds, connectionIds, _owner.CurrentStatusMessage));
        }

        public void LoadDocument(GraphDocument document, string status, bool markClean, bool resetHistory)
        {
            _owner._document = CloneDocument(document);
            _owner._selectedNodeIds = [];
            _owner._primarySelectedNodeId = null;
            _owner._pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
            _owner.CurrentStatusMessage = status;

            var historyState = CaptureHistoryState();
            if (resetHistory)
            {
                _owner._historyService.Reset(historyState);
            }

            if (markClean)
            {
                _owner._lastSavedDocumentSignature = historyState.Signature;
            }
        }

        public GraphEditorHistoryState CaptureHistoryState()
            => new(
                CloneDocument(_owner._document),
                _owner._selectedNodeIds.ToList(),
                _owner._primarySelectedNodeId,
                CreateDocumentSignature(_owner._document));

        private void RestoreHistoryState(GraphEditorHistoryState state, string status, GraphEditorDocumentChangeKind changeKind)
        {
            _owner._document = state.Document;
            _owner._selectedNodeIds = state.SelectedNodeIds.ToList();
            _owner._primarySelectedNodeId = state.PrimarySelectedNodeId;
            _owner._pendingConnection = GraphEditorPendingConnectionSnapshot.Create(false, null, null);
            _owner.CurrentStatusMessage = status;
            _owner.SelectionChanged?.Invoke(_owner, new GraphEditorSelectionChangedEventArgs(_owner._selectedNodeIds.ToList(), _owner._primarySelectedNodeId));
            _owner.PendingConnectionChanged?.Invoke(_owner, new GraphEditorPendingConnectionChangedEventArgs(_owner._pendingConnection));
            _owner.DocumentChanged?.Invoke(_owner, new GraphEditorDocumentChangedEventArgs(changeKind, statusMessage: _owner.CurrentStatusMessage));
        }
    }
}
