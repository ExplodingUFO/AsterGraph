using System.Text.Json;

namespace AsterGraph.Editor.Services;

/// <summary>
/// 管理剪贴板与片段 JSON 契约版本及兼容读取。
/// </summary>
internal static class GraphClipboardPayloadCompatibility
{
    public const int CurrentSchemaVersion = 1;

    public static GraphClipboardPayload CreatePayload(GraphSelectionFragment fragment, string format)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        return new GraphClipboardPayload(
            format,
            CurrentSchemaVersion,
            fragment.Origin,
            fragment.PrimaryNodeId,
            fragment.Nodes,
            fragment.Connections,
            fragment.Groups);
    }

    public static bool TryDeserialize(
        string? text,
        string format,
        JsonSerializerOptions options,
        out GraphSelectionFragment? fragment)
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
                || !string.Equals(formatElement.GetString(), format, StringComparison.Ordinal))
            {
                return false;
            }

            if (!root.TryGetProperty(nameof(GraphClipboardPayload.SchemaVersion), out var versionElement))
            {
                var legacyPayload = JsonSerializer.Deserialize<GraphClipboardPayloadLegacy>(text, options);
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

            var payload = JsonSerializer.Deserialize<GraphClipboardPayload>(text, options);
            if (payload is null || payload.Nodes.Count == 0)
            {
                return false;
            }

            fragment = new GraphSelectionFragment(
                payload.Nodes,
                payload.Connections,
                payload.Origin,
                payload.PrimaryNodeId,
                payload.Groups);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
