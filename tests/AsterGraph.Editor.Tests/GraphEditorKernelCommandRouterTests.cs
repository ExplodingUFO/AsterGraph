using System.Reflection;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Abstractions.Styling;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Configuration;
using AsterGraph.Editor.Events;
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
        var hostType = typeof(GraphEditorKernel).GetNestedType("GraphEditorKernelCommandRouterHost", BindingFlags.NonPublic);
        var implementedInterfaces = typeof(GraphEditorKernel).GetInterfaces();

        Assert.NotNull(routerType);
        Assert.NotNull(hostType);
        Assert.DoesNotContain("ParseNodePosition", methods);
        Assert.DoesNotContain("TryGetRequiredArgument", methods);
        Assert.DoesNotContain(
            implementedInterfaces,
            iface => iface.FullName == "AsterGraph.Editor.Kernel.Internal.IGraphEditorKernelCommandRouterHost");
    }

    [Fact]
    public void EditorAssembly_ContainsDedicatedWorkspaceLoadCoordinator()
    {
        var coordinatorType = typeof(GraphEditorKernel).Assembly.GetType("AsterGraph.Editor.Services.GraphEditorWorkspaceLoadCoordinator");

        Assert.NotNull(coordinatorType);
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

    [Fact]
    public void GraphEditorKernel_SaveWorkspace_PassesLiveDocumentToWorkspaceService()
    {
        var workspace = new MutatingWorkspaceService();
        var kernel = CreateKernel(workspace);

        kernel.SaveWorkspace();

        Assert.Equal(3, kernel.CreateDocumentSnapshot().Nodes.Count);
        Assert.False(kernel.IsDirty);
    }

    [Fact]
    public void GraphEditorKernel_LoadWorkspace_ReplacesDocumentAndResetsTransientState()
    {
        var workspace = new LoadedWorkspaceService(CreateLoadedDocument());
        var kernel = CreateKernel(workspace);
        GraphEditorDocumentChangedEventArgs? documentChanged = null;
        kernel.DocumentChanged += (_, args) => documentChanged = args;

        kernel.UpdateViewportSize(1280, 720);
        kernel.PanBy(40, -20);
        kernel.ZoomAt(1.2, new GraphPoint(640, 360));
        kernel.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        kernel.StartConnection(SourceNodeId, SourcePortId);

        var loaded = kernel.LoadWorkspace();

        Assert.True(loaded);
        Assert.Equal("Loaded Kernel Graph", kernel.CreateDocumentSnapshot().Title);
        Assert.Single(kernel.CreateDocumentSnapshot().Connections);
        Assert.Empty(kernel.GetSelectionSnapshot().SelectedNodeIds);
        Assert.False(kernel.GetPendingConnectionSnapshot().HasPendingConnection);
        Assert.Equal(0.88, kernel.GetViewportSnapshot().Zoom);
        Assert.Equal(110, kernel.GetViewportSnapshot().PanX);
        Assert.Equal(96, kernel.GetViewportSnapshot().PanY);
        Assert.Equal(1280, kernel.GetViewportSnapshot().ViewportWidth);
        Assert.Equal(720, kernel.GetViewportSnapshot().ViewportHeight);
        Assert.Equal("Workspace loaded from disk.", kernel.CurrentStatusMessage);
        Assert.NotNull(documentChanged);
        Assert.Equal(GraphEditorDocumentChangeKind.WorkspaceLoaded, documentChanged!.ChangeKind);
        Assert.Equal("Workspace loaded from disk.", documentChanged.StatusMessage);
    }

    [Fact]
    public void GraphEditorKernel_LoadWorkspace_WhenMissingDiagnosticPublicationThrows_RaisesRecoverableFailure()
    {
        var kernel = CreateKernel();
        GraphEditorRecoverableFailureEventArgs? failure = null;
        kernel.RecoverableFailureRaised += (_, args) => failure = args;
        kernel.DiagnosticPublished += _ => throw new InvalidOperationException("diagnostic boom");

        var loaded = kernel.LoadWorkspace();

        Assert.False(loaded);
        Assert.NotNull(failure);
        Assert.Equal("workspace.load.failed", failure!.Code);
        Assert.Contains("diagnostic boom", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphEditorKernel_LoadWorkspace_WhenExistsThrows_RaisesRecoverableFailure()
    {
        var kernel = CreateKernel(new ThrowingExistsWorkspaceService());
        GraphEditorRecoverableFailureEventArgs? failure = null;
        kernel.RecoverableFailureRaised += (_, args) => failure = args;

        var loaded = kernel.LoadWorkspace();

        Assert.False(loaded);
        Assert.NotNull(failure);
        Assert.Equal("workspace.load.failed", failure!.Code);
        Assert.Contains("exists boom", failure.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphEditorKernel_LoadWorkspace_WhenLoadThrows_RaisesRecoverableFailure()
    {
        var kernel = CreateKernel(new ThrowingLoadWorkspaceService());
        GraphEditorRecoverableFailureEventArgs? failure = null;
        kernel.RecoverableFailureRaised += (_, args) => failure = args;

        var loaded = kernel.LoadWorkspace();

        Assert.False(loaded);
        Assert.NotNull(failure);
        Assert.Equal("workspace.load.failed", failure!.Code);
        Assert.Contains("load boom", failure.Message, StringComparison.Ordinal);
    }

    private static GraphEditorKernel CreateKernel(IGraphWorkspaceService? workspaceService = null)
        => new(
            CreateDocument(),
            CreateCatalog(),
            new DefaultPortCompatibilityService(),
            workspaceService ?? new EmptyWorkspaceService(),
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

    private static GraphDocument CreateLoadedDocument()
        => new(
            "Loaded Kernel Graph",
            "Loaded from workspace service.",
            [
                new GraphNode(
                    "loaded-source",
                    "Loaded Source",
                    "Tests",
                    "Kernel",
                    "Loaded source node for workspace tests.",
                    new GraphPoint(160, 120),
                    new GraphSize(220, 140),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    DefinitionId),
                new GraphNode(
                    "loaded-target",
                    "Loaded Target",
                    "Tests",
                    "Kernel",
                    "Loaded target node for workspace tests.",
                    new GraphPoint(460, 180),
                    new GraphSize(220, 140),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#F3B36B",
                    DefinitionId),
            ],
            [
                new GraphConnection("loaded-connection", "loaded-source", SourcePortId, "loaded-target", TargetPortId, "loaded-link", "#55D8C1"),
            ]);

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

    private sealed class MutatingWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://mutating";

        public void Save(GraphDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            var nodes = Assert.IsType<List<GraphNode>>(document.Nodes);
            nodes.Add(new GraphNode(
                "workspace-node",
                "Workspace Added",
                "Tests",
                "Kernel",
                "Added during save to verify live document semantics.",
                new GraphPoint(640, 160),
                new GraphSize(220, 140),
                [],
                [],
                "#55D8C1",
                DefinitionId));
        }

        public GraphDocument Load()
            => throw new InvalidOperationException("No saved snapshot.");

        public bool Exists()
            => false;
    }

    private sealed class LoadedWorkspaceService : IGraphWorkspaceService
    {
        private readonly GraphDocument _document;

        public LoadedWorkspaceService(GraphDocument document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public string WorkspacePath => "workspace://loaded";

        public void Save(GraphDocument document)
            => throw new InvalidOperationException("Save should not be called in this test.");

        public GraphDocument Load() => _document;

        public bool Exists() => true;
    }

    private sealed class ThrowingExistsWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://exists-throw";

        public void Save(GraphDocument document)
            => throw new InvalidOperationException("Save should not be called in this test.");

        public GraphDocument Load()
            => throw new InvalidOperationException("Load should not be called when Exists throws.");

        public bool Exists()
            => throw new InvalidOperationException("exists boom");
    }

    private sealed class ThrowingLoadWorkspaceService : IGraphWorkspaceService
    {
        public string WorkspacePath => "workspace://load-throw";

        public void Save(GraphDocument document)
            => throw new InvalidOperationException("Save should not be called in this test.");

        public GraphDocument Load()
            => throw new InvalidOperationException("load boom");

        public bool Exists() => true;
    }
}
