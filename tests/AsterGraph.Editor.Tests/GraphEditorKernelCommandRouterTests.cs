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
    private const string CompositeNodeId = "tests.kernel.composite-001";
    private const string ChildGraphId = "graph-child-001";
    private const string ChildSourceNodeId = "tests.kernel.child-source-001";
    private const string ChildTargetNodeId = "tests.kernel.child-target-001";

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
            initial:nodes.add:True:-|selection.set:True:-|selection.delete:False:-|nodes.move:True:-|nodes.resize:True:-|nodes.parameters.set:False:Parameter editing requires node-edit permissions and a shared node definition selection.|groups.create:False:-|groups.collapse:False:-|groups.move:False:-|groups.resize:False:-|groups.membership.set:False:-|groups.promote:False:-|composites.expose-port:False:-|composites.unexpose-port:False:-|scopes.enter:False:-|scopes.exit:False:-|connections.start:True:-|connections.complete:False:-|connections.connect:True:-|connections.cancel:False:-|connections.delete:True:-|connections.disconnect:True:-|connections.break-port:True:-|connections.disconnect-incoming:True:-|connections.disconnect-outgoing:True:-|connections.disconnect-all:True:-|history.undo:False:-|history.redo:False:-|viewport.fit:True:-|viewport.pan:True:-|viewport.resize:True:-|viewport.reset:True:-|viewport.center-node:True:-|viewport.center:True:-|workspace.save:True:-|workspace.load:False:No saved snapshot yet. Save once to create one.
            selected:nodes.add:True:-|selection.set:True:-|selection.delete:True:-|nodes.move:True:-|nodes.resize:True:-|nodes.parameters.set:False:Parameter editing requires node-edit permissions and a shared node definition selection.|groups.create:True:-|groups.collapse:False:-|groups.move:False:-|groups.resize:False:-|groups.membership.set:False:-|groups.promote:False:-|composites.expose-port:False:-|composites.unexpose-port:False:-|scopes.enter:False:-|scopes.exit:False:-|connections.start:True:-|connections.complete:False:-|connections.connect:True:-|connections.cancel:False:-|connections.delete:True:-|connections.disconnect:True:-|connections.break-port:True:-|connections.disconnect-incoming:True:-|connections.disconnect-outgoing:True:-|connections.disconnect-all:True:-|history.undo:False:-|history.redo:False:-|viewport.fit:True:-|viewport.pan:True:-|viewport.resize:True:-|viewport.reset:True:-|viewport.center-node:True:-|viewport.center:True:-|workspace.save:True:-|workspace.load:False:No saved snapshot yet. Save once to create one.
            pending:nodes.add:True:-|selection.set:True:-|selection.delete:True:-|nodes.move:True:-|nodes.resize:True:-|nodes.parameters.set:False:Parameter editing requires node-edit permissions and a shared node definition selection.|groups.create:True:-|groups.collapse:False:-|groups.move:False:-|groups.resize:False:-|groups.membership.set:False:-|groups.promote:False:-|composites.expose-port:False:-|composites.unexpose-port:False:-|scopes.enter:False:-|scopes.exit:False:-|connections.start:True:-|connections.complete:True:-|connections.connect:True:-|connections.cancel:True:-|connections.delete:True:-|connections.disconnect:True:-|connections.break-port:True:-|connections.disconnect-incoming:True:-|connections.disconnect-outgoing:True:-|connections.disconnect-all:True:-|history.undo:False:-|history.redo:False:-|viewport.fit:True:-|viewport.pan:True:-|viewport.resize:True:-|viewport.reset:True:-|viewport.center-node:True:-|viewport.center:True:-|workspace.save:True:-|workspace.load:False:No saved snapshot yet. Save once to create one.
            """;

        Assert.Equal(expected.ReplaceLineEndings("\n"), signature.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void GraphEditorKernel_DisconnectConnection_CommandUsesDisconnectPermissionInsteadOfDeletePermission()
    {
        var behavior = GraphEditorBehaviorOptions.Default with
        {
            Commands = GraphEditorCommandPermissions.Default with
            {
                Connections = new ConnectionCommandPermissions
                {
                    AllowCreate = true,
                    AllowDelete = false,
                    AllowDisconnect = true,
                },
            },
        };
        var kernel = CreateKernel(behaviorOptions: behavior);

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.connect",
                ("sourceNodeId", SourceNodeId),
                ("sourcePortId", SourcePortId),
                ("targetNodeId", TargetNodeId),
                ("targetPortId", TargetPortId))));

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.False(descriptors["connections.delete"].IsEnabled);
        Assert.True(descriptors["connections.disconnect"].IsEnabled);

        var connectionId = Assert.Single(kernel.CreateDocumentSnapshot().Connections).Id;

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.disconnect",
                ("connectionId", connectionId))));
        Assert.Empty(kernel.CreateDocumentSnapshot().Connections);
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptors_ExposeCanonicalMetadataAndKernelSource()
    {
        var kernel = CreateKernel();
        kernel.UpdateViewportSize(1280, 720);

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var saveWorkspace = descriptors["workspace.save"];
        Assert.Equal("Save Workspace", saveWorkspace.Title);
        Assert.Equal("workspace", saveWorkspace.Group);
        Assert.Equal("save", saveWorkspace.IconKey);
        Assert.Equal("Ctrl+S", saveWorkspace.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, saveWorkspace.Source);
        Assert.True(saveWorkspace.CanExecute);
        Assert.True(saveWorkspace.IsEnabled);

        var deleteSelection = descriptors["selection.delete"];
        Assert.Equal("Delete Selection", deleteSelection.Title);
        Assert.Equal("selection", deleteSelection.Group);
        Assert.Equal("delete", deleteSelection.IconKey);
        Assert.Equal("Delete", deleteSelection.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, deleteSelection.Source);
        Assert.False(deleteSelection.CanExecute);
        Assert.False(deleteSelection.IsEnabled);

        var cancelPendingConnection = descriptors["connections.cancel"];
        Assert.Equal("Cancel Pending Connection", cancelPendingConnection.Title);
        Assert.Equal("connections", cancelPendingConnection.Group);
        Assert.Equal("cancel", cancelPendingConnection.IconKey);
        Assert.Equal("Escape", cancelPendingConnection.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, cancelPendingConnection.Source);

        var undo = descriptors["history.undo"];
        Assert.Equal("Undo", undo.Title);
        Assert.Equal("history", undo.Group);
        Assert.Equal("undo", undo.IconKey);
        Assert.Equal("Ctrl+Z", undo.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, undo.Source);
        Assert.False(undo.CanExecute);
        Assert.False(undo.IsEnabled);

        var redo = descriptors["history.redo"];
        Assert.Equal("Redo", redo.Title);
        Assert.Equal("history", redo.Group);
        Assert.Equal("redo", redo.IconKey);
        Assert.Equal("Ctrl+Y", redo.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, redo.Source);
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptors_ExposeCompositePromotionMetadata()
    {
        var kernel = CreateKernel();
        kernel.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);
        var groupId = kernel.TryCreateNodeGroupFromSelection("Composite Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var promote = descriptors["groups.promote"];
        Assert.Equal("Promote Group To Composite", promote.Title);
        Assert.Equal("groups", promote.Group);
        Assert.Equal("group-promote", promote.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, promote.Source);
        Assert.True(promote.CanExecute);
        Assert.True(promote.IsEnabled);

        var expose = descriptors["composites.expose-port"];
        Assert.Equal("Expose Composite Port", expose.Title);
        Assert.Equal("composites", expose.Group);
        Assert.Equal("composite-expose", expose.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, expose.Source);
        Assert.False(expose.CanExecute);
        Assert.False(expose.IsEnabled);

        var unexpose = descriptors["composites.unexpose-port"];
        Assert.Equal("Unexpose Composite Port", unexpose.Title);
        Assert.Equal("composites", unexpose.Group);
        Assert.Equal("composite-unexpose", unexpose.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, unexpose.Source);
        Assert.False(unexpose.CanExecute);
        Assert.False(unexpose.IsEnabled);
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptors_ExposeScopeNavigationMetadata()
    {
        var kernel = CreateScopedKernel();

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var enter = descriptors["scopes.enter"];
        Assert.Equal("Enter Composite Scope", enter.Title);
        Assert.Equal("scopes", enter.Group);
        Assert.Equal("scope-enter", enter.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, enter.Source);
        Assert.True(enter.CanExecute);

        var exit = descriptors["scopes.exit"];
        Assert.Equal("Return To Parent Scope", exit.Title);
        Assert.Equal("scopes", exit.Group);
        Assert.Equal("scope-exit", exit.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, exit.Source);
        Assert.False(exit.CanExecute);
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
    public void GraphEditorKernel_TryExecuteCommand_AcceptsCompositePromotionPayloads()
    {
        var kernel = CreateKernel();

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "selection.set",
                ("nodeId", SourceNodeId),
                ("nodeId", TargetNodeId),
                ("primaryNodeId", TargetNodeId),
                ("updateStatus", "false"))));
        Assert.True(kernel.TryExecuteCommand(CreateCommand("groups.create", ("title", "Composite Cluster"))));

        var groupId = Assert.Single(kernel.CreateDocumentSnapshot().Groups!).Id;
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "groups.promote",
                ("groupId", groupId),
                ("title", "Composite Cluster"),
                ("updateStatus", "false"))));

        var descriptorsAfterPromote = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.True(descriptorsAfterPromote["composites.expose-port"].CanExecute);
        Assert.False(descriptorsAfterPromote["composites.unexpose-port"].CanExecute);

        var compositeNode = Assert.Single(kernel.CreateDocumentSnapshot().Nodes, node => node.Composite is not null);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "composites.expose-port",
                ("compositeNodeId", compositeNode.Id),
                ("childNodeId", SourceNodeId),
                ("childPortId", SourcePortId),
                ("label", "Composite Output"),
                ("updateStatus", "false"))));

        var exposedCompositeNode = Assert.Single(kernel.CreateDocumentSnapshot().Nodes, node => node.Composite is not null);
        var boundaryPortId = Assert.Single(exposedCompositeNode.Composite!.Outputs ?? []).Id;
        var descriptorsAfterExpose = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.True(descriptorsAfterExpose["composites.unexpose-port"].CanExecute);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "composites.unexpose-port",
                ("compositeNodeId", compositeNode.Id),
                ("boundaryPortId", boundaryPortId),
                ("updateStatus", "false"))));
        Assert.False(kernel.TryExecuteCommand(CreateCommand("groups.promote", ("title", "missing-group-id"))));
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsScopeNavigationPayloads()
    {
        var kernel = CreateScopedKernel();

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "scopes.enter",
                ("compositeNodeId", CompositeNodeId),
                ("updateStatus", "false"))));
        Assert.Equal(
            [ChildSourceNodeId, ChildTargetNodeId],
            kernel.GetNodePositions().Select(position => position.NodeId).OrderBy(id => id, StringComparer.Ordinal));

        Assert.True(kernel.TryExecuteCommand(CreateCommand("scopes.exit", ("updateStatus", "false"))));
        Assert.Equal(
            [CompositeNodeId, SourceNodeId, TargetNodeId],
            kernel.GetNodePositions().Select(position => position.NodeId).OrderBy(id => id, StringComparer.Ordinal));
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

    private static GraphEditorKernel CreateKernel(
        IGraphWorkspaceService? workspaceService = null,
        GraphEditorBehaviorOptions? behaviorOptions = null)
        => new(
            CreateDocument(),
            CreateCatalog(),
            new DefaultPortCompatibilityService(),
            workspaceService ?? new EmptyWorkspaceService(),
            GraphEditorStyleOptions.Default,
            behaviorOptions ?? GraphEditorBehaviorOptions.Default);

    private static GraphEditorKernel CreateScopedKernel(
        IGraphWorkspaceService? workspaceService = null,
        GraphEditorBehaviorOptions? behaviorOptions = null)
        => new(
            CreateScopedDocument(),
            CreateCatalog(),
            new DefaultPortCompatibilityService(),
            workspaceService ?? new EmptyWorkspaceService(),
            GraphEditorStyleOptions.Default,
            behaviorOptions ?? GraphEditorBehaviorOptions.Default);

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

    private static GraphDocument CreateScopedDocument()
        => GraphDocument.CreateScoped(
            "Kernel Scoped Graph",
            "Regression coverage for kernel scope navigation.",
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [
                        new GraphNode(
                            SourceNodeId,
                            "Kernel Source",
                            "Tests",
                            "Kernel",
                            "Source node for root scope coverage.",
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
                            "Target node for root scope coverage.",
                            new GraphPoint(420, 160),
                            new GraphSize(220, 140),
                            [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                            [],
                            "#F3B36B",
                            DefinitionId),
                        new GraphNode(
                            CompositeNodeId,
                            "Composite Kernel Node",
                            "Tests",
                            "Kernel",
                            "Composite shell for scope navigation.",
                            new GraphPoint(760, 160),
                            new GraphSize(260, 180),
                            [],
                            [],
                            "#A67CF5",
                            null,
                            [],
                            null,
                            new GraphCompositeNode(ChildGraphId, [], [])),
                    ],
                    []),
                new GraphScope(
                    ChildGraphId,
                    [
                        new GraphNode(
                            ChildSourceNodeId,
                            "Child Source",
                            "Tests",
                            "Kernel",
                            "Child scope source node.",
                            new GraphPoint(60, 80),
                            new GraphSize(220, 140),
                            [],
                            [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                            "#6AD5C4",
                            DefinitionId),
                        new GraphNode(
                            ChildTargetNodeId,
                            "Child Target",
                            "Tests",
                            "Kernel",
                            "Child scope target node.",
                            new GraphPoint(340, 120),
                            new GraphSize(220, 140),
                            [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                            [],
                            "#F3B36B",
                            DefinitionId),
                    ],
                    []),
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
