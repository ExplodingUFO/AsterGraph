using Avalonia;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasLifecycleHost
{
    bool AttachPlatformSeams { get; }

    bool IsAttachedToVisualTree { get; set; }

    GraphEditorViewModel? ViewModel { get; }

    void ReplacePlatformSeams(GraphEditorViewModel? previous, GraphEditorViewModel? current);

    void ApplyPlatformSeams(GraphEditorViewModel? current);

    void ClearPlatformSeams(GraphEditorViewModel? current);

    void AttachViewModel(GraphEditorViewModel? previous, GraphEditorViewModel? current);

    void RebuildScene();
}

internal sealed class NodeCanvasLifecycleCoordinator
{
    private readonly INodeCanvasLifecycleHost _host;

    public NodeCanvasLifecycleCoordinator(INodeCanvasLifecycleHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void HandlePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);

        if (change.Property == NodeCanvas.ViewModelProperty)
        {
            var previous = change.GetOldValue<GraphEditorViewModel?>();
            var current = change.GetNewValue<GraphEditorViewModel?>();
            if (_host.AttachPlatformSeams && _host.IsAttachedToVisualTree)
            {
                _host.ReplacePlatformSeams(previous, current);
            }

            _host.AttachViewModel(previous, current);
        }
        else if (change.Property == NodeCanvas.NodeVisualPresenterProperty
            || change.Property == NodeCanvas.NodeBodyPresenterProperty)
        {
            _host.RebuildScene();
        }
    }

    public void HandleAttachedToVisualTree()
    {
        _host.IsAttachedToVisualTree = true;

        if (_host.AttachPlatformSeams)
        {
            _host.ApplyPlatformSeams(_host.ViewModel);
        }
    }

    public void HandleDetachedFromVisualTree()
    {
        if (_host.AttachPlatformSeams)
        {
            _host.ClearPlatformSeams(_host.ViewModel);
        }

        _host.IsAttachedToVisualTree = false;
    }
}
