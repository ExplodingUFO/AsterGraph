namespace AsterGraph.Editor.Runtime;

/// <summary>
/// 定义图编辑器运行时的轻量级批量变更作用域。
/// </summary>
public interface IGraphEditorMutationScope : IDisposable
{
    /// <summary>
    /// 获取批量变更标签。
    /// </summary>
    string? Label { get; }
}
