using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Core.Models;

namespace AsterGraph.Editor.Services;

/// <summary>
/// Serializes graph selection fragments to and from the editor clipboard payload format.
/// </summary>
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

    /// <summary>
    /// Serializes a graph selection fragment into the shipped clipboard payload format.
    /// </summary>
    /// <param name="fragment">Fragment to serialize.</param>
    /// <returns>Serialized clipboard text.</returns>
    public string Serialize(GraphSelectionFragment fragment)
    {
        ArgumentNullException.ThrowIfNull(fragment);

        return JsonSerializer.Serialize(
            GraphClipboardPayloadCompatibility.CreatePayload(fragment, ClipboardFormat),
            JsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize clipboard text into a graph selection fragment.
    /// </summary>
    /// <param name="text">Clipboard text.</param>
    /// <param name="fragment">Deserialized fragment when successful.</param>
    /// <returns><see langword="true"/> when deserialization succeeds.</returns>
    public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        => GraphClipboardPayloadCompatibility.TryDeserialize(text, ClipboardFormat, JsonOptions, out fragment);
}
