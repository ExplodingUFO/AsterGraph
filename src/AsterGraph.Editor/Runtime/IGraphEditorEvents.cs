using AsterGraph.Editor.Events;

namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义宿主可订阅的图编辑器运行时事件入口。
/// </summary>
public interface IGraphEditorEvents
{
    /// <summary>
    /// 图文档变化事件。
    /// </summary>
    event EventHandler<GraphEditorDocumentChangedEventArgs>? DocumentChanged;

    /// <summary>
    /// 选择变化事件。
    /// </summary>
    event EventHandler<GraphEditorSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// 视口变化事件。
    /// </summary>
    event EventHandler<GraphEditorViewportChangedEventArgs>? ViewportChanged;

    /// <summary>
    /// 片段导出事件。
    /// </summary>
    event EventHandler<GraphEditorFragmentEventArgs>? FragmentExported;

    /// <summary>
    /// 片段导入事件。
    /// </summary>
    event EventHandler<GraphEditorFragmentEventArgs>? FragmentImported;

    /// <summary>
    /// 命令执行事件。
    /// </summary>
    event EventHandler<GraphEditorCommandExecutedEventArgs>? CommandExecuted;

    /// <summary>
    /// 待完成连线状态变化事件。
    /// </summary>
    event EventHandler<GraphEditorPendingConnectionChangedEventArgs>? PendingConnectionChanged
    {
        add => throw new NotSupportedException();
        remove => throw new NotSupportedException();
    }

    /// <summary>
    /// 可恢复失败事件。
    /// </summary>
    event EventHandler<GraphEditorRecoverableFailureEventArgs>? RecoverableFailure;
}
