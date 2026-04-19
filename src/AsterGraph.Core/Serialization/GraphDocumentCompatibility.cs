using System.Text.Json;
using AsterGraph.Core.Models;

namespace AsterGraph.Core.Serialization;

/// <summary>
/// 管理图文档 JSON 契约版本与兼容读取。
/// </summary>
internal static class GraphDocumentCompatibility
{
    public const int CurrentSchemaVersion = 2;

    public static GraphDocumentSerializer.GraphDocumentFilePayload CreatePayload(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new GraphDocumentSerializer.GraphDocumentFilePayload(
            CurrentSchemaVersion,
            document.Title,
            document.Description,
            document.Nodes,
            document.Connections,
            document.Groups);
    }

    public static GraphDocument Deserialize(string json, JsonSerializerOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        ArgumentNullException.ThrowIfNull(options);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty(nameof(GraphDocumentSerializer.GraphDocumentFilePayload.SchemaVersion), out var versionElement))
        {
            var legacy = JsonSerializer.Deserialize<GraphDocument>(json, options);
            return NormalizeDocument(legacy ?? throw new InvalidOperationException("Failed to deserialize legacy graph document."));
        }

        var schemaVersion = versionElement.GetInt32();
        if (schemaVersion is not 1 and not CurrentSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported graph document schema version '{schemaVersion}'. Current version is '{CurrentSchemaVersion}'.");
        }

        var payload = JsonSerializer.Deserialize<GraphDocumentSerializer.GraphDocumentFilePayload>(json, options)
            ?? throw new InvalidOperationException("Failed to deserialize versioned graph document.");
        return NormalizeDocument(payload.ToDocument());
    }

    private static GraphDocument NormalizeDocument(GraphDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document with
        {
            Nodes = document.Nodes
                .Select(node => node with { Surface = node.Surface ?? GraphNodeSurfaceState.Default })
                .ToList(),
            Groups = document.Groups?.ToList() ?? [],
        };
    }
}
