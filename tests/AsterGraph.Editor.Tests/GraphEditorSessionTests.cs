using System.Reflection;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Events;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Models;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.Runtime;
using AsterGraph.Editor.Services;
using AsterGraph.Editor.ViewModels;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSessionTests
{
    private const string SourceNodeId = "tests.session.source-001";
    private const string TargetNodeId = "tests.session.target-001";
    private const string SourcePortId = "out";
    private const string TargetPortId = "in";
    private const string TargetParameterKey = "gain";
    private const string CompositeNodeId = "tests.session.composite-001";
    private const string RootStandaloneNodeId = "tests.session.root-standalone-001";
    private const string ChildGraphId = "graph-child-001";
    private const string ChildSourceNodeId = "tests.session.child-source-001";
    private const string ChildTargetNodeId = "tests.session.child-target-001";

    [Fact]
    public void IGraphEditorSession_ExposesCommandsQueriesAndEventsProperties()
    {
        var sessionType = typeof(IGraphEditorSession);

        Assert.True(sessionType.IsPublic);
        Assert.True(sessionType.IsInterface);

        AssertProperty(sessionType, nameof(IGraphEditorSession.Commands), typeof(IGraphEditorCommands));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Queries), typeof(IGraphEditorQueries));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Events), typeof(IGraphEditorEvents));
        AssertProperty(sessionType, nameof(IGraphEditorSession.Automation), typeof(AsterGraph.Editor.Automation.IGraphEditorAutomationRunner));
    }

    [Fact]
    public void GraphEditorSession_PreservesConcreteBeginConnectionCompatibilityShim()
    {
        var method = typeof(GraphEditorSession).GetMethod(nameof(GraphEditorSession.BeginConnection), [typeof(string), typeof(string)]);

        Assert.NotNull(method);
        Assert.Contains(
            method!.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
    }

    [Fact]
    public void GraphEditorSession_DelegatesStockContextMenuDescriptorBuildingToDedicatedCollaborator()
    {
        var methodNames = typeof(GraphEditorSession)
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("BuildCanvasMenuDescriptors", methodNames);
        Assert.DoesNotContain("BuildSelectionMenuDescriptors", methodNames);
        Assert.DoesNotContain("BuildNodeMenuDescriptors", methodNames);
        Assert.DoesNotContain("BuildPortMenuDescriptors", methodNames);
        Assert.DoesNotContain("BuildConnectionMenuDescriptors", methodNames);
        Assert.DoesNotContain("BuildCompatibleTargetItems", methodNames);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_NoLongerStoresGraphEditorViewModelAsItsRuntimeStateOwner()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(new NodeDefinitionId("tests.session.kernel-owner")));

        Assert.DoesNotContain(
            session.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
            field => field.FieldType == typeof(GraphEditorViewModel));
    }

    [Fact]
    public void AsterGraphEditorFactory_Create_EditorSession_NoLongerStoresGraphEditorViewModelAsItsRuntimeStateOwner()
    {
        var editor = AsterGraphEditorFactory.Create(CreateOptions(new NodeDefinitionId("tests.session.editor-kernel-owner")));
        var session = editor.Session;
        var host = session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(session);

        Assert.NotNull(host);
        Assert.IsNotType<GraphEditorViewModel>(host);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_DescriptorSupport_DoesNotExposeCompatibilityEditor()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(new NodeDefinitionId("tests.session.descriptor-support.runtime")));
        var descriptorSupport = session.GetType()
            .GetField("_descriptorSupport", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(session);
        var compatibilityEditor = descriptorSupport!.GetType()
            .GetProperty("CompatibilityEditor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(descriptorSupport);

        Assert.Null(compatibilityEditor);
    }

    [Fact]
    public void AsterGraphEditorFactory_Create_DescriptorSupport_PreservesCompatibilityEditorForRetainedRoute()
    {
        var editor = AsterGraphEditorFactory.Create(CreateOptions(new NodeDefinitionId("tests.session.descriptor-support.retained")));
        var descriptorSupport = editor.Session.GetType()
            .GetField("_descriptorSupport", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(editor.Session);
        var compatibilityEditor = descriptorSupport!.GetType()
            .GetProperty("CompatibilityEditor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .GetValue(descriptorSupport);

        Assert.Same(editor, compatibilityEditor);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_CreateDocumentSnapshot_ReturnsDetachedSnapshot()
    {
        var definitionId = new NodeDefinitionId("tests.session.detached-snapshot");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateMutableDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var snapshot = session.Queries.CreateDocumentSnapshot();
        var leakedNodes = Assert.IsType<List<GraphNode>>(snapshot.Nodes);
        leakedNodes.Add(
            new GraphNode(
                "external-node-001",
                "External Node",
                "Tests",
                string.Empty,
                string.Empty,
                new GraphPoint(0, 0),
                new GraphSize(120, 80),
                [],
                [],
                "#FFFFFF",
                definitionId,
                []));

        var after = session.Queries.CreateDocumentSnapshot();

        Assert.DoesNotContain(after.Nodes, node => node.Id == "external-node-001");
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_DoesNotTrackExternalMutationsFromTheOriginalDocument()
    {
        var definitionId = new NodeDefinitionId("tests.session.external-mutation");
        var document = CreateMutableDocument(definitionId);
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = document,
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var sourceNodes = Assert.IsType<List<GraphNode>>(document.Nodes);
        sourceNodes.Add(
            new GraphNode(
                "source-owned-node-001",
                "Source Owned Node",
                "Tests",
                string.Empty,
                string.Empty,
                new GraphPoint(32, 48),
                new GraphSize(120, 80),
                [],
                [],
                "#FFFFFF",
                definitionId,
                []));

        var snapshot = session.Queries.CreateDocumentSnapshot();

        Assert.DoesNotContain(snapshot.Nodes, node => node.Id == "source-owned-node-001");
    }

    [Fact]
    public void IGraphEditorCommands_BeginConnection_DefaultShimMatchesStartConnectionBehavior()
    {
        var definitionId = new NodeDefinitionId("tests.session.begin-shim");
        IGraphEditorCommands commands = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId)).Commands;

        commands.BeginConnection(SourceNodeId, SourcePortId);

        var session = Assert.IsAssignableFrom<IGraphEditorSession>(commands);
        var pending = session.Queries.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);
    }

    [Fact]
    public void IGraphEditorCommands_DefinesHostFacingEditAndViewportActions()
    {
        var commandsType = typeof(IGraphEditorCommands);

        Assert.True(commandsType.IsPublic);
        Assert.True(commandsType.IsInterface);

        AssertMethod(commandsType, nameof(IGraphEditorCommands.Undo));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.Redo));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ClearSelection), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SetSelection), typeof(IReadOnlyList<string>), typeof(string), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.AddNode), typeof(NodeDefinitionId), typeof(GraphPoint?));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.DeleteSelection));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SetNodePositions), typeof(IReadOnlyList<NodePositionSnapshot>), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetSelectedNodeParameterValue), typeof(string), typeof(object));
        Assert.Equal(typeof(bool), commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetSelectedNodeParameterValue), [typeof(string), typeof(object)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.StartConnection), typeof(string), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CompleteConnection), typeof(string), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CompleteConnection), typeof(GraphConnectionTargetRef));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CancelPendingConnection));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.DeleteConnection), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryReconnectConnection), typeof(string), typeof(bool));
        Assert.Equal(typeof(bool), commandsType.GetMethod(nameof(IGraphEditorCommands.TryReconnectConnection), [typeof(string), typeof(bool)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TrySetConnectionNoteText), typeof(string), typeof(string), typeof(bool));
        Assert.Equal(typeof(bool), commandsType.GetMethod(nameof(IGraphEditorCommands.TrySetConnectionNoteText), [typeof(string), typeof(string), typeof(bool)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.BreakConnectionsForPort), typeof(string), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.PanBy), typeof(double), typeof(double));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ZoomAt), typeof(double), typeof(GraphPoint));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.UpdateViewportSize), typeof(double), typeof(double));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.ResetView), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.FitToViewport), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CenterViewOnNode), typeof(string));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.CenterViewAt), typeof(GraphPoint), typeof(bool));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryWrapSelectionToComposite), typeof(string), typeof(bool));
        Assert.Equal(typeof(string), commandsType.GetMethod(nameof(IGraphEditorCommands.TryWrapSelectionToComposite), [typeof(string), typeof(bool)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryEnterCompositeChildGraph), typeof(string), typeof(bool));
        Assert.Equal(typeof(bool), commandsType.GetMethod(nameof(IGraphEditorCommands.TryEnterCompositeChildGraph), [typeof(string), typeof(bool)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryReturnToParentGraphScope), typeof(bool));
        Assert.Equal(typeof(bool), commandsType.GetMethod(nameof(IGraphEditorCommands.TryReturnToParentGraphScope), [typeof(bool)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.SaveWorkspace));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.LoadWorkspace));
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryExecuteCommand), typeof(GraphEditorCommandInvocationSnapshot));
        Assert.Equal(typeof(bool), commandsType.GetMethod(nameof(IGraphEditorCommands.TryExecuteCommand), [typeof(GraphEditorCommandInvocationSnapshot)])!.ReturnType);
    }

    [Fact]
    public void IGraphEditorQueries_DefinesHostFacingSnapshotAndDiscoveryReads()
    {
        var queriesType = typeof(IGraphEditorQueries);

        Assert.True(queriesType.IsPublic);
        Assert.True(queriesType.IsInterface);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.CreateDocumentSnapshot));
        Assert.Equal(typeof(GraphDocument), queriesType.GetMethod(nameof(IGraphEditorQueries.CreateDocumentSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetSelectionSnapshot));
        Assert.Equal(typeof(GraphEditorSelectionSnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetSelectionSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetViewportSnapshot));
        Assert.Equal(typeof(GraphEditorViewportSnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetViewportSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCapabilitySnapshot));
        Assert.Equal(typeof(GraphEditorCapabilitySnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetCapabilitySnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetFeatureDescriptors));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorFeatureDescriptorSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetFeatureDescriptors))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetRegisteredNodeDefinitions));
        Assert.Equal(typeof(IReadOnlyList<INodeDefinition>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetRegisteredNodeDefinitions))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetSharedSelectionDefinition));
        Assert.Equal(typeof(INodeDefinition), queriesType.GetMethod(nameof(IGraphEditorQueries.GetSharedSelectionDefinition))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetSelectedNodeParameterSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorNodeParameterSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetSelectedNodeParameterSnapshots))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetScopeNavigationSnapshot));
        Assert.Equal(
            typeof(GraphEditorScopeNavigationSnapshot),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetScopeNavigationSnapshot))!.ReturnType);
        Assert.NotNull(typeof(GraphEditorScopeNavigationSnapshot).GetProperty(nameof(GraphEditorScopeNavigationSnapshot.CurrentScopeId)));
        Assert.NotNull(typeof(GraphEditorScopeNavigationSnapshot).GetProperty(nameof(GraphEditorScopeNavigationSnapshot.ParentScopeId)));
        Assert.NotNull(typeof(GraphEditorScopeNavigationSnapshot).GetProperty(nameof(GraphEditorScopeNavigationSnapshot.CanNavigateToParent)));
        Assert.NotNull(typeof(GraphEditorScopeNavigationSnapshot).GetProperty(nameof(GraphEditorScopeNavigationSnapshot.Breadcrumbs)));
        Assert.NotNull(typeof(GraphEditorScopeBreadcrumbSnapshot).GetProperty(nameof(GraphEditorScopeBreadcrumbSnapshot.ScopeId)));
        Assert.NotNull(typeof(GraphEditorScopeBreadcrumbSnapshot).GetProperty(nameof(GraphEditorScopeBreadcrumbSnapshot.Title)));

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCommandDescriptors));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorCommandDescriptorSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetCommandDescriptors))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.BuildContextMenuDescriptors), typeof(ContextMenuContext));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.BuildContextMenuDescriptors), [typeof(ContextMenuContext)])!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetNodePositions));
        Assert.Equal(typeof(IReadOnlyList<NodePositionSnapshot>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetNodePositions))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetPendingConnectionSnapshot));
        Assert.Equal(typeof(GraphEditorPendingConnectionSnapshot), queriesType.GetMethod(nameof(IGraphEditorQueries.GetPendingConnectionSnapshot))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompatibleConnectionTargets), typeof(string), typeof(string));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorCompatibleConnectionTargetSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleConnectionTargets))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompatiblePortTargets), typeof(string), typeof(string));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorCompatiblePortTargetSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompatiblePortTargets))!.ReturnType);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetCompatibleTargets), typeof(string), typeof(string));
#pragma warning disable CS0618
        Assert.Equal(typeof(IReadOnlyList<CompatiblePortTarget>), queriesType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets))!.ReturnType);
#pragma warning restore CS0618
    }

    [Fact]
    public void IGraphEditorEvents_ReusesExistingTypedEventArgs()
    {
        var eventsType = typeof(IGraphEditorEvents);

        Assert.True(eventsType.IsPublic);
        Assert.True(eventsType.IsInterface);

        AssertEvent(eventsType, nameof(IGraphEditorEvents.AutomationStarted), typeof(GraphEditorAutomationStartedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.AutomationProgress), typeof(GraphEditorAutomationProgressEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.AutomationCompleted), typeof(GraphEditorAutomationCompletedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.DocumentChanged), typeof(GraphEditorDocumentChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.SelectionChanged), typeof(GraphEditorSelectionChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.ViewportChanged), typeof(GraphEditorViewportChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.FragmentExported), typeof(GraphEditorFragmentEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.FragmentImported), typeof(GraphEditorFragmentEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.CommandExecuted), typeof(GraphEditorCommandExecutedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.PendingConnectionChanged), typeof(GraphEditorPendingConnectionChangedEventArgs));
        AssertEvent(eventsType, nameof(IGraphEditorEvents.RecoverableFailure), typeof(GraphEditorRecoverableFailureEventArgs));
    }

    [Fact]
    public void GraphEditorCompatiblePortTargetSnapshot_IsRuntimeSafeAndMvvmFree()
    {
        var snapshotType = typeof(GraphEditorCompatiblePortTargetSnapshot);

        Assert.True(snapshotType.IsPublic);
        Assert.DoesNotContain(
            snapshotType.GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => property.PropertyType == typeof(NodeViewModel) || property.PropertyType == typeof(PortViewModel));
    }

    [Fact]
    public void GraphEditorCompatibleConnectionTargetSnapshot_IsRuntimeSafeAndMvvmFree()
    {
        var snapshotType = typeof(GraphEditorCompatibleConnectionTargetSnapshot);

        Assert.True(snapshotType.IsPublic);
        Assert.DoesNotContain(
            snapshotType.GetProperties(BindingFlags.Public | BindingFlags.Instance),
            property => property.PropertyType == typeof(NodeViewModel) || property.PropertyType == typeof(PortViewModel));
    }

    [Fact]
    public void IGraphEditorSessionHost_RuntimeBoundary_NoLongerDefinesLegacyCompatibleTargetShim()
    {
        var hostType = typeof(IGraphEditorSessionHost);

        Assert.NotNull(hostType.GetMethod(nameof(IGraphEditorSessionHost.GetCompatibleConnectionTargets), [typeof(string), typeof(string)]));
        Assert.NotNull(hostType.GetMethod(nameof(IGraphEditorSessionHost.GetCompatiblePortTargets), [typeof(string), typeof(string)]));
        Assert.Null(hostType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets), [typeof(string), typeof(string)]));
    }

    [Fact]
    public void GraphEditorKernelCompatibilityQueries_RuntimeBoundary_NoLongerDefinesLegacyCompatibleTargetShim()
    {
        var compatibilityQueryType = typeof(AsterGraphEditorFactory).Assembly
            .GetType("AsterGraph.Editor.Kernel.Internal.GraphEditorKernelCompatibilityQueries");

        Assert.NotNull(compatibilityQueryType);
        Assert.NotNull(compatibilityQueryType!.GetMethod("GetCompatibleTargetStates"));
        Assert.NotNull(compatibilityQueryType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleConnectionTargets), [typeof(GraphDocument), typeof(string), typeof(string)]));
        Assert.NotNull(compatibilityQueryType.GetMethod(nameof(IGraphEditorQueries.GetCompatiblePortTargets), [typeof(GraphDocument), typeof(string), typeof(string)]));
        Assert.Null(compatibilityQueryType.GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets), [typeof(GraphDocument), typeof(string), typeof(string)]));
    }

    [Fact]
    public void IGraphEditorQueries_GetCompatibleTargets_IsMarkedAsCompatibilityOnlyShim()
    {
        var method = typeof(IGraphEditorQueries).GetMethod(nameof(IGraphEditorQueries.GetCompatibleTargets), [typeof(string), typeof(string)]);

        Assert.NotNull(method);
        var attribute = Assert.Single(
            method!.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
        var obsolete = Assert.IsType<ObsoleteAttribute>(attribute);
        Assert.Contains("canonical runtime queries", obsolete.Message, StringComparison.Ordinal);
        Assert.Contains("later minor releases may add stronger warnings", obsolete.Message, StringComparison.Ordinal);
        Assert.Contains("future major release may remove it", obsolete.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CompatiblePortTarget_IsMarkedAsCompatibilityOnlyShim()
    {
#pragma warning disable CS0618
        var attribute = Assert.Single(
            typeof(CompatiblePortTarget).GetCustomAttributes(typeof(ObsoleteAttribute), inherit: false),
            attribute => attribute is ObsoleteAttribute);
#pragma warning restore CS0618
        var obsolete = Assert.IsType<ObsoleteAttribute>(attribute);
        Assert.Contains("Retained compatibility shim", obsolete.Message, StringComparison.Ordinal);
        Assert.Contains("canonical runtime queries", obsolete.Message, StringComparison.Ordinal);
        Assert.Contains("future major release may remove it", obsolete.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void GraphEditorCompatiblePortTargetSnapshot_NormalizesBlankDisplayMetadata()
    {
        var snapshot = new GraphEditorCompatiblePortTargetSnapshot(
            "node-1",
            "",
            "port-1",
            " ",
            new PortTypeId("float"),
            null!,
            PortCompatibilityResult.Exact());

        Assert.Equal("node-1", snapshot.NodeTitle);
        Assert.Equal("port-1", snapshot.PortLabel);
        Assert.Equal(string.Empty, snapshot.PortAccentHex);
    }

    [Fact]
    public void GraphEditorSession_GetCompatibleTargets_ComposesCompatibilityShimFromCanonicalSnapshots()
    {
        var definitionId = new NodeDefinitionId("tests.session.compatibility-bridge");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        var snapshot = Assert.Single(session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId));
#pragma warning disable CS0618
        var compatibleTarget = Assert.Single(session.Queries.GetCompatibleTargets(SourceNodeId, SourcePortId));
#pragma warning restore CS0618

        Assert.Equal(snapshot.NodeId, compatibleTarget.Node.Id);
        Assert.Equal(snapshot.NodeTitle, compatibleTarget.Node.Title);
        Assert.Equal("Tests", compatibleTarget.Node.Category);
        Assert.Equal("Session target node.", compatibleTarget.Node.Description);
        Assert.Equal(snapshot.PortId, compatibleTarget.Port.Id);
        Assert.Equal(snapshot.PortLabel, compatibleTarget.Port.Label);
        Assert.Equal(snapshot.PortTypeId, compatibleTarget.Port.TypeId);
        Assert.Equal(snapshot.PortAccentHex, compatibleTarget.Port.AccentHex);
        Assert.Equal(snapshot.Compatibility, compatibleTarget.Compatibility);
    }

    [Fact]
    public void GraphEditorSession_GetCompatibleConnectionTargets_IncludeParameterEndpoints()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateParameterEndpointOptions());

        var compatibleTargets = session.Queries.GetCompatibleConnectionTargets(SourceNodeId, SourcePortId);
        var parameterTarget = Assert.Single(compatibleTargets, target =>
            target.TargetKind == GraphConnectionTargetKind.Parameter
            && target.NodeId == TargetNodeId
            && target.TargetId == TargetParameterKey);
        var portTarget = Assert.Single(compatibleTargets, target =>
            target.TargetKind == GraphConnectionTargetKind.Port
            && target.NodeId == TargetNodeId
            && target.TargetId == TargetPortId);

        Assert.Equal("Gain", parameterTarget.TargetLabel);
        Assert.Equal(new PortTypeId("float"), parameterTarget.TargetTypeId);
        Assert.Equal("#F3B36B", parameterTarget.TargetAccentHex);
        Assert.Equal("Input", portTarget.TargetLabel);
    }

    [Fact]
    public void GraphEditorSession_CompleteConnection_AcceptsTypedParameterTarget()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateParameterEndpointOptions());

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(new GraphConnectionTargetRef(TargetNodeId, TargetParameterKey, GraphConnectionTargetKind.Parameter));

        var connection = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections);
        Assert.Equal(TargetNodeId, connection.TargetNodeId);
        Assert.Equal(TargetParameterKey, connection.TargetPortId);
        Assert.Equal(GraphConnectionTargetKind.Parameter, connection.TargetKind);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_ExecutesCommandsQueriesAndEvents()
    {
        var definitionId = new NodeDefinitionId("tests.session.node");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        GraphEditorDocumentChangedEventArgs? documentChanged = null;
        GraphEditorViewportChangedEventArgs? viewportChanged = null;

        session.Events.DocumentChanged += (_, args) => documentChanged = args;
        session.Events.ViewportChanged += (_, args) => viewportChanged = args;

        var before = session.Queries.CreateDocumentSnapshot();

        session.Commands.AddNode(definitionId, new GraphPoint(420, 220));
        session.Commands.PanBy(12, 18);

        var after = session.Queries.CreateDocumentSnapshot();

        Assert.Equal(before.Nodes.Count + 1, after.Nodes.Count);
        Assert.NotNull(documentChanged);
        Assert.Equal(GraphEditorDocumentChangeKind.NodesAdded, documentChanged!.ChangeKind);
        Assert.NotNull(viewportChanged);
        Assert.Equal(122, viewportChanged!.PanX);
        Assert.Equal(114, viewportChanged.PanY);
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_NavigatesCompositeChildGraph_AndEditsWithinActiveScope()
    {
        var definitionId = new NodeDefinitionId("tests.session.scope-navigation");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateScopedNavigationDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var rootNavigation = session.Queries.GetScopeNavigationSnapshot();
        Assert.Equal("graph-root", rootNavigation.CurrentScopeId);
        Assert.Null(rootNavigation.ParentScopeId);
        Assert.False(rootNavigation.CanNavigateToParent);
        Assert.Equal(["graph-root"], rootNavigation.Breadcrumbs.Select(breadcrumb => breadcrumb.ScopeId));
        Assert.Equal(
            [CompositeNodeId, RootStandaloneNodeId],
            session.Queries.GetNodePositions().Select(position => position.NodeId).OrderBy(id => id, StringComparer.Ordinal));

        Assert.True(session.Commands.TryEnterCompositeChildGraph(CompositeNodeId, updateStatus: false));

        var childNavigation = session.Queries.GetScopeNavigationSnapshot();
        Assert.Equal(ChildGraphId, childNavigation.CurrentScopeId);
        Assert.Equal("graph-root", childNavigation.ParentScopeId);
        Assert.True(childNavigation.CanNavigateToParent);
        Assert.Equal(
            ["graph-root", ChildGraphId],
            childNavigation.Breadcrumbs.Select(breadcrumb => breadcrumb.ScopeId));
        Assert.Equal(
            [ChildSourceNodeId, ChildTargetNodeId],
            session.Queries.GetNodePositions().Select(position => position.NodeId).OrderBy(id => id, StringComparer.Ordinal));

        var compatibleTarget = Assert.Single(session.Queries.GetCompatiblePortTargets(ChildSourceNodeId, SourcePortId));
        Assert.Equal(ChildTargetNodeId, compatibleTarget.NodeId);

        session.Commands.AddNode(definitionId, new GraphPoint(360, 220));

        var document = session.Queries.CreateDocumentSnapshot();
        var rootScope = Assert.Single(document.GraphScopes, scope => scope.Id == "graph-root");
        var childScope = Assert.Single(document.GraphScopes, scope => scope.Id == ChildGraphId);
        Assert.Equal(
            [CompositeNodeId, RootStandaloneNodeId],
            rootScope.Nodes.Select(node => node.Id).OrderBy(id => id, StringComparer.Ordinal));
        Assert.Equal(3, childScope.Nodes.Count);

        Assert.True(session.Commands.TryReturnToParentGraphScope(updateStatus: false));
        Assert.Equal("graph-root", session.Queries.GetScopeNavigationSnapshot().CurrentScopeId);
        Assert.Equal(
            [CompositeNodeId, RootStandaloneNodeId],
            session.Queries.GetNodePositions().Select(position => position.NodeId).OrderBy(id => id, StringComparer.Ordinal));
    }

    [Fact]
    public void AsterGraphEditorFactory_CreateSession_SupportsRuntimeOnlySelectionConnectionAndViewportCommands()
    {
        var definitionId = new NodeDefinitionId("tests.session.runtime");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.UpdateViewportSize(1280, 720);
        session.Commands.SetSelection([SourceNodeId], SourceNodeId, updateStatus: false);
        session.Commands.SetNodePositions(
            [
                new NodePositionSnapshot(SourceNodeId, new GraphPoint(240, 180)),
                new NodePositionSnapshot(TargetNodeId, new GraphPoint(620, 220)),
            ],
            updateStatus: false);

        var selection = session.Queries.GetSelectionSnapshot();
        Assert.Equal([SourceNodeId], selection.SelectedNodeIds);
        Assert.Equal(SourceNodeId, selection.PrimarySelectedNodeId);

        var positions = session.Queries.GetNodePositions().ToDictionary(snapshot => snapshot.NodeId, StringComparer.Ordinal);
        Assert.Equal(new GraphPoint(240, 180), positions[SourceNodeId].Position);
        Assert.Equal(new GraphPoint(620, 220), positions[TargetNodeId].Position);

        var capabilities = session.Queries.GetCapabilitySnapshot();
        Assert.True(capabilities.CanSetSelection);
        Assert.True(capabilities.CanMoveNodes);
        Assert.True(capabilities.CanCreateConnections);
        Assert.True(capabilities.CanDeleteConnections);
        Assert.True(capabilities.CanBreakConnections);
        Assert.True(capabilities.CanUpdateViewport);
        Assert.True(capabilities.CanFitToViewport);
        Assert.True(capabilities.CanCenterViewport);

        var compatibleTargets = session.Queries.GetCompatiblePortTargets(SourceNodeId, SourcePortId);
        var target = Assert.Single(compatibleTargets);
        Assert.Equal(TargetNodeId, target.NodeId);
        Assert.Equal(TargetPortId, target.PortId);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);

        var pending = session.Queries.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);

        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        session.Commands.FitToViewport(updateStatus: false);
        session.Commands.CenterViewOnNode(TargetNodeId);
        session.Commands.CenterViewAt(new GraphPoint(430, 260), updateStatus: false);

        var after = session.Queries.CreateDocumentSnapshot();
        Assert.Single(after.Connections);
        Assert.False(session.Queries.GetPendingConnectionSnapshot().HasPendingConnection);
    }

    [Fact]
    public void GraphEditorViewModel_Session_ExposesSharedRuntimeSurface()
    {
        var definitionId = new NodeDefinitionId("tests.session.compat");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var session = editor.Session;

        Assert.Same(session, editor.Session);

        editor.SelectSingleNode(editor.Nodes[0], updateStatus: false);
        Assert.Single(editor.SelectedNodes);

        session.Commands.ClearSelection();

        Assert.Empty(editor.SelectedNodes);
        var editorSnapshot = editor.CreateDocumentSnapshot();
        var sessionSnapshot = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(editorSnapshot.Title, sessionSnapshot.Title);
        Assert.Equal(editorSnapshot.Description, sessionSnapshot.Description);
        Assert.Equal(editorSnapshot.Nodes.Count, sessionSnapshot.Nodes.Count);
        Assert.Equal(editorSnapshot.Connections.Count, sessionSnapshot.Connections.Count);
    }

    [Fact]
    public void GraphEditorViewModel_Session_PublishesPendingConnectionChangesForDirectEditorOperations()
    {
        var definitionId = new NodeDefinitionId("tests.session.pending-event");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var pendingSnapshots = new List<GraphEditorPendingConnectionSnapshot>();

        editor.Session.Events.PendingConnectionChanged += (_, args) => pendingSnapshots.Add(args.PendingConnection);

        editor.StartConnection(SourceNodeId, SourcePortId);
        editor.CancelPendingConnection();

        Assert.Equal(2, pendingSnapshots.Count);
        Assert.True(pendingSnapshots[0].HasPendingConnection);
        Assert.Equal(SourceNodeId, pendingSnapshots[0].SourceNodeId);
        Assert.Equal(SourcePortId, pendingSnapshots[0].SourcePortId);
        Assert.False(pendingSnapshots[1].HasPendingConnection);
    }

    [Fact]
    public void RuntimeSession_CaptureInspectionSnapshot_UsesNormalizedPendingConnectionState()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspection-pending");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.StartConnection(SourceNodeId, SourcePortId);

        var snapshot = session.Diagnostics.CaptureInspectionSnapshot();
        var pending = session.Queries.GetPendingConnectionSnapshot();

        Assert.Equal(pending.HasPendingConnection, snapshot.PendingConnection.HasPendingConnection);
        Assert.Equal(pending.SourceNodeId, snapshot.PendingConnection.SourceNodeId);
        Assert.Equal(pending.SourcePortId, snapshot.PendingConnection.SourcePortId);
    }

    [Fact]
    public void RuntimeSession_FeatureDescriptors_ExposeExplicitDiscoveryWithoutFacadeInspection()
    {
        using var activitySource = new System.Diagnostics.ActivitySource("tests.session.features");
        using var loggerFactory = new NoOpLoggerFactory();
        var diagnosticsSink = new RecordingDiagnosticsSink();
        var fragmentWorkspace = new RecordingFragmentWorkspaceService("fragment://tests.session.features");
        var fragmentLibrary = new RecordingFragmentLibraryService("library://tests.session.features");
        var serializer = new RecordingClipboardPayloadSerializer();
        var definitionId = new NodeDefinitionId("tests.session.features");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
            FragmentWorkspaceService = fragmentWorkspace,
            FragmentLibraryService = fragmentLibrary,
            ClipboardPayloadSerializer = serializer,
            ContextMenuAugmentor = new SessionFeatureAugmentor(),
            NodePresentationProvider = new SessionFeaturePresentationProvider(),
            LocalizationProvider = new SessionFeatureLocalizationProvider(),
            DiagnosticsSink = diagnosticsSink,
            Instrumentation = new GraphEditorInstrumentationOptions(loggerFactory, activitySource),
        });

        var descriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.True(descriptors["capability.selection.set"].IsAvailable);
        Assert.True(descriptors["capability.nodes.move"].IsAvailable);
        Assert.True(descriptors["surface.automation.runner"].IsAvailable);
        Assert.True(descriptors["event.automation.started"].IsAvailable);
        Assert.True(descriptors["event.automation.progress"].IsAvailable);
        Assert.True(descriptors["event.automation.completed"].IsAvailable);
        Assert.True(descriptors["service.workspace"].IsAvailable);
        Assert.True(descriptors["service.diagnostics"].IsAvailable);
        Assert.True(descriptors["service.fragment-workspace"].IsAvailable);
        Assert.True(descriptors["service.fragment-library"].IsAvailable);
        Assert.True(descriptors["service.clipboard-payload-serializer"].IsAvailable);
        Assert.True(descriptors["query.compatible-port-target-snapshot"].IsAvailable);
        Assert.True(descriptors["query.compatible-target-mvvm-shim"].IsAvailable);
        Assert.True(descriptors["integration.plugin-loader"].IsAvailable);
        Assert.True(descriptors["integration.context-menu-augmentor"].IsAvailable);
        Assert.True(descriptors["integration.node-presentation-provider"].IsAvailable);
        Assert.True(descriptors["integration.localization-provider"].IsAvailable);
        Assert.True(descriptors["integration.diagnostics-sink"].IsAvailable);
        Assert.True(descriptors["integration.instrumentation.logger"].IsAvailable);
        Assert.True(descriptors["integration.instrumentation.activity-source"].IsAvailable);
    }

    [Fact]
    public void GraphEditorSession_ViewModelOverload_PreservesRetainedDescriptorSupportAndStockMenuDescriptors()
    {
        var definitionId = new NodeDefinitionId("tests.session.viewmodel-overload");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId) with
        {
            WorkspaceService = new EmptyWorkspaceService(),
            FragmentWorkspaceService = new RecordingFragmentWorkspaceService("fragment://tests.session.viewmodel-overload"),
            FragmentLibraryService = new RecordingFragmentLibraryService("library://tests.session.viewmodel-overload"),
            ClipboardPayloadSerializer = new RecordingClipboardPayloadSerializer(),
            ContextMenuAugmentor = new SessionFeatureAugmentor(),
            NodePresentationProvider = new SessionFeaturePresentationProvider(),
            LocalizationProvider = new SessionFeatureLocalizationProvider(),
        });
        var overloadSession = new GraphEditorSession(editor);
        var menuContext = new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90));

        var retainedFeatureDescriptors = editor.Session.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
        var overloadFeatureDescriptors = overloadSession.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
        var retainedMenuSignature = RenderMenuSignature(editor.Session.Queries.BuildContextMenuDescriptors(menuContext));
        var overloadMenuSignature = RenderMenuSignature(overloadSession.Queries.BuildContextMenuDescriptors(menuContext));

        Assert.Equal(retainedFeatureDescriptors, overloadFeatureDescriptors);
        Assert.Equal(retainedMenuSignature, overloadMenuSignature);
    }

    [Fact]
    public void RuntimeSession_CaptureInspectionSnapshot_IncludesFeatureDescriptors()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspection-features");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        var inspection = session.Diagnostics.CaptureInspectionSnapshot();
        var queryDescriptors = session.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
        var inspectionDescriptors = inspection.FeatureDescriptors
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(queryDescriptors, inspectionDescriptors);
    }

    [Fact]
    public void RuntimeSession_CommandAndMenuDescriptors_UseStableDataOnlyContracts()
    {
        var definitionId = new NodeDefinitionId("tests.session.menu-descriptors");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            WorkspaceService = new EmptyWorkspaceService(),
        });
        session.Commands.UpdateViewportSize(1280, 720);

        var commandDescriptors = session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var canvasMenu = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90)));
        var addNode = Assert.Single(canvasMenu, item => item.Id == "canvas-add-node");
        var addNodeGroup = Assert.Single(addNode.Children);
        var addNodeItem = Assert.Single(addNodeGroup.Children);
        var fitView = Assert.Single(canvasMenu, item => item.Id == "canvas-fit-view");
        var loadItem = Assert.Single(canvasMenu, item => item.Id == "canvas-load");

        Assert.True(commandDescriptors["nodes.add"].IsEnabled);
        Assert.True(commandDescriptors["viewport.fit"].IsEnabled);
        Assert.False(commandDescriptors["workspace.load"].IsEnabled);

        Assert.NotNull(addNodeItem.Command);
        Assert.Equal("nodes.add", addNodeItem.Command!.CommandId);
        Assert.Contains(addNodeItem.Command.Arguments, argument => argument.Name == "definitionId");
        Assert.Equal("viewport.fit", fitView.Command!.CommandId);
        Assert.Equal("workspace.load", loadItem.Command!.CommandId);
        Assert.False(loadItem.IsEnabled);
    }

    [Fact]
    public void RuntimeSession_SelectionContextMenuDescriptors_ExposeCreateGroupAndWrapCompositeActions()
    {
        var definitionId = new NodeDefinitionId("tests.session.selection-group-menu");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        session.Commands.SetSelection([SourceNodeId, TargetNodeId], SourceNodeId, updateStatus: false);

        var selectionMenu = session.Queries.BuildContextMenuDescriptors(
            new ContextMenuContext(
                ContextMenuTargetKind.Selection,
                new GraphPoint(160, 90),
                selectedNodeId: SourceNodeId,
                selectedNodeIds: [SourceNodeId, TargetNodeId]));

        var createGroup = Assert.Single(selectionMenu, item => item.Id == "selection-create-group");
        var wrapComposite = Assert.Single(selectionMenu, item => item.Id == "selection-wrap-composite");
        Assert.True(createGroup.IsEnabled);
        Assert.Equal("groups.create", createGroup.Command!.CommandId);
        Assert.Contains(createGroup.Command.Arguments, argument => argument.Name == "title" && argument.Value == "Group");
        Assert.True(wrapComposite.IsEnabled);
        Assert.Equal("composites.wrap-selection", wrapComposite.Command!.CommandId);
    }

    [Fact]
    public void RuntimeSession_TryWrapSelectionToComposite_CreatesCompositeShellAndSelectsIt()
    {
        var definitionId = new NodeDefinitionId("tests.session.wrap-selection");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        session.Commands.SetSelection([SourceNodeId, TargetNodeId], TargetNodeId, updateStatus: false);

        var compositeNodeId = session.Commands.TryWrapSelectionToComposite("Composite Cluster", updateStatus: false);
        var compositeSnapshot = Assert.Single(session.Queries.GetCompositeNodeSnapshots());
        var selection = session.Queries.GetSelectionSnapshot();

        Assert.False(string.IsNullOrWhiteSpace(compositeNodeId));
        Assert.Equal(compositeNodeId, compositeSnapshot.NodeId);
        Assert.Equal([compositeNodeId], selection.SelectedNodeIds);
        Assert.Equal(compositeNodeId, selection.PrimarySelectedNodeId);
    }

    [Fact]
    public void RuntimeSession_CompositeContextMenuDescriptors_ExposeEnterAndBoundaryAuthoringActions()
    {
        var definitionId = new NodeDefinitionId("tests.session.composite-menus");
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateScopedNavigationDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        });

        var compositeNodeContext = new ContextMenuContext(
            ContextMenuTargetKind.Node,
            new GraphPoint(160, 90),
            selectedNodeId: CompositeNodeId,
            selectedNodeIds: [CompositeNodeId],
            clickedNodeId: CompositeNodeId);
        var nodeMenu = session.Queries.BuildContextMenuDescriptors(compositeNodeContext);
        var enterScope = Assert.Single(nodeMenu, item => item.Id == "node-enter-composite-scope");

        Assert.True(enterScope.IsEnabled);
        Assert.Equal("scopes.enter", enterScope.Command!.CommandId);
        Assert.Contains(enterScope.Command.Arguments, argument => argument.Name == "compositeNodeId" && argument.Value == CompositeNodeId);

        Assert.True(session.Commands.TryEnterCompositeChildGraph(CompositeNodeId, updateStatus: false));

        var childPortContext = new ContextMenuContext(
            ContextMenuTargetKind.Port,
            new GraphPoint(120, 80),
            selectedNodeId: ChildSourceNodeId,
            selectedNodeIds: [ChildSourceNodeId],
            clickedPortNodeId: ChildSourceNodeId,
            clickedPortId: SourcePortId);
        var childPortMenu = session.Queries.BuildContextMenuDescriptors(childPortContext);
        var exposeCompositePort = Assert.Single(childPortMenu, item => item.Id == "port-expose-composite-port");

        Assert.True(exposeCompositePort.IsEnabled);
        Assert.Equal("composites.expose-port", exposeCompositePort.Command!.CommandId);
        Assert.Contains(exposeCompositePort.Command.Arguments, argument => argument.Name == "compositeNodeId" && argument.Value == CompositeNodeId);
        Assert.Contains(exposeCompositePort.Command.Arguments, argument => argument.Name == "childNodeId" && argument.Value == ChildSourceNodeId);
        Assert.Contains(exposeCompositePort.Command.Arguments, argument => argument.Name == "childPortId" && argument.Value == SourcePortId);

        var boundaryPortId = session.Commands.TryExposeCompositePort(
            CompositeNodeId,
            ChildSourceNodeId,
            SourcePortId,
            "Composite Output",
            updateStatus: false);
        Assert.False(string.IsNullOrWhiteSpace(boundaryPortId));
        Assert.True(session.Commands.TryReturnToParentGraphScope(updateStatus: false));

        var boundaryPortContext = new ContextMenuContext(
            ContextMenuTargetKind.Port,
            new GraphPoint(120, 80),
            selectedNodeId: CompositeNodeId,
            selectedNodeIds: [CompositeNodeId],
            clickedPortNodeId: CompositeNodeId,
            clickedPortId: boundaryPortId);
        var boundaryPortMenu = session.Queries.BuildContextMenuDescriptors(boundaryPortContext);
        var unexposeCompositePort = Assert.Single(boundaryPortMenu, item => item.Id == "port-unexpose-composite-port");

        Assert.True(unexposeCompositePort.IsEnabled);
        Assert.Equal("composites.unexpose-port", unexposeCompositePort.Command!.CommandId);
        Assert.Contains(unexposeCompositePort.Command.Arguments, argument => argument.Name == "compositeNodeId" && argument.Value == CompositeNodeId);
        Assert.Contains(unexposeCompositePort.Command.Arguments, argument => argument.Name == "boundaryPortId" && argument.Value == boundaryPortId);
    }

    [Fact]
    public void RuntimeSession_TryReconnectConnection_StartsPendingConnectionFromOriginalSource()
    {
        var definitionId = new NodeDefinitionId("tests.session.reconnect");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;

        Assert.True(session.Commands.TryReconnectConnection(connectionId, updateStatus: false));
        Assert.Empty(session.Queries.CreateDocumentSnapshot().Connections);

        var pending = session.Queries.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);
    }

    [Fact]
    public void RuntimeSession_TryExecuteCommand_AcceptsEdgeWorkflowInvocationSnapshots()
    {
        var definitionId = new NodeDefinitionId("tests.session.edge-command-routing");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;

        Assert.True(session.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                "connections.note.set",
                [
                    new GraphEditorCommandArgumentSnapshot("connectionId", connectionId),
                    new GraphEditorCommandArgumentSnapshot("text", "Preview branch"),
                    new GraphEditorCommandArgumentSnapshot("updateStatus", "false"),
                ])));

        Assert.Equal(
            "Preview branch",
            Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Presentation?.NoteText);

        Assert.True(session.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                "connections.reconnect",
                [
                    new GraphEditorCommandArgumentSnapshot("connectionId", connectionId),
                    new GraphEditorCommandArgumentSnapshot("updateStatus", "false"),
                ])));

        Assert.Empty(session.Queries.CreateDocumentSnapshot().Connections);
        var pending = session.Queries.GetPendingConnectionSnapshot();
        Assert.True(pending.HasPendingConnection);
        Assert.Equal(SourceNodeId, pending.SourceNodeId);
        Assert.Equal(SourcePortId, pending.SourcePortId);
    }

    [Fact]
    public void RuntimeSession_ConnectionContextMenuDescriptors_ExposeReconnectAndConnectionNoteWorkflow()
    {
        var definitionId = new NodeDefinitionId("tests.session.edge-menu");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);
        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;
        Assert.True(session.Commands.TrySetConnectionNoteText(connectionId, "Preview branch", updateStatus: false));

        var connectionContext = new ContextMenuContext(
            ContextMenuTargetKind.Connection,
            new GraphPoint(160, 90),
            selectedConnectionId: connectionId,
            selectedConnectionIds: [connectionId],
            clickedConnectionId: connectionId);
        var menu = session.Queries.BuildContextMenuDescriptors(connectionContext);

        var noteHint = Assert.Single(menu, item => item.Id == "connection-note-hint");
        Assert.Equal("Double-click label to edit note", noteHint.Header);
        Assert.False(noteHint.IsEnabled);

        var clearNote = Assert.Single(menu, item => item.Id == "connection-clear-note");
        Assert.Equal("connections.note.set", clearNote.Command!.CommandId);
        Assert.Contains(clearNote.Command.Arguments, argument => argument.Name == "connectionId" && argument.Value == connectionId);
        Assert.Contains(clearNote.Command.Arguments, argument => argument.Name == "text" && argument.Value == string.Empty);

        var reconnect = Assert.Single(menu, item => item.Id == "connection-reconnect");
        Assert.Equal("connections.reconnect", reconnect.Command!.CommandId);
        Assert.Contains(reconnect.Command.Arguments, argument => argument.Name == "connectionId" && argument.Value == connectionId);
    }

    [Fact]
    public void RuntimeSession_StockContextMenuDescriptorSignatures_RemainStableAcrossTargets()
    {
        var definitionId = new NodeDefinitionId("tests.session.stock-menu-signatures");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId) with
        {
            WorkspaceService = new EmptyWorkspaceService(),
            FragmentWorkspaceService = new RecordingFragmentWorkspaceService("fragment://tests.session.stock-menu"),
        });

        session.Commands.UpdateViewportSize(1280, 720);
        session.Commands.SetSelection([SourceNodeId, TargetNodeId], SourceNodeId, updateStatus: false);

        var selectionContext = new ContextMenuContext(
            ContextMenuTargetKind.Selection,
            new GraphPoint(160, 90),
            selectedNodeId: SourceNodeId,
            selectedNodeIds: [SourceNodeId, TargetNodeId]);
        var nodeContext = new ContextMenuContext(
            ContextMenuTargetKind.Node,
            new GraphPoint(160, 90),
            selectedNodeId: SourceNodeId,
            selectedNodeIds: [SourceNodeId, TargetNodeId],
            clickedNodeId: SourceNodeId);
        var outputPortContext = new ContextMenuContext(
            ContextMenuTargetKind.Port,
            new GraphPoint(160, 90),
            selectedNodeId: SourceNodeId,
            selectedNodeIds: [SourceNodeId, TargetNodeId],
            clickedPortNodeId: SourceNodeId,
            clickedPortId: SourcePortId);

        var signature = string.Join(
            Environment.NewLine,
            [
                $"canvas:{RenderMenuSignature(session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90))))}",
                $"selection:{RenderMenuSignature(session.Queries.BuildContextMenuDescriptors(selectionContext))}",
                $"node:{RenderMenuSignature(session.Queries.BuildContextMenuDescriptors(nodeContext))}",
                $"port-output:{RenderMenuSignature(session.Queries.BuildContextMenuDescriptors(outputPortContext))}",
            ]);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;
        var inputPortContext = new ContextMenuContext(
            ContextMenuTargetKind.Port,
            new GraphPoint(160, 90),
            selectedNodeId: TargetNodeId,
            selectedNodeIds: [TargetNodeId],
            clickedPortNodeId: TargetNodeId,
            clickedPortId: TargetPortId);
        var connectionContext = new ContextMenuContext(
            ContextMenuTargetKind.Connection,
            new GraphPoint(160, 90),
            selectedConnectionId: connectionId,
            selectedConnectionIds: [connectionId],
            clickedConnectionId: connectionId);

        signature = string.Join(
            Environment.NewLine,
            [
                signature,
                $"port-input:{RenderMenuSignature(session.Queries.BuildContextMenuDescriptors(inputPortContext))}",
                $"connection:{RenderMenuSignature(session.Queries.BuildContextMenuDescriptors(connectionContext))}",
            ]);

        var expected = """
            canvas:canvas-add-node~Add Node~add~True~-~False~-~[add-category-Tests~Tests~-~True~-~False~-~[add-node-tests-session-stock-menu-signatures~Session Node~node~True~-~False~nodes.add(definitionId=tests.session.stock-menu-signatures,worldX=160,worldY=90)~[-]]]||canvas-sep-1~~-~False~-~True~-~[-]||canvas-fit-view~Fit View~fit~True~-~False~viewport.fit()~[-]||canvas-reset-view~Reset View~reset~True~-~False~viewport.reset()~[-]||canvas-sep-2~~-~False~-~True~-~[-]||canvas-save~Save Snapshot~save~True~-~False~workspace.save()~[-]||canvas-load~Load Snapshot~load~False~No saved snapshot yet. Save once to create one.~False~workspace.load()~[-]||canvas-import-fragment~Import Fragment~import~False~-~False~fragments.import()~[-]||canvas-cancel-pending~Cancel Pending Connection~cancel~False~-~False~connections.cancel()~[-]
            selection:selection-delete~Delete 2 Selected Nodes~delete~True~-~False~selection.delete()~[-]||selection-export~Export Fragment~export~False~-~False~fragments.export-selection()~[-]||selection-create-group~Create Group~group-create~True~-~False~groups.create(title=Group)~[-]||selection-wrap-composite~Wrap Selection To Composite~composite-wrap~True~-~False~composites.wrap-selection()~[-]||selection-sep-1~~-~False~-~True~-~[-]||selection-align~Align~align~True~-~False~-~[selection-align-left~Left~align-left~False~-~False~layout.align-left()~[-]>>selection-align-center~Center~align-center~False~-~False~layout.align-center()~[-]>>selection-align-right~Right~align-right~False~-~False~layout.align-right()~[-]>>selection-align-top~Top~align-top~False~-~False~layout.align-top()~[-]>>selection-align-middle~Middle~align-middle~False~-~False~layout.align-middle()~[-]>>selection-align-bottom~Bottom~align-bottom~False~-~False~layout.align-bottom()~[-]]||selection-distribute~Distribute~distribute~True~-~False~-~[selection-distribute-horizontal~Horizontally~distribute-horizontal~False~-~False~layout.distribute-horizontal()~[-]>>selection-distribute-vertical~Vertically~distribute-vertical~False~-~False~layout.distribute-vertical()~[-]]
            node:node-inspect~Inspect Source Node~inspect~False~-~False~nodes.inspect(nodeId=tests.session.source-001)~[-]||node-center~Center View Here~center~True~-~False~viewport.center-node(nodeId=tests.session.source-001)~[-]||node-sep-1~~-~False~-~True~-~[-]||node-delete~Delete Node~delete~False~-~False~nodes.delete-by-id(nodeId=tests.session.source-001)~[-]||node-duplicate~Duplicate Node~duplicate~False~-~False~nodes.duplicate(nodeId=tests.session.source-001)~[-]||node-disconnect~Disconnect~disconnect~True~-~False~-~[node-disconnect-in~Incoming~disconnect~True~-~False~connections.disconnect-incoming(nodeId=tests.session.source-001)~[-]>>node-disconnect-out~Outgoing~disconnect~True~-~False~connections.disconnect-outgoing(nodeId=tests.session.source-001)~[-]>>node-disconnect-all~All~disconnect~True~-~False~connections.disconnect-all(nodeId=tests.session.source-001)~[-]]||node-create-connection~Create Connection From~connect~True~-~False~-~[node-connect-tests.session.source-001-out~Output~-~True~-~False~-~[compatible-node-tests.session.target-001~Target Node~-~True~-~False~-~[compatible-port-tests.session.target-001-in~Input~connect~True~-~False~connections.connect(sourceNodeId=tests.session.source-001,sourcePortId=out,targetNodeId=tests.session.target-001,targetPortId=in)~[-]]]]
            port-output:port-start~Start Connection~connect~True~-~False~connections.start(sourceNodeId=tests.session.source-001,sourcePortId=out)~[-]||port-compatible-targets~Compatible Targets~compatible~True~-~False~-~[compatible-node-tests.session.target-001~Target Node~-~True~-~False~-~[compatible-port-tests.session.target-001-in~Input~connect~True~-~False~connections.connect(sourceNodeId=tests.session.source-001,sourcePortId=out,targetNodeId=tests.session.target-001,targetPortId=in)~[-]]]||port-sep-1~~-~False~-~True~-~[-]||port-info~Type: float~type~False~-~False~-~[-]
            port-input:port-break-connections~Break Connections~disconnect~True~-~False~connections.break-port(nodeId=tests.session.target-001,portId=in)~[-]||port-sep-2~~-~False~-~True~-~[-]||port-info~Type: float~type~False~-~False~-~[-]
            connection:connection-note-hint~Double-click label to add note~inspect~False~-~False~-~[-]||connection-sep-1~~-~False~-~True~-~[-]||connection-reconnect~Reconnect From Source~connect~True~-~False~connections.reconnect(connectionId=connection-001)~[-]||connection-disconnect~Disconnect Connection~disconnect~True~-~False~connections.disconnect(connectionId=connection-001)~[-]||connection-conversion~No implicit conversion~conversion~False~-~False~-~[-]
            """;

        Assert.Equal(expected.ReplaceLineEndings("\n"), signature.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void RuntimeSession_CanvasMenuDescriptors_UseLiveNodeCatalogDefinitionsAfterSessionCreation()
    {
        var initialDefinitionId = new NodeDefinitionId("tests.session.live-catalog.initial");
        var lateDefinitionId = new NodeDefinitionId("tests.session.live-catalog.late");
        var catalog = CreateCatalog(initialDefinitionId);
        var session = AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = CreateDocument(initialDefinitionId),
            NodeCatalog = catalog,
            CompatibilityService = new DefaultPortCompatibilityService(),
            WorkspaceService = new EmptyWorkspaceService(),
        });

        session.Commands.UpdateViewportSize(1280, 720);
        var initialMenu = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90)));
        var initialAddNodeGroup = Assert.Single(Assert.Single(initialMenu, item => item.Id == "canvas-add-node").Children);
        Assert.Single(initialAddNodeGroup.Children);

        catalog.RegisterDefinition(new NodeDefinition(
            lateDefinitionId,
            "Late Session Node",
            "Tests",
            "Runtime",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));

        var updatedMenu = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90)));
        var updatedAddNodeGroup = Assert.Single(Assert.Single(updatedMenu, item => item.Id == "canvas-add-node").Children);
        var lateItem = Assert.Single(updatedAddNodeGroup.Children, item => item.Id == "add-node-tests-session-live-catalog-late");

        Assert.Equal(2, updatedAddNodeGroup.Children.Count);
        Assert.Equal("Late Session Node", lateItem.Header);
        Assert.Equal("nodes.add", lateItem.Command!.CommandId);
        Assert.Contains(lateItem.Command.Arguments, argument => argument.Name == "definitionId" && argument.Value == lateDefinitionId.Value);
    }

    [Fact]
    public void RuntimeSession_TryExecuteCommand_AcceptsCanonicalInvocationSnapshot()
    {
        var definitionId = new NodeDefinitionId("tests.session.execute-descriptor");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        var executed = session.Commands.TryExecuteCommand(
            new GraphEditorCommandInvocationSnapshot(
                "nodes.add",
                [
                    new GraphEditorCommandArgumentSnapshot("definitionId", definitionId.Value),
                    new GraphEditorCommandArgumentSnapshot("worldX", "640"),
                    new GraphEditorCommandArgumentSnapshot("worldY", "220"),
                ]));

        Assert.True(executed);
        Assert.Equal(3, session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    [Fact]
    public void GraphEditorViewModel_InspectorConnectionSummaries_StayAlignedWithSelectionAndConnectionChanges()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspector-cache");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var session = editor.Session;

        editor.SelectSingleNode(editor.FindNode(SourceNodeId), updateStatus: false);
        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorDownstream);

        session.Commands.StartConnection(SourceNodeId, SourcePortId);
        session.Commands.CompleteConnection(TargetNodeId, TargetPortId);

        Assert.Equal("0 incoming  ·  1 outgoing", editor.InspectorConnections);
        Assert.Contains("Target Node", editor.InspectorDownstream, StringComparison.Ordinal);
        Assert.Contains("Input", editor.InspectorDownstream, StringComparison.Ordinal);

        editor.SelectSingleNode(editor.FindNode(TargetNodeId), updateStatus: false);
        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Contains("Output", editor.InspectorUpstream, StringComparison.Ordinal);

        var connectionId = Assert.Single(session.Queries.CreateDocumentSnapshot().Connections).Id;
        session.Commands.DeleteConnection(connectionId);

        Assert.Equal("0 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Equal("None", editor.InspectorUpstream);
    }

    [Fact]
    public void GraphEditorViewModel_InspectorConnectionSummaries_RemainAccurateAfterUndoRedoRestore()
    {
        var definitionId = new NodeDefinitionId("tests.session.inspector-restore");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));

        editor.ConnectPorts(SourceNodeId, SourcePortId, TargetNodeId, TargetPortId);
        editor.SelectSingleNode(editor.FindNode(TargetNodeId), updateStatus: false);

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);

        editor.Undo();
        editor.Redo();
        editor.SelectSingleNode(editor.FindNode(TargetNodeId), updateStatus: false);

        Assert.Equal("1 incoming  ·  0 outgoing", editor.InspectorConnections);
        Assert.Contains("Source Node", editor.InspectorUpstream, StringComparison.Ordinal);
        Assert.Single(editor.Connections);
    }

    [Fact]
    public void FactoryEditorSession_And_CreateSession_ExposeEquivalentSharedCanonicalSignatures()
    {
        var definitionId = new NodeDefinitionId("tests.session.migration-signature");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var runtimeSession = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));
        var retainedHost = editor.Session.GetType()
            .GetField("_host", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(editor.Session);
        var runtimeFields = runtimeSession.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        var retainedSignature = CaptureSessionSignature(editor.Session);
        var runtimeSignature = CaptureSessionSignature(runtimeSession);
        var retainedCommandIds = editor.Session.Queries.GetCommandDescriptors().Select(descriptor => descriptor.Id).ToHashSet(StringComparer.Ordinal);
        var runtimeCommandIds = runtimeSession.Queries.GetCommandDescriptors().Select(descriptor => descriptor.Id).ToHashSet(StringComparer.Ordinal);

        Assert.NotNull(retainedHost);
        Assert.IsNotType<GraphEditorViewModel>(retainedHost);
        Assert.DoesNotContain(runtimeFields, field => field.FieldType == typeof(GraphEditorViewModel));
        Assert.Equal(retainedSignature, runtimeSignature);
        Assert.True(runtimeCommandIds.IsSubsetOf(retainedCommandIds));
        Assert.Contains("nodes.duplicate", retainedCommandIds);
        Assert.DoesNotContain("nodes.duplicate", runtimeCommandIds);
    }

    [Fact]
    public void FactoryEditorSession_ExposesRetainedCommandMetadataThroughSharedDescriptorContract()
    {
        var definitionId = new NodeDefinitionId("tests.session.retained-command-metadata");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(definitionId));
        var runtimeSession = AsterGraphEditorFactory.CreateSession(CreateOptions(definitionId));

        var retainedCommands = editor.Session.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var runtimeCommands = runtimeSession.Queries.GetCommandDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        var duplicate = retainedCommands["nodes.duplicate"];
        Assert.Equal("Duplicate Node", duplicate.Title);
        Assert.Equal("nodes", duplicate.Group);
        Assert.Equal("duplicate", duplicate.IconKey);
        Assert.Null(duplicate.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Retained, duplicate.Source);
        Assert.True(duplicate.CanExecute);

        var retainedDisconnect = retainedCommands["connections.disconnect"];
        var runtimeDisconnect = runtimeCommands["connections.disconnect"];

        Assert.Equal("Disconnect Connection", retainedDisconnect.Title);
        Assert.Equal("connections", retainedDisconnect.Group);
        Assert.Equal("disconnect", retainedDisconnect.IconKey);
        Assert.Equal(GraphEditorCommandSourceKind.Retained, retainedDisconnect.Source);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, runtimeDisconnect.Source);

        var undo = runtimeCommands["history.undo"];
        Assert.Equal("Undo", undo.Title);
        Assert.Equal("history", undo.Group);
        Assert.Equal("undo", undo.IconKey);
        Assert.Equal("Ctrl+Z", undo.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Kernel, undo.Source);

        var copy = retainedCommands["clipboard.copy"];
        Assert.Equal("Copy Selection", copy.Title);
        Assert.Equal("clipboard", copy.Group);
        Assert.Equal("copy", copy.IconKey);
        Assert.Equal("Ctrl+C", copy.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Retained, copy.Source);

        var paste = retainedCommands["clipboard.paste"];
        Assert.Equal("Paste Selection", paste.Title);
        Assert.Equal("clipboard", paste.Group);
        Assert.Equal("paste", paste.IconKey);
        Assert.Equal("Ctrl+V", paste.DefaultShortcut);
        Assert.Equal(GraphEditorCommandSourceKind.Retained, paste.Source);
    }

    private static void AssertProperty(Type declaringType, string propertyName, Type propertyType)
    {
        var property = declaringType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property!.PropertyType);
        Assert.Null(property.SetMethod);
    }

    private static void AssertMethod(Type declaringType, string methodName, params Type[] parameterTypes)
    {
        var method = declaringType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, parameterTypes);

        Assert.NotNull(method);
    }

    private static void AssertEvent(Type declaringType, string eventName, Type eventArgsType)
    {
        var eventInfo = declaringType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(eventInfo);

        var handlerType = eventInfo!.EventHandlerType!;
        Assert.True(handlerType.IsGenericType);
        Assert.Equal(typeof(EventHandler<>), handlerType.GetGenericTypeDefinition());
        Assert.Equal(eventArgsType, handlerType.GetGenericArguments()[0]);
    }

    private static string RenderMenuSignature(IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> items)
        => string.Join("||", items.Select(RenderMenuItemSignature));

    private static string RenderMenuItemSignature(GraphEditorMenuItemDescriptorSnapshot item)
    {
        var command = item.Command is null
            ? "-"
            : $"{item.Command.CommandId}({string.Join(",", item.Command.Arguments.Select(argument => $"{argument.Name}={argument.Value}"))})";
        var children = item.Children.Count == 0
            ? "-"
            : string.Join(">>", item.Children.Select(RenderMenuItemSignature));

        return $"{item.Id}~{item.Header}~{item.IconKey ?? "-"}~{item.IsEnabled}~{item.DisabledReason ?? "-"}~{item.IsSeparator}~{command}~[{children}]";
    }

    private static SessionSignature CaptureSessionSignature(IGraphEditorSession session)
        => new(
            string.Join("|", session.Queries.GetFeatureDescriptors().Select(descriptor => $"{descriptor.Category}:{descriptor.Id}:{descriptor.IsAvailable}")),
            string.Join("|", session.Queries.GetCommandDescriptors()
                .Where(descriptor => GraphEditorTestCommandContracts.SharedCanonicalCommandIds.Contains(descriptor.Id, StringComparer.Ordinal))
                .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
                .Select(descriptor => $"{descriptor.Id}:{descriptor.IsEnabled}")),
            string.Join("|", session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(160, 90))).Select(descriptor => descriptor.Id)),
            session.Queries.GetCapabilitySnapshot());

    private static AsterGraphEditorOptions CreateOptions(NodeDefinitionId definitionId)
        => new()
        {
            Document = CreateDocument(definitionId),
            NodeCatalog = CreateCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };

    private static AsterGraphEditorOptions CreateParameterEndpointOptions()
    {
        var definitionId = new NodeDefinitionId("tests.session.parameter-endpoint");
        return new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Parameter Endpoint Graph",
                "Runtime parameter target coverage.",
                [
                    new GraphNode(
                        SourceNodeId,
                        "Source Node",
                        "Tests",
                        "Runtime",
                        "Session source node.",
                        new GraphPoint(120, 160),
                        new GraphSize(240, 160),
                        [],
                        [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                        "#6AD5C4",
                        definitionId),
                    new GraphNode(
                        TargetNodeId,
                        "Target Node",
                        "Tests",
                        "Runtime",
                        "Session target node.",
                        new GraphPoint(520, 180),
                        new GraphSize(240, 160),
                        [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                        [],
                        "#F3B36B",
                        definitionId),
                ],
                []),
            NodeCatalog = CreateParameterEndpointCatalog(definitionId),
            CompatibilityService = new DefaultPortCompatibilityService(),
        };
    }

    private sealed record SessionSignature(
        string FeatureDescriptorSignature,
        string CommandDescriptorSignature,
        string CanvasMenuSignature,
        GraphEditorCapabilitySnapshot CapabilitySnapshot);

    private static GraphDocument CreateDocument(NodeDefinitionId definitionId)
        => new(
            "Session Graph",
            "Runtime session regression coverage.",
            [
                new GraphNode(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Runtime",
                    "Session source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    definitionId),
                new GraphNode(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Runtime",
                    "Session target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                    [],
                    "#6AD5C4",
                    definitionId),
            ],
            []);

    private static GraphDocument CreateMutableDocument(NodeDefinitionId definitionId)
        => new(
            "Session Graph",
            "Runtime session regression coverage.",
            new List<GraphNode>
            {
                new(
                    SourceNodeId,
                    "Source Node",
                    "Tests",
                    "Runtime",
                    "Session source node.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    new List<GraphPort>(),
                    new List<GraphPort> { new(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float")) },
                    "#6AD5C4",
                    definitionId),
                new(
                    TargetNodeId,
                    "Target Node",
                    "Tests",
                    "Runtime",
                    "Session target node.",
                    new GraphPoint(520, 180),
                    new GraphSize(240, 160),
                    new List<GraphPort> { new(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float")) },
                    new List<GraphPort>(),
                    "#6AD5C4",
                    definitionId),
            },
            new List<GraphConnection>());

    private static GraphDocument CreateScopedNavigationDocument(NodeDefinitionId definitionId)
        => GraphDocument.CreateScoped(
            "Scoped Session Graph",
            "Runtime session scope-navigation coverage.",
            "graph-root",
            [
                new GraphScope(
                    "graph-root",
                    [
                        new GraphNode(
                            CompositeNodeId,
                            "Composite Node",
                            "Tests",
                            "Runtime",
                            "Composite shell for scope navigation tests.",
                            new GraphPoint(120, 160),
                            new GraphSize(260, 180),
                            [],
                            [],
                            "#A67CF5",
                            null,
                            [],
                            null,
                            new GraphCompositeNode(ChildGraphId, [], [])),
                        new GraphNode(
                            RootStandaloneNodeId,
                            "Root Standalone",
                            "Tests",
                            "Runtime",
                            "Root scope node that should remain visible only in the root graph.",
                            new GraphPoint(460, 180),
                            new GraphSize(240, 160),
                            [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                            [],
                            "#F3B36B",
                            definitionId),
                    ],
                    []),
                new GraphScope(
                    ChildGraphId,
                    [
                        new GraphNode(
                            ChildSourceNodeId,
                            "Child Source",
                            "Tests",
                            "Runtime",
                            "Child scope source node.",
                            new GraphPoint(80, 100),
                            new GraphSize(220, 150),
                            [],
                            [new GraphPort(SourcePortId, "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                            "#6AD5C4",
                            definitionId),
                        new GraphNode(
                            ChildTargetNodeId,
                            "Child Target",
                            "Tests",
                            "Runtime",
                            "Child scope target node.",
                            new GraphPoint(360, 140),
                            new GraphSize(220, 150),
                            [new GraphPort(TargetPortId, "Input", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
                            [],
                            "#F3B36B",
                            definitionId),
                    ],
                    []),
            ]);

    private static NodeCatalog CreateCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Session Node",
            "Tests",
            "Runtime",
            [new PortDefinition(TargetPortId, "Input", new PortTypeId("float"), "#F3B36B")],
            [new PortDefinition(SourcePortId, "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private static NodeCatalog CreateParameterEndpointCatalog(NodeDefinitionId definitionId)
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            definitionId,
            "Parameter Session Node",
            "Tests",
            "Runtime",
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

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed class NoOpLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
            => new NoOpLogger();

        public void Dispose()
        {
        }
    }

    private sealed class NoOpLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }

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

    private sealed class RecordingFragmentWorkspaceService(string fragmentPath) : IGraphFragmentWorkspaceService
    {
        public string FragmentPath { get; } = fragmentPath;

        public void Save(GraphSelectionFragment fragment, string? path = null)
        {
        }

        public GraphSelectionFragment Load(string? path = null)
            => throw new InvalidOperationException("No fragment snapshot.");

        public bool Exists(string? path = null)
            => false;

        public void Delete(string? path = null)
        {
        }
    }

    private sealed class RecordingFragmentLibraryService(string libraryPath) : IGraphFragmentLibraryService
    {
        public string LibraryPath { get; } = libraryPath;

        public IReadOnlyList<FragmentTemplateInfo> EnumerateTemplates()
            => [];

        public string SaveTemplate(GraphSelectionFragment fragment, string? name = null)
            => Path.Combine(LibraryPath, $"{name ?? "fragment"}.json");

        public GraphSelectionFragment LoadTemplate(string path)
            => throw new InvalidOperationException("No fragment template.");

        public void DeleteTemplate(string path)
        {
        }
    }

    private sealed class RecordingClipboardPayloadSerializer : IGraphClipboardPayloadSerializer
    {
        public string Serialize(GraphSelectionFragment fragment)
            => "serialized-fragment";

        public bool TryDeserialize(string? text, out GraphSelectionFragment? fragment)
        {
            fragment = null;
            return false;
        }
    }

    private sealed class SessionFeatureAugmentor : IGraphContextMenuAugmentor
    {
        public IReadOnlyList<MenuItemDescriptor> Augment(
            GraphEditorViewModel editor,
            ContextMenuContext context,
            IReadOnlyList<MenuItemDescriptor> stockItems)
            => stockItems;
    }

    private sealed class SessionFeaturePresentationProvider : INodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(NodePresentationContext context)
            => NodePresentationState.Empty;

        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => NodePresentationState.Empty;
    }

    private sealed class SessionFeatureLocalizationProvider : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => fallback;
    }
}
