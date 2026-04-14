using Avalonia;
using Avalonia.Input;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasNodeDragHost
{
    GraphEditorViewModel? ViewModel { get; }

    NodeCanvasInteractionSession InteractionSession { get; }

    void FocusCanvas();

    void HideSelectionAdorner();

    void HideGuideAdorners();

    void BringNodeVisualToFront(NodeViewModel node);

    NodeCanvasDragSession CreateDragSession(IReadOnlyList<NodeViewModel> nodes);
}

internal readonly record struct NodeCanvasNodeDragStartResult(bool Handled, bool CapturePointer);

internal sealed class NodeCanvasNodeDragCoordinator
{
    private readonly INodeCanvasNodeDragHost _host;

    public NodeCanvasNodeDragCoordinator(INodeCanvasNodeDragHost host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }

    public NodeCanvasNodeDragStartResult BeginNodeDrag(
        NodeViewModel node,
        Point dragStart,
        bool isLeftButtonPressed,
        KeyModifiers modifiers)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (_host.ViewModel is null || !isLeftButtonPressed)
        {
            return default;
        }

        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            _host.FocusCanvas();
            _host.ViewModel.ToggleNodeSelection(node);
            return new NodeCanvasNodeDragStartResult(Handled: true, CapturePointer: false);
        }

        if (modifiers.HasFlag(KeyModifiers.Shift))
        {
            _host.FocusCanvas();
            _host.ViewModel.AddNodeToSelection(node);
            return new NodeCanvasNodeDragStartResult(Handled: true, CapturePointer: false);
        }

        _host.FocusCanvas();
        if (node.IsSelected && _host.ViewModel.HasMultipleSelection)
        {
            _host.ViewModel.SetSelection(_host.ViewModel.SelectedNodes.ToList(), node);
        }
        else
        {
            _host.ViewModel.SelectSingleNode(node);
        }

        _host.BringNodeVisualToFront(node);
        _host.HideSelectionAdorner();
        _host.HideGuideAdorners();

        var dragNodes = node.IsSelected && _host.ViewModel.HasMultipleSelection
            ? _host.ViewModel.SelectedNodes.ToList()
            : [node];
        _host.InteractionSession.BeginNodeDrag(node, dragStart, _host.CreateDragSession(dragNodes));
        _host.ViewModel.BeginHistoryInteraction();
        return new NodeCanvasNodeDragStartResult(Handled: true, CapturePointer: true);
    }
}
