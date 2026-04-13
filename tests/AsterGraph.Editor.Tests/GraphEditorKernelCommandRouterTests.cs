using System.Reflection;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Kernel;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorKernelCommandRouterTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.kernel.command-router");
    private const string SourceNodeId = "tests.kernel.source-001";
    private const string TargetNodeId = "tests.kernel.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";

    [Fact]
    public void GraphEditorKernel_DelegatesCanonicalCommandRoutingToDedicatedCollaborator()
    {
        var methods = typeof(GraphEditorKernel)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);
        var routerType = typeof(GraphEditorKernel).Assembly.GetType("AsterGraph.Editor.Kernel.Internal.GraphEditorKernelCommandRouter");

        Assert.NotNull(routerType);
        Assert.DoesNotContain("ParseNodePosition", methods);
        Assert.DoesNotContain("TryGetRequiredArgument", methods);
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptorSignatures_RemainStableAcrossSelectionAndPendingConnectionStates()
    {
        var kernel = CreateKernel();
        kernel.UpdateViewportSize(1280, 720);

        var signature = string.Join(
            Environment.NewLine,
            [
                $"initial:{RenderDescriptorSignature(kernel.GetCommandDescriptors())}",
            ]);

        kernel.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        signature = string.Join(
            Environment.NewLine,
            [
                signature,
                $"selected:{RenderDescriptorSignature(kernel.GetCommandDescriptors())}",
            ]);

        kernel.StartConnection(SourceNodeId, SourcePortId);
        signature = string.Join(
            Environment.NewLine,
            [
                signature,
                $"pending:{RenderDescriptorSignature(kernel.GetCommandDescriptors())}",
            ]);

        var expected = """
            initial:nodes.add:True:-|selection.set:True:-|selection.delete:False:-|nodes.move:True:-|connections.start:True:-|connections.complete:False:-|connections.connect:True:-|connections.cancel:False:-|connections.delete:True:-|connections.break-port:True:-|connections.disconnect-incoming:True:-|connections.disconnect-outgoing:True:-|connections.disconnect-all:True:-|viewport.fit:True:-|viewport.pan:True:-|viewport.resize:True:-|viewport.reset:True:-|viewport.center-node:True:-|viewport.center:True:-|workspace.save:True:-|workspace.load:False:No saved snapshot yet. Save once to create one.
            selected:nodes.add:True:-|selection.set:True:-|selection.delete:True:-|nodes.move:True:-|connections.start:True:-|connections.complete:False:-|connections.connect:True:-|connections.cancel:False:-|connections.delete:True:-|connections.break-port:True:-|connections.disconnect-incoming:True:-|connections.disconnect-outgoing:True:-|connections.disconnect-all:True:-|viewport.fit:True:-|viewport.pan:True:-|viewport.resize:True:-|viewport.reset:True:-|viewport.center-node:True:-|viewport.center:True:-|workspace.save:True:-|workspace.load:False:No saved snapshot yet. Save once to create one.
            pending:nodes.add:True:-|selection.set:True:-|selection.delete:True:-|nodes.move:True:-|connections.start:True:-|connections.complete:True:-|connections.connect:True:-|connections.cancel:True:-|connections.delete:True:-|connections.break-port:True:-|connections.disconnect-incoming:True:-|connections.disconnect-outgoing:True:-|connections.disconnect-all:True:-|viewport.fit:True:-|viewport.pan:True:-|viewport.resize:True:-|viewport.reset:True:-|viewport.center-node:True:-|viewport.center:True:-|workspace.save:True:-|workspace.load:False:No saved snapshot yet. Save once to create one.
            """;

        Assert.Equal(expected.ReplaceLineEndings("\n"), signature.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsCanonicalKernelPayloads()
    {
        var kernel = CreateKernel();

        var added = kernel.TryExecuteCommand(
            CreateCommand(
                "nodes.add",
                ("definitionId", DefinitionId.Value),
                ("worldX", "640"),
                ("worldY", "220")));
        var selected = kernel.TryExecuteCommand(
            CreateCommand(
                "selection.set",
                ("nodeId", SourceNodeId),
                ("primaryNodeId", SourceNodeId),
                ("updateStatus", "false")));
        var moved = kernel.TryExecuteCommand(
            CreateCommand(
                "nodes.move",
                ("position", $"{SourceNodeId}|300|210"),
                ("updateStatus", "false")));
        var resized = kernel.TryExecuteCommand(
            CreateCommand(
                "viewport.resize",
                ("width", "1280"),
                ("height", "720")));
        var connected = kernel.TryExecuteCommand(
            CreateCommand(
                "connections.connect",
                ("sourceNodeId", SourceNodeId),
                ("sourcePortId", SourcePortId),
                ("targetNodeId", TargetNodeId),
                ("targetPortId", TargetPortId)));

        Assert.True(added);
        Assert.True(selected);
        Assert.True(moved);
        Assert.True(resized);
        Assert.True(connected);
        Assert.Equal(3, kernel.CreateDocumentSnapshot().Nodes.Count);
        Assert.Equal(SourceNodeId, kernel.GetSelectionSnapshot().PrimarySelectedNodeId);
        Assert.Contains(kernel.GetNodePositions(), position => position.NodeId == SourceNodeId && position.Position == new GraphPoint(300, 210));
        Assert.Equal(1280, kernel.GetViewportSnapshot().ViewportWidth);
        Assert.Equal(720, kernel.GetViewportSnapshot().ViewportHeight);
        Assert.Single(kernel.CreateDocumentSnapshot().Connections);
        Assert.False(kernel.TryExecuteCommand(CreateCommand("nodes.move", ("position", "bad-payload"))));
    }

    private static GraphEditorKernel CreateKernel()
        => new(
            CreateDocument(),
            CreateCatalog(),
            new DefaultPortCompatibilityService(),
            new EmptyWorkspaceService(),
            GraphEditorStyleOptions.Default,
            GraphEditorBehaviorOptions.Default);

    private static GraphDocument CreateDocument()
        => new(
            "Kernel Command Router Graph",
            "Regression coverage for kernel command router extraction.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Kernel Source",
                    "Tests",
                    "Kernel",
                    "Source node for command router tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefinitionId),
                new GraphNode(
                    TargetNodeId,
                    "Kernel Target",
                    "Tests",
                    "Kernel",
                    "Target node for command router tests.",
                    new GraphPoint(420, 160),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefinitionId),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Kernel Node",
            "Tests",
            "Kernel",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private static GraphEditorCommandInvocationSnapshot CreateCommand(
        string commandId,
        params (string Name, string Value)[] arguments)
        => new(
            commandId,
            arguments.Select(argument => new GraphEditorCommandArgumentSnapshot(argument.Name, argument.Value)).ToList());

    private static string RenderDescriptorSignature(IReadOnlyList<GraphEditorCommandDescriptorSnapshot> descriptors)
        => string.Join(
            "|",
            descriptors.Select(descriptor => $"{descriptor.Id}:{descriptor.IsEnabled}:{descriptor.DisabledReason ?? "-"}"));

    private sealed class EmptyWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://empty";

        public void Save(GraphDocument document)
        {
        }

        public GraphDocument Load()
            => throw new InvalidOperationException("No saved snapshot.");

        public bool Exists()
            => false;
    }
}
