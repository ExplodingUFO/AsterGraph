using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.Hosting;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelPresentationLocalizationCoordinatorHost :
        IGraphEditorPresentationLocalizationCoordinatorHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelPresentationLocalizationCoordinatorHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IGraphEditorSession IGraphEditorPresentationLocalizationCoordinatorHost.Session => _owner.Session;

        IReadOnlyList<NodeViewModel> IGraphEditorPresentationLocalizationCoordinatorHost.Nodes => _owner.Nodes;

        NodeViewModel? IGraphEditorPresentationLocalizationCoordinatorHost.FindNode(string nodeId)
            => _owner.FindNode(nodeId);

        INodePresentationProvider? IGraphEditorPresentationLocalizationCoordinatorHost.CurrentNodePresentationProvider
            => _owner._nodePresentationProvider;

        IGraphLocalizationProvider? IGraphEditorPresentationLocalizationCoordinatorHost.CurrentLocalizationProvider
            => _owner._localizationProvider;

        bool IGraphEditorPresentationLocalizationCoordinatorHost.TrySetNodePresentationProvider(
            INodePresentationProvider? provider)
            => _owner.TrySetNodePresentationProvider(provider);

        bool IGraphEditorPresentationLocalizationCoordinatorHost.TrySetLocalizationProvider(
            IGraphLocalizationProvider? provider)
            => _owner.TrySetLocalizationProvider(provider);

        void IGraphEditorPresentationLocalizationCoordinatorHost.RefreshSelectionProjection()
            => _owner.RefreshSelectionProjection();

        void IGraphEditorPresentationLocalizationCoordinatorHost.RaiseComputedPropertyChanges()
            => _owner.RaiseComputedPropertyChanges();

        void IGraphEditorPresentationLocalizationCoordinatorHost.NotifyFragmentLibraryCaptionChanged()
            => _owner.OnPropertyChanged(nameof(FragmentLibraryCaption));
    }
}
