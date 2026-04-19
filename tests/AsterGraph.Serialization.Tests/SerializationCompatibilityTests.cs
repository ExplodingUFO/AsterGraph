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
        Assert.Equal(4, GraphDocumentCompatibility.CurrentSchemaVersion);
    }

    [Fact]
    public void GraphDocumentSerializer_WritesSchemaVersion()
    {
        var document = CreateDocument();

        var json = GraphDocumentSerializer.Serialize(document);

        Assert.Contains("\"SchemaVersion\": 4", json);
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
    public void GraphDocumentSerializer_ReadsSchemaVersion1Payload_WithDefaultSurfaceState()
    {
        const string json = """
        {
          "SchemaVersion": 1,
          "Title": "Legacy v1",
          "Description": "Old payload",
          "Nodes": [
            {
              "Id": "node-001",
              "Title": "Legacy Node",
              "Category": "Tests",
              "Subtitle": "Legacy",
              "Description": "Older payload without surface state.",
              "Position": {
                "X": 12,
                "Y": 24
              },
              "Size": {
                "Width": 240,
                "Height": 160
              },
              "Inputs": [],
              "Outputs": [],
              "AccentHex": "#6AD5C4",
              "DefinitionId": {
                "Value": "tests.node"
              },
              "ParameterValues": []
            }
          ],
          "Connections": []
        }
        """;

        var restored = GraphDocumentSerializer.Deserialize(json);
        var node = Assert.Single(restored.Nodes);

        Assert.NotNull(node.Surface);
        Assert.Equal(GraphNodeExpansionState.Collapsed, node.Surface!.ExpansionState);
        Assert.Null(node.Surface.GroupId);
        Assert.Empty(restored.Groups ?? []);
    }

    [Fact]
    public void GraphDocumentSerializer_ReadsSchemaVersion2Payload_AndPreservesLegacyGroupFrame()
    {
        const string json = """
        {
          "SchemaVersion": 2,
          "Title": "Legacy v2",
          "Description": "Old group bounds payload",
          "Nodes": [
            {
              "Id": "node-001",
              "Title": "Legacy Node",
              "Category": "Tests",
              "Subtitle": "Legacy",
              "Description": "Older payload with group bounds.",
              "Position": {
                "X": 100,
                "Y": 120
              },
              "Size": {
                "Width": 240,
                "Height": 160
              },
              "Inputs": [],
              "Outputs": [],
              "AccentHex": "#6AD5C4",
              "DefinitionId": {
                "Value": "tests.node"
              },
              "ParameterValues": [],
              "Surface": {
                "ExpansionState": "Collapsed",
                "GroupId": "group-001"
              }
            }
          ],
          "Connections": [],
          "Groups": [
            {
              "Id": "group-001",
              "Title": "Legacy Group",
              "Position": {
                "X": 76,
                "Y": 76
              },
              "Size": {
                "Width": 288,
                "Height": 232
              },
              "NodeIds": [
                "node-001"
              ],
              "IsCollapsed": false
            }
          ]
        }
        """;

        var restored = GraphDocumentSerializer.Deserialize(json);
        var group = Assert.Single(restored.Groups!);

        Assert.Equal(new GraphPoint(76, 76), group.Position);
        Assert.Equal(new GraphSize(288, 232), group.Size);
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
        var serializer = new GraphClipboardPayloadSerializer();

        var json = serializer.Serialize(fragment);

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
        var serializer = new GraphClipboardPayloadSerializer();

        var restored = serializer.TryDeserialize(legacyJson, out var deserialized);

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
        var serializer = new GraphClipboardPayloadSerializer();

        var restored = serializer.TryDeserialize(json, out var fragment);

        Assert.False(restored);
        Assert.Null(fragment);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Serialization Test",
            "Exercise versioned graph document payloads.",
            [CreateNode()],
            [CreateConnection()],
            []);

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
            new NodeDefinitionId("tests.node"),
            null,
            new GraphNodeSurfaceState(GraphNodeExpansionState.Expanded));

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
