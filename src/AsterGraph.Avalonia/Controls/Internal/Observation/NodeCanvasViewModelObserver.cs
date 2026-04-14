using System.Collections.Specialized;
using System.ComponentModel;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasViewModelObserverHost
{
    GraphEditorViewModel? ViewModel { get; }

    void UpdateViewportTransform();

    void RenderConnections();

    void UpdateSelectionState();

    void ApplySelectionAdornerStyle();

    void ApplyGuideAdornerStyle();

    void HideGuideAdorners();

    void RebuildScene();

    void UpdateNodePosition(NodeViewModel node);

    void UpdateNodeVisual(NodeViewModel node);
}

internal sealed class NodeCanvasViewModelObserver
{
    private readonly INodeCanvasViewModelObserverHost _host;

    public NodeCanvasViewModelObserver(INodeCanvasViewModelObserverHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void AttachViewModel(GraphEditorViewModel? previous, GraphEditorViewModel? current)
    {
        if (previous is not null)
        {
            previous.PropertyChanged -= HandleViewModelPropertyChanged;
            previous.Nodes.CollectionChanged -= HandleNodesCollectionChanged;
            previous.Connections.CollectionChanged -= HandleConnectionsCollectionChanged;

            foreach (var node in previous.Nodes)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
            }
        }

        if (current is not null)
        {
            current.PropertyChanged += HandleViewModelPropertyChanged;
            current.Nodes.CollectionChanged += HandleNodesCollectionChanged;
            current.Connections.CollectionChanged += HandleConnectionsCollectionChanged;

            foreach (var node in current.Nodes)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
            }
        }

        _host.ApplySelectionAdornerStyle();
        _host.ApplyGuideAdornerStyle();
        _host.RebuildScene();
    }

    private void HandleViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(GraphEditorViewModel.Zoom):
            case nameof(GraphEditorViewModel.PanX):
            case nameof(GraphEditorViewModel.PanY):
                _host.UpdateViewportTransform();
                if (_host.ViewModel?.HasPendingConnection == true)
                {
                    _host.RenderConnections();
                }
                break;
            case nameof(GraphEditorViewModel.SelectedNode):
                _host.UpdateSelectionState();
                _host.RenderConnections();
                break;
            case nameof(GraphEditorViewModel.StyleOptions):
                _host.ApplySelectionAdornerStyle();
                _host.ApplyGuideAdornerStyle();
                break;
            case nameof(GraphEditorViewModel.BehaviorOptions):
                if (_host.ViewModel?.BehaviorOptions.DragAssist.EnableAlignmentGuides != true)
                {
                    _host.HideGuideAdorners();
                }
                break;
            case nameof(GraphEditorViewModel.PendingSourceNode):
            case nameof(GraphEditorViewModel.PendingSourcePort):
                _host.RenderConnections();
                break;
        }
    }

    private void HandleNodesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (NodeViewModel node in args.OldItems)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (NodeViewModel node in args.NewItems)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
            }
        }

        _host.RebuildScene();
    }

    private void HandleConnectionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        => _host.RenderConnections();

    private void HandleNodePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is not NodeViewModel node)
        {
            return;
        }

        switch (args.PropertyName)
        {
            case nameof(NodeViewModel.X):
            case nameof(NodeViewModel.Y):
                _host.UpdateNodePosition(node);
                _host.RenderConnections();
                break;
            case nameof(NodeViewModel.Height):
                _host.UpdateNodeVisual(node);
                _host.RenderConnections();
                break;
            case nameof(NodeViewModel.IsSelected):
            case nameof(NodeViewModel.Presentation):
                _host.UpdateNodeVisual(node);
                break;
        }
    }
}
