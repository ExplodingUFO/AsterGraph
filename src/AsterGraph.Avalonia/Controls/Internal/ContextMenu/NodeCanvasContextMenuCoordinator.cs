using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Controls.Internal;

internal interface INodeCanvasContextMenuHost
{
    GraphEditorViewModel? ViewModel { get; }

    bool EnableDefaultContextMenu { get; }

    IGraphContextMenuPresenter? ContextMenuPresenter { get; }

    IGraphContextMenuPresenter StockContextMenuPresenter { get; }

    void FocusCanvas();

    GraphPoint ResolveWorldPosition(ContextRequestedEventArgs args, Control relativeTo);

    NodeCanvasContextMenuSnapshot CreateContextMenuSnapshot();
}

internal sealed class NodeCanvasContextMenuCoordinator
{
    private readonly INodeCanvasContextMenuHost _host;
    private readonly Control _canvas;

    public NodeCanvasContextMenuCoordinator(INodeCanvasContextMenuHost host, Control canvas)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
    }

    public bool OpenNodeContextMenu(Control target, NodeViewModel node, ContextRequestedEventArgs args)
    {
        var viewModel = _host.ViewModel;
        if (viewModel is null)
        {
            return false;
        }

        _host.FocusCanvas();
        var useSelectionTools = viewModel.HasMultipleSelection && node.IsSelected;
        if (useSelectionTools)
        {
            viewModel.SetSelection(viewModel.SelectedNodes.ToList(), node);
        }
        else
        {
            viewModel.SelectSingleNode(node);
        }

        return OpenContextMenu(
            target,
            NodeCanvasContextMenuContextFactory.CreateNodeContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, _canvas),
                node.Id,
                useSelectionTools,
                hostContext: viewModel.HostContext));
    }

    public bool OpenPortContextMenu(Control target, NodeViewModel node, PortViewModel port, ContextRequestedEventArgs args)
    {
        var viewModel = _host.ViewModel;
        if (viewModel is null)
        {
            return false;
        }

        _host.FocusCanvas();
        viewModel.SelectNode(node);
        return OpenContextMenu(
            target,
            NodeCanvasContextMenuContextFactory.CreatePortContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, _canvas),
                node.Id,
                port.Id,
                hostContext: viewModel.HostContext));
    }

    public bool HandleCanvasContextRequested(Control target, ContextRequestedEventArgs args)
    {
        var viewModel = _host.ViewModel;
        if (viewModel is null || args.Handled || !_host.EnableDefaultContextMenu)
        {
            return false;
        }

        _host.FocusCanvas();
        return OpenContextMenu(
            target,
            NodeCanvasContextMenuContextFactory.CreateCanvasContext(
                CreateContextMenuSnapshot(),
                ResolveWorldPosition(args, _canvas),
                useSelectionTools: viewModel.HasMultipleSelection,
                hostContext: viewModel.HostContext));
    }

    public GraphPoint ResolveWorldPosition(ContextRequestedEventArgs args, Control relativeTo)
        => _host.ResolveWorldPosition(args, relativeTo);

    public NodeCanvasContextMenuSnapshot CreateContextMenuSnapshot()
        => _host.CreateContextMenuSnapshot();

    public bool OpenContextMenu(Control target, ContextMenuContext context)
    {
        var viewModel = _host.ViewModel;
        if (viewModel is null || !_host.EnableDefaultContextMenu)
        {
            return false;
        }

        var descriptors = viewModel.Session.Queries.BuildContextMenuDescriptors(context);
        if (descriptors.Count == 0)
        {
            return false;
        }

        (_host.ContextMenuPresenter ?? _host.StockContextMenuPresenter).Open(
            target,
            descriptors,
            viewModel.Session.Commands,
            viewModel.StyleOptions.ContextMenu);
        return true;
    }
}
