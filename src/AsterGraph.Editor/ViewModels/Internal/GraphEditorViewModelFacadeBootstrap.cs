using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelFacadeBootstrap
    {
        public GraphEditorViewModelFacadeBootstrap(GraphEditorViewModel owner, GraphEditorHistoryService historyService)
        {
            ArgumentNullException.ThrowIfNull(owner);
            ArgumentNullException.ThrowIfNull(historyService);

            DocumentProjectionApplier = new GraphEditorDocumentProjectionApplier();
            PresentationLocalizationCoordinatorHost = new GraphEditorViewModelPresentationLocalizationCoordinatorHost(owner);
            PresentationLocalizationCoordinator = new GraphEditorPresentationLocalizationCoordinator(PresentationLocalizationCoordinatorHost);
            StorageProjectionHost = new GraphEditorViewModelStorageProjectionHost(owner);
            StorageProjectionSupport = new GraphEditorStorageProjectionSupport(StorageProjectionHost);
            SelectionProjection = new GraphEditorSelectionProjection(
                owner.LocalizeText,
                (key, fallback, arguments) => owner.LocalizeFormat(key, fallback, arguments));
            KernelProjectionHost = new GraphEditorViewModelKernelProjectionHost(owner);
            KernelProjectionApplier = new GraphEditorKernelProjectionApplier(KernelProjectionHost);
            HistoryStateHost = new GraphEditorViewModelHistoryStateHost(owner);
            HistoryStateCoordinator = new GraphEditorHistoryStateCoordinator(HistoryStateHost, historyService);
            SelectionCoordinatorHost = new GraphEditorViewModelSelectionCoordinatorHost(owner);
            SelectionCoordinator = new GraphEditorSelectionCoordinator(SelectionCoordinatorHost);
            SelectionStateSynchronizerHost = new GraphEditorViewModelSelectionStateSynchronizerHost(owner);
            SelectionStateSynchronizer = new GraphEditorSelectionStateSynchronizer(SelectionStateSynchronizerHost);
            SelectionProjectionApplierHost = new GraphEditorViewModelSelectionProjectionApplierHost(owner);
            SelectionProjectionApplier = new GraphEditorSelectionProjectionApplier(SelectionProjectionApplierHost, SelectionProjection);
            ParameterEditHost = new GraphEditorViewModelParameterEditHost(owner);
            ParameterEditCoordinator = new GraphEditorParameterEditCoordinator(ParameterEditHost);
            DocumentCollectionSynchronizerHost = new GraphEditorViewModelDocumentCollectionSynchronizerHost(owner);
            DocumentCollectionSynchronizer = new GraphEditorDocumentCollectionSynchronizer(DocumentCollectionSynchronizerHost, DocumentProjectionApplier);
            PersistenceCoordinatorHost = new GraphEditorViewModelPersistenceCoordinatorHost(owner);
            DocumentLoadCoordinator = new GraphEditorDocumentLoadCoordinator(PersistenceCoordinatorHost);
            NodePositionDirtyTrackerHost = new GraphEditorViewModelNodePositionDirtyTrackerHost(owner);
            NodePositionDirtyTracker = new GraphEditorNodePositionDirtyTracker(NodePositionDirtyTrackerHost);
            RetainedEventPublisherHost = new GraphEditorViewModelRetainedEventPublisherHost(owner);
            RetainedEventPublisher = new GraphEditorRetainedEventPublisher(RetainedEventPublisherHost);
            NodeLayoutCoordinatorHost = new GraphEditorViewModelNodeLayoutCoordinatorHost(owner);
            NodeLayoutCoordinator = new GraphEditorNodeLayoutCoordinator(NodeLayoutCoordinatorHost);
            WorkspaceSaveCoordinator = new GraphEditorWorkspaceSaveCoordinator(PersistenceCoordinatorHost);
        }

        public GraphEditorDocumentProjectionApplier DocumentProjectionApplier { get; }

        public GraphEditorSelectionProjection SelectionProjection { get; }

        public GraphEditorViewModelKernelProjectionHost KernelProjectionHost { get; }

        public GraphEditorViewModelSelectionProjectionApplierHost SelectionProjectionApplierHost { get; }

        public GraphEditorKernelProjectionApplier KernelProjectionApplier { get; }

        public GraphEditorViewModelHistoryStateHost HistoryStateHost { get; }

        public GraphEditorHistoryStateCoordinator HistoryStateCoordinator { get; }

        public GraphEditorSelectionCoordinator SelectionCoordinator { get; }

        public GraphEditorViewModelSelectionCoordinatorHost SelectionCoordinatorHost { get; }

        public GraphEditorViewModelSelectionStateSynchronizerHost SelectionStateSynchronizerHost { get; }

        public GraphEditorSelectionStateSynchronizer SelectionStateSynchronizer { get; }

        public GraphEditorSelectionProjectionApplier SelectionProjectionApplier { get; }

        public GraphEditorViewModelParameterEditHost ParameterEditHost { get; }

        public GraphEditorParameterEditCoordinator ParameterEditCoordinator { get; }

        public GraphEditorViewModelDocumentCollectionSynchronizerHost DocumentCollectionSynchronizerHost { get; }

        public GraphEditorDocumentCollectionSynchronizer DocumentCollectionSynchronizer { get; }

        public GraphEditorDocumentLoadCoordinator DocumentLoadCoordinator { get; }

        public GraphEditorViewModelNodePositionDirtyTrackerHost NodePositionDirtyTrackerHost { get; }

        public GraphEditorNodePositionDirtyTracker NodePositionDirtyTracker { get; }

        public GraphEditorViewModelRetainedEventPublisherHost RetainedEventPublisherHost { get; }

        public GraphEditorRetainedEventPublisher RetainedEventPublisher { get; }

        public GraphEditorViewModelNodeLayoutCoordinatorHost NodeLayoutCoordinatorHost { get; }

        public GraphEditorNodeLayoutCoordinator NodeLayoutCoordinator { get; }

        public GraphEditorViewModelPresentationLocalizationCoordinatorHost PresentationLocalizationCoordinatorHost { get; }

        public GraphEditorPresentationLocalizationCoordinator PresentationLocalizationCoordinator { get; }

        public GraphEditorViewModelStorageProjectionHost StorageProjectionHost { get; }

        public GraphEditorStorageProjectionSupport StorageProjectionSupport { get; }

        public GraphEditorViewModelPersistenceCoordinatorHost PersistenceCoordinatorHost { get; }

        public GraphEditorWorkspaceSaveCoordinator WorkspaceSaveCoordinator { get; }
    }
}
