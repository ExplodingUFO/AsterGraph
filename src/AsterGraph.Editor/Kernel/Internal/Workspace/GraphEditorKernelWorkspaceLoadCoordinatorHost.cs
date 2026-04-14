using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.Kernel;

internal sealed partial class GraphEditorKernel
{
    private sealed class GraphEditorKernelWorkspaceLoadCoordinatorHost : IGraphEditorWorkspaceLoadCoordinatorHost
    {
        private readonly GraphEditorKernel _owner;

        public GraphEditorKernelWorkspaceLoadCoordinatorHost(GraphEditorKernel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        bool IGraphEditorWorkspaceLoadCoordinatorHost.CanLoadWorkspace
            => _owner._behaviorOptions.Commands.Workspace.AllowLoad;

        bool IGraphEditorWorkspaceLoadCoordinatorHost.WorkspaceExists
            => _owner._workspaceService.Exists();

        string IGraphEditorWorkspaceLoadCoordinatorHost.WorkspacePath
            => _owner._workspaceService.WorkspacePath;

        GraphDocument IGraphEditorWorkspaceLoadCoordinatorHost.LoadWorkspaceDocument()
            => _owner._workspaceService.Load();

        void IGraphEditorWorkspaceLoadCoordinatorHost.SetWorkspaceLoadDisabledStatus()
            => _owner.CurrentStatusMessage = "Loading is disabled by host permissions.";

        void IGraphEditorWorkspaceLoadCoordinatorHost.HandleWorkspaceLoadMissing(string statusMessage)
        {
            _owner.CurrentStatusMessage = statusMessage;
            _owner.DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.load.missing",
                "workspace.load",
                _owner.CurrentStatusMessage,
                GraphEditorDiagnosticSeverity.Warning));
        }

        void IGraphEditorWorkspaceLoadCoordinatorHost.ApplyLoadedWorkspaceDocument(GraphDocument document, string statusMessage)
        {
            _owner.LoadDocument(document, statusMessage, markClean: true, resetHistory: true);
            _owner.CancelPendingConnection();
            _owner.ClearSelection(updateStatus: false);
            _owner.ResetView(updateStatus: false);
        }

        void IGraphEditorWorkspaceLoadCoordinatorHost.HandleWorkspaceLoadSucceeded(string diagnosticMessage)
        {
            _owner.DiagnosticPublished?.Invoke(new GraphEditorDiagnostic(
                "workspace.load.succeeded",
                "workspace.load",
                diagnosticMessage,
                GraphEditorDiagnosticSeverity.Info));
            _owner.DocumentChanged?.Invoke(
                _owner,
                new GraphEditorDocumentChangedEventArgs(GraphEditorDocumentChangeKind.WorkspaceLoaded, statusMessage: _owner.CurrentStatusMessage));
        }

        void IGraphEditorWorkspaceLoadCoordinatorHost.HandleWorkspaceLoadFailed(string statusMessage, Exception exception)
        {
            _owner.CurrentStatusMessage = statusMessage;
            _owner.RecoverableFailureRaised?.Invoke(
                _owner,
                new GraphEditorRecoverableFailureEventArgs(
                    "workspace.load.failed",
                    "workspace.load",
                    _owner.CurrentStatusMessage,
                    exception));
        }
    }
}
