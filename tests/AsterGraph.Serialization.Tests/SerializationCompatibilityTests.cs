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
        Assert.Equal(5, GraphDocumentCompatibility.CurrentSchemaVersion);
    }

    [Fact]
    public void GraphDocumentSerializer_WritesSchemaVersion()
    {
        var document = CreateDocument();

        var json = GraphDocumentSerializer.Serialize(document);

        Assert.Contains("\"SchemaVersion\": 5", json);
        Assert.Contains("\"RootGraphId\": \"graph-root\"", json);
        Assert.Contains("\"GraphScopes\"", json);
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
        Assert.Equal("graph-root", restored.RootGraphId);
        var graphScopes = Assert.IsAssignableFrom<IReadOnlyList<GraphScope>>(restored.GraphScopes);
        Assert.Single(graphScopes);
        Assert.Equal("graph-root", graphScopes[0].Id);
    }

    [Fact]
    public void GraphDocumentSerializer_WritesAndReadsScopedCompositePayload_WithEdgePresentation()
    {
        var document = CreateScopedDocument();

        var json = GraphDocumentSerializer.Serialize(document);
        var restored = GraphDocumentSerializer.Deserialize(json);

        Assert.Contains("\"RootGraphId\": \"graph-root\"", json);
        Assert.Contains("\"ChildGraphId\": \"graph-composite-001\"", json);
        Assert.Contains("\"NoteText\": \"Preview branch\"", json);
        Assert.Contains("\"Vertices\": [", json);

        Assert.Equal("graph-root", restored.RootGraphId);
        Assert.Equal(2, restored.GraphScopes!.Count);

        var rootScope = Assert.Single(restored.GraphScopes, scope => scope.Id == "graph-root");
        var rootCompositeNode = Assert.Single(rootScope.Nodes, node => node.Id == "node-composite-001");
        var composite = Assert.IsType<GraphCompositeNode>(rootCompositeNode.Composite);
        Assert.Equal("graph-composite-001", composite.ChildGraphId);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<GraphCompositeBoundaryPort>>(composite.Outputs));

        var childScope = Assert.Single(restored.GraphScopes, scope => scope.Id == "graph-composite-001");
        var childConnection = Assert.Single(childScope.Connections);
        Assert.NotNull(childConnection.Presentation);
        Assert.Equal("Preview branch", childConnection.Presentation!.NoteText);
        Assert.Equal(
            [new GraphPoint(360d, 120d), new GraphPoint(420d, 300d)],
            childConnection.Presentation.Route?.Vertices);
    }

    [Fact]
    public void GraphDocumentSerializer_WritesAndReadsParameterTargetKind()
    {
        var document = new GraphDocument(
            "Parameter Target Graph",
            "Parameter connection target roundtrip.",
            [CreateNode("node-parameter-source"), CreateNode("node-parameter-target")],
            [
                CreateConnection(
                    "connection-parameter-001",
                    "node-parameter-source",
                    "output-001",
                    "node-parameter-target",
                    "gain") with
                {
                    TargetKind = GraphConnectionTargetKind.Parameter,
                },
            ],
            []);

        var json = GraphDocumentSerializer.Serialize(document);
        var restored = GraphDocumentSerializer.Deserialize(json);
        var connection = Assert.Single(restored.Connections);

        Assert.Contains("\"TargetKind\": \"Parameter\"", json);
        Assert.Equal(GraphConnectionTargetKind.Parameter, connection.TargetKind);
        Assert.Equal("gain", connection.TargetPortId);
    }

    [Fact]
    public void GraphDocument_Constructor_CanonicalizesRootScopeAgainstTopLevelFields()
    {
        var document = new GraphDocument(
            "Scoped Graph",
            "Constructor canonicalization.",
            [CreateNode("node-root-top")],
            [],
            [],
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [CreateNode("node-root-conflict")],
                    []),
                new GraphScope(
                    "graph-composite-001",
                    [CreateNode("node-child-001")],
                    []),
            ]);

        var rootScope = Assert.Single(document.GraphScopes, scope => scope.Id == "graph-root");
        Assert.Equal("node-root-top", Assert.Single(rootScope.Nodes).Id);
    }

    [Fact]
    public void GraphDocument_WithExpression_UpdatesRootScopeSnapshot()
    {
        var updatedNode = CreateNode("node-root-updated");
        var document = CreateScopedDocument();

        var mutated = document with
        {
            Nodes = [updatedNode],
        };

        var rootScope = Assert.Single(mutated.GraphScopes, scope => scope.Id == "graph-root");
        Assert.Equal(updatedNode.Id, Assert.Single(rootScope.Nodes).Id);
    }

    [Fact]
    public void GraphDocument_WithExpression_DeepCopiesUpdatedRootCollections()
    {
        var externalNodes = new List<GraphNode> { CreateNode("node-root-001") };
        var document = CreateScopedDocument();

        var mutated = document with
        {
            Nodes = externalNodes,
        };

        externalNodes.Add(CreateNode("node-root-002"));

        Assert.Single(mutated.Nodes);
        var rootScope = Assert.Single(mutated.GraphScopes, scope => scope.Id == "graph-root");
        Assert.Single(rootScope.Nodes);
    }

    [Fact]
    public void GraphDocument_RetainsLegacyDeconstructContract_WithoutInlineParameterMetadata()
    {
        var document = new GraphDocument(
            "Legacy Graph",
            "Compatibility contract.",
            [
                new GraphNode(
                    "node-001",
                    "Node",
                    "Tests",
                    "Compat",
                    "Carries legacy inline parameter metadata.",
                    new GraphPoint(0, 0),
                    new GraphSize(240, 160),
                    [new GraphPort("input-001", "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#6AD5C4",
                    new NodeDefinitionId("tests.node"),
                    []),
            ],
            []);

        var (title, description, nodes, connections, groups) = document;

        Assert.Equal("Legacy Graph", title);
        Assert.Equal("Compatibility contract.", description);
        Assert.Empty(connections);
        Assert.Null(groups);
        Assert.Null(typeof(GraphPort).GetProperty("InlineParameterKey"));
        Assert.NotNull(typeof(GraphDocument).GetConstructor(
        [
            typeof(string),
            typeof(string),
            typeof(IReadOnlyList<GraphNode>),
            typeof(IReadOnlyList<GraphConnection>),
            typeof(IReadOnlyList<GraphNodeGroup>),
        ]));
    }

    [Fact]
    public void GraphDocument_WithExpression_KeepsGraphScopeIdsUnique_WhenRootGraphIdChanges()
    {
        var document = CreateScopedDocument();

        var mutated = document with
        {
            RootGraphId = "graph-composite-001",
        };

        Assert.Equal(
            mutated.GraphScopes.Select(scope => scope.Id).Distinct(StringComparer.Ordinal).Count(),
            mutated.GraphScopes.Count);
    }

    [Fact]
    public void GraphDocument_CreateScoped_DeepCopiesNestedScopeCollections()
    {
        var rootNodes = new List<GraphNode> { CreateNode("node-root-001") };
        var childNodes = new List<GraphNode> { CreateNode("node-child-001") };
        var scopes = new List<GraphScope>
        {
            new(
                "graph-root",
                rootNodes,
                []),
            new(
                "graph-composite-001",
                childNodes,
                []),
        };

        var document = GraphDocument.CreateScoped(
            "Scoped Graph",
            "Deep copy contract.",
            "graph-root",
            scopes);

        rootNodes.Add(CreateNode("node-root-002"));
        childNodes.Clear();

        var rootScope = Assert.Single(document.GraphScopes, scope => scope.Id == "graph-root");
        Assert.Single(rootScope.Nodes);

        var childScope = Assert.Single(document.GraphScopes, scope => scope.Id == "graph-composite-001");
        Assert.Single(childScope.Nodes);
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
    public void GraphDocumentSerializer_ReadsLegacyConnectionWithoutTargetKind_AsPortTarget()
    {
        const string json = """
        {
          "Title": "Legacy Graph",
          "Description": "Legacy connection payload",
          "Nodes": [
            {
              "Id": "node-source",
              "Title": "Source",
              "Category": "Tests",
              "Subtitle": "Legacy",
              "Description": "",
              "Position": { "X": 0, "Y": 0 },
              "Size": { "Width": 220, "Height": 140 },
              "Inputs": [],
              "Outputs": [
                {
                  "Id": "output-001",
                  "Label": "Output",
                  "Direction": "Output",
                  "DataType": "float",
                  "AccentHex": "#6AD5C4",
                  "TypeId": { "Value": "float" }
                }
              ],
              "AccentHex": "#6AD5C4",
              "DefinitionId": { "Value": "tests.node" },
              "ParameterValues": []
            },
            {
              "Id": "node-target",
              "Title": "Target",
              "Category": "Tests",
              "Subtitle": "Legacy",
              "Description": "",
              "Position": { "X": 240, "Y": 0 },
              "Size": { "Width": 220, "Height": 140 },
              "Inputs": [],
              "Outputs": [],
              "AccentHex": "#F3B36B",
              "DefinitionId": { "Value": "tests.node" },
              "ParameterValues": []
            }
          ],
          "Connections": [
            {
              "Id": "connection-001",
              "SourceNodeId": "node-source",
              "SourcePortId": "output-001",
              "TargetNodeId": "node-target",
              "TargetPortId": "input-001",
              "Label": "Output to Input",
              "AccentHex": "#6AD5C4"
            }
          ]
        }
        """;

        var restored = GraphDocumentSerializer.Deserialize(json);
        var connection = Assert.Single(restored.Connections);

        Assert.Equal(GraphConnectionTargetKind.Port, connection.TargetKind);
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

    private static GraphDocument CreateScopedDocument()
        => GraphDocument.CreateScoped(
            "Scoped Graph",
            "Exercise graph scopes and composite payloads.",
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [
                        CreateNode("node-root-001"),
                        CreateNode(
                            "node-composite-001",
                            composite: new GraphCompositeNode(
                                "graph-composite-001",
                                [],
                                [
                                    new GraphCompositeBoundaryPort(
                                        "boundary-output-001",
                                        "Composite Output",
                                        PortDirection.Output,
                                        "float",
                                        "#6AD5C4",
                                        "node-child-001",
                                        "output-001",
                                        new PortTypeId("float")),
                                ])),
                    ],
                    []),
                new GraphScope(
                    "graph-composite-001",
                    [
                        CreateNode("node-child-001"),
                        CreateNode("node-child-002"),
                    ],
                    [
                        CreateConnection(
                            "connection-child-001",
                            "node-child-001",
                            "output-001",
                            "node-child-002",
                            "input-001",
                            presentation: new GraphEdgePresentation(
                                "Preview branch",
                                new GraphConnectionRoute(
                                [
                                    new GraphPoint(360d, 120d),
                                    new GraphPoint(420d, 300d),
                                ]))),
                    ]),
            ]);

    private static GraphSelectionFragment CreateFragment()
        => new(
            [CreateNode()],
            [CreateConnection()],
            new GraphPoint(12, 24),
            "node-001");

    private static GraphNode CreateNode(
        string nodeId = "node-001",
        GraphCompositeNode? composite = null)
        => new(
            nodeId,
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
            new GraphNodeSurfaceState(GraphNodeExpansionState.Expanded),
            composite);

    private static GraphConnection CreateConnection(
        string connectionId = "connection-001",
        string sourceNodeId = "node-001",
        string sourcePortId = "output-001",
        string targetNodeId = "node-002",
        string targetPortId = "input-001",
        GraphEdgePresentation? presentation = null)
        => new(
            connectionId,
            sourceNodeId,
            sourcePortId,
            targetNodeId,
            targetPortId,
            "Output to Input",
            "#6AD5C4",
            new ConversionId("tests.float-pass-through"),
            presentation);
}
