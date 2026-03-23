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
            GraphClipboardPayloadCompatibility.CreatePayload(fragment, ClipboardFormat),
            JsonOptions);
    }

    public static bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        => GraphClipboardPayloadCompatibility.TryDeserialize(text, ClipboardFormat, JsonOptions, out fragment);
}
