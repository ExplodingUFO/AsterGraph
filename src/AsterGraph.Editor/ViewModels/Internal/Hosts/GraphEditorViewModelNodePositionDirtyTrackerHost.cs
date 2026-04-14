using AsterGraph.Editor.Services;

namespace AsterGraph.Editor.ViewModels;

public sealed partial class GraphEditorViewModel
{
    private sealed class GraphEditorViewModelNodePositionDirtyTrackerHost : IGraphEditorNodePositionDirtyTrackerHost
    {
        private readonly GraphEditorViewModel _owner;

        public GraphEditorViewModelNodePositionDirtyTrackerHost(GraphEditorViewModel owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        bool IGraphEditorNodePositionDirtyTrackerHost.IsDirtyTrackingSuspended
            => _owner._suspendDirtyTracking;

        bool IGraphEditorNodePositionDirtyTrackerHost.IsDirty
        {
            get => _owner.IsDirty;
            set => _owner.IsDirty = value;
        }
    }
}
