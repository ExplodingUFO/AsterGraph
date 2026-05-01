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
using AsterGraph.Editor.Models;
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
    private const string TargetParameterKey = "gain";
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

        var initial = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        kernel.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        var selected = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        kernel.StartConnection(SourceNodeId, SourcePortId);
        var pending = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.False(initial["selection.delete"].IsEnabled);
        Assert.Equal("Select one or more nodes before deleting.", initial["selection.delete"].DisabledReason);
        Assert.False(initial["viewport.fit-selection"].IsEnabled);
        Assert.Equal("Select one or more nodes before fitting the selection.", initial["viewport.fit-selection"].DisabledReason);
        Assert.True(initial["viewport.focus-current-scope"].IsEnabled);

        Assert.True(selected["selection.delete"].IsEnabled);
        Assert.Null(selected["selection.delete"].DisabledReason);
        Assert.True(selected["viewport.fit-selection"].IsEnabled);
        Assert.True(selected["viewport.focus-selection"].IsEnabled);
        Assert.True(selected["fragments.export-selection"].IsEnabled);
        Assert.True(selected["composites.wrap-selection"].IsEnabled);

        Assert.True(pending["connections.complete"].IsEnabled);
        Assert.True(pending["connections.cancel"].IsEnabled);
        Assert.Equal(initial.Keys.Order(StringComparer.Ordinal), selected.Keys.Order(StringComparer.Ordinal));
        Assert.Equal(selected.Keys.Order(StringComparer.Ordinal), pending.Keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void GraphEditorKernel_LayoutCommandDescriptors_TrackSelectionCardinalityAndPermissions()
    {
        var kernel = CreateKernel();
        kernel.SetSelection([SourceNodeId, TargetNodeId], SourceNodeId, updateStatus: false);

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.True(descriptors["layout.align-left"].IsEnabled);
        Assert.False(descriptors["layout.distribute-horizontal"].IsEnabled);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, descriptors["layout.align-left"].Source);

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "nodes.add",
                ("definitionId", DefinitionId.Value),
                ("worldX", "840"),
                ("worldY", "260"))));
        var thirdNodeId = Assert.Single(
            kernel.CreateDocumentSnapshot().Nodes,
            node =>
                !string.Equals(node.Id, SourceNodeId, StringComparison.Ordinal)
                && !string.Equals(node.Id, TargetNodeId, StringComparison.Ordinal)).Id;

        kernel.SetSelection([SourceNodeId, TargetNodeId, thirdNodeId], SourceNodeId, updateStatus: false);
        descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.True(descriptors["layout.distribute-horizontal"].IsEnabled);
        Assert.True(descriptors["layout.distribute-vertical"].IsEnabled);

        var readOnlyKernel = CreateKernel(behaviorOptions: GraphEditorBehaviorOptions.Default with
        {
            Commands = GraphEditorCommandPermissions.Default with
            {
                Layout = new LayoutCommandPermissions
                {
                    AllowAlign = false,
                    AllowDistribute = false,
                },
            },
        });
        readOnlyKernel.SetSelection([SourceNodeId, TargetNodeId], SourceNodeId, updateStatus: false);
        descriptors = readOnlyKernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        Assert.False(descriptors["layout.align-left"].IsEnabled);
        Assert.False(descriptors["layout.distribute-horizontal"].IsEnabled);
    }

    [Fact]
    public void GraphEditorKernel_GroupRouteAndLayoutAffordances_ExposeDisabledRecoveryMetadata()
    {
        var kernel = CreateKernel();

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.False(descriptors["groups.collapse"].IsEnabled);
        Assert.Equal("Create a group before toggling group collapse.", descriptors["groups.collapse"].DisabledReason);
        Assert.Equal("Create a group first.", descriptors["groups.collapse"].RecoveryHint);
        Assert.Equal("groups.create", descriptors["groups.collapse"].RecoveryCommandId);

        Assert.False(descriptors["connections.route-vertex.insert"].IsEnabled);
        Assert.Equal("Create a connection before editing route vertices.", descriptors["connections.route-vertex.insert"].DisabledReason);
        Assert.Equal("Create a connection first.", descriptors["connections.route-vertex.insert"].RecoveryHint);
        Assert.Equal("connections.connect", descriptors["connections.route-vertex.insert"].RecoveryCommandId);

        Assert.False(descriptors["layout.align-center"].IsEnabled);
        Assert.Equal("Select at least two nodes before aligning.", descriptors["layout.align-center"].DisabledReason);
        Assert.Equal("Select at least two nodes first.", descriptors["layout.align-center"].RecoveryHint);
        Assert.Equal("nodes.add", descriptors["layout.align-center"].RecoveryCommandId);

        Assert.False(descriptors["layout.distribute-horizontal"].IsEnabled);
        Assert.Equal("Select at least three nodes before distributing.", descriptors["layout.distribute-horizontal"].DisabledReason);
        Assert.Equal("Select at least three nodes first.", descriptors["layout.distribute-horizontal"].RecoveryHint);
        Assert.Equal("nodes.add", descriptors["layout.distribute-horizontal"].RecoveryCommandId);
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

        var setConnectionSelection = descriptors["selection.connections.set"];
        Assert.Equal("Set Connection Selection", setConnectionSelection.Title);
        Assert.Equal("selection", setConnectionSelection.Group);
        Assert.Equal("select", setConnectionSelection.IconKey);
        Assert.Null(setConnectionSelection.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, setConnectionSelection.Source);
        Assert.True(setConnectionSelection.CanExecute);

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

        var exportImage = descriptors["export.scene-image"];
        Assert.Equal("Export Scene As Image", exportImage.Title);
        Assert.Equal("export", exportImage.Group);
        Assert.Equal("export", exportImage.IconKey);
        Assert.Null(exportImage.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, exportImage.Source);
        Assert.True(exportImage.CanExecute);
        Assert.True(exportImage.IsEnabled);

        var inspectNode = descriptors["nodes.inspect"];
        Assert.Equal("Inspect Node", inspectNode.Title);
        Assert.Equal("nodes", inspectNode.Group);
        Assert.Equal("inspect", inspectNode.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, inspectNode.Source);
        Assert.True(inspectNode.CanExecute);
        Assert.True(inspectNode.IsEnabled);

        var duplicateNode = descriptors["nodes.duplicate"];
        Assert.Equal("Duplicate Node", duplicateNode.Title);
        Assert.Equal("nodes", duplicateNode.Group);
        Assert.Equal("duplicate", duplicateNode.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, duplicateNode.Source);
        Assert.True(duplicateNode.CanExecute);
        Assert.True(duplicateNode.IsEnabled);

        var deleteNode = descriptors["nodes.delete-by-id"];
        Assert.Equal("Delete Node", deleteNode.Title);
        Assert.Equal("nodes", deleteNode.Group);
        Assert.Equal("delete", deleteNode.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, deleteNode.Source);
        Assert.True(deleteNode.CanExecute);
        Assert.True(deleteNode.IsEnabled);
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptors_ExposeExperiencePolishNavigationAndDisabledReasons()
    {
        var kernel = CreateKernel();
        kernel.UpdateViewportSize(1280, 720);

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.Equal("Select one or more nodes before deleting.", descriptors["selection.delete"].DisabledReason);
        Assert.Equal("Select at least two nodes before aligning.", descriptors["layout.align-left"].DisabledReason);
        Assert.Equal("Select at least three nodes before distributing.", descriptors["layout.distribute-horizontal"].DisabledReason);

        var fitSelection = descriptors["viewport.fit-selection"];
        Assert.Equal("Fit Selection", fitSelection.Title);
        Assert.Equal("viewport", fitSelection.Group);
        Assert.Equal("fit-selection", fitSelection.IconKey);
        Assert.False(fitSelection.IsEnabled);
        Assert.Equal("Select one or more nodes before fitting the selection.", fitSelection.DisabledReason);

        var focusSelection = descriptors["viewport.focus-selection"];
        Assert.Equal("Focus Selection", focusSelection.Title);
        Assert.Equal("viewport", focusSelection.Group);
        Assert.Equal("focus", focusSelection.IconKey);
        Assert.False(focusSelection.IsEnabled);
        Assert.Equal("Select one or more nodes before focusing the selection.", focusSelection.DisabledReason);

        var focusCurrentScope = descriptors["viewport.focus-current-scope"];
        Assert.Equal("Focus Current Scope", focusCurrentScope.Title);
        Assert.True(focusCurrentScope.IsEnabled);
        Assert.Null(focusCurrentScope.DisabledReason);

        Assert.False(kernel.TryExecuteCommand(CreateCommand("viewport.fit-selection")));
        Assert.Equal("Select one or more nodes before fitting the selection.", kernel.CurrentStatusMessage);

        kernel.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.True(descriptors["viewport.fit-selection"].IsEnabled);
        Assert.True(descriptors["viewport.focus-selection"].IsEnabled);
        Assert.Null(descriptors["viewport.fit-selection"].DisabledReason);
        Assert.Null(descriptors["viewport.focus-selection"].DisabledReason);
    }

    [Fact]
    public void GraphEditorKernel_ViewportSelectionCommands_RunThroughSharedCommandRoute()
    {
        var kernel = CreateKernel();
        kernel.UpdateViewportSize(1280, 720);
        kernel.PanBy(240, -120);
        kernel.SetSelection([TargetNodeId], TargetNodeId, updateStatus: false);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("viewport.fit-selection")));
        var fitSelectionViewport = kernel.GetViewportSnapshot();
        Assert.Equal("Viewport fit to selection.", kernel.CurrentStatusMessage);
        Assert.NotEqual(240, fitSelectionViewport.PanX);

        kernel.PanBy(-180, 75);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("viewport.focus-selection")));
        var focusSelectionViewport = kernel.GetViewportSnapshot();
        Assert.Equal("Viewport focused on selection.", kernel.CurrentStatusMessage);
        Assert.Equal(fitSelectionViewport.Zoom, focusSelectionViewport.Zoom);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("viewport.focus-current-scope")));
        Assert.Equal("Viewport focused on current scope.", kernel.CurrentStatusMessage);
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptors_ExposeCompositeWorkflowMetadata()
    {
        var kernel = CreateKernel();
        kernel.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var wrapSelection = descriptors["composites.wrap-selection"];
        Assert.Equal("Wrap Selection To Composite", wrapSelection.Title);
        Assert.Equal("composites", wrapSelection.Group);
        Assert.Equal("composite-wrap", wrapSelection.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, wrapSelection.Source);
        Assert.True(wrapSelection.CanExecute);

        var groupId = kernel.TryCreateNodeGroupFromSelection("Composite Cluster");
        Assert.False(string.IsNullOrWhiteSpace(groupId));

        descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

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
    public void GraphEditorKernel_CommandDescriptors_ExposeEdgeWorkflowMetadata()
    {
        var kernel = CreateKernel();
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.connect",
                ("sourceNodeId", SourceNodeId),
                ("sourcePortId", SourcePortId),
                ("targetNodeId", TargetNodeId),
                ("targetPortId", TargetPortId))));

        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var noteSet = descriptors["connections.note.set"];
        Assert.Equal("Set Connection Note", noteSet.Title);
        Assert.Equal("connections", noteSet.Group);
        Assert.Equal("inspect", noteSet.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, noteSet.Source);
        Assert.True(noteSet.CanExecute);

        var reconnect = descriptors["connections.reconnect"];
        Assert.Equal("Reconnect Connection", reconnect.Title);
        Assert.Equal("connections", reconnect.Group);
        Assert.Equal("connect", reconnect.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, reconnect.Source);
        Assert.True(reconnect.CanExecute);
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
        var expanded = kernel.TryExecuteCommand(
            CreateCommand(
                "nodes.surface.expand",
                ("nodeId", SourceNodeId),
                ("expansionState", nameof(GraphNodeExpansionState.Expanded))));
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
        Assert.True(expanded);
        Assert.True(connected);
        Assert.Equal(3, kernel.CreateDocumentSnapshot().Nodes.Count);
        Assert.Equal(SourceNodeId, kernel.GetSelectionSnapshot().PrimarySelectedNodeId);
        Assert.Contains(kernel.GetNodePositions(), position => position.NodeId == SourceNodeId && position.Position == new GraphPoint(300, 210));
        Assert.Equal(1280, kernel.GetViewportSnapshot().ViewportWidth);
        Assert.Equal(720, kernel.GetViewportSnapshot().ViewportHeight);
        Assert.Equal(
            GraphNodeExpansionState.Expanded,
            Assert.Single(kernel.GetNodeSurfaceSnapshots(), surface => surface.NodeId == SourceNodeId).ExpansionState);
        Assert.Single(kernel.CreateDocumentSnapshot().Connections);
        Assert.False(kernel.TryExecuteCommand(CreateCommand("nodes.move", ("position", "bad-payload"))));
    }

    [Fact]
    public void GraphEditorKernel_SpatialAuthoringCommands_RunThroughCanonicalCommandRoute()
    {
        var kernel = CreateKernel();
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "nodes.add",
                ("definitionId", DefinitionId.Value),
                ("worldX", "790"),
                ("worldY", "260"))));
        var thirdNodeId = Assert.Single(
            kernel.CreateDocumentSnapshot().Nodes,
            node =>
                !string.Equals(node.Id, SourceNodeId, StringComparison.Ordinal)
                && !string.Equals(node.Id, TargetNodeId, StringComparison.Ordinal)).Id;

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "selection.set",
                ("nodeId", SourceNodeId),
                ("nodeId", TargetNodeId),
                ("nodeId", thirdNodeId),
                ("primaryNodeId", SourceNodeId),
                ("updateStatus", "false"))));
        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        foreach (var commandId in new[]
        {
            "selection.transform.move",
            "groups.create",
            "layout.align-left",
            "layout.distribute-horizontal",
            "layout.snap-selection",
        })
        {
            Assert.True(descriptors[commandId].IsEnabled);
            Assert.Equal(GraphEditorCommandSourceKind.Kernel, descriptors[commandId].Source);
        }

        Assert.True(kernel.TryExecuteCommand(CreateCommand("layout.distribute-horizontal", ("updateStatus", "false"))));
        Assert.Equal("Distributed selection horizontally.", kernel.CurrentStatusMessage);
        Assert.NotEqual(
            new GraphPoint(420, 160),
            Assert.Single(kernel.GetNodePositions(), position => position.NodeId == TargetNodeId).Position);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("layout.align-left", ("updateStatus", "false"))));
        Assert.Equal("Aligned selection left.", kernel.CurrentStatusMessage);
        var selectedNodeIds = new HashSet<string>([SourceNodeId, TargetNodeId, thirdNodeId], StringComparer.Ordinal);
        Assert.All(
            kernel.GetNodePositions().Where(position => selectedNodeIds.Contains(position.NodeId)),
            position => Assert.Equal(120, position.Position.X));

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "selection.transform.move",
                ("deltaX", "12"),
                ("deltaY", "4"),
                ("constrainToPrimaryAxis", "true"),
                ("updateStatus", "false"))));
        Assert.Equal(
            new GraphPoint(132, 160),
            Assert.Single(kernel.GetNodePositions(), position => position.NodeId == SourceNodeId).Position);

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "layout.snap-selection",
                ("gridSize", "25"),
                ("updateStatus", "false"))));
        Assert.Equal("Snapped selected nodes to grid.", kernel.CurrentStatusMessage);
        Assert.Equal(
            new GraphPoint(125, 150),
            Assert.Single(kernel.GetNodePositions(), position => position.NodeId == SourceNodeId).Position);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("groups.create", ("title", "Spatial Cluster"))));
        var group = Assert.Single(kernel.CreateDocumentSnapshot().Groups!);
        Assert.Equal("Spatial Cluster", group.Title);
        Assert.Equal([SourceNodeId, TargetNodeId, thirdNodeId], group.NodeIds);
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsTypedParameterTargets()
    {
        var kernel = CreateParameterEndpointKernel();

        var connected = kernel.TryExecuteCommand(
            CreateCommand(
                "connections.connect",
                ("sourceNodeId", SourceNodeId),
                ("sourcePortId", SourcePortId),
                ("targetNodeId", TargetNodeId),
                ("targetPortId", TargetParameterKey),
                ("targetKind", nameof(GraphConnectionTargetKind.Parameter))));

        var connection = Assert.Single(kernel.CreateDocumentSnapshot().Connections);

        Assert.True(connected);
        Assert.Equal(TargetParameterKey, connection.TargetPortId);
        Assert.Equal(GraphConnectionTargetKind.Parameter, connection.TargetKind);
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsEdgeWorkflowPayloads()
    {
        var kernel = CreateKernel();

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.connect",
                ("sourceNodeId", SourceNodeId),
                ("sourcePortId", SourcePortId),
                ("targetNodeId", TargetNodeId),
                ("targetPortId", TargetPortId))));

        var connectionId = Assert.Single(kernel.CreateDocumentSnapshot().Connections).Id;
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.label.set",
                ("connectionId", connectionId),
                ("label", "Preview Flow"),
                ("updateStatus", "false"))));
        Assert.Equal(
            "Preview Flow",
            Assert.Single(kernel.CreateDocumentSnapshot().Connections).Label);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.note.set",
                ("connectionId", connectionId),
                ("text", "Preview branch"),
                ("updateStatus", "false"))));
        Assert.Equal(
            "Preview branch",
            Assert.Single(kernel.CreateDocumentSnapshot().Connections).Presentation?.NoteText);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.route-vertex.insert",
                ("connectionId", connectionId),
                ("vertexIndex", "0"),
                ("worldX", "360"),
                ("worldY", "120"),
                ("updateStatus", "false"))));
        Assert.Equal(
            [new GraphPoint(360d, 120d)],
            Assert.Single(kernel.CreateDocumentSnapshot().Connections).Presentation?.Route?.Vertices);
        kernel.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "selection.connections.set",
                ("connectionId", connectionId),
                ("primaryConnectionId", connectionId),
                ("updateStatus", "false"))));
        var selection = kernel.GetSelectionSnapshot();
        Assert.Empty(selection.SelectedNodeIds);
        Assert.Equal([connectionId], selection.SelectedConnectionIds);
        Assert.Equal(connectionId, selection.PrimarySelectedConnectionId);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.route-vertex.move",
                ("connectionId", connectionId),
                ("vertexIndex", "0"),
                ("worldX", "420"),
                ("worldY", "300"),
                ("updateStatus", "false"))));
        Assert.Equal(
            [new GraphPoint(420d, 300d)],
            Assert.Single(kernel.CreateDocumentSnapshot().Connections).Presentation?.Route?.Vertices);
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.route-vertex.remove",
                ("connectionId", connectionId),
                ("vertexIndex", "0"),
                ("updateStatus", "false"))));
        Assert.Empty(
            Assert.Single(kernel.CreateDocumentSnapshot().Connections).Presentation?.Route?.Vertices
            ?? []);

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.reconnect",
                ("connectionId", connectionId),
                ("updateStatus", "false"))));
        Assert.Empty(kernel.CreateDocumentSnapshot().Connections);
        var pending = kernel.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);
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
    public void GraphEditorKernel_TryExecuteCommand_AcceptsCompositeWrapSelectionPayloads()
    {
        var kernel = CreateKernel();

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "selection.set",
                ("nodeId", SourceNodeId),
                ("nodeId", TargetNodeId),
                ("primaryNodeId", TargetNodeId),
                ("updateStatus", "false"))));
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "composites.wrap-selection",
                ("title", "Composite Cluster"),
                ("updateStatus", "false"))));

        var document = kernel.CreateDocumentSnapshot();
        var compositeNode = Assert.Single(document.Nodes, node => node.Composite is not null);
        var childScope = Assert.Single(document.GraphScopes, scope => string.Equals(scope.Id, compositeNode.Composite!.ChildGraphId, StringComparison.Ordinal));
        var selection = kernel.GetSelectionSnapshot();

        Assert.Equal("Composite Cluster", compositeNode.Title);
        Assert.Empty(document.Groups ?? []);
        Assert.Equal([SourceNodeId, TargetNodeId], childScope.Nodes.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal([compositeNode.Id], selection.SelectedNodeIds);
        Assert.Equal(compositeNode.Id, selection.PrimarySelectedNodeId);
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
    public void GraphEditorKernel_TryExecuteCommand_RejectsUnsupportedSceneImageFormat()
    {
        var kernel = CreateKernel();

        Assert.False(kernel.TryExecuteCommand(CreateCommand("export.scene-image", ("format", "bmp"))));
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsCanonicalNodeQuickToolPayloads()
    {
        var kernel = CreateKernel();

        Assert.True(kernel.TryExecuteCommand(CreateCommand("nodes.inspect", ("nodeId", TargetNodeId))));
        Assert.Equal([TargetNodeId], kernel.GetSelectionSnapshot().SelectedNodeIds);
        Assert.Equal(TargetNodeId, kernel.GetSelectionSnapshot().PrimarySelectedNodeId);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("nodes.duplicate", ("nodeId", TargetNodeId))));
        Assert.Equal(3, kernel.CreateDocumentSnapshot().Nodes.Count);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("nodes.delete-by-id", ("nodeId", TargetNodeId))));
        Assert.DoesNotContain(kernel.CreateDocumentSnapshot().Nodes, node => string.Equals(node.Id, TargetNodeId, StringComparison.Ordinal));
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsSemanticClipboardPayloads()
    {
        var kernel = CreateKernel();
        kernel.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);

        Assert.True(kernel.TryExecuteCommand(CreateCommand("clipboard.copy")));
        Assert.True(kernel.TryExecuteCommand(CreateCommand("clipboard.paste")));

        var document = kernel.CreateDocumentSnapshot();
        Assert.Equal(3, document.Nodes.Count);
        Assert.Contains(document.Nodes, node => !string.Equals(node.Id, SourceNodeId, StringComparison.Ordinal)
            && node.Title == "Kernel Source");
    }

    [Fact]
    public void GraphEditorKernel_TryExecuteCommand_AcceptsSemanticConnectionEditPayloads()
    {
        var deleteReconnectKernel = CreateKernel();
        var insertedNodeId = InsertCompatibleNodeThroughCommandRoute(deleteReconnectKernel);

        deleteReconnectKernel.SetSelection([insertedNodeId], insertedNodeId, updateStatus: false);

        Assert.True(deleteReconnectKernel.TryExecuteCommand(CreateCommand("selection.delete-reconnect")));

        var deleteReconnectDocument = deleteReconnectKernel.CreateDocumentSnapshot();
        Assert.DoesNotContain(deleteReconnectDocument.Nodes, node => string.Equals(node.Id, insertedNodeId, StringComparison.Ordinal));
        Assert.Contains(deleteReconnectDocument.Connections, connection =>
            connection.SourceNodeId == SourceNodeId
            && connection.TargetNodeId == TargetNodeId);

        var detachKernel = CreateKernel();
        insertedNodeId = InsertCompatibleNodeThroughCommandRoute(detachKernel);

        detachKernel.SetSelection([insertedNodeId], insertedNodeId, updateStatus: false);

        Assert.True(detachKernel.TryExecuteCommand(CreateCommand("selection.detach-connections")));

        var detachDocument = detachKernel.CreateDocumentSnapshot();
        Assert.Contains(detachDocument.Nodes, node => string.Equals(node.Id, insertedNodeId, StringComparison.Ordinal));
        Assert.Contains(detachDocument.Connections, connection =>
            connection.SourceNodeId == SourceNodeId
            && connection.TargetNodeId == TargetNodeId);
        var reconnectedConnectionId = Assert.Single(detachDocument.Connections, connection =>
            connection.SourceNodeId == SourceNodeId
            && connection.TargetNodeId == TargetNodeId).Id;
        detachKernel.SetConnectionSelection([reconnectedConnectionId], reconnectedConnectionId, updateStatus: false);
        Assert.True(detachKernel.TryExecuteCommand(CreateCommand("connections.delete-selected")));
    }

    [Fact]
    public void GraphEditorKernel_CommandDescriptors_ExposeSemanticEditingCommandRoutes()
    {
        var kernel = CreateKernel();
        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.Equal("Delete And Reconnect", descriptors["selection.delete-reconnect"].Title);
        Assert.False(descriptors["selection.delete-reconnect"].IsEnabled);
        Assert.Equal("Select a middle node first, then retry.", descriptors["selection.delete-reconnect"].RecoveryHint);
        Assert.Equal("nodes.add", descriptors["selection.delete-reconnect"].RecoveryCommandId);

        Assert.Equal("Insert Node Into Connection", descriptors["nodes.insert-into-connection"].Title);
        Assert.False(descriptors["nodes.insert-into-connection"].IsEnabled);
        Assert.Equal("Create a connection first.", descriptors["nodes.insert-into-connection"].RecoveryHint);
        Assert.Equal("connections.connect", descriptors["nodes.insert-into-connection"].RecoveryCommandId);

        Assert.Equal("Delete Selected Connections", descriptors["connections.delete-selected"].Title);
        Assert.Equal("Slice Connections", descriptors["connections.slice"].Title);
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

    [Fact]
    public void GraphEditorKernel_HighFrictionDisabledCommands_HaveRecoveryHints()
    {
        var kernel = CreateKernel();
        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var highFrictionCommands = new[]
        {
            "selection.delete", "clipboard.copy", "fragments.export-selection",
            "groups.create", "composites.wrap-selection",
            "layout.align-left", "viewport.fit",
        };

        foreach (var commandId in highFrictionCommands)
        {
            var descriptor = descriptors[commandId];
            if (!descriptor.IsEnabled && !string.IsNullOrWhiteSpace(descriptor.DisabledReason))
            {
                Assert.False(
                    string.IsNullOrWhiteSpace(descriptor.RecoveryHint),
                    $"High-friction command '{commandId}' has DisabledReason but no RecoveryHint.");
            }
        }
    }

    [Fact]
    public void GraphEditorKernel_RecoveryCommandIds_ReferenceValidCommands()
    {
        var kernel = CreateKernel();
        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        foreach (var descriptor in descriptors.Values)
        {
            if (!string.IsNullOrWhiteSpace(descriptor.RecoveryCommandId))
            {
                Assert.True(
                    descriptors.ContainsKey(descriptor.RecoveryCommandId),
                    $"RecoveryCommandId '{descriptor.RecoveryCommandId}' for command '{descriptor.Id}' references unknown command.");
            }
        }
    }

    [Fact]
    public void GraphEditorKernel_SelectionRequiredCommands_RecoveryPointsToAddNode()
    {
        var kernel = CreateKernel();
        var descriptors = kernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.False(descriptors["selection.delete"].IsEnabled);
        Assert.Equal("Select nodes first, then retry.", descriptors["selection.delete"].RecoveryHint);
        Assert.Equal("nodes.add", descriptors["selection.delete"].RecoveryCommandId);
    }

    [Fact]
    public void GraphEditorKernel_PermissionDisabledCommands_HaveRecoveryHintButNoRecoveryCommand()
    {
        var readOnlyKernel = CreateKernel(behaviorOptions: GraphEditorBehaviorOptions.Default with
        {
            Commands = GraphEditorCommandPermissions.Default with
            {
                Nodes = new NodeCommandPermissions { AllowCreate = false, AllowDelete = false },
                Clipboard = new ClipboardCommandPermissions { AllowCopy = false },
            },
        });
        var descriptors = readOnlyKernel.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.False(descriptors["selection.delete"].IsEnabled);
        Assert.Contains("host permissions", descriptors["selection.delete"].DisabledReason!, StringComparison.Ordinal);
        Assert.Null(descriptors["selection.delete"].RecoveryCommandId);
    }

    private static GraphEditorKernel CreateKernel(
        IGraphWorkspaceService? workspaceService = null,
        GraphEditorBehaviorOptions? behaviorOptions = null)
        => new(
            CreateDocument(),
            CreateCatalog(),
            new DefaultPortCompatibilityService(),
            workspaceService ?? new EmptyWorkspaceService(),
            new EmptyFragmentWorkspaceService(),
            new EmptyFragmentLibraryService(),
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
            new EmptyFragmentWorkspaceService(),
            new EmptyFragmentLibraryService(),
            GraphEditorStyleOptions.Default,
            behaviorOptions ?? GraphEditorBehaviorOptions.Default);

    private static GraphEditorKernel CreateParameterEndpointKernel(
        IGraphWorkspaceService? workspaceService = null,
        GraphEditorBehaviorOptions? behaviorOptions = null)
        => new(
            CreateParameterEndpointDocument(),
            CreateParameterEndpointCatalog(),
            new DefaultPortCompatibilityService(),
            workspaceService ?? new EmptyWorkspaceService(),
            new EmptyFragmentWorkspaceService(),
            new EmptyFragmentLibraryService(),
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

    private static GraphDocument CreateParameterEndpointDocument()
        => new(
            "Kernel Parameter Graph",
            "Regression coverage for parameter connection targets.",
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

    private static NodeCatalog CreateParameterEndpointCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            DefinitionId,
            "Kernel Node",
            "Tests",
            "Kernel",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")],
            [
                new NodeParameterDefinition(
                    TargetParameterKey,
                    "Gain",
                    new PortTypeId("float"),
                    ParameterEditorKind.Number,
                    defaultValue: 1.0d),
            ]));
        return catalog;
    }

    private static string InsertCompatibleNodeThroughCommandRoute(GraphEditorKernel kernel)
    {
        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "connections.connect",
                ("sourceNodeId", SourceNodeId),
                ("sourcePortId", SourcePortId),
                ("targetNodeId", TargetNodeId),
                ("targetPortId", TargetPortId))));

        var connectionId = Assert.Single(kernel.CreateDocumentSnapshot().Connections).Id;

        Assert.True(kernel.TryExecuteCommand(
            CreateCommand(
                "nodes.insert-into-connection",
                ("connectionId", connectionId),
                ("definitionId", DefinitionId.Value),
                ("inputTargetId", TargetPortId),
                ("inputTargetKind", GraphConnectionTargetKind.Port.ToString()),
                ("outputPortId", SourcePortId),
                ("worldX", "280"),
                ("worldY", "160"))));

        return Assert.Single(kernel.CreateDocumentSnapshot().Nodes, node =>
            !string.Equals(node.Id, SourceNodeId, StringComparison.Ordinal)
            && !string.Equals(node.Id, TargetNodeId, StringComparison.Ordinal)).Id;
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

    private sealed class EmptyFragmentWorkspaceService : IGraphFragmentWorkspaceService
    {
        public string FragmentPath => "fragment://empty";

        public void Save(GraphSelectionFragment fragment, string? path = null)
        {
        }

        public GraphSelectionFragment Load(string? path = null)
            => throw new InvalidOperationException("No saved fragment.");

        public bool Exists(string? path = null)
            => false;

        public void Delete(string? path = null)
        {
        }
    }

    private sealed class EmptyFragmentLibraryService : IGraphFragmentLibraryService
    {
        public string LibraryPath => "library://empty";

        public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
            => [];

        public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
            => throw new InvalidOperationException("Template save is not used in this test.");

        public GraphSelectionFragment LoadTemplate(string path)
            => throw new InvalidOperationException("No saved template.");

        public void DeleteTemplate(string path)
        {
        }
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
