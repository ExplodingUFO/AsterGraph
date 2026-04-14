using System.ComponentModel;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Editor.Services;

internal interface IGraphEditorNodePositionDirtyTrackerHost
{
    bool IsDirtyTrackingSuspended { get; }

    bool IsDirty { get; set; }

    void RaiseComputedPropertyChanges();
}

internal sealed class GraphEditorNodePositionDirtyTracker
{
    private readonly IGraphEditorNodePositionDirtyTrackerHost _host;

    public GraphEditorNodePositionDirtyTracker(IGraphEditorNodePositionDirtyTrackerHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void HandleNodePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (_host.IsDirtyTrackingSuspended || sender is not NodeViewModel)
        {
            return;
        }

        if (args.PropertyName is nameof(NodeViewModel.X) or nameof(NodeViewModel.Y))
        {
            if (!_host.IsDirty)
            {
                _host.IsDirty = true;
                _host.RaiseComputedPropertyChanges();
            }
        }
    }
}
