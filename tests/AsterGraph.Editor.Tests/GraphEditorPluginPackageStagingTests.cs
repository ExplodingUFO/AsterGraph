using AsterGraph.Editor.Plugins;
using AsterGraph.Editor.Plugins.Internal;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorPluginPackageStagingTests
{
    [Fact]
    public void Stage_WithDeclaredPayloadAndTrustedCandidate_StagesPackageIntoDeterministicCache()
    {
        var tempDirectory = CreateTempDirectory();
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackageWithPluginPayload(
            tempDirectory,
            "AsterGraph.PackageStage.Success",
            "1.0.0",
            title: "Trusted Stage Candidate",
            description: "Stages trusted package payloads into a deterministic cache.",
            pluginAssemblyPath: GetSamplePluginAssemblyPath(),
            pluginTypeName: "AsterGraph.TestPlugins.SamplePlugin");
        var stagingRoot = Path.Combine(tempDirectory, "staging-root");
        var candidate = CreateCandidate(packagePath);

        var stage = AsterGraphPluginPackageStagingService.Stage(new GraphEditorPluginPackageStageRequest(candidate, stagingRoot));

        Assert.Equal(GraphEditorPluginStageOutcome.Staged, stage.Outcome);
        Assert.False(stage.UsedCache);
        Assert.NotNull(stage.PackageIdentity);
        Assert.Equal("AsterGraph.PackageStage.Success", stage.PackageIdentity!.Id);
        Assert.Equal("1.0.0", stage.PackageIdentity.Version);
        Assert.NotNull(stage.StagingDirectory);
        Assert.NotNull(stage.MainAssemblyPath);
        Assert.Equal("AsterGraph.TestPlugins.SamplePlugin", stage.PluginTypeName);
        Assert.StartsWith(Path.GetFullPath(stagingRoot), stage.StagingDirectory!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Path.Combine("AsterGraph.PackageStage.Success", "1.0.0"), stage.StagingDirectory!, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(stage.MainAssemblyPath));
        Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(stage.MainAssemblyPath!)!, "AsterGraph.TestPlugins.deps.json")));
        Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(stage.MainAssemblyPath!)!, "AsterGraph.TestPlugins.runtimeconfig.json")));
    }

    [Fact]
    public void Stage_WhenPackageAndTrustFactsAreUnchanged_ReturnsCacheHit()
    {
        var tempDirectory = CreateTempDirectory();
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackageWithPluginPayload(
            tempDirectory,
            "AsterGraph.PackageStage.Cache",
            "1.0.0",
            title: "Cache Stage Candidate",
            description: "Reuses staged outputs when package and trust facts remain unchanged.",
            pluginAssemblyPath: GetSamplePluginAssemblyPath(),
            pluginTypeName: "AsterGraph.TestPlugins.SamplePlugin");
        var stagingRoot = Path.Combine(tempDirectory, "staging-root");
        var candidate = CreateCandidate(packagePath);
        var request = new GraphEditorPluginPackageStageRequest(candidate, stagingRoot);

        var first = AsterGraphPluginPackageStagingService.Stage(request);
        var second = AsterGraphPluginPackageStagingService.Stage(request);

        Assert.Equal(GraphEditorPluginStageOutcome.Staged, first.Outcome);
        Assert.Equal(GraphEditorPluginStageOutcome.CacheHit, second.Outcome);
        Assert.True(second.UsedCache);
        Assert.Equal(first.StagingDirectory, second.StagingDirectory);
        Assert.Equal(first.MainAssemblyPath, second.MainAssemblyPath);
        Assert.Equal(first.PluginTypeName, second.PluginTypeName);
    }

    [Fact]
    public void Stage_WithoutDeclaredPayloadMetadata_RefusesPackageBeforeExtraction()
    {
        var tempDirectory = CreateTempDirectory();
        var packagePath = PluginPackageTestHelper.CreateUnsignedPackageWithPluginPayload(
            tempDirectory,
            "AsterGraph.PackageStage.MissingMetadata",
            "1.0.0",
            title: "Missing Metadata Candidate",
            description: "Refuses packages that do not declare a payload entry.",
            pluginAssemblyPath: GetSamplePluginAssemblyPath(),
            pluginTypeName: "AsterGraph.TestPlugins.SamplePlugin",
            includePluginMetadata: false);
        var candidate = CreateCandidate(packagePath);

        var stage = AsterGraphPluginPackageStagingService.Stage(new GraphEditorPluginPackageStageRequest(candidate, Path.Combine(tempDirectory, "staging-root")));

        Assert.Equal(GraphEditorPluginStageOutcome.Refused, stage.Outcome);
        Assert.False(stage.UsedCache);
        Assert.Null(stage.StagingDirectory);
        Assert.Null(stage.MainAssemblyPath);
        Assert.Null(stage.PluginTypeName);
        Assert.Equal("stage.payload.not-declared", stage.ReasonCode);
        Assert.NotNull(stage.ReasonMessage);
    }

    private static GraphEditorPluginCandidateSnapshot CreateCandidate(string packagePath)
    {
        var inspection = AsterGraphPluginPackageArchiveInspector.Inspect(packagePath);
        return new GraphEditorPluginCandidateSnapshot(
            GraphEditorPluginCandidateSourceKind.PackageDirectory,
            Path.GetDirectoryName(packagePath) ?? packagePath,
            inspection.Manifest,
            AsterGraphPluginPreloadEvaluator.EvaluateCompatibility(inspection.Manifest),
            new GraphEditorPluginTrustEvaluation(
                GraphEditorPluginTrustDecision.Allowed,
                GraphEditorPluginTrustEvaluationSource.HostPolicy,
                "trust.allowed.package-staging-tests",
                "Allowed for package staging tests."),
            new GraphEditorPluginProvenanceEvidence(
                inspection.ProvenanceEvidence.PackageIdentity,
                new GraphEditorPluginSignatureEvidence(
                    GraphEditorPluginSignatureStatus.Valid,
                    GraphEditorPluginSignatureKind.Repository,
                    new GraphEditorPluginSignerIdentity("AsterGraph Tests", "STAGE1234"),
                    reasonCode: "signature.valid.package-staging-tests",
                    reasonMessage: "Valid signature fixture for package staging tests.")),
            packagePath: packagePath);
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "astergraph-plugin-staging-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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
}
