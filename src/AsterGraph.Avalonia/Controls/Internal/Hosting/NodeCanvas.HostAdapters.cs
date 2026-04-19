using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class NodeCanvas
{
    private sealed class NodeCanvasLifecycleHost : INodeCanvasLifecycleHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasLifecycleHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public bool AttachPlatformSeams => _owner.AttachPlatformSeams;

        public bool IsAttachedToVisualTree
        {
            get => _owner._isAttachedToVisualTree;
            set => _owner._isAttachedToVisualTree = value;
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public void ReplacePlatformSeams(GraphEditorViewModel? previous, GraphEditorViewModel? current)
            => GraphEditorPlatformSeamBinder.Replace(previous, current, _owner);

        public void ApplyPlatformSeams(GraphEditorViewModel? current)
            => GraphEditorPlatformSeamBinder.Apply(current, _owner);

        public void ClearPlatformSeams(GraphEditorViewModel? current)
            => GraphEditorPlatformSeamBinder.Clear(current);

        public void AttachViewModel(GraphEditorViewModel? previous, GraphEditorViewModel? current)
            => _owner.AttachViewModel(previous, current);

        public void RebuildScene()
            => _owner.RebuildScene();
    }

    private sealed class NodeCanvasContextMenuHost : INodeCanvasContextMenuHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasContextMenuHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public bool EnableDefaultContextMenu => _owner.EnableDefaultContextMenu;

        public IGraphContextMenuPresenter? ContextMenuPresenter => _owner.ContextMenuPresenter;

        public IGraphContextMenuPresenter StockContextMenuPresenter => _owner._stockContextMenuPresenter;

        public void FocusCanvas()
            => _owner.Focus();

        public GraphPoint ResolveWorldPosition(ContextRequestedEventArgs args, Control relativeTo)
            => _owner.ResolveWorldPosition(args, relativeTo);

        public NodeCanvasContextMenuSnapshot CreateContextMenuSnapshot()
            => _owner.CreateContextMenuSnapshot();
    }

    private sealed class NodeCanvasSceneHostAdapter : INodeCanvasSceneHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasSceneHostAdapter(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public Grid? SceneRoot => _owner._sceneRoot;

        public Canvas? GroupLayer => _owner._groupLayer;

        public Canvas? ConnectionLayer => _owner._connectionLayer;

        public Canvas? NodeLayer => _owner._nodeLayer;

        public Control CoordinateRoot => _owner;

        public GridBackground? BackgroundGrid => _owner._backgroundGrid;

        public Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals => _owner._nodeVisuals;

        public Dictionary<string, NodeCanvasRenderedGroupVisual> GroupVisuals => _owner._groupVisuals;

        public IGraphNodeVisualPresenter? NodeVisualPresenter => _owner.NodeVisualPresenter;

        public IGraphNodeVisualPresenter StockNodeVisualPresenter => _owner._stockNodeVisualPresenter;

        public NodeCanvasInteractionSession InteractionSession => _owner._interactionSession;

        public NodeCanvasContextMenuCoordinator ContextMenuCoordinator => _owner._contextMenuCoordinator;

        public void FocusCanvas()
            => _owner.Focus();

        public void BeginNodeDrag(NodeViewModel node, PointerPressedEventArgs args)
            => _owner.BeginNodeDrag(node, args);

        public void ActivatePort(NodeViewModel node, PortViewModel port)
            => _owner.ActivatePortFromVisual(node, port);
    }

    private sealed class NodeCanvasViewModelObserverHost : INodeCanvasViewModelObserverHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasViewModelObserverHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public void UpdateViewportTransform()
            => _owner.UpdateViewportTransform();

        public void RenderConnections()
            => _owner.RenderConnections();

        public void UpdateSelectionState()
            => _owner.UpdateSelectionState();

        public void ApplySelectionAdornerStyle()
            => _owner.ApplySelectionAdornerStyle();

        public void ApplyGuideAdornerStyle()
            => _owner.ApplyGuideAdornerStyle();

        public void HideGuideAdorners()
            => _owner.HideGuideAdorners();

        public void RebuildScene()
            => _owner.RebuildScene();

        public void UpdateNodePosition(NodeViewModel node)
            => _owner.UpdateNodePosition(node);

        public void UpdateNodeVisual(NodeViewModel node)
            => _owner.UpdateNodeVisual(node);
    }

    private sealed class NodeCanvasOverlayHost : INodeCanvasOverlayHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasOverlayHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorStyleOptions StyleOptions => _owner.ViewModel?.StyleOptions ?? GraphEditorStyleOptions.Default;

        public GraphEditorBehaviorOptions BehaviorOptions => _owner.ViewModel?.BehaviorOptions ?? GraphEditorBehaviorOptions.Default;

        public double Zoom => _owner.ViewModel?.Zoom ?? 1;

        public IReadOnlyList<NodeViewModel> Nodes => _owner.ViewModel?.Nodes ?? [];

        public IReadOnlyList<NodeViewModel> SelectedNodes => _owner.ViewModel?.SelectedNodes ?? [];

        public NodeViewModel? SelectedNode => _owner.ViewModel?.SelectedNode;

        public Size Bounds => _owner.Bounds.Size;

        public Border? SelectionAdorner => _owner._selectionAdorner;

        public Border? VerticalGuideAdorner => _owner._verticalGuideAdorner;

        public Border? HorizontalGuideAdorner => _owner._horizontalGuideAdorner;

        public GraphPoint WorldToScreen(double x, double y)
            => _owner.WorldToScreen(x, y);

        public GraphPoint ScreenToWorld(GraphPoint point)
            => _owner.ViewModel?.ScreenToWorld(point) ?? point;

        public IReadOnlyList<NodeViewModel> GetNodesInRectangle(GraphPoint firstCorner, GraphPoint secondCorner)
            => _owner.ViewModel?.GetNodesInRectangle(firstCorner, secondCorner) ?? [];

        public NodeCanvasInteractionSession InteractionSession => _owner._interactionSession;

        public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode, string? status = null)
            => _owner.ViewModel?.SetSelection(nodes, primaryNode, status);
    }

    private sealed class NodeCanvasNodeDragHost : INodeCanvasNodeDragHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasNodeDragHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public NodeCanvasInteractionSession InteractionSession => _owner._interactionSession;

        public void FocusCanvas()
            => _owner.Focus();

        public void HideSelectionAdorner()
            => _owner.HideSelectionAdorner();

        public void HideGuideAdorners()
            => _owner.HideGuideAdorners();

        public void BringNodeVisualToFront(NodeViewModel node)
        {
            if (_owner._nodeLayer is not null && _owner._nodeVisuals.TryGetValue(node, out var visual))
            {
                _owner._nodeLayer.Children.Remove(visual.Root);
                _owner._nodeLayer.Children.Add(visual.Root);
            }
        }

        public NodeCanvasDragSession CreateDragSession(IReadOnlyList<NodeViewModel> nodes)
            => _owner.CreateDragSession(nodes);
    }

    private sealed class NodeCanvasPointerInteractionHost : INodeCanvasPointerInteractionHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasPointerInteractionHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public bool EnableAltLeftDragPanning => _owner.EnableAltLeftDragPanning;

        public NodeCanvasInteractionSession InteractionSession => _owner._interactionSession;

        public void FocusCanvas()
            => _owner.Focus();

        public void HideSelectionAdorner()
            => _owner.HideSelectionAdorner();

        public void HideGuideAdorners()
            => _owner.HideGuideAdorners();

        public void RenderConnections()
            => _owner.RenderConnections();

        public void UpdateMarqueeSelection(Point currentScreenPosition, bool finalize)
            => _owner.UpdateMarqueeSelection(currentScreenPosition, finalize);

        public GraphPoint ApplyDragAssist(NodeCanvasDragSession dragSession, double deltaX, double deltaY)
            => _owner.ApplyDragAssist(dragSession, deltaX, deltaY);
    }

    private sealed class NodeCanvasWheelInteractionHost : INodeCanvasWheelInteractionHost
    {
        private readonly NodeCanvas _owner;

        public NodeCanvasWheelInteractionHost(NodeCanvas owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public GraphEditorViewModel? ViewModel => _owner.ViewModel;

        public bool EnableDefaultWheelViewportGestures => _owner.EnableDefaultWheelViewportGestures;

        public NodeCanvasInteractionSession InteractionSession => _owner._interactionSession;
    }
}
