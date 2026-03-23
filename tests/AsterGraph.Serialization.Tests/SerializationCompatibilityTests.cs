using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Core.Serialization;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Serialization.Tests;

public sealed class SerializationCompatibilityTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    [Fact]
    public void GraphDocumentCompatibility_ExposesCurrentSchemaVersion()
    {
        Assert.Equal(1, GraphDocumentCompatibility.CurrentSchemaVersion);
    }

    [Fact]
    public void GraphDocumentSerializer_WritesSchemaVersion()
    {
        var document = CreateDocument();

        var json = GraphDocumentSerializer.Serialize(document);

        Assert.Contains("\"SchemaVersion\": 1", json);
    }

    [Fact]
    public void GraphDocumentSerializer_ReadsLegacyDocumentShape()
    {
        var document = CreateDocument();
        var legacyJson = JsonSerializer.Serialize(document, JsonOptions);

        var restored = GraphDocumentSerializer.Deserialize(legacyJson);

        Assert.Equal(document.Title, restored.Title);
        Assert.Equal(document.Nodes.Count, restored.Nodes.Count);
        Assert.Equal(document.Connections.Count, restored.Connections.Count);
    }

    [Fact]
    public void GraphDocumentSerializer_RejectsUnknownSchemaVersion()
    {
        const string json = """
        {
          "SchemaVersion": 99,
          "Title": "Future",
          "Description": "Unsupported",
          "Nodes": [],
          "Connections": []
        }
        """;

        var exception = Assert.Throws<InvalidOperationException>(() => GraphDocumentSerializer.Deserialize(json));
        Assert.Contains("Unsupported graph document schema version", exception.Message);
    }

    [Fact]
    public void GraphClipboardPayloadCompatibility_ExposesCurrentSchemaVersion()
    {
        Assert.Equal(1, GraphClipboardPayloadCompatibility.CurrentSchemaVersion);
    }

    [Fact]
    public void GraphClipboardPayloadSerializer_WritesSchemaVersion()
    {
        var fragment = CreateFragment();

        var json = GraphClipboardPayloadSerializer.Serialize(fragment);

        Assert.Contains("\"SchemaVersion\": 1", json);
    }

    [Fact]
    public void GraphClipboardPayloadSerializer_ReadsLegacyPayloadShape()
    {
        var fragment = CreateFragment();
        var legacyPayload = new
        {
            Format = "astergraph.clipboard/v1",
            Origin = fragment.Origin,
            PrimaryNodeId = fragment.PrimaryNodeId,
            Nodes = fragment.Nodes,
            Connections = fragment.Connections,
        };

        var legacyJson = JsonSerializer.Serialize(legacyPayload, JsonOptions);

        var restored = GraphClipboardPayloadSerializer.TryDeserialize(legacyJson, out var deserialized);

        Assert.True(restored);
        Assert.NotNull(deserialized);
        Assert.Equal(fragment.Nodes.Count, deserialized!.Nodes.Count);
        Assert.Equal(fragment.Connections.Count, deserialized.Connections.Count);
    }

    [Fact]
    public void GraphClipboardPayloadSerializer_RejectsUnknownSchemaVersion()
    {
        var payload = new
        {
            Format = "astergraph.clipboard/v1",
            SchemaVersion = 99,
            Origin = new GraphPoint(0, 0),
            PrimaryNodeId = "node-001",
            Nodes = new[] { CreateNode() },
            Connections = Array.Empty<GraphConnection>(),
        };

        var json = JsonSerializer.Serialize(payload, JsonOptions);

        var restored = GraphClipboardPayloadSerializer.TryDeserialize(json, out var fragment);

        Assert.False(restored);
        Assert.Null(fragment);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Serialization Test",
            "Exercise versioned graph document payloads.",
            [CreateNode()],
            [CreateConnection()]);

    private static GraphSelectionFragment CreateFragment()
        => new(
            [CreateNode()],
            [CreateConnection()],
            new GraphPoint(12, 24),
            "node-001");

    private static GraphNode CreateNode()
        => new(
            "node-001",
            "Test Node",
            "Tests",
            "Subtitle",
            "Description",
            new GraphPoint(12, 24),
            new GraphSize(240, 160),
            [],
            [
                new GraphPort(
                    "output-001",
                    "Output",
                    PortDirection.Output,
                    "float",
                    "#6AD5C4",
                    new PortTypeId("float")),
            ],
            "#6AD5C4",
            new NodeDefinitionId("tests.node"));

    private static GraphConnection CreateConnection()
        => new(
            "connection-001",
            "node-001",
            "output-001",
            "node-002",
            "input-001",
            "Output to Input",
            "#6AD5C4",
            new ConversionId("tests.float-pass-through"));
}
