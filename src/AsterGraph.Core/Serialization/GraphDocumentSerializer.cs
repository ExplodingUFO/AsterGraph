using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Core.Models;

namespace AsterGraph.Core.Serialization;

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

    public static string Serialize(GraphDocument document)
        => JsonSerializer.Serialize(document, JsonOptions);

    public static GraphDocument Deserialize(string json)
        => JsonSerializer.Deserialize<GraphDocument>(json, JsonOptions)
           ?? throw new InvalidOperationException("Failed to deserialize graph document.");

    public static void Save(GraphDocument document, string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, Serialize(document));
    }

    public static GraphDocument Load(string path)
        => Deserialize(File.ReadAllText(path));
}
