using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Avalonia.Controls;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class AsterGraphBuiltInToolbarTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.toolbar.node");
    private const string SourceNodeId = "tests.toolbar.source";
    private const string TargetNodeId = "tests.toolbar.target";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const string ConnectionId = "tests.toolbar.connection";

    [Fact]
    public void ToolbarTypes_ArePublicControlsWithCanonicalContextProperties()
    {
        Assert.True(typeof(NodeToolbar).IsPublic);
        Assert.True(typeof(EdgeToolbar).IsPublic);
        Assert.True(typeof(Control).IsAssignableFrom(typeof(NodeToolbar)));
        Assert.True(typeof(Control).IsAssignableFrom(typeof(EdgeToolbar)));

        Assert.NotNull(typeof(NodeToolbar).GetProperty(nameof(NodeToolbar.Session)));
        Assert.NotNull(typeof(NodeToolbar).GetProperty(nameof(NodeToolbar.NodeId)));
        Assert.NotNull(typeof(NodeToolbar).GetProperty(nameof(NodeToolbar.SelectedNodeIds)));
        Assert.NotNull(typeof(NodeToolbar).GetProperty(nameof(NodeToolbar.PrimarySelectedNodeId)));

        Assert.NotNull(typeof(EdgeToolbar).GetProperty(nameof(EdgeToolbar.Session)));
        Assert.NotNull(typeof(EdgeToolbar).GetProperty(nameof(EdgeToolbar.ConnectionId)));
        Assert.NotNull(typeof(EdgeToolbar).GetProperty(nameof(EdgeToolbar.SelectedNodeIds)));
        Assert.NotNull(typeof(EdgeToolbar).GetProperty(nameof(EdgeToolbar.PrimarySelectedNodeId)));
    }

    [AvaloniaFact]
    public void NodeToolbar_RendersCanonicalNodeActionsAndExecutesDuplicateCommand()
    {
        var session = CreateSession();
        var executedCommandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => executedCommandIds.Add(args.CommandId);
        var toolbar = new NodeToolbar
        {
            Session = session,
            NodeId = SourceNodeId,
            SelectedNodeIds = [SourceNodeId],
            PrimarySelectedNodeId = SourceNodeId,
        };
        var window = Show(toolbar);
        try
        {
            var duplicate = FindButton(toolbar, "Duplicate Node");

            Assert.False(toolbar.Focusable);
            Assert.True(duplicate.Focusable);
            Assert.True(duplicate.IsEnabled);
            Assert.Equal("Duplicate Node", duplicate.Content);
            Assert.Equal("Duplicate Node", AutomationProperties.GetName(duplicate));
            Assert.Null(ToolTip.GetTip(duplicate));

            var initialNodeCount = session.Queries.CreateDocumentSnapshot().Nodes.Count;
            duplicate.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Contains(executedCommandIds, commandId => commandId == "nodes.duplicate");
            Assert.Equal(initialNodeCount + 1, session.Queries.CreateDocumentSnapshot().Nodes.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeToolbar_DisabledActionsExposeTooltipRecoveryText()
    {
        var session = CreateSession(
            GraphEditorBehaviorOptions.Default with
            {
                Commands = GraphEditorCommandPermissions.Default with
                {
                    Nodes = new NodeCommandPermissions
                    {
                        AllowCreate = false,
                        AllowDuplicate = false,
                    },
                },
            },
            new DisabledDuplicateToolProvider());
        var toolbar = new NodeToolbar
        {
            Session = session,
            NodeId = SourceNodeId,
            SelectedNodeIds = [SourceNodeId],
            PrimarySelectedNodeId = SourceNodeId,
        };
        var window = Show(toolbar);
        try
        {
            var duplicate = FindButton(toolbar, "Duplicate Node");

            Assert.False(duplicate.IsEnabled);
            Assert.Equal("Node duplication is disabled by host permissions.\nEnable duplicate permission before retrying.", ToolTip.GetTip(duplicate));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void EdgeToolbar_RendersCanonicalEdgeActionsAndExecutesDisconnectCommand()
    {
        var session = CreateSession();
        var executedCommandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => executedCommandIds.Add(args.CommandId);
        var toolbar = new EdgeToolbar
        {
            Session = session,
            ConnectionId = ConnectionId,
            SelectedNodeIds = [SourceNodeId],
            PrimarySelectedNodeId = SourceNodeId,
        };
        var window = Show(toolbar);
        try
        {
            var disconnect = FindButton(toolbar, "Disconnect Connection");
            var clearNote = FindButton(toolbar, "Clear Connection Note");

            Assert.False(toolbar.Focusable);
            Assert.True(disconnect.Focusable);
            Assert.True(clearNote.Focusable);
            Assert.Equal("Disconnect Connection", AutomationProperties.GetName(disconnect));
            Assert.Equal("Clear Connection Note", AutomationProperties.GetName(clearNote));

            disconnect.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Contains(executedCommandIds, commandId => commandId == "connections.disconnect");
            Assert.DoesNotContain(
                session.Queries.CreateDocumentSnapshot().Connections,
                connection => connection.Id == ConnectionId);
        }
        finally
        {
            window.Close();
        }
    }

    private static Window Show(Control content)
    {
        var window = new Window
        {
            Width = 640,
            Height = 240,
            Content = content,
        };
        window.Show();
        return window;
    }

    private static Button FindButton(Control root, string automationName)
        => root.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => string.Equals(
                AutomationProperties.GetName(button),
                automationName,
                StringComparison.Ordinal));

    private static IGraphEditorSession CreateSession(
        GraphEditorBehaviorOptions? behaviorOptions = null,
        IGraphEditorToolProvider? toolProvider = null)
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            BehaviorOptions = behaviorOptions,
            ToolProvider = toolProvider,
        });

    private static GraphDocument CreateDocument()
        => new(
            "Toolbar Graph",
            "Node and edge toolbar contract coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Toolbar Source",
                    "Tests",
                    "Toolbar",
                    "Source node for toolbar tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Toolbar Target",
                    "Tests",
                    "Toolbar",
                    "Target node for toolbar tests.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefinitionId),
            ],
            [
                new GraphConnection(
                    ConnectionId,
                    SourceNodeId,
                    SourcePortId,
                    TargetNodeId,
                    TargetPortId,
                    "Toolbar Connection",
                    "#6AD5C4",
                    Presentation: new GraphEdgePresentation(NoteText: "Toolbar note")),
            ]);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Toolbar Node",
            "Tests",
            "Toolbar",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private sealed class DisabledDuplicateToolProvider : IGraphEditorToolProvider
    {
        public IReadOnlyList<GraphEditorToolDescriptorSnapshot> GetToolDescriptors(GraphEditorToolProviderContext context)
        {
            if (context.Context.Kind != GraphEditorToolContextKind.Node)
            {
                return [];
            }

            var descriptor = new GraphEditorCommandDescriptorSnapshot(
                "node-duplicate",
                "Duplicate Node",
                "Node",
                "duplicate",
                null,
                GraphEditorCommandSourceKind.Kernel,
                isEnabled: false,
                "Node duplication is disabled by host permissions.",
                "Enable duplicate permission before retrying.");
            var invocation = new GraphEditorCommandInvocationSnapshot(
                "nodes.duplicate",
                [new GraphEditorCommandArgumentSnapshot("nodeId", context.Context.NodeId!)]);
            return [new GraphEditorToolDescriptorSnapshot("node-duplicate", GraphEditorToolContextKind.Node, descriptor, invocation, order: 40)];
        }
    }
}
