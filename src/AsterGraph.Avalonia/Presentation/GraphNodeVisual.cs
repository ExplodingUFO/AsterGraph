using Avalonia.Controls;

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
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(portAnchors);

        Root = root;
        PortAnchors = new Dictionary<string, Control>(portAnchors, StringComparer.Ordinal);
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
    /// 展示器可选的内部状态对象。
    /// </summary>
    public object? PresenterState { get; }
}
