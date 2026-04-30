using System.Text.Json;
using System.Text.Json.Serialization;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class EditorClipboardAndFragmentCompatibilityTests
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
    public async Task CopySelectionAsync_WritesVersionedClipboardPayload()
    {
        var bridge = new TestClipboardBridge();
        var editor = CreateEditor(bridge);
        editor.SelectSingleNode(editor.Nodes[0]);

        await editor.CopySelectionAsync();

        Assert.NotNull(bridge.Text);
        Assert.Contains("\"SchemaVersion\": 1", bridge.Text);
    }

    [Fact]
    public void ClipboardPayloadSerializer_RoundTripsSourceBackedSelectionShape()
    {
        var serializer = new GraphClipboardPayloadSerializer();
        var fragment = CreateSourceBackedFragment();

        var json = serializer.Serialize(fragment);

        Assert.True(serializer.TryDeserialize(json, out var roundTripped));
        Assert.NotNull(roundTripped);
        var sourceNode = Assert.Single(roundTripped.Nodes, candidate => candidate.Id == "source-node");
        var targetNode = Assert.Single(roundTripped.Nodes, candidate => candidate.Id == "target-node");
        var input = Assert.Single(targetNode.Inputs);
        var output = Assert.Single(sourceNode.Outputs);
        Assert.Equal("Inputs", input.GroupName);
        Assert.Equal("Outputs", output.GroupName);
        var connection = Assert.Single(roundTripped.Connections);
        Assert.Equal("source-node", connection.SourceNodeId);
        Assert.Equal("target-node", connection.TargetNodeId);
        var group = Assert.Single(roundTripped.Groups);
        Assert.Equal("group-001", group.Id);
        Assert.Equal(["source-node", "target-node"], group.NodeIds);
    }

    [Fact]
    public async Task CopySelectionAsync_WritesNodesPortsGroupsAndConnections()
    {
        var bridge = new TestClipboardBridge();
        var editor = CreateEditor(
            bridge,
            nodes: [CreateNode("source-node"), CreateInputNode("target-node")],
            connections:
            [
                new GraphConnection("connection-001", "source-node", "result", "target-node", "input", "float", "#6AD5C4"),
            ],
            groups:
            [
                new GraphNodeGroup("group-001", "Pipeline", new GraphPoint(90, 120), new GraphSize(620, 260), ["source-node", "target-node"]),
            ]);
        editor.SetSelection([editor.Nodes[0], editor.Nodes[1]], editor.Nodes[0]);

        await editor.CopySelectionAsync();

        Assert.NotNull(bridge.Text);
        using var document = JsonDocument.Parse(bridge.Text);
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("Nodes").GetArrayLength());
        Assert.Equal(1, root.GetProperty("Connections").GetArrayLength());
        Assert.Equal(1, root.GetProperty("Groups").GetArrayLength());
        Assert.Equal(1, root.GetProperty("Nodes")[0].GetProperty("Outputs").GetArrayLength());
        Assert.Equal(1, root.GetProperty("Nodes")[1].GetProperty("Inputs").GetArrayLength());
    }

    [Fact]
    public async Task PasteSelectionAsync_ReadsSourceBackedGroups()
    {
        var bridge = new TestClipboardBridge
        {
            Text = new GraphClipboardPayloadSerializer().Serialize(CreateSourceBackedFragment()),
        };
        var editor = CreateEditor(bridge, nodes: []);

        await editor.PasteSelectionAsync();

        Assert.Equal(2, editor.Nodes.Count);
        var group = Assert.Single(editor.GetNodeGroups());
        Assert.Equal(2, group.NodeIds.Count);
        Assert.All(group.NodeIds, nodeId => Assert.Contains(editor.Nodes, node => node.Id == nodeId));
    }

    [Fact]
    public async Task PasteSelectionAsync_ReadsLegacyClipboardPayload()
    {
        var bridge = new TestClipboardBridge
        {
            Text = CreateLegacyClipboardJson(),
        };
        var editor = CreateEditor(bridge, nodes: []);

        await editor.PasteSelectionAsync();

        Assert.Single(editor.Nodes);
    }

    [Fact]
    public void ExportSelectionFragmentTo_WritesVersionedFragmentPayload()
    {
        var editor = CreateEditor();
        editor.SelectSingleNode(editor.Nodes[0]);
        var path = Path.Combine(Path.GetTempPath(), $"astergraph-fragment-{Guid.NewGuid():N}.json");

        try
        {
            var exported = editor.ExportSelectionFragmentTo(path);

            Assert.True(exported);
            Assert.Contains("\"SchemaVersion\": 1", File.ReadAllText(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void ImportFragmentFrom_ReadsLegacyFragmentPayload()
    {
        var editor = CreateEditor(nodes: []);
        var path = Path.Combine(Path.GetTempPath(), $"astergraph-fragment-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, CreateLegacyClipboardJson());

        try
        {
            var imported = editor.ImportFragmentFrom(path);

            Assert.True(imported);
            Assert.Single(editor.Nodes);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static GraphEditorViewModel CreateEditor(
        TestClipboardBridge? bridge = null,
        IReadOnlyList<GraphNode>? nodes = null,
        IReadOnlyList<GraphConnection>? connections = null,
        IReadOnlyList<GraphNodeGroup>? groups = null)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new TestNodeDefinitionProvider());

        var document = new GraphDocument(
            "Editor Compatibility",
            "Exercise clipboard and fragment compatibility.",
            nodes ?? [CreateNode()],
            connections ?? [],
            groups);

        var editor = new GraphEditorViewModel(
            document,
            catalog,
            new DefaultPortCompatibilityService());
        editor.SetTextClipboardBridge(bridge);
        return editor;
    }

    private static string CreateLegacyClipboardJson()
    {
        var payload = new
        {
            Format = "astergraph.clipboard/v1",
            Origin = new GraphPoint(10, 20),
            PrimaryNodeId = "legacy-node-001",
            Nodes = new[] { CreateNode("legacy-node-001") },
            Connections = Array.Empty<GraphConnection>(),
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static GraphSelectionFragment CreateSourceBackedFragment()
        => new(
            [CreateNode("source-node"), CreateInputNode("target-node")],
            [new GraphConnection("connection-001", "source-node", "result", "target-node", "input", "float", "#6AD5C4")],
            new GraphPoint(120, 160),
            "source-node",
            [new GraphNodeGroup("group-001", "Pipeline", new GraphPoint(90, 120), new GraphSize(620, 260), ["source-node", "target-node"])]);

    private static GraphNode CreateNode(string id = "editor-node-001")
        => new(
            id,
            "Editor Sample",
            "Tests",
            "Produces a float output",
            "Used by editor-level compatibility tests.",
            new GraphPoint(120, 160),
            new GraphSize(240, 160),
            [],
            [
                new GraphPort(
                    "result",
                    "Result",
                    PortDirection.Output,
                    "float",
                    "#6AD5C4",
                    new PortTypeId("float"),
                    "Outputs"),
            ],
            "#6AD5C4",
            new NodeDefinitionId("tests.editor.sample"));

    private static GraphNode CreateInputNode(string id)
        => new(
            id,
            "Editor Input",
            "Tests",
            "Consumes a float input",
            "Used by editor-level compatibility tests.",
            new GraphPoint(420, 160),
            new GraphSize(240, 160),
            [
                new GraphPort(
                    "input",
                    "Input",
                    PortDirection.Input,
                    "float",
                    "#F3B36B",
                    new PortTypeId("float"),
                    "Inputs",
                    1,
                    2),
            ],
            [
                new GraphPort(
                    "result",
                    "Result",
                    PortDirection.Output,
                    "float",
                    "#6AD5C4",
                    new PortTypeId("float"),
                    "Outputs"),
            ],
            "#F3B36B",
            new NodeDefinitionId("tests.editor.sample"),
            Surface: new GraphNodeSurfaceState(GroupId: "group-001"));

    private sealed class TestClipboardBridge : IGraphTextClipboardBridge
    {
        public string? Text { get; set; }

        public Task<string?> ReadTextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Text);

        public Task WriteTextAsync(string text, CancellationToken cancellationToken = default)
        {
            Text = text;
            return Task.CompletedTask;
        }
    }

    private sealed class TestNodeDefinitionProvider : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            =>
            [
                new NodeDefinition(
                    new NodeDefinitionId("tests.editor.sample"),
                    "Editor Sample",
                    "Tests",
                    "Produces a float output",
                    [],
                    [
                        new PortDefinition(
                            "result",
                            "Result",
                            new PortTypeId("float"),
                            "#6AD5C4"),
                    ],
                    description: "Minimal test node for editor compatibility tests.",
                    accentHex: "#6AD5C4",
                    defaultWidth: 240,
                    defaultHeight: 160),
            ];
    }
}
