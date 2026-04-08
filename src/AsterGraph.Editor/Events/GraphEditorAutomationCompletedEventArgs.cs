using AsterGraph.Editor.Automation;

namespace AsterGraph.Editor.Events;

/// <summary>
/// 图编辑器自动化完成事件参数。
/// </summary>
public sealed class GraphEditorAutomationCompletedEventArgs : EventArgs
{
    /// <summary>
    /// 初始化自动化完成事件参数。
    /// </summary>
    public GraphEditorAutomationCompletedEventArgs(GraphEditorAutomationExecutionSnapshot result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Result = result;
    }

    public GraphEditorAutomationExecutionSnapshot Result { get; }
}
