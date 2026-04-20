using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Core.Models;

namespace AsterGraph.Core.Serialization;

/// <summary>
/// 图文档序列化器，负责稳定的文件契约和版本兼容读取。
/// </summary>
public static class GraphDocumentSerializer
{
    // Keep the serializer stable and explicit so saved demo/editor documents survive host-side refactors.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    /// <summary>
    /// 将图文档序列化为当前版本的 JSON 文本。
    /// </summary>
    /// <param name="document">要序列化的图文档。</param>
    /// <returns>序列化后的 JSON 文本。</returns>
    public static string Serialize(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return JsonSerializer.Serialize(GraphDocumentCompatibility.CreatePayload(document), JsonOptions);
    }

    /// <summary>
    /// 从 JSON 文本反序列化图文档，同时兼容旧的无版本文件格式。
    /// </summary>
    /// <param name="json">图文档 JSON 文本。</param>
    /// <returns>反序列化后的图文档。</returns>
    public static GraphDocument Deserialize(string json)
        => GraphDocumentCompatibility.Deserialize(json, JsonOptions);

    /// <summary>
    /// 将图文档保存到指定文件路径。
    /// </summary>
    /// <param name="document">要保存的图文档。</param>
    /// <param name="path">目标文件路径。</param>
    public static void Save(GraphDocument document, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(document));
    }

    /// <summary>
    /// 从指定文件路径加载图文档。
    /// </summary>
    /// <param name="path">源文件路径。</param>
    /// <returns>反序列化后的图文档。</returns>
    public static GraphDocument Load(string path)
        => Deserialize(File.ReadAllText(path));

    internal sealed record GraphDocumentFilePayload(
        int SchemaVersion,
        string Title,
        string Description,
        string RootGraphId,
        IReadOnlyList<GraphScope> GraphScopes)
    {
        public GraphDocument ToDocument()
            => GraphDocument.CreateScoped(Title, Description, RootGraphId, GraphScopes);
    }
}
