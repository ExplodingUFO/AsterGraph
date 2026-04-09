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
}
