namespace AsterGraph.Editor.Services;

/// <summary>
/// 定义片段剪贴板负载的宿主可替换序列化契约。
/// </summary>
public interface IGraphClipboardPayloadSerializer
{
    /// <summary>
    /// 将片段序列化为文本。
    /// </summary>
    /// <param name="fragment">要序列化的片段。</param>
    /// <returns>序列化后的文本。</returns>
    string Serialize(GraphSelectionFragment fragment);

    /// <summary>
    /// 尝试从文本中反序列化片段。
    /// </summary>
    /// <param name="text">候选文本。</param>
    /// <param name="fragment">解析成功时返回片段。</param>
    /// <returns>解析成功时返回 <see langword="true"/>。</returns>
    bool TryDeserialize(string? text, out GraphSelectionFragment? fragment);
}
