using System;
using System.IO;
using AsterGraph.Demo;
using AsterGraph.Demo.Definitions;
using AsterGraph.Demo.ViewModels;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoScenarioLaunchTests
{
    [Fact]
    public void StartupOptionsParser_ParsesScenarioFlagAndKeepsAvaloniaArgs()
    {
        var startup = DemoStartupOptionsParser.Parse(["--scenario", "ai-pipeline", "--trace"]);

        Assert.Equal(DemoGraphFactory.AiPipelineScenario, startup.ShellOptions.InitialScenario);
        Assert.False(startup.ShellOptions.RestoreLastWorkspaceOnStartup);
        Assert.True(startup.ShellOptions.EnableStatePersistence);
        Assert.Equal(new[] { "--trace" }, startup.AvaloniaArgs);
    }

    [Fact]
    public void StartupOptionsParser_ParsesScenarioEqualsForm()
    {
        var startup = DemoStartupOptionsParser.Parse(["--scenario=AI-PIPELINE"]);

        Assert.Equal(DemoGraphFactory.AiPipelineScenario, startup.ShellOptions.InitialScenario);
        Assert.Empty(startup.AvaloniaArgs);
    }

    [Fact]
    public void StartupOptionsParser_ParsesTerrainShaderScenarioWithoutChangingOmittedStartupBehavior()
    {
        var defaultStartup = DemoStartupOptionsParser.Parse(["--trace"]);
        var terrainStartup = DemoStartupOptionsParser.Parse(["--scenario=Terrain-Shader"]);

        Assert.Null(defaultStartup.ShellOptions.InitialScenario);
        Assert.True(defaultStartup.ShellOptions.RestoreLastWorkspaceOnStartup);
        Assert.Equal(new[] { "--trace" }, defaultStartup.AvaloniaArgs);
        Assert.Equal(DemoGraphFactory.TerrainShaderScenario, terrainStartup.ShellOptions.InitialScenario);
        Assert.False(terrainStartup.ShellOptions.RestoreLastWorkspaceOnStartup);
        Assert.Empty(terrainStartup.AvaloniaArgs);
    }

    [Fact]
    public void StartupOptionsParser_RejectsUnknownScenario()
    {
        Assert.Throws<ArgumentException>(() => DemoStartupOptionsParser.Parse(["--scenario", "etl"]));
    }

    [Fact]
    public void DemoGraphFactory_ExposesHostOwnedScenarioPresetCatalog()
    {
        Assert.Contains(
            DemoGraphFactory.ScenarioPresets,
            preset => preset.Id == DemoGraphFactory.TerrainShaderScenario
                && preset.Title == "Terrain Shader Graph");
        Assert.Contains(
            DemoGraphFactory.ScenarioPresets,
            preset => preset.Id == DemoGraphFactory.AiPipelineScenario
                && preset.Title == "AI Workflow / Agent Pipeline");
        Assert.Equal(
            DemoGraphFactory.ScenarioPresets.Select(preset => preset.Id).Distinct(StringComparer.Ordinal).Count(),
            DemoGraphFactory.ScenarioPresets.Count);
    }

    [Fact]
    public void DemoGraphFactory_CreatesTerrainShaderScenarioThroughPresetId()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new DemoNodeDefinitionProvider());

        var defaultDocument = DemoGraphFactory.CreateStartupDocument(catalog, null);
        var presetDocument = DemoGraphFactory.CreateScenario(catalog, DemoGraphFactory.TerrainShaderScenario);

        Assert.Equal(defaultDocument.Title, presetDocument.Title);
        Assert.Equal("Terrain Shader Graph", presetDocument.Title);
        Assert.Equal(6, presetDocument.Nodes.Count);
        Assert.NotNull(presetDocument.Groups);
        Assert.Contains(presetDocument.Groups, group => group.Id == "terrain-authoring");
    }

    [Fact]
    public void DemoGraphFactory_CreatesPrewiredAiPipelineScenario()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterProvider(new DemoNodeDefinitionProvider());

        var document = DemoGraphFactory.CreateScenario(catalog, DemoGraphFactory.AiPipelineScenario);

        Assert.Equal("AI Workflow / Agent Pipeline", document.Title);
        Assert.Equal(6, document.Nodes.Count);
        Assert.Equal(6, document.Connections.Count);
        Assert.Contains(document.Nodes, node => node.Id == "input" && node.DefinitionId?.Value == "aster.demo.ai-input");
        Assert.Contains(document.Nodes, node => node.Id == "prompt" && node.ParameterValues?.Any(parameter => parameter.Key == "systemPrompt") == true);
        Assert.Contains(document.Nodes, node => node.Id == "llm" && node.ParameterValues?.Any(parameter => parameter.Key == "temperature") == true);
        Assert.Contains(document.Nodes, node => node.Id == "output" && node.DefinitionId?.Value == "aster.demo.ai-output");
        Assert.Contains(document.Connections, connection => connection.SourceNodeId == "prompt" && connection.TargetNodeId == "llm");
        Assert.Contains(document.Connections, connection => connection.SourceNodeId == "parser" && connection.TargetNodeId == "output");
        Assert.NotNull(document.Groups);
        Assert.Contains(document.Groups, group => group.Id == "ai-pipeline-run");
    }

    [Fact]
    public void MainWindowViewModel_RunsAiPipelineMockRuntimeFeedback()
    {
        var viewModel = new MainWindowViewModel(new MainWindowShellOptions(
            StorageRootPath: Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N")),
            EnableStatePersistence: true,
            RestoreLastWorkspaceOnStartup: false,
            InitialScenario: DemoGraphFactory.AiPipelineScenario));

        viewModel.RunAiPipelineMockRunner();
        var successOverlay = viewModel.GetAiPipelineRuntimeOverlay();

        Assert.True(successOverlay.IsAvailable);
        Assert.Contains(successOverlay.NodeOverlays, node => node.NodeId == "output" && node.Status == GraphEditorRuntimeOverlayStatus.Success);
        Assert.Contains(successOverlay.ConnectionOverlays, connection => connection.ConnectionId == "parser.payload->output.payload" && connection.ItemCount == 1);
        Assert.Contains(successOverlay.RecentLogs, log => log.Id == "ai-pipeline-run-completed");

        viewModel.RunAiPipelineMockRunner(forceError: true);
        var errorOverlay = viewModel.GetAiPipelineRuntimeOverlay();

        Assert.Contains(errorOverlay.NodeOverlays, node => node.NodeId == "llm" && node.Status == GraphEditorRuntimeOverlayStatus.Error);
        Assert.Contains(errorOverlay.ConnectionOverlays, connection => connection.ConnectionId == "llm.response->parser.response" && connection.IsStale);
        Assert.Contains(errorOverlay.RecentLogs, log => log.Id == "ai-pipeline-run-error");
    }
}
