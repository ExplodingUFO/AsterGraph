using System.Collections.ObjectModel;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelPersistenceCoordinatorHost :
        IGraphEditorDocumentLoadCoordinatorHost,
        IGraphEditorWorkspaceSaveCoordinatorHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelPersistenceCoordinatorHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        bool IGraphEditorDocumentLoadCoordinatorHost.IsDirtyTrackingSuspended
        {
            get => _owner._suspendDirtyTracking;
            set => _owner._suspendDirtyTracking = value;
        }

        bool IGraphEditorDocumentLoadCoordinatorHost.IsHistoryTrackingSuspended
        {
            get => _owner._suspendHistoryTracking;
            set => _owner._suspendHistoryTracking = value;
        }

        bool IGraphEditorDocumentLoadCoordinatorHost.IsSelectionTrackingSuspended
        {
            get => _owner._suspendSelectionTracking;
            set => _owner._suspendSelectionTracking = value;
        }

        string IGraphEditorDocumentLoadCoordinatorHost.Title
        {
            get => _owner.Title;
            set => _owner.Title = value;
        }

        string IGraphEditorDocumentLoadCoordinatorHost.Description
        {
            get => _owner.Description;
            set => _owner.Description = value;
        }

        ObservableCollection<NodeViewModel> IGraphEditorDocumentLoadCoordinatorHost.SelectedNodes
            => _owner.SelectedNodes;

        ObservableCollection<NodeParameterViewModel> IGraphEditorDocumentLoadCoordinatorHost.SelectedNodeParameters
            => _owner.SelectedNodeParameters;

        NodeViewModel? IGraphEditorDocumentLoadCoordinatorHost.SelectedNode
        {
            get => _owner.SelectedNode;
            set => _owner.SelectedNode = value;
        }

        bool IGraphEditorDocumentLoadCoordinatorHost.IsDirty
        {
            get => _owner.IsDirty;
            set => _owner.IsDirty = value;
        }

        string IGraphEditorDocumentLoadCoordinatorHost.StatusMessage
        {
            get => _owner.StatusMessage;
            set => _owner.StatusMessage = value;
        }

        void IGraphEditorDocumentLoadCoordinatorHost.ApplyDocumentProjection(GraphDocument document)
            => _owner._documentProjectionApplier.ApplyDocument(
                document,
                _owner.Nodes,
                _owner.Connections,
                _owner._presentationLocalizationCoordinator.ApplyNodePresentation,
                _owner.HandleNodePropertyChanged);

        void IGraphEditorDocumentLoadCoordinatorHost.ClearPendingInteractionState()
            => _owner._pendingInteractionState = null;

        GraphEditorHistoryState IGraphEditorDocumentLoadCoordinatorHost.CaptureHistoryState()
            => _owner.CaptureHistoryState();

        void IGraphEditorDocumentLoadCoordinatorHost.ResetHistory(GraphEditorHistoryState state)
            => _owner._historyService.Reset(state);

        void IGraphEditorDocumentLoadCoordinatorHost.SetLastSavedDocumentSignature(string signature)
            => _owner._lastSavedDocumentSignature = signature;

        void IGraphEditorDocumentLoadCoordinatorHost.RefreshSelectionProjection()
            => _owner.RefreshSelectionProjection();

        void IGraphEditorDocumentLoadCoordinatorHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();

        bool IGraphEditorWorkspaceSaveCoordinatorHost.CanSaveWorkspace
            => _owner.CommandPermissions.Workspace.AllowSave;

        string IGraphEditorWorkspaceSaveCoordinatorHost.WorkspacePath
            => _owner.WorkspacePath;

        GraphDocument IGraphEditorWorkspaceSaveCoordinatorHost.CreateWorkspaceDocumentSnapshot()
            => _owner.CreateViewModelDocumentSnapshot();

        void IGraphEditorWorkspaceSaveCoordinatorHost.SaveWorkspaceDocument(GraphDocument document)
            => _owner._workspaceService.Save(document);

        string IGraphEditorWorkspaceSaveCoordinatorHost.CreateWorkspaceDocumentSignature(GraphDocument document)
            => GraphEditorViewModel.CreateDocumentSignature(document);

        void IGraphEditorWorkspaceSaveCoordinatorHost.SetWorkspaceSaveDisabledStatus()
            => _owner.SetStatus("editor.status.workspace.save.disabledByPermissions", "Saving is disabled by host permissions.");

        void IGraphEditorWorkspaceSaveCoordinatorHost.HandleWorkspaceSaveSucceeded(string signature, string statusMessage)
        {
            _owner._lastSavedDocumentSignature = signature;
            _owner.IsDirty = false;
            _owner.SetStatus("editor.status.workspace.saved", statusMessage);
            _owner.PublishRuntimeDiagnostic(
                "workspace.save.succeeded",
                "workspace.save",
                _owner.StatusMessage,
                GraphEditorDiagnosticSeverity.Info);
        }

        void IGraphEditorWorkspaceSaveCoordinatorHost.HandleWorkspaceSaveFailed(string statusMessage, Exception exception)
        {
            _owner.SetStatus("editor.status.workspace.save.failed", statusMessage);
            _owner.PublishRecoverableFailure("workspace.save.failed", "workspace.save", _owner.StatusMessage, exception);
            _owner.PublishRuntimeDiagnostic(
                "workspace.save.failed",
                "workspace.save",
                _owner.StatusMessage,
                GraphEditorDiagnosticSeverity.Warning,
                exception);
        }

        void IGraphEditorWorkspaceSaveCoordinatorHost.CompleteWorkspaceSaveAttempt()
            => _owner.RaiseComputedPropertyChanges();
    }
}
