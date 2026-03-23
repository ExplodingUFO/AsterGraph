namespace AsterGraph.Editor.Events;

/// <summary>
/// 图片段导入导出事件参数。
/// </summary>
public sealed class GraphEditorFragmentEventArgs : EventArgs
{
    /// <summary>
    /// 初始化片段事件参数。
    /// </summary>
    public GraphEditorFragmentEventArgs(string path, int nodeCount, int connectionCount)
    {
        Path = path;
        NodeCount = nodeCount;
        ConnectionCount = connectionCount;
    }

    /// <summary>
    /// 片段文件路径。
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 片段中的节点数量。
    /// </summary>
    public int NodeCount { get; }

    /// <summary>
    /// 片段中的连线数量。
    /// </summary>
    public int ConnectionCount { get; }
}
