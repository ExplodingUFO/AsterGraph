using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

public sealed class GraphClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
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

    public string Serialize(GraphSelectionFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        return JsonSerializer.Serialize(
            GraphClipboardPayloadCompatibility.CreatePayload(fragment, ClipboardFormat),
            JsonOptions);
    }

    public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        => GraphClipboardPayloadCompatibility.TryDeserialize(text, ClipboardFormat, JsonOptions, out fragment);
}
