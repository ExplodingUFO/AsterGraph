using System.Reflection;
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
using AsterGraph.Editor.Runtime;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginInspectionContractsTests
{
    [Fact]
    public void IGraphEditorQueries_DefinesCanonicalPluginLoadInspectionRead()
    {
        var method = typeof(IGraphEditorQueries).GetMethod(nameof(IGraphEditorQueries.GetPluginLoadSnapshots));

        Assert.NotNull(method);
        Assert.Equal(typeof(IReadOnlyList<GraphEditorPluginLoadSnapshot>), method!.ReturnType);
    }

    [Fact]
    public void PluginLoadInspectionContractSurface_IsRuntimeFirstAndFreeOfAvaloniaAndGraphEditorViewModel()
    {
        var publicTypes = new Type[]
        {
            typeof(GraphEditorPluginLoadSnapshot),
            typeof(GraphEditorPluginContributionSummarySnapshot),
            typeof(GraphEditorPluginLoadSourceKind),
            typeof(GraphEditorPluginLoadStatus),
            typeof(GraphEditorPluginManifest),
            typeof(GraphEditorPluginManifestProvenance),
            typeof(GraphEditorPluginCompatibilityManifest),
            typeof(GraphEditorPluginPackageIdentity),
            typeof(GraphEditorPluginSignerIdentity),
            typeof(GraphEditorPluginSignatureKind),
            typeof(GraphEditorPluginSignatureStatus),
            typeof(GraphEditorPluginSignatureEvidence),
            typeof(GraphEditorPluginProvenanceEvidence),
            typeof(GraphEditorPluginTrustEvaluation),
            typeof(GraphEditorPluginTrustDecision),
            typeof(GraphEditorPluginTrustEvaluationSource),
        };

        foreach (var type in publicTypes)
        {
            Assert.True(type.IsPublic);
            Assert.DoesNotContain(GetPubliclyReferencedTypes(type), IsDisallowedType);
        }
    }

    [Fact]
    public void CreateSession_WithAssemblyPluginRegistration_ExposesStructuredPluginLoadSnapshot()
    {
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath())));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());
        var inspection = session.Diagnostics.CaptureInspectionSnapshot();
        var compatibility = GetCompatibility(snapshot);

        Assert.Equal(GraphEditorPluginLoadSourceKind.Assembly, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Loaded, snapshot.Status);
        Assert.Equal(Path.GetFullPath(GetSamplePluginAssemblyPath()), snapshot.Source);
        Assert.NotNull(snapshot.Manifest);
        Assert.Equal(GraphEditorPluginManifestSourceKind.AssemblyPath, snapshot.Manifest!.Provenance.SourceKind);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Unknown, compatibility!.Status);
        Assert.NotNull(snapshot.TrustEvaluation);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginProvenanceEvidence.NotProvided, snapshot.ProvenanceEvidence);
        Assert.Equal(GraphEditorPluginSignatureStatus.NotProvided, snapshot.ProvenanceEvidence.Signature.Status);
        Assert.True(snapshot.ActivationAttempted);
        Assert.Null(snapshot.RequestedPluginTypeName);
        Assert.Equal("AsterGraph.TestPlugins.SamplePlugin", snapshot.ResolvedPluginTypeName);
        Assert.NotNull(snapshot.Descriptor);
        Assert.Equal("tests.sample-plugin", snapshot.Descriptor!.Id);
        Assert.Equal(1, snapshot.Contributions.NodeDefinitionProviderCount);
        Assert.Equal(1, snapshot.Contributions.ContextMenuAugmentorCount);
        Assert.Equal(1, snapshot.Contributions.NodePresentationProviderCount);
        Assert.Equal(1, snapshot.Contributions.LocalizationProviderCount);
        Assert.Null(snapshot.FailureMessage);
        Assert.Equal(session.Queries.GetPluginLoadSnapshots(), inspection.PluginLoadSnapshots);
    }

    [Fact]
    public void CreateSession_WithMissingAssemblyPluginRegistration_ExposesStructuredFailureSnapshot()
    {
        var missingAssemblyPath = Path.Combine(Path.GetTempPath(), "astergraph-plugin-tests", Guid.NewGuid().ToString("N"), "missing-plugin.dll");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromAssemblyPath(missingAssemblyPath)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());
        var compatibility = GetCompatibility(snapshot);

        Assert.Equal(GraphEditorPluginLoadSourceKind.Assembly, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Failed, snapshot.Status);
        Assert.Equal(Path.GetFullPath(missingAssemblyPath), snapshot.Source);
        Assert.NotNull(snapshot.Manifest);
        Assert.Equal(GraphEditorPluginManifestSourceKind.AssemblyPath, snapshot.Manifest!.Provenance.SourceKind);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Unknown, compatibility!.Status);
        Assert.NotNull(snapshot.TrustEvaluation);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginProvenanceEvidence.NotProvided, snapshot.ProvenanceEvidence);
        Assert.True(snapshot.ActivationAttempted);
        Assert.Null(snapshot.Descriptor);
        Assert.Null(snapshot.ResolvedPluginTypeName);
        Assert.NotNull(snapshot.FailureMessage);
        Assert.Contains("missing-plugin.dll", snapshot.FailureMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, snapshot.Contributions.NodeDefinitionProviderCount);
        Assert.Equal(0, snapshot.Contributions.ContextMenuAugmentorCount);
        Assert.Equal(0, snapshot.Contributions.NodePresentationProviderCount);
        Assert.Equal(0, snapshot.Contributions.LocalizationProviderCount);
    }

    [Fact]
    public void Create_And_CreateSession_ExposeEquivalentPluginLoadSnapshots_AndDiscoverability()
    {
        var registration = GraphEditorPluginRegistration.FromAssemblyPath(GetSamplePluginAssemblyPath());
        var editor = AsterGraphEditorFactory.Create(CreateOptions(registration));
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(registration));

        var retainedSnapshots = editor.Session.Queries.GetPluginLoadSnapshots()
            .OrderBy(snapshot => snapshot.Source, StringComparer.Ordinal)
            .ToList();
        var runtimeSnapshots = session.Queries.GetPluginLoadSnapshots()
            .OrderBy(snapshot => snapshot.Source, StringComparer.Ordinal)
            .ToList();
        var retainedFeatures = editor.Session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var runtimeFeatures = session.Queries.GetFeatureDescriptors().ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);

        Assert.Equal(runtimeSnapshots, retainedSnapshots);
        Assert.True(retainedFeatures["query.plugin-load-snapshots"].IsAvailable);
        Assert.True(runtimeFeatures["query.plugin-load-snapshots"].IsAvailable);
    }

    [Fact]
    public void CreateSession_WithBlockingTrustPolicy_ExposesStructuredBlockedSnapshotWithoutExecutingContributionCode()
    {
        var plugin = new RegisterTrackingPlugin();
        var manifest = new GraphEditorPluginManifest(
            "tests.blocked-plugin",
            "Blocked Plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                typeof(RegisterTrackingPlugin).FullName ?? nameof(RegisterTrackingPlugin)),
            version: "1.0.0",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            new BlockByManifestIdTrustPolicy("tests.blocked-plugin"),
            GraphEditorPluginRegistration.FromPlugin(plugin, manifest)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());
        var compatibility = GetCompatibility(snapshot);
        var diagnostics = session.Diagnostics.GetRecentDiagnostics();

        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.Equal(manifest, snapshot.Manifest);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Compatible, compatibility!.Status);
        Assert.NotNull(snapshot.TrustEvaluation);
        Assert.Equal(GraphEditorPluginTrustDecision.Blocked, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginTrustEvaluationSource.HostPolicy, snapshot.TrustEvaluation.Source);
        Assert.Equal("trust.blocked.by-manifest-id", snapshot.TrustEvaluation.ReasonCode);
        Assert.Equal(GraphEditorPluginProvenanceEvidence.NotProvided, snapshot.ProvenanceEvidence);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Null(snapshot.Descriptor);
        Assert.Null(snapshot.ResolvedPluginTypeName);
        Assert.Null(snapshot.FailureMessage);
        Assert.Equal(0, plugin.RegisterCallCount);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "plugin.load.blocked");
    }

    [Fact]
    public void CreateSession_WithIncompatiblePluginManifest_ExposesStructuredBlockedSnapshotWithoutExecutingContributionCode()
    {
        var plugin = new IncompatibleRegisterTrackingPlugin();
        var manifest = new GraphEditorPluginManifest(
            "tests.incompatible-plugin",
            "Incompatible Plugin",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.DirectRegistration,
                typeof(IncompatibleRegisterTrackingPlugin).FullName ?? nameof(IncompatibleRegisterTrackingPlugin)),
            version: "1.0.0",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "9999.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(
            GraphEditorPluginRegistration.FromPlugin(plugin, manifest)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());
        var compatibility = GetCompatibility(snapshot);
        var diagnostics = session.Diagnostics.GetRecentDiagnostics();

        Assert.Equal(GraphEditorPluginLoadStatus.Blocked, snapshot.Status);
        Assert.Equal(manifest, snapshot.Manifest);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Incompatible, compatibility!.Status);
        Assert.Equal("compatibility.astergraph.minimum-version", compatibility.ReasonCode);
        Assert.NotNull(snapshot.TrustEvaluation);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginProvenanceEvidence.NotProvided, snapshot.ProvenanceEvidence);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Null(snapshot.Descriptor);
        Assert.Null(snapshot.ResolvedPluginTypeName);
        Assert.Null(snapshot.FailureMessage);
        Assert.Equal(0, plugin.RegisterCallCount);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "plugin.load.incompatible");
    }

    [Fact]
    public void CreateSession_WithPackageRegistration_ExposesStructuredPackageFailureSnapshot()
    {
        var packageDirectory = Path.Combine(Path.GetTempPath(), "astergraph-plugin-inspection-tests", Guid.NewGuid().ToString("N"));
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackage(
            packageDirectory,
            "AsterGraph.PackageInspection",
            "1.0.0",
            title: "Package Inspection Candidate",
            description: "Inspection package failure coverage.");
        var session = AsterGraphEditorFactory.CreateSession(CreateOptions(GraphEditorPluginRegistration.FromPackagePath(packagePath)));
        var snapshot = Assert.Single(session.Queries.GetPluginLoadSnapshots());
        var compatibility = GetCompatibility(snapshot);

        Assert.Equal(GraphEditorPluginLoadSourceKind.Package, snapshot.SourceKind);
        Assert.Equal(GraphEditorPluginLoadStatus.Failed, snapshot.Status);
        Assert.Equal(packagePath, snapshot.Source);
        Assert.Equal(packagePath, snapshot.PackagePath);
        Assert.NotNull(snapshot.Manifest);
        Assert.Equal(GraphEditorPluginManifestSourceKind.PackageArchive, snapshot.Manifest!.Provenance.SourceKind);
        Assert.NotNull(compatibility);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Unknown, compatibility!.Status);
        Assert.NotNull(snapshot.TrustEvaluation);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, snapshot.TrustEvaluation!.Decision);
        Assert.Equal(GraphEditorPluginProvenanceEvidence.NotProvided, snapshot.ProvenanceEvidence);
        Assert.False(snapshot.ActivationAttempted);
        Assert.Null(snapshot.Descriptor);
        Assert.Null(snapshot.ResolvedPluginTypeName);
        Assert.NotNull(snapshot.FailureMessage);
        Assert.Contains("Package registrations are not supported until verified staging is implemented.", snapshot.FailureMessage!, StringComparison.Ordinal);
    }

    private static AsterGraphEditorOptions CreateOptions(params GraphEditorPluginRegistration[] pluginRegistrations)
        => CreateOptions(null, pluginRegistrations);

    private static AsterGraphEditorOptions CreateOptions(
        IGraphEditorPluginTrustPolicy? trustPolicy,
        params GraphEditorPluginRegistration[] pluginRegistrations)
        => new()
        {
            Document = CreateDocument(),
            NodeCatalog = CreateCatalog(),
            CompatibilityService = new DefaultPortCompatibilityService(),
            PluginRegistrations = pluginRegistrations,
            PluginTrustPolicy = trustPolicy,
        };

    private static GraphEditorPluginCompatibilityEvaluation? GetCompatibility(GraphEditorPluginLoadSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var property = typeof(GraphEditorPluginLoadSnapshot).GetProperty("Compatibility");
        Assert.NotNull(property);
        return Assert.IsType<GraphEditorPluginCompatibilityEvaluation>(property!.GetValue(snapshot));
    }

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

    private static IReadOnlyList<Type> GetPubliclyReferencedTypes(Type type)
    {
        var referencedTypes = new List<Type>();

        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            switch (member)
            {
                case PropertyInfo property:
                    referencedTypes.Add(property.PropertyType);
                    break;
                case MethodInfo method:
                    referencedTypes.Add(method.ReturnType);
                    referencedTypes.AddRange(method.GetParameters().Select(parameter => parameter.ParameterType));
                    break;
                case ConstructorInfo constructor:
                    referencedTypes.AddRange(constructor.GetParameters().Select(parameter => parameter.ParameterType));
                    break;
                case EventInfo eventInfo when eventInfo.EventHandlerType is not null:
                    referencedTypes.Add(eventInfo.EventHandlerType);
                    break;
            }
        }

        return referencedTypes
            .Where(candidate => candidate != typeof(void))
            .SelectMany(ExpandType)
            .Distinct()
            .ToList();
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var expanded in ExpandType(elementType))
            {
                yield return expanded;
            }

            yield break;
        }

        if (type.IsGenericType)
        {
            yield return type.GetGenericTypeDefinition();

            foreach (var argument in type.GetGenericArguments())
            {
                foreach (var expanded in ExpandType(argument))
                {
                    yield return expanded;
                }
            }

            yield break;
        }

        yield return type;
    }

    private static bool IsDisallowedType(Type type)
    {
        var fullName = type.FullName ?? string.Empty;
        return fullName.StartsWith("Avalonia.", StringComparison.Ordinal)
            || fullName.Contains("GraphEditorViewModel", StringComparison.Ordinal)
            || fullName.Contains("NodeViewModel", StringComparison.Ordinal);
    }

    private static GraphDocument CreateDocument()
        => new(
            "Plugin Inspection Graph",
            "Plugin inspection regression coverage.",
            [
                new GraphNode(
                    "source-node",
                    "Source Node",
                    "Tests",
                    "Plugins",
                    "Source node for plugin inspection tests.",
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

    private sealed class RegisterTrackingPlugin : IGraphEditorPlugin
    {
        public int RegisterCallCount { get; private set; }

        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.blocked-plugin", "Blocked Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            RegisterCallCount++;
            builder.AddContextMenuAugmentor(new PassThroughMenuAugmentor());
        }
    }

    private sealed class IncompatibleRegisterTrackingPlugin : IGraphEditorPlugin
    {
        public int RegisterCallCount { get; private set; }

        public GraphEditorPluginDescriptor Descriptor { get; } = new("tests.incompatible-plugin", "Incompatible Plugin");

        public void Register(GraphEditorPluginBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            RegisterCallCount++;
            builder.AddContextMenuAugmentor(new PassThroughMenuAugmentor());
        }
    }

    private sealed class PassThroughMenuAugmentor : IGraphEditorPluginContextMenuAugmentor
    {
        public IReadOnlyList<GraphEditorMenuItemDescriptorSnapshot> Augment(GraphEditorPluginMenuAugmentationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return context.StockItems;
        }
    }

    private sealed class BlockByManifestIdTrustPolicy(string blockedId) : IGraphEditorPluginTrustPolicy
    {
        public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            return StringComparer.Ordinal.Equals(context.Manifest.Id, blockedId)
                ? new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Blocked,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.blocked.by-manifest-id",
                    $"Blocked manifest '{context.Manifest.Id}' for contract coverage.")
                : new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Allowed,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.allowed.by-manifest-id",
                    $"Allowed manifest '{context.Manifest.Id}'.");
        }
    }
}
