using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 提供节点展示器创建或更新可视树时所需的编辑器状态与交互回调。
/// </summary>
/// <remarks>
/// This context remains a compatibility-shaped presenter seam built on top of the retained
/// facade. Phase 16 narrows how Avalonia attaches platform seams around it, but does not yet
/// replace the presenter model with a pure runtime/session contract.
/// </remarks>
public sealed class GraphNodeVisualContext
{
    /// <summary>
    /// 初始化节点展示上下文。
    /// </summary>
    public GraphNodeVisualContext(
        GraphEditorViewModel editor,
        NodeViewModel node,
        GraphEditorStyleOptions styleOptions,
        Action focusCanvas,
        Action<NodeViewModel, PointerPressedEventArgs> beginNodeDrag,
        Func<NodeViewModel, double, bool, bool> trySetNodeWidth,
        Func<NodeViewModel, GraphNodeExpansionState, bool> trySetNodeExpansionState,
        Func<NodeViewModel, PortViewModel, bool> hasIncomingConnection,
        Func<NodeViewModel, PortViewModel, NodeParameterViewModel?> resolveInlineParameter,
        Action<NodeViewModel, PortViewModel> activatePort,
        Func<Control, NodeViewModel, ContextRequestedEventArgs, bool> openNodeContextMenu,
        Func<Control, NodeViewModel, PortViewModel, ContextRequestedEventArgs, bool> openPortContextMenu)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(styleOptions);
        ArgumentNullException.ThrowIfNull(focusCanvas);
        ArgumentNullException.ThrowIfNull(beginNodeDrag);
        ArgumentNullException.ThrowIfNull(trySetNodeWidth);
        ArgumentNullException.ThrowIfNull(trySetNodeExpansionState);
        ArgumentNullException.ThrowIfNull(hasIncomingConnection);
        ArgumentNullException.ThrowIfNull(resolveInlineParameter);
        ArgumentNullException.ThrowIfNull(activatePort);
        ArgumentNullException.ThrowIfNull(openNodeContextMenu);
        ArgumentNullException.ThrowIfNull(openPortContextMenu);

        Editor = editor;
        Node = node;
        StyleOptions = styleOptions;
        FocusCanvas = focusCanvas;
        BeginNodeDrag = beginNodeDrag;
        TrySetNodeWidth = trySetNodeWidth;
        TrySetNodeExpansionState = trySetNodeExpansionState;
        HasIncomingConnection = hasIncomingConnection;
        ResolveInlineParameter = resolveInlineParameter;
        ActivatePort = activatePort;
        OpenNodeContextMenu = openNodeContextMenu;
        OpenPortContextMenu = openPortContextMenu;
    }

    /// <summary>
    /// 当前编辑器实例。
    /// </summary>
    /// <remarks>
    /// Compatibility-oriented retained facade reference. Prefer session/query/runtime data for
    /// new non-presenter integration work when possible.
    /// </remarks>
    public GraphEditorViewModel Editor { get; }

    /// <summary>
    /// 当前节点视图模型。
    /// </summary>
    public NodeViewModel Node { get; }

    /// <summary>
    /// 当前编辑器样式选项。
    /// </summary>
    public GraphEditorStyleOptions StyleOptions { get; }

    /// <summary>
    /// 请求将焦点切回画布。
    /// </summary>
    public Action FocusCanvas { get; }

    /// <summary>
    /// 请求开始节点拖动交互。
    /// </summary>
    public Action<NodeViewModel, PointerPressedEventArgs> BeginNodeDrag { get; }

    /// <summary>
    /// Requests a persisted node-width mutation.
    /// </summary>
    public Func<NodeViewModel, double, bool, bool> TrySetNodeWidth { get; }

    /// <summary>
    /// Requests a persisted node expansion-state mutation.
    /// </summary>
    public Func<NodeViewModel, GraphNodeExpansionState, bool> TrySetNodeExpansionState { get; }

    /// <summary>
    /// Determines whether the given input port currently has an incoming connection.
    /// </summary>
    public Func<NodeViewModel, PortViewModel, bool> HasIncomingConnection { get; }

    /// <summary>
    /// Resolves the shared node-parameter view model bound to one inline-editable input port.
    /// </summary>
    public Func<NodeViewModel, PortViewModel, NodeParameterViewModel?> ResolveInlineParameter { get; }

    /// <summary>
    /// 请求激活某个端口。
    /// </summary>
    public Action<NodeViewModel, PortViewModel> ActivatePort { get; }

    /// <summary>
    /// 请求打开节点上下文菜单。
    /// </summary>
    public Func<Control, NodeViewModel, ContextRequestedEventArgs, bool> OpenNodeContextMenu { get; }

    /// <summary>
    /// 请求打开端口上下文菜单。
    /// </summary>
    public Func<Control, NodeViewModel, PortViewModel, ContextRequestedEventArgs, bool> OpenPortContextMenu { get; }
}
