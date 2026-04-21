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
