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

public sealed class AsterGraphBuiltInNodeResizerTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.node-resizer.node");
    private const string NodeId = "tests.node-resizer.node-001";
    private const string InputPortId = "in";
    private const string OutputPortId = "out";

    [Fact]
    public void NodeResizerType_IsPublicControlWithCanonicalContextProperties()
    {
        Assert.True(typeof(NodeResizer).IsPublic);
        Assert.True(typeof(Control).IsAssignableFrom(typeof(NodeResizer)));

        Assert.NotNull(typeof(NodeResizer).GetProperty(nameof(NodeResizer.Session)));
        Assert.NotNull(typeof(NodeResizer).GetProperty(nameof(NodeResizer.NodeId)));
        Assert.NotNull(typeof(NodeResizer).GetProperty(nameof(NodeResizer.ResizeStep)));
        Assert.NotNull(typeof(NodeResizer).GetProperty(nameof(NodeResizer.MinimumWidth)));
        Assert.NotNull(typeof(NodeResizer).GetProperty(nameof(NodeResizer.MinimumHeight)));
    }

    [AvaloniaFact]
    public void NodeResizer_RendersStableHandlesAndCommitsResizeThroughSessionCommand()
    {
        var session = CreateSession();
        var executedCommandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => executedCommandIds.Add(args.CommandId);
        var resizer = new NodeResizer
        {
            Session = session,
            NodeId = NodeId,
            ResizeStep = 24d,
        };
        var window = Show(resizer);
        try
        {
            var rightHandle = FindHandle(resizer, "Resize Node Right");
            var bottomHandle = FindHandle(resizer, "Resize Node Down");
            var cornerHandle = FindHandle(resizer, "Resize Node Diagonal");

            Assert.Equal("PART_NodeResizerRightHandle", rightHandle.Name);
            Assert.Equal("PART_NodeResizerBottomHandle", bottomHandle.Name);
            Assert.Equal("PART_NodeResizerCornerHandle", cornerHandle.Name);
            var initialSize = GetNodeSize(session);

            rightHandle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            bottomHandle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            cornerHandle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal(3, executedCommandIds.Count(commandId => commandId == "nodes.resize"));
            var resizedSize = GetNodeSize(session);
            Assert.True(resizedSize.Width >= initialSize.Width + 48d);
            Assert.True(resizedSize.Height >= initialSize.Height + 48d);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void NodeResizer_ClampsRequestedSizeToMinimums()
    {
        var session = CreateSession();
        var resizer = new NodeResizer
        {
            Session = session,
            NodeId = NodeId,
            ResizeStep = -500d,
            MinimumWidth = 400d,
            MinimumHeight = 260d,
        };
        var window = Show(resizer);
        try
        {
            var cornerHandle = FindHandle(resizer, "Resize Node Diagonal");

            Assert.Equal("PART_NodeResizerCornerHandle", cornerHandle.Name);
            Assert.Equal(-500d, resizer.ResizeStep);

            cornerHandle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal(
                new GraphSize(resizer.MinimumWidth, resizer.MinimumHeight),
                GetNodeSize(session));
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
            Width = 320,
            Height = 220,
            Content = content,
        };
        window.Show();
        return window;
    }

    private static Button FindHandle(Control root, string automationName)
        => root.GetVisualDescendants()
            .OfType<Button>()
            .Single(button => string.Equals(
                AutomationProperties.GetName(button),
                automationName,
                StringComparison.Ordinal));

    private static GraphSize GetNodeSize(IGraphEditorSession session)
        => Assert.Single(session.Queries.GetNodeSurfaceSnapshots(), node => node.NodeId == NodeId).Size;

    private static IGraphEditorSession CreateSession()
        => AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

    private static GraphDocument CreateDocument()
        => new(
            "NodeResizer Graph",
            "Standalone node resizer contract coverage.",
            [
                new GraphNode(
                    NodeId,
                    "Resizable Node",
                    "Tests",
                    "Node Resizer",
                    "Source node for node resizer tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [new GraphPort(InputPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [new GraphPort(OutputPortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefinitionId),
            ],
            []);

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Resizable Node",
            "Tests",
            "Node Resizer",
            [new PortDefinition(InputPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(OutputPortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }
}
