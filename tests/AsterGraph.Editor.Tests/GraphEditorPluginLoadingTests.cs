using AsterGraph.Abstractions.Compatibility;
using AsterGraph.Abstractions.Definitions;
using AsterGraph.Abstractions.Identifiers;
using AsterGraph.Core.Compatibility;
using AsterGraph.Core.Models;
using AsterGraph.Editor.Catalog;
using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Menus;
using AsterGraph.Editor.Plugins;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginLoadingTests
{
    [Fact]
    public void CreateSession_WithAssemblyPluginRegistration_PublishesSuccessDiagnosticAndDoesNotApplyPluginContributionsYet()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(diagnostics, GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath())));
        var descriptors = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var canvasMenu = session.Queries.BuildContextMenuDescriptors(new ContextMenuContext(ContextMenuTargetKind.Canvas, new GraphPoint(240, 180)));
        var addNode = Assert.Single(canvasMenu, item => item.Id == "canvas-add-node");
        var pluginLoadDiagnostic = Assert.Single(diagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.succeeded");

        Assert.True(descriptors["integration.plugin-loader"].IsAvailable);
        Assert.Contains(session.Diagnostics.GetRecentDiagnostics(), diagnostic => diagnostic.Code == "plugin.load.succeeded");
        Assert.Equal(GraphEditorDiagnosticSeverity.Info, pluginLoadDiagnostic.Severity);
        Assert.DoesNotContain(
            addNode.Children.SelectMany(group => group.Children),
            item => item.Header.Contains("Sample Plugin Node", StringComparison.Ordinal));
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

    private static AsterGraphEditorOptions CreateOptions(
        IGraphEditorDiagnosticsSink diagnosticsSink,
        params GraphEditorPluginRegistration[] pluginRegistrations)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            DiagnosticsSink = diagnosticsSink,
            PluginRegistrations = pluginRegistrations,
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
            builder.AddLocalizationProvider(new DirectLocalizationProvider());
        }
    }

    private sealed class DirectLocalizationProvider : IGraphEditorPluginLocalizationProvider
    {
        public string GetString(string key, string fallback)
            => fallback;
    }
}
