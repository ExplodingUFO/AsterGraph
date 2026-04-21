using Avalonia.Controls;
using Avalonia.Input;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Avalonia.Presentation;
using AsterGraph.Core.Models;
using AsterGraph.Editor.ViewModels;

namespace AsterGraph.Avalonia.Presentation;

public enum GraphNodeResizeHandleKind
{
    Right = 0,
    Bottom = 1,
    BottomRight = 2,
}

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
        INodeParameterEditorRegistry? nodeParameterEditorRegistry,
        IGraphNodeBodyPresenter? nodeBodyPresenter,
        Action focusCanvas,
        Action<NodeViewModel, PointerPressedEventArgs> beginNodeDrag,
        Action<NodeViewModel, GraphNodeResizeHandleKind, PointerPressedEventArgs> beginNodeResize,
        Action beginHistoryInteraction,
        Action<string> completeHistoryInteraction,
        Func<NodeViewModel, GraphSize, bool, bool> trySetNodeSize,
        Func<NodeViewModel, double, bool, bool> trySetNodeWidth,
        Func<NodeViewModel, GraphNodeExpansionState, bool> trySetNodeExpansionState,
        Func<NodeViewModel, PortViewModel, bool> hasIncomingConnection,
        Action<NodeViewModel, PortViewModel> activatePort,
        Action<NodeViewModel, GraphConnectionTargetRef> activateConnectionTarget,
        Func<Control, NodeViewModel, ContextRequestedEventArgs, bool> openNodeContextMenu,
        Func<Control, NodeViewModel, PortViewModel, ContextRequestedEventArgs, bool> openPortContextMenu,
        GraphSize? surfacePreviewSize = null)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(styleOptions);
        ArgumentNullException.ThrowIfNull(focusCanvas);
        ArgumentNullException.ThrowIfNull(beginNodeDrag);
        ArgumentNullException.ThrowIfNull(beginNodeResize);
        ArgumentNullException.ThrowIfNull(beginHistoryInteraction);
        ArgumentNullException.ThrowIfNull(completeHistoryInteraction);
        ArgumentNullException.ThrowIfNull(trySetNodeSize);
        ArgumentNullException.ThrowIfNull(trySetNodeWidth);
        ArgumentNullException.ThrowIfNull(trySetNodeExpansionState);
        ArgumentNullException.ThrowIfNull(hasIncomingConnection);
        ArgumentNullException.ThrowIfNull(activatePort);
        ArgumentNullException.ThrowIfNull(activateConnectionTarget);
        ArgumentNullException.ThrowIfNull(openNodeContextMenu);
        ArgumentNullException.ThrowIfNull(openPortContextMenu);

        Editor = editor;
        Node = node;
        StyleOptions = styleOptions;
        NodeParameterEditorRegistry = nodeParameterEditorRegistry;
        NodeBodyPresenter = nodeBodyPresenter;
        InteractionFocus = editor.InteractionFocus;
        FocusCanvas = focusCanvas;
        BeginNodeDrag = beginNodeDrag;
        BeginNodeResize = beginNodeResize;
        BeginHistoryInteraction = beginHistoryInteraction;
        CompleteHistoryInteraction = completeHistoryInteraction;
        TrySetNodeSize = trySetNodeSize;
        TrySetNodeWidth = trySetNodeWidth;
        TrySetNodeExpansionState = trySetNodeExpansionState;
        HasIncomingConnection = hasIncomingConnection;
        ActivatePort = activatePort;
        ActivateConnectionTarget = activateConnectionTarget;
        OpenNodeContextMenu = openNodeContextMenu;
        OpenPortContextMenu = openPortContextMenu;
        SurfacePreviewSize = surfacePreviewSize;
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
    /// Optional replaceable registry used by node-side parameter editor surfaces.
    /// </summary>
    public INodeParameterEditorRegistry? NodeParameterEditorRegistry { get; }

    /// <summary>
    /// Optional narrow body presenter used by the stock shell composition only.
    /// </summary>
    public IGraphNodeBodyPresenter? NodeBodyPresenter { get; }

    /// <summary>
    /// Current host-facing interaction focus snapshot.
    /// </summary>
    public GraphEditorInteractionFocusState InteractionFocus { get; }

    /// <summary>
    /// 请求将焦点切回画布。
    /// </summary>
    public Action FocusCanvas { get; }

    /// <summary>
    /// 请求开始节点拖动交互。
    /// </summary>
    public Action<NodeViewModel, PointerPressedEventArgs> BeginNodeDrag { get; }

    /// <summary>
    /// Requests a canvas-owned node resize session using the given handle kind.
    /// </summary>
    public Action<NodeViewModel, GraphNodeResizeHandleKind, PointerPressedEventArgs> BeginNodeResize { get; }

    /// <summary>
    /// Begins one grouped history interaction boundary.
    /// </summary>
    public Action BeginHistoryInteraction { get; }

    /// <summary>
    /// Completes the active grouped history interaction boundary.
    /// </summary>
    public Action<string> CompleteHistoryInteraction { get; }

    /// <summary>
    /// Requests a persisted node-size mutation.
    /// </summary>
    public Func<NodeViewModel, GraphSize, bool, bool> TrySetNodeSize { get; }

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
    /// 请求激活某个端口。
    /// </summary>
    public Action<NodeViewModel, PortViewModel> ActivatePort { get; }

    /// <summary>
    /// Requests activation of a typed connection target such as a parameter endpoint.
    /// </summary>
    public Action<NodeViewModel, GraphConnectionTargetRef> ActivateConnectionTarget { get; }

    /// <summary>
    /// Optional transient host-owned preview size used during interaction-time resize.
    /// </summary>
    public GraphSize? SurfacePreviewSize { get; }

    /// <summary>
    /// 请求打开节点上下文菜单。
    /// </summary>
    public Func<Control, NodeViewModel, ContextRequestedEventArgs, bool> OpenNodeContextMenu { get; }

    /// <summary>
    /// 请求打开端口上下文菜单。
    /// </summary>
    public Func<Control, NodeViewModel, PortViewModel, ContextRequestedEventArgs, bool> OpenPortContextMenu { get; }
}
