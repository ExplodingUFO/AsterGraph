namespace AsterGraph.Editor.Automation;

/// <summary>
/// 定义宿主可见的自动化执行入口。
/// </summary>
public interface IGraphEditorAutomationRunner
{
    /// <summary>
    /// 执行一个自动化运行请求。
    /// </summary>
    /// <param name="request">自动化运行请求。</param>
    /// <returns>自动化执行结果快照。</returns>
    GraphEditorAutomationExecutionSnapshot Execute(GraphEditorAutomationRunRequest request);
}
