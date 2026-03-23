using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal static class GraphClipboardPayloadSerializer
{
    private const string ClipboardFormat = "astergraph.clipboard/v1";
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    public static string Serialize(GraphSelectionFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        return JsonSerializer.Serialize(
            new GraphClipboardPayload(
                ClipboardFormat,
                CurrentSchemaVersion,
                fragment.Origin,
                fragment.PrimaryNodeId,
                fragment.Nodes,
                fragment.Connections),
            JsonOptions);
    }

    public static bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
    {
        fragment = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(text);
            var root = document.RootElement;
            if (!root.TryGetProperty(nameof(GraphClipboardPayload.Format), out var formatElement)
                || !string.Equals(formatElement.GetString(), ClipboardFormat, StringComparison.Ordinal))
            {
                return false;
            }

            if (!root.TryGetProperty(nameof(GraphClipboardPayload.SchemaVersion), out var versionElement))
            {
                var legacyPayload = JsonSerializer.Deserialize<GraphClipboardPayloadLegacy>(text, JsonOptions);
                if (legacyPayload is null || legacyPayload.Nodes.Count == 0)
                {
                    return false;
                }

                fragment = new GraphSelectionFragment(
                    legacyPayload.Nodes,
                    legacyPayload.Connections,
                    legacyPayload.Origin,
                    legacyPayload.PrimaryNodeId);
                return true;
            }

            var schemaVersion = versionElement.GetInt32();
            if (schemaVersion != CurrentSchemaVersion)
            {
                return false;
            }

            // 剪贴板里可能是任意宿主内容，因此这里必须采用失败即拒绝的解析策略。
            var payload = JsonSerializer.Deserialize<GraphClipboardPayload>(text, JsonOptions);
            if (payload is null || payload.Nodes.Count == 0)
            {
                return false;
            }

            fragment = new GraphSelectionFragment(
                payload.Nodes,
                payload.Connections,
                payload.Origin,
                payload.PrimaryNodeId);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
