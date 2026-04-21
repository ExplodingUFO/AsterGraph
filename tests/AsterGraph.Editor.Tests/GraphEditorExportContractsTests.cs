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

    private sealed class ExactCompatibilityService : IPortCompatibilityService
    {
        public PortCompatibilityResult Evaluate(PortTypeId sourceType, PortTypeId targetType)
            => sourceType == targetType
                ? PortCompatibilityResult.Exact()
                : PortCompatibilityResult.Rejected();
    }
}
