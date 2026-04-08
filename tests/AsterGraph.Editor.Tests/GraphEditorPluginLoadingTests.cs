using AsterGraph.Abstractions.Catalog;
using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Localization;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Presentation;
using AsterGraph.Editor.ViewModels;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginLoadingTests
{
    [Fact]
    public void CreateSession_WithAssemblyPluginRegistration_PublishesSuccessDiagnosticAndAppliesRuntimeContributions()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(diagnostics, GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath())));
        var descriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var canvasMenu = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var addNode = Assert.Single(canvasMenu, item => item.Id == "canvas-add-node");
        var pluginLoadDiagnostic = Assert.Single(diagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.succeeded");

        session.Commands.AddNode(new NodeDefinitionId("tests.sample-plugin.node"));
        var document = session.Queries.CreateDocumentSnapshot();

        Assert.True(descriptors["integration.plugin-loader"].IsAvailable);
        Assert.True(descriptors["query.plugin-load-snapshots"].IsAvailable);
        Assert.Contains(session.Diagnostics.GetRecentDiagnostics(), diagnostic => diagnostic.Code == "plugin.load.succeeded");
        Assert.Equal(GraphEditorDiagnosticSeverity.Info, pluginLoadDiagnostic.Severity);
        Assert.Equal("Plugin Add Node", addNode.Header);
        Assert.Contains(canvasMenu, item => item.Id == "plugin-sample-menu");
        Assert.Contains(document.Nodes, node => node.DefinitionId is { Value: "tests.sample-plugin.node" });
    }

    [Fact]
    public void Create_And_CreateSession_ExposeEquivalentPluginLoaderReadinessForAssemblyRegistrations()
    {
        var editorDiagnostics = new RecordingDiagnosticsSink();
        var runtimeDiagnostics = new RecordingDiagnosticsSink();
        var registration = GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath());
        var editor = AsterGraphEditorFactory.Create(CreateOptions(editorDiagnostics, registration));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(runtimeDiagnostics, registration));

        var retainedDescriptors = editor.Session.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();
        var runtimeDescriptors = session.Queries.GetFeatureDescriptors()
            .OrderBy(descriptor => descriptor.Id, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(runtimeDescriptors, retainedDescriptors);
        Assert.True(retainedDescriptors.Single(descriptor => descriptor.Id == "integration.plugin-loader").IsAvailable);
        Assert.Single(editorDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.succeeded");
        Assert.Single(runtimeDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.succeeded");
    }

    [Fact]
    public void CreateSession_WithMissingAssemblyPluginRegistration_SurfacesRecoverableDiagnosticWithoutThrowing()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var missingAssemblyPath = Path.Combine(Path.GetTempPath(), "astergraph-plugin-tests", Guid.NewGuid().ToString("N"), "missing-plugin.dll");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(diagnostics, GraphEditorPluginRegistration.FromAssemblyPath(missingAssemblyPath)));
        var descriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var failure = Assert.Single(diagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.failed");

        Assert.True(descriptors["integration.plugin-loader"].IsAvailable);
        Assert.Contains(session.Diagnostics.GetRecentDiagnostics(), diagnostic => diagnostic.Code == "plugin.load.failed");
        Assert.Equal(GraphEditorDiagnosticSeverity.Error, failure.Severity);
        Assert.Contains("missing-plugin.dll", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_And_CreateSession_WithDirectPluginRegistration_SurfaceEquivalentSnapshotsAndContributions()
    {
        var editorDiagnostics = new RecordingDiagnosticsSink();
        var runtimeDiagnostics = new RecordingDiagnosticsSink();
        var registration = GraphEditorPluginRegistration.FromPlugin(new DirectPlugin());
        var editor = AsterGraphEditorFactory.Create(CreateOptions(editorDiagnostics, registration));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(runtimeDiagnostics, registration));
        editor.RefreshNodePresentations();

        var retainedSnapshots = editor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = session.Queries.GetPluginLoadSnapshots();
        var retainedCanvasMenu = editor.Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var runtimeCanvasMenu = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == "source-node");

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        Assert.Equal(GraphEditorPluginLoadSourceKind.Direct, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, snapshot.Status);
        Assert.Equal("tests.direct-plugin", snapshot.Descriptor?.Id);
        Assert.Equal(1, snapshot.Contributions.NodeDefinitionProviderCount);
        Assert.Equal(1, snapshot.Contributions.ContextMenuAugmentorCount);
        Assert.Equal(1, snapshot.Contributions.NodePresentationProviderCount);
        Assert.Equal(1, snapshot.Contributions.LocalizationProviderCount);
        Assert.Equal("Direct Add Node", Assert.Single(runtimeCanvasMenu, item => item.Id == "canvas-add-node").Header);
        Assert.Equal("Direct Add Node", Assert.Single(retainedCanvasMenu, item => item.Id == "canvas-add-node").Header);
        Assert.Contains(runtimeCanvasMenu, item => item.Id == "direct-plugin-menu");
        Assert.Contains(retainedCanvasMenu, item => item.Id == "direct-plugin-menu");
        Assert.Contains(sourceNode.Presentation.TopRightBadges, badge => badge.Text == "Direct");
    }

    [Fact]
    public void Create_And_CreateSession_SurfaceEquivalentPluginLoadFailureDiagnosticsForBadAssemblyRegistrations()
    {
        var missingAssemblyPath = Path.Combine(Path.GetTempPath(), "astergraph-plugin-tests", Guid.NewGuid().ToString("N"), "missing-plugin.dll");
        var registration = GraphEditorPluginRegistration.FromAssemblyPath(missingAssemblyPath);
        var editorDiagnostics = new RecordingDiagnosticsSink();
        var runtimeDiagnostics = new RecordingDiagnosticsSink();

        var editor = AsterGraphEditorFactory.Create(CreateOptions(editorDiagnostics, registration));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(runtimeDiagnostics, registration));

        var retainedFailure = Assert.Single(editorDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.failed");
        var runtimeFailure = Assert.Single(runtimeDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.failed");

        Assert.Contains(editor.Session.Diagnostics.GetRecentDiagnostics(), diagnostic => diagnostic.Code == "plugin.load.failed");
        Assert.Contains(session.Diagnostics.GetRecentDiagnostics(), diagnostic => diagnostic.Code == "plugin.load.failed");
        Assert.Equal(retainedFailure.Code, runtimeFailure.Code);
        Assert.Equal(retainedFailure.Operation, runtimeFailure.Operation);
        Assert.Equal(retainedFailure.Message, runtimeFailure.Message);
        Assert.Equal(retainedFailure.Severity, runtimeFailure.Severity);
    }

    [Fact]
    public void CreateSession_WithMultiplePluginRegistrations_LoadsDirectAndAssemblyPluginsThroughCanonicalPath()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            diagnostics,
            GraphEditorPluginRegistration.FromPlugin(new DirectPlugin()),
            GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath())));

        Assert.NotNull(session);
        Assert.Equal(2, diagnostics.Diagnostics.Count(diagnostic => diagnostic.Code == "plugin.load.succeeded"));
    }

    [Fact]
    public void Create_WithAssemblyPluginRegistration_AppliesPluginPresentationAndHostOverridesWin()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var registration = GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath());
        var editor = AsterGraphEditorFactory.Create(CreateOptions(
            diagnostics,
            [registration],
            localizationProvider: new HostOverrideLocalizationProvider(),
            nodePresentationProvider: new HostOverridePresentationProvider()));

        editor.RefreshNodePresentations();
        var canvasMenu = editor.Session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var addNode = Assert.Single(canvasMenu, item => item.Id == "canvas-add-node");
        var sourceNode = Assert.Single(editor.Nodes, node => node.Id == "source-node");

        Assert.Equal("Host Add Node", addNode.Header);
        Assert.Equal("Host Subtitle", sourceNode.DisplaySubtitle);
        Assert.Contains(sourceNode.Presentation.TopRightBadges, badge => badge.Text == "Plugin");
        Assert.Contains(sourceNode.Presentation.TopRightBadges, badge => badge.Text == "Host");
        Assert.Contains(editor.BuildContextMenu(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180))), item => item.Id == "plugin-sample-menu");
    }

    private static AsterGraphEditorOptions CreateOptions(
        IGraphEditorDiagnosticsSink diagnosticsSink,
        params GraphEditorPluginRegistration[] pluginRegistrations)
        => CreateOptions(diagnosticsSink, pluginRegistrations, null, null);

    private static AsterGraphEditorOptions CreateOptions(
        IGraphEditorDiagnosticsSink diagnosticsSink,
        IReadOnlyList<GraphEditorPluginRegistration> pluginRegistrations,
        IGraphLocalizationProvider? localizationProvider,
        INodePresentationProvider? nodePresentationProvider)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            DiagnosticsSink = diagnosticsSink,
            PluginRegistrations = pluginRegistrations,
            LocalizationProvider = localizationProvider,
            NodePresentationProvider = nodePresentationProvider,
        };

    private static string GetSamplePluginAssemblyPath()
        => Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "AsterGraph.TestPlugins",
            "bin",
            "Debug",
            "net9.0",
            "AsterGraph.TestPlugins.dll"));

    private static GraphDocument CreateDocument()
        => new(
            "Plugin Graph",
            "Plugin loading regression coverage.",
            [
                new GraphNode(
                    "source-node",
                    "Source Node",
                    "Tests",
                    "Plugins",
                    "Source node for plugin loading tests.",
                    new GraphPoint(120, 160),
                    new GraphSize(240, 160),
                    [],
                    [new GraphPort("out", "Output", PortDirection.Output, "float", "#6AD5C4", new PortTypeId("float"))],
                    "#6AD5C4",
                    new NodeDefinitionId("tests.plugins.source")),
            ],
            []);

    private static NodeCatalog CreateCatalog()
    {
        var catalog = new NodeCatalog();
        catalog.RegisterDefinition(new NodeDefinition(
            new NodeDefinitionId("tests.plugins.source"),
            "Plugin Source",
            "Tests",
            "Plugins",
            [],
            [new PortDefinition("out", "Output", new PortTypeId("float"), "#6AD5C4")]));
        return catalog;
    }

    private sealed class RecordingDiagnosticsSink : IGraphEditorDiagnosticsSink
    {
        public List<GraphEditorDiagnostic> Diagnostics { get; } = [];

        public void Publish(GraphEditorDiagnostic diagnostic)
            => Diagnostics.Add(diagnostic);
    }

    private sealed class DirectPlugin : IGraphEditorPlugin
    {
        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.direct-plugin", "Direct Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AddNodeDefinitionProvider(new DirectNodeDefinitionProvider());
            builder.AddContextMenuAugmentor(new DirectContextMenuAugmentor());
            builder.AddNodePresentationProvider(new DirectPresentationProvider());
            builder.AddLocalizationProvider(new DirectLocalizationProvider());
        }
    }

    private sealed class DirectNodeDefinitionProvider : INodeDefinitionProvider
    {
        public IReadOnlyList<INodeDefinition> GetNodeDefinitions()
            => [new NodeDefinition(new NodeDefinitionId("tests.direct-plugin.node"), "Direct Plugin Node", "Tests", "Plugins", [], [])];
    }

    private sealed class DirectContextMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
    {
        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
            => context.StockItems
                .Concat(
                [
                    new GraphEditorMenuItemDescriptorSnapshot(
                        "direct-plugin-menu",
                        "Direct Plugin Menu",
                        iconKey: "plugin",
                        isEnabled: false),
                ])
                .ToList();
    }

    private sealed class DirectPresentationProvider : IGraphEditorPluginNodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(GraphEditorPluginNodePresentationContext context)
            => new(
                TopRightBadges:
                [
                    new NodeAdornmentDescriptor("Direct", "#6AD5C4"),
                ]);
    }

    private sealed class DirectLocalizationProvider : IGraphEditorPluginLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.menu.canvas.addNode"
                ? "Direct Add Node"
                : fallback;
    }

    private sealed class HostOverrideLocalizationProvider : IGraphLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => key == "editor.menu.canvas.addNode"
                ? "Host Add Node"
                : fallback;
    }

    private sealed class HostOverridePresentationProvider : INodePresentationProvider
    {
        public NodePresentationState GetNodePresentation(NodePresentationContext context)
            => new(
                SubtitleOverride: "Host Subtitle",
                TopRightBadges:
                [
                    new NodeAdornmentDescriptor("Host", "#F3B36B"),
                ]);

        public NodePresentationState GetNodePresentation(NodeViewModel node)
            => new(
                SubtitleOverride: "Host Subtitle",
                TopRightBadges:
                [
                    new NodeAdornmentDescriptor("Host", "#F3B36B"),
                ]);
    }
}
