using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using AsterGraph.Editor.ViewModels;
using AsterGraph.Editor.Events;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasViewModelObserverHost
{
    GraphEditorViewModel? ViewModel { get; }

    void UpdateViewportTransform();

    void RenderConnections();

    void UpdateSelectionState();

    void UpdateGroupVisuals();

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
    private string _lastGroupSignature = string.Empty;

    public NodeCanvasViewModelObserver(INodeCanvasViewModelObserverHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public void AttachViewModel(GraphEditorViewModel? previous, GraphEditorViewModel? current)
    {
        if (previous is not null)
        {
            previous.DocumentChanged -= HandleViewModelDocumentChanged;
            previous.PropertyChanged -= HandleViewModelPropertyChanged;
            previous.Nodes.CollectionChanged -= HandleNodesCollectionChanged;
            previous.Connections.CollectionChanged -= HandleConnectionsCollectionChanged;
            previous.SelectedNodeParameters.CollectionChanged -= HandleSelectedNodeParametersCollectionChanged;

            foreach (var node in previous.Nodes)
            {
                node.PropertyChanged -= HandleNodePropertyChanged;
            }

            foreach (var parameter in previous.SelectedNodeParameters)
            {
                parameter.PropertyChanged -= HandleSelectedNodeParameterPropertyChanged;
            }
        }

        if (current is not null)
        {
            current.DocumentChanged += HandleViewModelDocumentChanged;
            current.PropertyChanged += HandleViewModelPropertyChanged;
            current.Nodes.CollectionChanged += HandleNodesCollectionChanged;
            current.Connections.CollectionChanged += HandleConnectionsCollectionChanged;
            current.SelectedNodeParameters.CollectionChanged += HandleSelectedNodeParametersCollectionChanged;

            foreach (var node in current.Nodes)
            {
                node.PropertyChanged += HandleNodePropertyChanged;
            }

            foreach (var parameter in current.SelectedNodeParameters)
            {
                parameter.PropertyChanged += HandleSelectedNodeParameterPropertyChanged;
            }
        }

        _lastGroupSignature = CaptureGroupSignature(current);

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

    private void HandleViewModelDocumentChanged(object? sender, GraphEditorDocumentChangedEventArgs args)
    {
        var currentSignature = CaptureGroupSignature(_host.ViewModel);
        if (string.Equals(_lastGroupSignature, currentSignature, StringComparison.Ordinal))
        {
            return;
        }

        _lastGroupSignature = currentSignature;
        _host.RebuildScene();
    }

    private void HandleSelectedNodeParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (NodeParameterViewModel parameter in args.OldItems)
            {
                parameter.PropertyChanged -= HandleSelectedNodeParameterPropertyChanged;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (NodeParameterViewModel parameter in args.NewItems)
            {
                parameter.PropertyChanged += HandleSelectedNodeParameterPropertyChanged;
            }
        }

        _host.UpdateSelectionState();
    }

    private void HandleSelectedNodeParameterPropertyChanged(object? sender, PropertyChangedEventArgs args)
        => _host.UpdateSelectionState();

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
                _host.UpdateGroupVisuals();
                _host.RenderConnections();
                break;
            case nameof(NodeViewModel.Width):
            case nameof(NodeViewModel.Height):
            case nameof(NodeViewModel.Surface):
            case nameof(NodeViewModel.ActiveSurfaceTier):
                _host.UpdateNodeVisual(node);
                _host.UpdateGroupVisuals();
                _host.RenderConnections();
                break;
            case nameof(NodeViewModel.IsSelected):
                _host.UpdateNodeVisual(node);
                break;
            case nameof(NodeViewModel.Presentation):
                _host.UpdateNodeVisual(node);
                _host.UpdateGroupVisuals();
                _host.RenderConnections();
                break;
        }
    }

    private static string CaptureGroupSignature(GraphEditorViewModel? viewModel)
    {
        if (viewModel is null)
        {
            return string.Empty;
        }

        return string.Join(
            "|",
            viewModel.GetNodeGroupSnapshots()
                .OrderBy(group => group.Id, StringComparer.Ordinal)
                .Select(group =>
                    $"{group.Id}:{group.Title}:{group.Position.X.ToString("0.###", CultureInfo.InvariantCulture)}:{group.Position.Y.ToString("0.###", CultureInfo.InvariantCulture)}:{group.Size.Width.ToString("0.###", CultureInfo.InvariantCulture)}:{group.Size.Height.ToString("0.###", CultureInfo.InvariantCulture)}:{group.ExtraPadding.Left.ToString("0.###", CultureInfo.InvariantCulture)}:{group.ExtraPadding.Top.ToString("0.###", CultureInfo.InvariantCulture)}:{group.ExtraPadding.Right.ToString("0.###", CultureInfo.InvariantCulture)}:{group.ExtraPadding.Bottom.ToString("0.###", CultureInfo.InvariantCulture)}:{group.IsCollapsed}:{string.Join(",", group.NodeIds.OrderBy(id => id, StringComparer.Ordinal))}"));
    }
}
