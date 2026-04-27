using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorExportContractsTests
{
    [Fact]
    public void GraphSceneSvgExportService_ExportsSceneSnapshotToSvgFile()
    {
        var definitionId = new NodeDefinitionId("tests.export.svg-file");
        var exportDirectory = CreateTempDirectory();
        var exportPath = Path.Combine(exportDirectory, "graph-scene.svg");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var service = new GraphSceneSvgExportService(exportPath);

        var writtenPath = service.Export(session.Queries.GetSceneSnapshot());

        Assert.Equal(exportPath, writtenPath);
        Assert.True(File.Exists(exportPath));

        var svg = File.ReadAllText(exportPath);
        Assert.Contains("<svg", svg, StringComparison.Ordinal);
        Assert.Contains("Export Source", svg, StringComparison.Ordinal);
        Assert.Contains("Export Target", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphSceneSvgExportService_ExportsLargeConnectedScene()
    {
        var definitionId = new NodeDefinitionId("tests.export.svg-large-connected");
        var exportDirectory = CreateTempDirectory();
        var exportPath = Path.Combine(exportDirectory, "large-graph.svg");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            Document = CreateLargeExportDocument(definitionId, 1_000),
        });
        var service = new GraphSceneSvgExportService(exportPath);

        var writtenPath = service.Export(session.Queries.GetSceneSnapshot());

        Assert.Equal(exportPath, writtenPath);
        var svg = File.ReadAllText(exportPath);
        Assert.Contains("Large Node 0000", svg, StringComparison.Ordinal);
        Assert.Contains("Large Node 0999", svg, StringComparison.Ordinal);
        Assert.Contains("""<g id="connections">""", svg, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeSession_ExportSceneAsSvg_UsesHostSuppliedExportServiceAndDescriptors()
    {
        var definitionId = new NodeDefinitionId("tests.export.session-contract");
        var exportService = new RecordingSceneSvgExportService("svg://tests.export/default.svg");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            SceneSvgExportService = exportService,
        });

        var exported = session.Commands.TryExportSceneAsSvg("svg://tests.export/custom.svg");
        var featureDescriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var commandDescriptors = session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.True(exported);
        Assert.Equal("svg://tests.export/custom.svg", exportService.LastPath);
        Assert.NotNull(exportService.LastScene);
        Assert.Equal("Export Contract Graph", exportService.LastScene!.Document.Title);
        Assert.Equal(2, exportService.LastScene.Document.Nodes.Count);
        Assert.True(featureDescriptors["service.scene-svg-export"].IsAvailable);
        Assert.True(featureDescriptors["capability.export.scene-svg"].IsAvailable);
        Assert.True(commandDescriptors["export.scene-svg"].IsEnabled);
    }

    [Theory]
    [InlineData(GraphEditorSceneImageExportFormat.Png, ".png")]
    [InlineData(GraphEditorSceneImageExportFormat.Jpeg, ".jpg")]
    public void GraphSceneImageExportService_ExportsSceneSnapshotToImageFile(
        GraphEditorSceneImageExportFormat format,
        string expectedExtension)
    {
        var definitionId = new NodeDefinitionId($"tests.export.image-file.{format.ToString().ToLowerInvariant()}");
        var exportDirectory = CreateTempDirectory();
        var exportPath = Path.Combine(exportDirectory, $"graph-scene{expectedExtension}");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var service = new GraphSceneImageExportService(exportDirectory);

        var writtenPath = service.Export(
            session.Queries.GetSceneSnapshot(),
            format,
            exportPath,
            new GraphEditorSceneImageExportOptions
            {
                Scale = 1.25d,
                Quality = 88,
                BackgroundHex = "#102030",
            });

        Assert.Equal(exportPath, writtenPath);
        Assert.True(File.Exists(exportPath));

        var bytes = File.ReadAllBytes(exportPath);
        Assert.True(bytes.Length > 32);
        Assert.True(
            format switch
            {
                GraphEditorSceneImageExportFormat.Png => bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47,
                GraphEditorSceneImageExportFormat.Jpeg => bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
                _ => false,
            });
    }

    [Fact]
    public void RuntimeSession_ExportSceneAsImage_UsesHostSuppliedExportServiceAndDescriptors()
    {
        var definitionId = new NodeDefinitionId("tests.export.image-session-contract");
        var exportService = new RecordingSceneImageExportService("image://tests.export/default.png");
        var exportOptions = new GraphEditorSceneImageExportOptions
        {
            Scale = 1.5d,
            Quality = 83,
            BackgroundHex = "#203040",
        };
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            SceneImageExportService = exportService,
        });

        var exported = session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Jpeg,
            "image://tests.export/custom.jpg",
            exportOptions);
        var featureDescriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var commandDescriptors = session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.True(exported);
        Assert.Equal(GraphEditorSceneImageExportFormat.Jpeg, exportService.LastFormat);
        Assert.Equal("image://tests.export/custom.jpg", exportService.LastPath);
        Assert.Same(exportOptions, exportService.LastOptions);
        Assert.NotNull(exportService.LastScene);
        Assert.Equal("Export Contract Graph", exportService.LastScene!.Document.Title);
        Assert.True(featureDescriptors["service.scene-image-export"].IsAvailable);
        Assert.True(featureDescriptors["capability.export.scene-image"].IsAvailable);
        Assert.True(featureDescriptors["capability.export.scene-png"].IsAvailable);
        Assert.True(featureDescriptors["capability.export.scene-jpeg"].IsAvailable);
        Assert.True(commandDescriptors["export.scene-image"].IsEnabled);
    }

    [Fact]
    public void RuntimeSession_ExportSceneAsImage_SelectedNodeScopePassesBoundedSceneToService()
    {
        var definitionId = new NodeDefinitionId("tests.export.image-selected-scope");
        var exportService = new RecordingSceneImageExportService("image://tests.export/selected.png");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            SceneImageExportService = exportService,
        });
        session.Commands.SetSelection(["source-node"], "source-node", updateStatus: false);

        var exported = session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Png,
            "image://tests.export/selected.png",
            new GraphEditorSceneImageExportOptions
            {
                Scope = GraphEditorSceneImageExportScope.SelectedNodes,
            });

        Assert.True(exported);
        Assert.NotNull(exportService.LastScene);
        var node = Assert.Single(exportService.LastScene!.Document.Nodes);
        Assert.Equal("source-node", node.Id);
        Assert.Empty(exportService.LastScene.Document.Connections);
        Assert.Equal(["source-node"], exportService.LastScene.Selection.SelectedNodeIds);
    }

    [Fact]
    public void RuntimeSession_ExportSceneAsImage_SelectedNodeScopeFailsWithoutSelection()
    {
        var definitionId = new NodeDefinitionId("tests.export.image-selected-scope-empty");
        var exportService = new RecordingSceneImageExportService("image://tests.export/selected.png");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            SceneImageExportService = exportService,
        });

        var exported = session.Commands.TryExportSceneAsImage(
            GraphEditorSceneImageExportFormat.Png,
            "image://tests.export/selected.png",
            new GraphEditorSceneImageExportOptions
            {
                Scope = GraphEditorSceneImageExportScope.SelectedNodes,
            });

        Assert.False(exported);
        Assert.Null(exportService.LastScene);
    }

    [Fact]
    public void RuntimeSession_ExportSceneAsImage_ReturnsFalseForEmptyScene()
    {
        var definitionId = new NodeDefinitionId("tests.export.image-empty");
        var exportService = new RecordingSceneImageExportService("image://tests.export/empty.png");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument("Empty Export Graph", "No nodes yet.", [], []),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new ExactCompatibilityService(),
            SceneImageExportService = exportService,
        });
        var featureDescriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var commandDescriptors = session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var exported = session.Commands.TryExportSceneAsImage(GraphEditorSceneImageExportFormat.Png);

        Assert.False(exported);
        Assert.Null(exportService.LastScene);
        Assert.False(featureDescriptors["capability.export.scene-image"].IsAvailable);
        Assert.False(featureDescriptors["capability.export.scene-png"].IsAvailable);
        Assert.False(featureDescriptors["capability.export.scene-jpeg"].IsAvailable);
        Assert.False(commandDescriptors["export.scene-image"].IsEnabled);
    }

    private static AsterGraphEditorOptions CreateOptions(NodeDefinitionId definitionId)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new ExactCompatibilityService(),
        };

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Export Contract Graph",
            "Canonical export regression coverage.",
            [
                new GraphNode(
                    "source-node",
                    "Export Source",
                    "Tests",
                    "Export",
                    "Source node for export coverage.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [
                        new GraphPort("out", "Output", PortDirection.Output, "float", "#55D8C1", new PortTypeId("float")),
                    ],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    "target-node",
                    "Export Target",
                    "Tests",
                    "Export",
                    "Target node for export coverage.",
                    new GraphPoint(460, 160),
                    new GraphSize(240, 160),
                    [
                        new GraphPort("in", "Input", PortDirection.Input, "float", "#55D8C1", new PortTypeId("float")),
                    ],
                    [],
                    "#F3B36B",
                    definitionId),
            ],
            [
                new GraphConnection(
                    "connection-001",
                    "source-node",
                    "out",
                    "target-node",
                    "in",
                    "flow",
                    "#8ED8FF"),
            ]);

    private static GraphDocument CreateLargeExportDocument(NodeDefinitionId definitionId, int nodeCount)
    {
        var nodes = new List<GraphNode>(nodeCount);
        var connections = new List<GraphConnection>(nodeCount - 1);

        for (var index = 0; index < nodeCount; index++)
        {
            var nodeId = $"large-node-{index:0000}";
            nodes.Add(new GraphNode(
                nodeId,
                $"Large Node {index:0000}",
                "Tests",
                "Export",
                "Large connected export coverage.",
                new GraphPoint(120 + ((index % 50) * 280), 160 + ((index / 50) * 220)),
                new GraphSize(220, 140),
                index == 0
                    ? []
                    : [new GraphPort("in", "Input", PortDirection.Input, "float", "#55D8C1", new PortTypeId("float"))],
                index == nodeCount - 1
                    ? []
                    : [new GraphPort("out", "Output", PortDirection.Output, "float", "#55D8C1", new PortTypeId("float"))],
                "#6AD5C4",
                definitionId));

            if (index > 0)
            {
                connections.Add(new GraphConnection(
                    $"large-connection-{index - 1:0000}",
                    $"large-node-{index - 1:0000}",
                    "out",
                    nodeId,
                    "in",
                    string.Empty,
                    "#8ED8FF"));
            }
        }

        return new GraphDocument(
            "Large Export Contract Graph",
            "Large connected scene export coverage.",
            nodes,
            connections);
    }

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Export Contract Node",
            "Tests",
            "Export",
            [
                new PortDefinition("in", "Input", new PortTypeId("float"), "#55D8C1"),
            ],
            [
                new PortDefinition("out", "Output", new PortTypeId("float"), "#55D8C1"),
            ]));
        return catalog;
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.Editor.Tests", nameof(GraphEditorExportContractsTests), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class RecordingSceneSvgExportService(string exportPath) : IGraphSceneSvgExportService
    {
        public string ExportPath { get; } = exportPath;

        public GraphEditorSceneSnapshot? LastScene { get; private set; }

        public string? LastPath { get; private set; }

        public string Export(GraphEditorSceneSnapshot scene, string? path = null)
        {
            LastScene = scene;
            LastPath = path ?? ExportPath;
            return LastPath;
        }
    }

    private sealed class RecordingSceneImageExportService(string defaultPath) : IGraphSceneImageExportService
    {
        public GraphEditorSceneSnapshot? LastScene { get; private set; }

        public GraphEditorSceneImageExportFormat LastFormat { get; private set; }

        public string? LastPath { get; private set; }

        public GraphEditorSceneImageExportOptions? LastOptions { get; private set; }

        public string GetExportPath(GraphEditorSceneImageExportFormat format)
            => defaultPath;

        public string Export(
            GraphEditorSceneSnapshot scene,
            GraphEditorSceneImageExportFormat format,
            string? path = null,
            GraphEditorSceneImageExportOptions? options = null)
        {
            LastScene = scene;
            LastFormat = format;
            LastPath = path ?? defaultPath;
            LastOptions = options;
            return LastPath;
        }
    }

    private sealed class ExactCompatibilityService : IPortCompatibilityService
    {
        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
            => sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
    }
}
