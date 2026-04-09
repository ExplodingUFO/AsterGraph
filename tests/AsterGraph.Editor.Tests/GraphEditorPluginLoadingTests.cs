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
            nodePresentationProvider: new HostOverridePresentationProvider(),
            trustPolicy: null));

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

    [Fact]
    public void Create_And_CreateSession_WithBlockingTrustPolicy_SurfaceEquivalentBlockedSnapshotsAndDiscoverability()
    {
        var editorDiagnostics = new RecordingDiagnosticsSink();
        var runtimeDiagnostics = new RecordingDiagnosticsSink();
        var plugin = new TrackingDirectPlugin();
        var manifest = new GraphEditorPluginManifest(
            "tests.loading.blocked-plugin",
            "Blocked Loading Plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                typeof(TrackingDirectPlugin).FullName ?? nameof(TrackingDirectPlugin)),
            version: "1.0.0",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var registration = GraphEditorPluginRegistration.FromPlugin(plugin, manifest);
        var trustPolicy = new BlockManifestIdTrustPolicy("tests.loading.blocked-plugin");
        var editor = AsterGraphEditorFactory.Create(CreateOptions(editorDiagnostics, [registration], null, null, trustPolicy));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(runtimeDiagnostics, [registration], null, null, trustPolicy));

        var retainedSnapshots = editor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = session.Queries.GetPluginLoadSnapshots();
        var retainedFeatures = editor.Session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var runtimeFeatures = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        var compatibility = GetCompatibility(snapshot);
        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Compatible, compatibility!.Status);
        Assert.Equal(GraphEditorPluginTrustDecision.Blocked, snapshot.TrustEvaluation!.Decision);
        Assert.False(snapshot.ActivationAttempted);
        Assert.True(retainedFeatures["integration.plugin-trust-policy"].IsAvailable);
        Assert.True(runtimeFeatures["integration.plugin-trust-policy"].IsAvailable);
        Assert.Single(editorDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.blocked");
        Assert.Single(runtimeDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.blocked");
        Assert.Equal(0, plugin.RegisterCallCount);
    }

    [Fact]
    public void Create_And_CreateSession_WithIncompatibleManifest_SurfaceEquivalentBlockedSnapshotsAndDiagnostics()
    {
        var editorDiagnostics = new RecordingDiagnosticsSink();
        var runtimeDiagnostics = new RecordingDiagnosticsSink();
        var plugin = new IncompatibleTrackingDirectPlugin();
        var manifest = new GraphEditorPluginManifest(
            "tests.loading.incompatible-plugin",
            "Incompatible Loading Plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                typeof(IncompatibleTrackingDirectPlugin).FullName ?? nameof(IncompatibleTrackingDirectPlugin)),
            version: "1.0.0",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "9999.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var registration = GraphEditorPluginRegistration.FromPlugin(plugin, manifest);
        var editor = AsterGraphEditorFactory.Create(CreateOptions(editorDiagnostics, [registration], null, null, null));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(runtimeDiagnostics, [registration], null, null, null));

        var retainedSnapshots = editor.Session.Queries.GetPluginLoadSnapshots();
        var runtimeSnapshots = session.Queries.GetPluginLoadSnapshots();
        var retainedFeatures = editor.Session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var runtimeFeatures = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        var snapshot = Assert.Single(runtimeSnapshots);
        var compatibility = GetCompatibility(snapshot);
        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Incompatible, compatibility!.Status);
        Assert.Equal("compatibility.astergraph.minimum-version", compatibility.ReasonCode);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.False(snapshot.ActivationAttempted);
        Assert.True(retainedFeatures["integration.plugin-loader"].IsAvailable);
        Assert.True(runtimeFeatures["integration.plugin-loader"].IsAvailable);
        Assert.Single(editorDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
        Assert.Single(runtimeDiagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
        Assert.Equal(0, plugin.RegisterCallCount);
    }

    [Fact]
    public void DiscoverPluginCandidates_And_CreateSession_StayAlignedOnFallbackManifestAndTrustFacts_ForAssemblyCandidates()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "astergraph-plugin-loading-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var pluginAssemblyPath = Path.Combine(tempDirectory, "DiscoveryParityPlugin.dll");
        File.Copy(GetSamplePluginAssemblyPath(), pluginAssemblyPath, overwrite: true);
        pluginAssemblyPath = Path.GetFullPath(pluginAssemblyPath);

        var candidate = Assert.Single(AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            DirectorySources =
            [
                new GraphEditorPluginDirectoryDiscoverySource(tempDirectory),
            ],
        }));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            new RecordingDiagnosticsSink(),
            GraphEditorPluginRegistration.FromAssemblyPath(pluginAssemblyPath)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());

        Assert.Equal(pluginAssemblyPath, candidate.AssemblyPath);
        Assert.Equal(pluginAssemblyPath, snapshot.Source);
        Assert.Equal(candidate.Manifest, snapshot.Manifest);
        Assert.Equal(candidate.TrustEvaluation, snapshot.TrustEvaluation);
        Assert.Equal(candidate.Compatibility, GetCompatibility(snapshot));
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Unknown, candidate.Compatibility.Status);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, snapshot.Status);
    }

    [Fact]
    public void DiscoverPluginCandidates_And_CreateSession_StayAlignedOnHostProvidedManifestTrustFacts_ForManifestSources()
    {
        var provenanceEvidence = new GraphEditorPluginProvenanceEvidence(
            new GraphEditorPluginPackageIdentity("AsterGraph.LoadingManifestSource", "1.0.0"),
            new GraphEditorPluginSignatureEvidence(
                GraphEditorPluginSignatureStatus.Valid,
                GraphEditorPluginSignatureKind.Repository,
                new GraphEditorPluginSignerIdentity("AsterGraph Repository", "LOAD1234"),
                timestampUtc: new DateTimeOffset(2026, 04, 09, 0, 0, 0, TimeSpan.Zero),
                timestampAuthority: "tests.timestamp"));
        var manifest = new GraphEditorPluginManifest(
            "tests.loading.manifest-source",
            "Loading Manifest Source",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.Manifest,
                "tests.loading.manifest-source"),
            version: "1.0.0",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var trustPolicy = new BlockManifestIdTrustPolicy("tests.loading.some-other-plugin");

        var candidate = Assert.Single(AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            ManifestSources =
            [
                new TestManifestSource(
                    new GraphEditorPluginManifestSourceCandidate(
                        "tests.loading.manifest-source",
                        GetSamplePluginAssemblyPath(),
                        manifest,
                        "AsterGraph.TestPlugins.SamplePlugin",
                        provenanceEvidence)),
            ],
            TrustPolicy = trustPolicy,
        }));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            new RecordingDiagnosticsSink(),
            [
                GraphEditorPluginRegistration.FromAssemblyPath(
                    GetSamplePluginAssemblyPath(),
                    "AsterGraph.TestPlugins.SamplePlugin",
                    manifest,
                    provenanceEvidence),
            ],
            null,
            null,
            trustPolicy));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());

        Assert.Equal(candidate.Manifest, snapshot.Manifest);
        Assert.Equal(candidate.TrustEvaluation, snapshot.TrustEvaluation);
        Assert.Equal(candidate.Compatibility, GetCompatibility(snapshot));
        Assert.Equal(candidate.ProvenanceEvidence, snapshot.ProvenanceEvidence);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, snapshot.Status);
        Assert.Equal(GraphEditorPluginLoadSourceKind.Assembly, snapshot.SourceKind);
    }

    [Fact]
    public void CreateSession_WithPackageRegistration_ExposesStructuredPackageFailureWithoutActivation()
    {
        var diagnostics = new RecordingDiagnosticsSink();
        var tempDirectory = Path.Combine(Path.GetTempPath(), "astergraph-plugin-loading-tests", Guid.NewGuid().ToString("N"));
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackage(
            tempDirectory,
            "AsterGraph.PackageLoading",
            "1.0.0",
            title: "Package Loading Candidate",
            description: "Package loading refusal coverage.");

        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            diagnostics,
            GraphEditorPluginRegistration.FromPackagePath(packagePath)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());

        Assert.Equal(GraphEditorPluginLoadSourceKind.Package, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Failed, snapshot.Status);
        Assert.Equal(packagePath, snapshot.Source);
        Assert.Equal(packagePath, snapshot.PackagePath);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Equal(GraphEditorPluginManifestSourceKind.PackageArchive, snapshot.Manifest.Provenance.SourceKind);
        Assert.Equal(GraphEditorPluginSignatureStatus.NotProvided, snapshot.ProvenanceEvidence.Signature.Status);
        Assert.NotNull(snapshot.FailureMessage);
        Assert.Contains("Package registrations are not supported until verified staging is implemented.", snapshot.FailureMessage, StringComparison.Ordinal);
        Assert.Contains(diagnostics.Diagnostics, diagnostic => diagnostic.Code == "plugin.load.package-registration-not-supported");
    }

    private static GraphEditorPluginCompatibilityEvaluation? GetCompatibility(GraphEditorPluginLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var property = typeof(GraphEditorPluginLoadSnapshot).GetProperty("Compatibility");
        Assert.NotNull(property);
        return Assert.IsType<GraphEditorPluginCompatibilityEvaluation>(property!.GetValue(snapshot));
    }

    private static AsterGraphEditorOptions CreateOptions(
        IGraphEditorDiagnosticsSink diagnosticsSink,
        params GraphEditorPluginRegistration[] pluginRegistrations)
        => CreateOptions(diagnosticsSink, pluginRegistrations, null, null, null);

    private static AsterGraphEditorOptions CreateOptions(
        IGraphEditorDiagnosticsSink diagnosticsSink,
        IReadOnlyList<GraphEditorPluginRegistration> pluginRegistrations,
        IGraphLocalizationProvider? localizationProvider,
        INodePresentationProvider? nodePresentationProvider,
        IGraphEditorPluginTrustPolicy? trustPolicy)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            DiagnosticsSink = diagnosticsSink,
            PluginRegistrations = pluginRegistrations,
            LocalizationProvider = localizationProvider,
            NodePresentationProvider = nodePresentationProvider,
            PluginTrustPolicy = trustPolicy,
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

    private sealed class TrackingDirectPlugin : IGraphEditorPlugin
    {
        public int RegisterCallCount { get; private set; }

        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.loading.blocked-plugin", "Blocked Loading Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            RegisterCallCount++;
            builder.AddContextMenuAugmentor(new DirectContextMenuAugmentor());
        }
    }

    private sealed class IncompatibleTrackingDirectPlugin : IGraphEditorPlugin
    {
        public int RegisterCallCount { get; private set; }

        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.loading.incompatible-plugin", "Incompatible Loading Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            RegisterCallCount++;
            builder.AddContextMenuAugmentor(new DirectContextMenuAugmentor());
        }
    }

    private sealed class BlockManifestIdTrustPolicy(string blockedId) : IGraphEditorPluginTrustPolicy
    {
        public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return StringComparer.Ordinal.Equals(context.Manifest.Id, blockedId)
                ? new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Blocked,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.blocked.loading-tests",
                    $"Blocked manifest '{context.Manifest.Id}' for loading coverage.")
                : new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Allowed,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.allowed.loading-tests",
                    $"Allowed manifest '{context.Manifest.Id}'.");
        }
    }

    private sealed class TestManifestSource(params GraphEditorPluginManifestSourceCandidate[] candidates) : IGraphEditorPluginManifestSource
    {
        public IReadOnlyList<GraphEditorPluginManifestSourceCandidate> GetCandidates()
            => candidates;
    }
}
