using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Automation;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorAutomationExecutionTests
{
    private const string SourceNodeId = "tests.automation.source-001";
    private const string TargetNodeId = "tests.automation.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void AutomationRunner_ExecutesMultiStepRunInsideSharedMutationScope_AndPublishesTypedTelemetry()
    {
        var definitionId = new NodeDefinitionId("tests.automation.execution");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var startedEvents = new List<GraphEditorAutomationStartedEventArgs>();
        var progressEvents = new List<GraphEditorAutomationProgressEventArgs>();
        var completedEvents = new List<GraphEditorAutomationCompletedEventArgs>();
        var commandEvents = new List<GraphEditorCommandExecutedEventArgs>();

        session.Events.AutomationStarted += (_, args) => startedEvents.Add(args);
        session.Events.AutomationProgress += (_, args) => progressEvents.Add(args);
        session.Events.AutomationCompleted += (_, args) => completedEvents.Add(args);
        session.Events.CommandExecuted += (_, args) => commandEvents.Add(args);

        var result = session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "automation-run",
            [
                new GraphEditorAutomationStep("select-source", CreateCommand("selection.set", ("nodeId", SourceNodeId), ("primaryNodeId", SourceNodeId), ("updateStatus", "false"))),
                new GraphEditorAutomationStep("move-source", CreateCommand("nodes.move", ("position", $"{SourceNodeId}|300|210"), ("updateStatus", "false"))),
                new GraphEditorAutomationStep("resize", CreateCommand("viewport.resize", ("width", "1280"), ("height", "720"))),
                new GraphEditorAutomationStep("pan", CreateCommand("viewport.pan", ("deltaX", "12"), ("deltaY", "18"))),
                new GraphEditorAutomationStep("start-connection", CreateCommand("connections.start", ("sourceNodeId", SourceNodeId), ("sourcePortId", SourcePortId))),
                new GraphEditorAutomationStep("complete-connection", CreateCommand("connections.complete", ("targetNodeId", TargetNodeId), ("targetPortId", TargetPortId))),
            ]));

        Assert.True(result.Succeeded);
        Assert.True(result.UsedMutationScope);
        Assert.Equal("automation-run", result.MutationLabel);
        Assert.Equal(6, result.ExecutedStepCount);
        Assert.Equal(6, result.TotalStepCount);
        Assert.Equal(6, result.Steps.Count);
        Assert.All(result.Steps, step => Assert.True(step.Succeeded));
        Assert.Single(result.Inspection.Document.Connections);
        Assert.Equal(SourceNodeId, result.Inspection.Selection.PrimarySelectedNodeId);
        Assert.Contains(result.Inspection.NodePositions, position => position.NodeId == SourceNodeId && position.Position == new GraphPoint(300, 210));
        Assert.Equal(1280, result.Inspection.Viewport.ViewportWidth);
        Assert.Equal(720, result.Inspection.Viewport.ViewportHeight);

        var started = Assert.Single(startedEvents);
        Assert.Equal("automation-run", started.RunId);
        Assert.Equal(6, started.TotalStepCount);
        Assert.True(started.UsedMutationScope);
        Assert.Equal("automation-run", started.MutationLabel);

        Assert.Equal(6, progressEvents.Count);
        Assert.All(progressEvents, progress => Assert.Equal("automation-run", progress.RunId));
        Assert.Equal(
            ["selection.set", "nodes.move", "viewport.resize", "viewport.pan", "connections.start", "connections.complete"],
            progressEvents.Select(progress => progress.Step.CommandId).ToArray());

        var completed = Assert.Single(completedEvents);
        Assert.Equal(result, completed.Result);
        Assert.Equal(
            ["selection.set", "nodes.move", "viewport.resize", "viewport.pan", "connections.start", "connections.complete"],
            commandEvents.Select(args => args.CommandId).ToArray());
        Assert.All(commandEvents, args =>
        {
            Assert.True(args.IsInMutationScope);
            Assert.Equal("automation-run", args.MutationLabel);
        });

        var recentDiagnostics = session.Diagnostics.GetRecentDiagnostics(10);
        Assert.Contains(recentDiagnostics, diagnostic => diagnostic.Code == "automation.run.started");
        Assert.Contains(recentDiagnostics, diagnostic => diagnostic.Code == "automation.run.completed");
    }

    [Fact]
    public void AutomationRunner_FailsDisabledCommandAndPublishesMachineReadableFailureTelemetry()
    {
        var definitionId = new NodeDefinitionId("tests.automation.disabled");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            definitionId,
            GraphEditorBehaviorOptions.Default with
            {
                Commands = GraphEditorCommandPermissions.Default with
                {
                    Workspace = new WorkspaceCommandPermissions
                    {
                        AllowSave = true,
                        AllowLoad = false,
                    },
                },
            }));
        var progressEvents = new List<GraphEditorAutomationProgressEventArgs>();
        var completedEvents = new List<GraphEditorAutomationCompletedEventArgs>();
        var commandEvents = new List<GraphEditorCommandExecutedEventArgs>();

        session.Events.AutomationProgress += (_, args) => progressEvents.Add(args);
        session.Events.AutomationCompleted += (_, args) => completedEvents.Add(args);
        session.Events.CommandExecuted += (_, args) => commandEvents.Add(args);

        var result = session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "disabled-run",
            [
                new GraphEditorAutomationStep("load-workspace", CreateCommand("workspace.load")),
            ],
            runInMutationScope: false));

        Assert.False(result.Succeeded);
        Assert.Equal(1, result.ExecutedStepCount);
        Assert.Equal(1, result.TotalStepCount);
        var failedStep = Assert.Single(result.Steps);
        Assert.False(failedStep.Succeeded);
        Assert.Equal("automation.step.command-disabled", failedStep.FailureCode);
        Assert.Equal("automation.step.command-disabled", result.FailureCode);
        Assert.NotNull(result.FailureMessage);
        Assert.Contains("disabled", result.FailureMessage!, StringComparison.OrdinalIgnoreCase);

        var progress = Assert.Single(progressEvents);
        Assert.False(progress.Step.Succeeded);
        Assert.Equal("automation.step.command-disabled", progress.Step.FailureCode);
        Assert.Equal(result, Assert.Single(completedEvents).Result);
        Assert.Empty(commandEvents);

        var recentDiagnostics = session.Diagnostics.GetRecentDiagnostics(10);
        Assert.Contains(recentDiagnostics, diagnostic => diagnostic.Code == "automation.run.started");
        Assert.Contains(recentDiagnostics, diagnostic => diagnostic.Code == "automation.step.failed");
        Assert.Contains(recentDiagnostics, diagnostic => diagnostic.Code == "automation.run.completed");
    }

    [Fact]
    public void AutomationRunner_CanAddPluginContributedNodeDefinitionThroughCanonicalCommandIds()
    {
        var definitionId = new NodeDefinitionId("tests.automation.plugin-host");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations =
            [
                GraphEditorPluginRegistration.FromPlugin(new AutomationPlugin()),
            ],
        });

        var result = session.Automation.Execute(new GraphEditorAutomationRunRequest(
            "plugin-automation-run",
            [
                CreateStep("select-source", "selection.set", ("nodeId", SourceNodeId), ("primaryNodeId", SourceNodeId), ("updateStatus", "false")),
                CreateStep("add-plugin-node", "nodes.add", ("definitionId", "tests.automation.plugin"), ("worldX", "620"), ("worldY", "220")),
                CreateStep("move-plugin-node", "nodes.move", ("position", "tests-automation-plugin-001|648|244"), ("updateStatus", "false")),
            ]));

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.ExecutedStepCount);
        Assert.Contains(result.Inspection.Document.Nodes, node => node.DefinitionId is { Value: "tests.automation.plugin" });
        Assert.Contains(session.Queries.GetPluginLoadSnapshots(), snapshot => snapshot.Descriptor?.Id == "tests.automation.plugin");
    }

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            commandId,
            arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList());

    private static GraphEditorAutomationStep CreateStep(
        string stepId,
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(stepId, CreateCommand(commandId, arguments));

    private static AsterGraphEditorOptions CreateOptions(
        NodeDefinitionId definitionId,
        GraphEditorBehaviorOptions? behaviorOptions = null)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
            BehaviorOptions = behaviorOptions,
        };

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Automation Execution Graph",
            "Automation execution regression graph.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Automation Source",
                    "Tests",
                    "Automation",
                    "Source node for automation execution tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Automation Target",
                    "Tests",
                    "Automation",
                    "Target node for automation execution tests.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    definitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Automation Node",
            "Tests",
            "Automation",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private sealed class AutomationPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new(
            "tests.automation.plugin",
            "Automation Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddNodeDefinitionProvider(new AutomationPluginNodeDefinitionProvider());
        }
    }

    private sealed class AutomationPluginNodeDefinitionProvider : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            => [new NodeDefinition(new NodeDefinitionId("tests.automation.plugin"), "Automation Plugin Node", "Tests", "Automation", [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")], [])];
    }
}
