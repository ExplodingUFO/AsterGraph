using AsterGraph.Demo;
using AsterGraph.Demo.Definitions;
using AsterGraph.Editor.Catalog;
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
    public void StartupOptionsParser_RejectsUnknownScenario()
    {
        Assert.Throws<ArgumentException>(() => DemoStartupOptionsParser.Parse(["--scenario", "etl"]));
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
}
