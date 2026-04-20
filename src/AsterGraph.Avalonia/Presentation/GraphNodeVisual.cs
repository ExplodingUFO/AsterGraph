using Avalonia.Controls;
using AsterGraph.Core.Models;

namespace AsterGraph.Avalonia.Presentation;

/// <summary>
/// 表示节点展示器返回给 <c>NodeCanvas</c> 的可视树结果。
/// </summary>
public sealed class GraphNodeVisual
{
    /// <summary>
    /// 初始化节点可视树结果。
    /// </summary>
    public GraphNodeVisual(
        Control root,
        IReadOnlyDictionary<string, Control> portAnchors,
        object? presenterState = null)
        : this(root, portAnchors, connectionTargetAnchors: null, presenterState)
    {
    }

    /// <summary>
    /// 初始化节点可视树结果，并可选携带额外的 typed connection-target 锚点。
    /// </summary>
    public GraphNodeVisual(
        Control root,
        IReadOnlyDictionary<string, Control> portAnchors,
        IReadOnlyDictionary<GraphConnectionTargetRef, Control>? connectionTargetAnchors,
        object? presenterState = null)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(portAnchors);

        Root = root;
        PortAnchors = portAnchors is Dictionary<string, Control> mutablePortAnchors
            ? mutablePortAnchors
            : new Dictionary<string, Control>(portAnchors, StringComparer.Ordinal);
        ConnectionTargetAnchors = connectionTargetAnchors is Dictionary<GraphConnectionTargetRef, Control> mutableTargetAnchors
            ? mutableTargetAnchors
            : connectionTargetAnchors is null
                ? new Dictionary<GraphConnectionTargetRef, Control>()
                : new Dictionary<GraphConnectionTargetRef, Control>(connectionTargetAnchors);
        PresenterState = presenterState;
    }

    /// <summary>
    /// 节点根控件。
    /// </summary>
    public Control Root { get; }

    /// <summary>
    /// 端口锚点控件映射，键为端口标识。
    /// </summary>
    public IReadOnlyDictionary<string, Control> PortAnchors { get; }

    /// <summary>
    /// Additional typed connection-target anchors such as parameter endpoints.
    /// </summary>
    public IReadOnlyDictionary<GraphConnectionTargetRef, Control> ConnectionTargetAnchors { get; }

    /// <summary>
    /// 展示器可选的内部状态对象。
    /// </summary>
    public object? PresenterState { get; }
}
