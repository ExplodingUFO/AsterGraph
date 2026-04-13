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
    public async Task FragmentCommands_CopySelectionAsync_WritesVersionedClipboardPayload()
    {
        var bridge = new TestClipboardBridge();
        var editor = CreateEditor(bridge);
        var commands = new GraphEditorViewModel.GraphEditorFragmentCommands(new FragmentCommandHostAdapter(editor));
        editor.SelectSingleNode(editor.Nodes[0]);

        await commands.CopySelectionAsync();

        Assert.NotNull(bridge.Text);
        Assert.Contains("\"SchemaVersion\": 1", bridge.Text);
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

    [Fact]
    public void FragmentCommands_ImportFragmentFrom_ReadsLegacyFragmentPayload()
    {
        var editor = CreateEditor(nodes: []);
        var commands = new GraphEditorViewModel.GraphEditorFragmentCommands(new FragmentCommandHostAdapter(editor));
        var path = Path.Combine(Path.GetTempPath(), $"astergraph-fragment-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, CreateLegacyClipboardJson());

        try
        {
            var imported = commands.ImportFragmentFrom(path);

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

    private static GraphEditorViewModel CreateEditor(TestClipboardBridge? bridge = null, IReadOnlyList<GraphNode>? nodes = null)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new TestNodeDefinitionProvider());

        var document = new GraphDocument(
            "Editor Compatibility",
            "Exercise clipboard and fragment compatibility.",
            nodes ?? [CreateNode()],
            []);

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
                    new PortTypeId("float")),
            ],
            "#6AD5C4",
            new NodeDefinitionId("tests.editor.sample"));

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

    private sealed class FragmentCommandHostAdapter : GraphEditorViewModel.IGraphEditorFragmentCommandHost
    {
        private readonly GraphEditorViewModel.IGraphEditorFragmentCommandHost _inner;

        public FragmentCommandHostAdapter(GraphEditorViewModel editor)
        {
            ArgumentNullException.ThrowIfNull(editor);
            _inner = editor;
        }

        public GraphEditorCommandPermissions CommandPermissions => _inner.CommandPermissions;

        public GraphEditorBehaviorOptions BehaviorOptions => _inner.BehaviorOptions;

        public IEnumerable<NodeViewModel> SelectedNodes => _inner.SelectedNodes;

        public string? SelectedNodeId => _inner.SelectedNodeId;

        public string? SelectedNodeTitle => _inner.SelectedNodeTitle;

        public IEnumerable<ConnectionViewModel> Connections => _inner.Connections;

        public string? SelectedFragmentTemplatePath => _inner.SelectedFragmentTemplatePath;

        public string? StatusMessage => _inner.StatusMessage;

        public IGraphTextClipboardBridge? TextClipboardBridge => _inner.TextClipboardBridge;

        public IGraphClipboardPayloadSerializer ClipboardPayloadSerializer => _inner.ClipboardPayloadSerializer;

        public IGraphFragmentWorkspaceService FragmentWorkspaceService => _inner.FragmentWorkspaceService;

        public IGraphFragmentLibraryService FragmentLibraryService => _inner.FragmentLibraryService;

        public void StoreSelectionClipboard(GraphSelectionFragment fragment)
            => _inner.StoreSelectionClipboard(fragment);

        public GraphSelectionFragment? PeekSelectionClipboard()
            => _inner.PeekSelectionClipboard();

        public GraphPoint GetNextPasteOrigin()
            => _inner.GetNextPasteOrigin();

        public string CreateNodeId(NodeDefinitionId? definitionId, string fallbackKey)
            => _inner.CreateNodeId(definitionId, fallbackKey);

        public string CreateConnectionId()
            => _inner.CreateConnectionId();

        public void ApplyNodePresentation(NodeViewModel node)
            => _inner.ApplyNodePresentation(node);

        public void AddNode(NodeViewModel node)
            => _inner.AddNode(node);

        public void AddConnection(ConnectionViewModel connection)
            => _inner.AddConnection(connection);

        public void SetSelection(IReadOnlyList<NodeViewModel> nodes, NodeViewModel? primaryNode)
            => _inner.SetSelection(nodes, primaryNode);

        public void RefreshFragmentTemplates()
            => _inner.RefreshFragmentTemplates();

        public void RaiseComputedPropertyChanges()
            => _inner.RaiseComputedPropertyChanges();

        public string StatusText(string key, string fallback, params object?[] arguments)
            => _inner.StatusText(key, fallback, arguments);

        public void SetStatus(string key, string fallback, params object?[] arguments)
            => _inner.SetStatus(key, fallback, arguments);

        public void MarkDirty(string status)
            => _inner.MarkDirty(status);

        public void PublishRuntimeDiagnostic(string code, string operation, string message, GraphEditorDiagnosticSeverity severity, Exception? exception = null)
            => _inner.PublishRuntimeDiagnostic(code, operation, message, severity, exception);

        public void RaiseFragmentExported(string path, GraphSelectionFragment fragment)
            => _inner.RaiseFragmentExported(path, fragment);

        public void RaiseFragmentImported(string path, GraphSelectionFragment fragment)
            => _inner.RaiseFragmentImported(path, fragment);
    }
}
