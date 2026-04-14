using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelRetainedEventPublisherHost : IGraphEditorRetainedEventPublisherHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelRetainedEventPublisherHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        IReadOnlyList<NodeViewModel> IGraphEditorRetainedEventPublisherHost.SelectionNodes
            => _owner.SelectedNodes;

        NodeViewModel? IGraphEditorRetainedEventPublisherHost.PrimarySelectedNode
            => _owner.SelectedNode;

        bool IGraphEditorRetainedEventPublisherHost.HasPendingConnection
            => _owner.HasPendingConnection;

        NodeViewModel? IGraphEditorRetainedEventPublisherHost.PendingSourceNode
            => _owner.PendingSourceNode;

        PortViewModel? IGraphEditorRetainedEventPublisherHost.PendingSourcePort
            => _owner.PendingSourcePort;

        string IGraphEditorRetainedEventPublisherHost.CurrentStatusMessage
            => _owner.StatusMessage;

        void IGraphEditorRetainedEventPublisherHost.RaiseDocumentChanged(GraphEditorDocumentChangedEventArgs args)
            => _owner.DocumentChanged?.Invoke(_owner, args);

        void IGraphEditorRetainedEventPublisherHost.RaiseSelectionChanged(GraphEditorSelectionChangedEventArgs args)
            => _owner.SelectionChanged?.Invoke(_owner, args);

        void IGraphEditorRetainedEventPublisherHost.RaisePendingConnectionChanged(GraphEditorPendingConnectionChangedEventArgs args)
            => _owner.PendingConnectionChanged?.Invoke(_owner, args);
    }
}
