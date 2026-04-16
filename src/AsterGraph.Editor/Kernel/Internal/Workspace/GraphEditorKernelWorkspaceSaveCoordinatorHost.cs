using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelWorkspaceSaveCoordinatorHost : IGraphEditorWorkspaceSaveCoordinatorHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelWorkspaceSaveCoordinatorHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        bool IGraphEditorWorkspaceSaveCoordinatorHost.CanSaveWorkspace
            => _owner._behaviorOptions.Commands.Workspace.AllowSave;

        string IGraphEditorWorkspaceSaveCoordinatorHost.WorkspacePath
            => _owner._workspaceService.WorkspacePath;

        GraphDocument IGraphEditorWorkspaceSaveCoordinatorHost.CreateWorkspaceDocumentSnapshot()
            => _owner._document;

        void IGraphEditorWorkspaceSaveCoordinatorHost.SaveWorkspaceDocument(GraphDocument document)
            => _owner._workspaceService.Save(document);

        string IGraphEditorWorkspaceSaveCoordinatorHost.CreateWorkspaceDocumentSignature(GraphDocument document)
            => CreateDocumentSignature(document);

        void IGraphEditorWorkspaceSaveCoordinatorHost.SetWorkspaceSaveDisabledStatus()
            => _owner.CurrentStatusMessage = "Saving is disabled by host permissions.";

        void IGraphEditorWorkspaceSaveCoordinatorHost.HandleWorkspaceSaveSucceeded(string signature, string statusMessage)
        {
            _owner._lastSavedDocumentSignature = signature;
            _owner._historyService.ReplaceCurrent(_owner.CaptureHistoryState());
            _owner.CurrentStatusMessage = statusMessage;
            _owner.DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.save.succeeded",
                "workspace.save",
                _owner.CurrentStatusMessage,
                GraphEditorDiagnosticSeverity.Info));
            _owner.DocumentChanged?.Invoke(
                _owner,
                new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.WorkspaceSaved, statusMessage: _owner.CurrentStatusMessage));
        }

        void IGraphEditorWorkspaceSaveCoordinatorHost.HandleWorkspaceSaveFailed(string statusMessage, Exception exception)
        {
            _owner.CurrentStatusMessage = statusMessage;
            _owner.RecoverableFailureRaised?.Invoke(
                _owner,
                new GraphEditorRecoverableFailureEventArgs(
                    "workspace.save.failed",
                    "workspace.save",
                    _owner.CurrentStatusMessage,
                    exception));
        }

        void IGraphEditorWorkspaceSaveCoordinatorHost.CompleteWorkspaceSaveAttempt()
        {
        }
    }
}
