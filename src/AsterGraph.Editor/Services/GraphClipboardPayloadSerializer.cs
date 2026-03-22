using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

internal static class GraphClipboardPayloadSerializer
{
    private const string ClipboardFormat = "astergraph.clipboard/v1";
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
            // Clipboard text can contain arbitrary host content, so parsing must fail closed.
            var payload = JsonSerializer.Deserialize<GraphClipboardPayload>(text, JsonOptions);
            if (payload is null
                || !string.Equals(payload.Format, ClipboardFormat, StringComparison.Ordinal)
                || payload.Nodes.Count == 0)
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
