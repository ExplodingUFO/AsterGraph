using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelDocumentCollectionSynchronizerHost :
        IGraphEditorDocumentCollectionSynchronizerHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelDocumentCollectionSynchronizerHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        void IGraphEditorDocumentCollectionSynchronizerHost.CoerceSelectionToExistingNodes()
            => _owner.CoerceSelectionToExistingNodes();

        void IGraphEditorDocumentCollectionSynchronizerHost.NotifyFitViewCommandCanExecuteChanged()
            => _owner.FitViewCommand.NotifyCanExecuteChanged();

        void IGraphEditorDocumentCollectionSynchronizerHost.RefreshSelectionProjection()
            => _owner.RefreshSelectionProjection();

        void IGraphEditorDocumentCollectionSynchronizerHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();
    }
}
