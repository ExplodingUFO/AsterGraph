namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 定义宿主可见的图编辑器诊断与集成检查入口。
/// </summary>
public interface IGraphEditorDiagnostics
{
    /// <summary>
    /// 捕获当前编辑器状态的不可变检查快照。
    /// </summary>
    /// <returns>当前会话的检查快照。</returns>
    GraphEditorInspectionSnapshot CaptureInspectionSnapshot();

    /// <summary>
    /// 获取最近发布的诊断项。
    /// </summary>
    /// <param name="maxCount">返回的最大诊断条数。</param>
    /// <returns>最近诊断集合。</returns>
    IReadOnlyList<GraphEditorDiagnostic> GetRecentDiagnostics(int maxCount = 20);
}
