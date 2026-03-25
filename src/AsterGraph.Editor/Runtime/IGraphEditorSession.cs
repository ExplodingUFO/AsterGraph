namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义宿主可见的图编辑器运行时会话根接口。
/// </summary>
public interface IGraphEditorSession
{
    /// <summary>
    /// 获取命令与变更入口。
    /// </summary>
    IGraphEditorCommands Commands { get; }

    /// <summary>
    /// 获取只读查询入口。
    /// </summary>
    IGraphEditorQueries Queries { get; }

    /// <summary>
    /// 获取运行时事件入口。
    /// </summary>
    IGraphEditorEvents Events { get; }

    /// <summary>
    /// 开始一个轻量级的批量变更作用域。
    /// </summary>
    /// <param name="label">可选的批次标签。</param>
    /// <returns>批量变更作用域。</returns>
    IGraphEditorMutationScope BeginMutation(string? label = null);
}
