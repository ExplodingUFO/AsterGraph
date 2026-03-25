namespace AsterGraph.Editor.Diagnostics;

/// <summary>
/// 定义宿主可替换的图编辑器诊断发布器。
/// </summary>
public interface IGraphEditorDiagnosticsSink
{
    /// <summary>
    /// 发布一条诊断项。
    /// </summary>
    /// <param name="diagnostic">要发布的诊断项。</param>
    void Publish(GraphEditorDiagnostic diagnostic);
}
