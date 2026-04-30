using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorSessionFragmentContractsTests
{
    private static readonly NodeDefinitionId DefinitionId = new("tests.session.fragments");
    private const string FirstNodeId = "tests.session.fragments.node-001";
    private const string PresetSourceNodeId = "tests.session.fragments.source-001";
    private const string PresetTargetNodeId = "tests.session.fragments.target-001";
    private const string PresetGroupId = "group-001";

    [Fact]
    public void RuntimeContracts_ExposeFragmentStorageAndTemplateLibrarySurface()
    {
        var queriesType = typeof(IGraphEditorQueries);
        var commandsType = typeof(IGraphEditorCommands);

        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetFragmentStorageSnapshot));
        Assert.Equal(
            typeof(GraphEditorFragmentStorageSnapshot),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetFragmentStorageSnapshot))!.ReturnType);
        AssertMethod(queriesType, nameof(IGraphEditorQueries.GetFragmentTemplateSnapshots));
        Assert.Equal(
            typeof(IReadOnlyList<GraphEditorFragmentTemplateSnapshot>),
            queriesType.GetMethod(nameof(IGraphEditorQueries.GetFragmentTemplateSnapshots))!.ReturnType);
        Assert.NotNull(typeof(GraphEditorFragmentStorageSnapshot).GetProperty(nameof(GraphEditorFragmentStorageSnapshot.WorkspaceFragmentPath)));
        Assert.NotNull(typeof(GraphEditorFragmentStorageSnapshot).GetProperty(nameof(GraphEditorFragmentStorageSnapshot.TemplateLibraryPath)));
        Assert.NotNull(typeof(GraphEditorFragmentTemplateSnapshot).GetProperty(nameof(GraphEditorFragmentTemplateSnapshot.Path)));
        Assert.NotNull(typeof(GraphEditorFragmentTemplateSnapshot).GetProperty(nameof(GraphEditorFragmentTemplateSnapshot.Summary)));

        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryExportSelectionFragment), typeof(string));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryExportSelectionFragment), [typeof(string)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryImportFragment), typeof(string));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryImportFragment), [typeof(string)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryClearWorkspaceFragment), typeof(string));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryClearWorkspaceFragment), [typeof(string)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryExportSelectionAsTemplate), typeof(string));
        Assert.Equal(
            typeof(string),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryExportSelectionAsTemplate), [typeof(string)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryImportFragmentTemplate), typeof(string));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryImportFragmentTemplate), [typeof(string)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryApplyFragmentTemplatePreset), typeof(string));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryApplyFragmentTemplatePreset), [typeof(string)])!.ReturnType);
        AssertMethod(commandsType, nameof(IGraphEditorCommands.TryDeleteFragmentTemplate), typeof(string));
        Assert.Equal(
            typeof(bool),
            commandsType.GetMethod(nameof(IGraphEditorCommands.TryDeleteFragmentTemplate), [typeof(string)])!.ReturnType);
    }

    [Fact]
    public void SessionQueries_ExposeFragmentStorageAndTemplateMetadata()
    {
        var session = CreateSession();
        session.Commands.SetSelection([FirstNodeId], FirstNodeId, updateStatus: false);

        Assert.True(session.Commands.TryExportSelectionFragment());
        var templatePath = session.Commands.TryExportSelectionAsTemplate("Session Template");

        var storage = session.Queries.GetFragmentStorageSnapshot();
        var templates = session.Queries.GetFragmentTemplateSnapshots();
        var features = session.Queries.GetFeatureDescriptors().Select(feature => feature.Id).ToHashSet(StringComparer.Ordinal);

        Assert.True(storage.HasWorkspaceFragment);
        Assert.Equal(SessionHarness.FragmentPath, storage.WorkspaceFragmentPath);
        Assert.Equal(SessionHarness.LibraryPath, storage.TemplateLibraryPath);
        Assert.True(storage.IsTemplateLibraryEnabled);
        Assert.NotNull(storage.WorkspaceFragmentLastModified);
        Assert.True(storage.CanExportSelectionFragment);
        Assert.True(storage.CanImportFragment);
        Assert.True(storage.CanClearWorkspaceFragment);
        Assert.True(storage.CanExportSelectionAsTemplate);
        Assert.True(storage.CanImportFragmentTemplate);
        Assert.True(storage.CanDeleteFragmentTemplate);

        var template = Assert.Single(templates);
        Assert.StartsWith("Session Template", template.Name, StringComparison.Ordinal);
        Assert.Equal(templatePath, template.Path);
        Assert.Equal(1, template.NodeCount);
        Assert.Equal(0, template.ConnectionCount);
        Assert.Equal("1 nodes  ·  0 connections", template.Summary);

        Assert.Contains("query.fragment-storage-snapshot", features);
        Assert.Contains("query.fragment-template-snapshots", features);
    }

    [Fact]
    public void SessionCommands_RoundTripFragmentWorkspaceAndTemplateLibrary()
    {
        var session = CreateSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);
        session.Commands.SetSelection([FirstNodeId], FirstNodeId, updateStatus: false);

        Assert.True(session.Commands.TryExportSelectionFragment());
        var templatePath = session.Commands.TryExportSelectionAsTemplate("Roundtrip Template");

        session.Commands.DeleteSelection();
        Assert.True(session.Commands.TryImportFragment());
        var importedFragmentNode = Assert.Single(session.Queries.CreateDocumentSnapshot().Nodes);

        session.Commands.SetSelection([importedFragmentNode.Id], null, updateStatus: false);
        Assert.True(session.Commands.TryImportFragmentTemplate(templatePath));
        Assert.Equal(2, session.Queries.CreateDocumentSnapshot().Nodes.Count);

        Assert.True(session.Commands.TryDeleteFragmentTemplate(templatePath));
        Assert.True(session.Commands.TryClearWorkspaceFragment());
        Assert.Empty(session.Queries.GetFragmentTemplateSnapshots());
        Assert.False(session.Queries.GetFragmentStorageSnapshot().HasWorkspaceFragment);

        Assert.Contains("fragments.export-selection", commandIds);
        Assert.Contains("fragments.export-template", commandIds);
        Assert.Contains("fragments.import", commandIds);
        Assert.Contains("fragments.import-template", commandIds);
        Assert.Contains("fragments.delete-template", commandIds);
        Assert.Contains("fragments.clear-workspace", commandIds);
    }

    [Fact]
    public void SessionCommands_ApplyFragmentTemplatePreset_IsOneUndoableRemappedFragmentPaste()
    {
        var session = CreatePresetSession();
        var commandIds = new List<string>();
        session.Events.CommandExecuted += (_, args) => commandIds.Add(args.CommandId);
        session.Commands.SetSelection([PresetSourceNodeId, PresetTargetNodeId], PresetSourceNodeId, updateStatus: false);
        var templatePath = session.Commands.TryExportSelectionAsTemplate("Reusable Preset");

        Assert.True(session.Commands.TryApplyFragmentTemplatePreset(templatePath));

        var appliedDocument = session.Queries.CreateDocumentSnapshot();
        var appliedSelection = session.Queries.GetSelectionSnapshot();
        var pastedNodeIds = appliedSelection.SelectedNodeIds.ToHashSet(StringComparer.Ordinal);
        var pastedNodes = appliedDocument.Nodes.Where(node => pastedNodeIds.Contains(node.Id)).ToList();
        var originalConnection = Assert.Single(appliedDocument.Connections, connection => connection.Id == "connection-001");
        var pastedConnection = Assert.Single(appliedDocument.Connections, connection => connection.Id != originalConnection.Id);
        var pastedGroup = Assert.Single(appliedDocument.Groups ?? [], group => group.Id != PresetGroupId);

        Assert.Equal(4, appliedDocument.Nodes.Count);
        Assert.Equal(2, appliedDocument.Connections.Count);
        Assert.Equal(2, appliedDocument.Groups?.Count);
        Assert.Equal(2, pastedNodes.Count);
        Assert.DoesNotContain(PresetSourceNodeId, pastedNodeIds);
        Assert.DoesNotContain(PresetTargetNodeId, pastedNodeIds);
        Assert.Equal(pastedNodeIds.Order(StringComparer.Ordinal), pastedGroup.NodeIds.Order(StringComparer.Ordinal));
        Assert.All(pastedNodes, node => Assert.Equal(pastedGroup.Id, node.Surface?.GroupId));
        Assert.Contains(pastedNodeIds, nodeId => string.Equals(nodeId, pastedConnection.SourceNodeId, StringComparison.Ordinal));
        Assert.Contains(pastedNodeIds, nodeId => string.Equals(nodeId, pastedConnection.TargetNodeId, StringComparison.Ordinal));
        Assert.True(session.Queries.GetCapabilitySnapshot().CanUndo);

        session.Commands.Undo();

        var undoDocument = session.Queries.CreateDocumentSnapshot();
        Assert.Equal(2, undoDocument.Nodes.Count);
        Assert.Single(undoDocument.Connections);
        Assert.Single(undoDocument.Groups ?? []);
        Assert.DoesNotContain(undoDocument.Nodes, node => pastedNodeIds.Contains(node.Id));
        Assert.Contains("fragments.apply-template-preset", commandIds);
    }

    [Fact]
    public void SessionCommands_ApplyFragmentTemplatePreset_RoutesThroughCommandInvocation()
    {
        var session = CreatePresetSession();
        session.Commands.SetSelection([PresetSourceNodeId, PresetTargetNodeId], PresetSourceNodeId, updateStatus: false);
        var templatePath = session.Commands.TryExportSelectionAsTemplate("Routed Preset");

        Assert.True(session.Commands.TryExecuteCommand(new GraphEditorCommandInvocationSnapshot(
            "fragments.apply-template-preset",
            [new GraphEditorCommandArgumentSnapshot("path", templatePath)])));

        Assert.Equal(4, session.Queries.CreateDocumentSnapshot().Nodes.Count);
    }

    private static IGraphEditorSession CreateSession()
    {
        Directory.CreateDirectory(SessionHarness.RootPath);
        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Fragment Contract Graph",
                "Covers canonical fragment storage and template-library contracts.",
                [
                    new GraphNode(
                        FirstNodeId,
                        "Fragment Node",
                        "Tests",
                        "GraphEditorSession",
                        "Used by fragment contract tests.",
                        new GraphPoint(120, 180),
                        new GraphSize(240, 160),
                        [],
                        [],
                        "#6AD5C4",
                        DefinitionId),
                ],
                []),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = SessionHarness.RootPath,
        });
    }

    private static IGraphEditorSession CreatePresetSession()
    {
        Directory.CreateDirectory(SessionHarness.RootPath);
        return AsterGraphEditorFactory.CreateSession(new AsterGraphEditorOptions
        {
            Document = new GraphDocument(
                "Reusable Preset Graph",
                "Covers reusable preset application through fragment templates.",
                [
                    CreatePresetSourceNode(),
                    CreatePresetTargetNode(),
                ],
                [
                    new GraphConnection("connection-001", PresetSourceNodeId, "out", PresetTargetNodeId, "in", "Source to target", "#6AD5C4"),
                ],
                [
                    new GraphNodeGroup(PresetGroupId, "Preset Group", new GraphPoint(90, 140), new GraphSize(620, 260), [PresetSourceNodeId, PresetTargetNodeId]),
                ]),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            StorageRootPath = Path.Combine(
                Path.GetTempPath(),
                "astergraph-session-preset-application",
                Guid.NewGuid().ToString("N")),
        });
    }

    private static GraphNode CreatePresetSourceNode()
        => new(
            PresetSourceNodeId,
            "Preset Source",
            "Tests",
            "GraphEditorSession",
            "Source node for reusable preset tests.",
            new GraphPoint(120, 180),
            new GraphSize(240, 160),
            [],
            [new GraphPort("out", "Out", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
            "#6AD5C4",
            DefinitionId,
            Surface: new GraphNodeSurfaceState(GroupId: PresetGroupId));

    private static GraphNode CreatePresetTargetNode()
        => new(
            PresetTargetNodeId,
            "Preset Target",
            "Tests",
            "GraphEditorSession",
            "Target node for reusable preset tests.",
            new GraphPoint(440, 180),
            new GraphSize(240, 160),
            [new GraphPort("in", "In", PortDirection.Input, "float", "#F3B36B", new PortTypeId("float"))],
            [],
            "#F3B36B",
            DefinitionId,
            Surface: new GraphNodeSurfaceState(GroupId: PresetGroupId));

    private static INodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(
            new NodeDefinition(
                DefinitionId,
                "Fragment Node",
                "Tests",
                "Session fragment contract node.",
                [],
                []));
        return catalog;
    }

    private static void AssertMethod(Type type, string methodName, params Type[] parameterTypes)
        => Assert.NotNull(type.GetMethod(methodName, parameterTypes));

    private static class SessionHarness
    {
        public static readonly string RootPath = Path.Combine(
            Path.GetTempPath(),
            "astergraph-session-fragment-contracts",
            Guid.NewGuid().ToString("N"));

        public static readonly string FragmentPath = Path.Combine(RootPath, "selection-fragment.json");

        public static readonly string LibraryPath = Path.Combine(RootPath, "fragments");
    }
}
