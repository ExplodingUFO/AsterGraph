using System.Reflection;
using AsterGraph.Editor.Hosting;
using AsterGraph.Editor.Plugins;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginDiscoveryTests
{
    [Fact]
    public void DiscoverPluginCandidates_WithDirectorySource_EnumeratesAssemblyCandidatesWithoutLoadingPluginAssembly()
    {
        var tempDirectory = CreateTempDirectory();
        var pluginAssemblyPath = CopySamplePluginAssembly(tempDirectory, "DirectoryDiscoveryPlugin.dll");

        var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            DirectorySources =
            [
                new GraphEditorPluginDirectoryDiscoverySource(tempDirectory),
            ],
        });

        var candidate = Assert.Single(candidates);
        Assert.Equal(GraphEditorPluginCandidateSourceKind.Directory, candidate.SourceKind);
        Assert.Equal(Path.GetFullPath(tempDirectory), candidate.Source);
        Assert.Equal(pluginAssemblyPath, candidate.AssemblyPath);
        Assert.Equal(GraphEditorPluginManifestSourceKind.AssemblyPath, candidate.Manifest.Provenance.SourceKind);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, candidate.TrustEvaluation.Decision);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Unknown, candidate.Compatibility.Status);
        Assert.DoesNotContain(
            AppDomain.CurrentDomain.GetAssemblies(),
            assembly => string.Equals(GetAssemblyLocation(assembly), pluginAssemblyPath, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DiscoverPluginCandidates_WithManifestSource_ProjectsManifestTrustAndCompatibilityState()
    {
        var tempDirectory = CreateTempDirectory();
        var pluginAssemblyPath = CopySamplePluginAssembly(tempDirectory, "ManifestDiscoveryPlugin.dll");
        var manifest = new GraphEditorPluginManifest(
            "tests.discovery.manifest-source",
            "Manifest Source Candidate",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.Manifest,
                "tests.manifest-source"),
            version: "1.2.3",
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "0.0.1",
                maximumAsterGraphVersion: "9.0.0",
                targetFramework: "net9.0",
                runtimeSurface: "session-first"),
            capabilitySummary: "menus");
        var provenanceEvidence = new GraphEditorPluginProvenanceEvidence(
            new GraphEditorPluginPackageIdentity("AsterGraph.ManifestDiscovery", "1.2.3"),
            new GraphEditorPluginSignatureEvidence(
                GraphEditorPluginSignatureStatus.Valid,
                GraphEditorPluginSignatureKind.Repository,
                new GraphEditorPluginSignerIdentity("AsterGraph Repository", "DISC1234"),
                timestampUtc: new DateTimeOffset(2026, 04, 09, 0, 0, 0, TimeSpan.Zero),
                timestampAuthority: "tests.timestamp"));

        var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            ManifestSources =
            [
                new TestManifestSource(
                    new GraphEditorPluginManifestSourceCandidate(
                        "tests.manifest-source",
                        pluginAssemblyPath,
                        manifest,
                        "AsterGraph.TestPlugins.SamplePlugin",
                        provenanceEvidence)),
            ],
            TrustPolicy = new BlockManifestIdTrustPolicy("tests.discovery.manifest-source"),
        });

        var candidate = Assert.Single(candidates);
        Assert.Equal(GraphEditorPluginCandidateSourceKind.ManifestSource, candidate.SourceKind);
        Assert.Equal("tests.manifest-source", candidate.Source);
        Assert.Equal(pluginAssemblyPath, candidate.AssemblyPath);
        Assert.Equal("AsterGraph.TestPlugins.SamplePlugin", candidate.PluginTypeName);
        Assert.Equal(manifest, candidate.Manifest);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Compatible, candidate.Compatibility.Status);
        Assert.Equal(GraphEditorPluginTrustDecision.Blocked, candidate.TrustEvaluation.Decision);
        Assert.Equal(provenanceEvidence, candidate.ProvenanceEvidence);
        Assert.DoesNotContain(
            AppDomain.CurrentDomain.GetAssemblies(),
            assembly => string.Equals(GetAssemblyLocation(assembly), pluginAssemblyPath, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DiscoverPluginCandidates_WithManifestOutsideSupportedVersion_ReportsIncompatibleCandidate()
    {
        var tempDirectory = CreateTempDirectory();
        var pluginAssemblyPath = CopySamplePluginAssembly(tempDirectory, "IncompatibleDiscoveryPlugin.dll");
        var manifest = new GraphEditorPluginManifest(
            "tests.discovery.incompatible",
            "Incompatible Candidate",
            new GraphEditorPluginManifestProvenance(
                GraphEditorPluginManifestSourceKind.Manifest,
                "tests.incompatible-source"),
            compatibility: new GraphEditorPluginCompatibilityManifest(
                minimumAsterGraphVersion: "999.0.0"));

        var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            ManifestSources =
            [
                new TestManifestSource(
                    new GraphEditorPluginManifestSourceCandidate(
                        "tests.incompatible-source",
                        pluginAssemblyPath,
                        manifest)),
            ],
        });

        var candidate = Assert.Single(candidates);
        Assert.Equal(GraphEditorPluginCompatibilityStatus.Incompatible, candidate.Compatibility.Status);
        Assert.NotNull(candidate.Compatibility.ReasonCode);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, candidate.TrustEvaluation.Decision);
    }

    [Fact]
    public void DiscoverPluginCandidates_WithPackageDirectorySource_ProjectsPackageMetadataWithoutExtraction()
    {
        var tempDirectory = CreateTempDirectory();
        Directory.CreateDirectory(tempDirectory);
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackage(
            tempDirectory,
            "AsterGraph.PackageDiscovery",
            "1.2.3",
            title: "Package Discovery Candidate",
            description: "Package discovery regression coverage.");

        var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            PackageDirectorySources =
            [
                new GraphEditorPluginPackageDiscoverySource(tempDirectory),
            ],
        });

        var candidate = Assert.Single(candidates);
        Assert.Equal(GraphEditorPluginCandidateSourceKind.PackageDirectory, candidate.SourceKind);
        Assert.Equal(Path.GetFullPath(tempDirectory), candidate.Source);
        Assert.Null(candidate.AssemblyPath);
        Assert.Equal(packagePath, candidate.PackagePath);
        Assert.Equal(GraphEditorPluginManifestSourceKind.PackageArchive, candidate.Manifest.Provenance.SourceKind);
        Assert.Equal("AsterGraph.PackageDiscovery", candidate.Manifest.Id);
        Assert.Equal("1.2.3", candidate.Manifest.Version);
        Assert.Equal("AsterGraph.PackageDiscovery", candidate.ProvenanceEvidence.PackageIdentity?.Id);
        Assert.Equal("1.2.3", candidate.ProvenanceEvidence.PackageIdentity?.Version);
        Assert.Equal(GraphEditorPluginSignatureStatus.Unsigned, candidate.ProvenanceEvidence.Signature.Status);
        Assert.Equal(GraphEditorPluginTrustDecision.Allowed, candidate.TrustEvaluation.Decision);
        Assert.Equal([packagePath], Directory.GetFiles(tempDirectory));
    }

    [Fact]
    public void DiscoverPluginCandidates_WithPackageDirectorySource_PassesPackagePathIntoTrustPolicyContext()
    {
        var tempDirectory = CreateTempDirectory();
        Directory.CreateDirectory(tempDirectory);
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackage(
            tempDirectory,
            "AsterGraph.PackageTrust",
            "2.0.0",
            title: "Package Trust Candidate",
            description: "Trust policy package-path coverage.");

        var trustPolicy = new BlockPackagePathTrustPolicy(packagePath);

        var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            PackageDirectorySources =
            [
                new GraphEditorPluginPackageDiscoverySource(tempDirectory),
            ],
            TrustPolicy = trustPolicy,
        });

        var candidate = Assert.Single(candidates);
        Assert.Equal(GraphEditorPluginTrustDecision.Blocked, candidate.TrustEvaluation.Decision);
        Assert.Equal(packagePath, trustPolicy.SeenPackagePath);
    }

    [Fact]
    public void DiscoverPluginCandidates_WithSignedPackageMarker_ReportsUnknownSignatureState()
    {
        var tempDirectory = CreateTempDirectory();
        Directory.CreateDirectory(tempDirectory);
        var packagePath = PluginPackageTestHelper.CreateSignedMarkerPackage(
            tempDirectory,
            "AsterGraph.PackageSigned",
            "3.0.0",
            title: "Signed Package Candidate",
            description: "Package signature marker coverage.");

        var candidates = AsterGraphEditorFactory.DiscoverPluginCandidates(new GraphEditorPluginDiscoveryOptions
        {
            PackageDirectorySources =
            [
                new GraphEditorPluginPackageDiscoverySource(tempDirectory),
            ],
        });

        var candidate = Assert.Single(candidates);
        Assert.Equal(packagePath, candidate.PackagePath);
        Assert.Equal(GraphEditorPluginSignatureStatus.Unknown, candidate.ProvenanceEvidence.Signature.Status);
        Assert.Equal("signature.validation.unavailable", candidate.ProvenanceEvidence.Signature.ReasonCode);
    }

    private static string CopySamplePluginAssembly(string targetDirectory, string fileName)
    {
        Directory.CreateDirectory(targetDirectory);
        var targetPath = Path.Combine(targetDirectory, fileName);
        File.Copy(GetSamplePluginAssemblyPath(), targetPath, overwrite: true);
        return Path.GetFullPath(targetPath);
    }

    private static string CreateTempDirectory()
        => Path.Combine(Path.GetTempPath(), "astergraph-plugin-discovery-tests", Guid.NewGuid().ToString("N"));

    private static string GetSamplePluginAssemblyPath()
        => TestPluginArtifactPathHelper.GetSamplePluginAssemblyPath();

    private static string? GetAssemblyLocation(Assembly assembly)
    {
        try
        {
            return string.IsNullOrWhiteSpace(assembly.Location)
                ? null
                : Path.GetFullPath(assembly.Location);
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    private sealed class TestManifestSource(params GraphEditorPluginManifestSourceCandidate[] candidates) : IGraphEditorPluginManifestSource
    {
        public IReadOnlyList<GraphEditorPluginManifestSourceCandidate> GetCandidates()
            => candidates;
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
                    "trust.blocked.discovery-tests",
                    $"Blocked manifest '{context.Manifest.Id}' for discovery coverage.")
                : new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Allowed,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.allowed.discovery-tests",
                    $"Allowed manifest '{context.Manifest.Id}'.");
        }
    }

    private sealed class BlockPackagePathTrustPolicy(string blockedPackagePath) : IGraphEditorPluginTrustPolicy
    {
        public string? SeenPackagePath { get; private set; }

        public GraphEditorPluginTrustEvaluation Evaluate(GraphEditorPluginTrustPolicyContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            SeenPackagePath = context.PackagePath;
            return StringComparer.OrdinalIgnoreCase.Equals(context.PackagePath, blockedPackagePath)
                ? new GraphEditorPluginTrustEvaluation(
                    GraphEditorPluginTrustDecision.Blocked,
                    GraphEditorPluginTrustEvaluationSource.HostPolicy,
                    "trust.blocked.package-path",
                    $"Blocked package '{context.PackagePath}'.")
                : GraphEditorPluginTrustEvaluation.ImplicitAllow();
        }
    }
}
