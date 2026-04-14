using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Avalonia.Controls.Internal;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls;

public partial class NodeCanvas
{
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

        public Canvas? ConnectionLayer => _owner._connectionLayer;

        public Canvas? NodeLayer => _owner._nodeLayer;

        public Control CoordinateRoot => _owner;

        public GridBackground? BackgroundGrid => _owner._backgroundGrid;

        public Dictionary<NodeViewModel, NodeCanvasRenderedNodeVisual> NodeVisuals => _owner._nodeVisuals;

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
}
