using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoProofReleaseSurfaceTests
{
    private static readonly string[] AdapterMatrixProofMarkerLines =
    [
        "HELLOWORLD_WPF_OK:True",
    ];

    private static readonly string[] OfficialCapabilityModules =
    [
        "Selection",
        "History",
        "Clipboard",
        "Shortcut Policy",
        "Layout",
        "MiniMap",
        "Stencil",
        "Fragment Library",
        "Export",
        "Baseline Edge Authoring",
    ];

    private static readonly string[] AdvancedEditingCapabilityModules =
    [
        "Node Surface Authoring",
        "Hierarchy Semantics",
        "Composite Scope Authoring",
        "Edge Semantics",
        "Edge Geometry Tooling",
    ];

    [Fact]
    public void QuickStart_UsesAvaloniaAsDefaultOnboardingPath()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        foreach (var contents in new[] { quickStart, quickStartZh })
        {
            Assert.Contains("onboarding", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("default", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Avalonia", contents, StringComparison.Ordinal);
            Assert.False(
                HasLineWith(contents, "WPF", "onboarding"),
                "Quick Start docs must not describe WPF as an onboarding path.");
        }
    }

    [Fact]
    public void HostIntegrationDocs_RequireCanonicalRouteThenAdapterForWpf()
    {
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");

        foreach (var contents in new[] { hostIntegration, hostIntegrationZh })
        {
            Assert.Contains("canonical", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("adapter", contents, StringComparison.OrdinalIgnoreCase);
            Assert.True(HasLineWith(contents, "WPF", "partial"), "WPF Partial guidance must appear in host integration docs.");
            Assert.True(HasLineWith(contents, "WPF", "fallback"), "WPF Fallback guidance must appear in host integration docs.");
            Assert.True(HasLineWith(contents, "WPF", "host-owned"), "WPF flow must reference host-owned projection.");
            Assert.False(
                HasLineWith(contents, "WPF", "retained")
                && HasLineWith(contents, "WPF", "MVVM"),
                "WPF Partial/Fallback guidance must avoid retained-MVVM claim.");
            Assert.Contains("session", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("runtime", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("projection", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("canonical route", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("adapter-specific", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void RepositorySurface_UsesAsterGraphSolutionName()
    {
        var repoRoot = GetRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(repoRoot, "AsterGraph.sln")));
        Assert.False(File.Exists(Path.Combine(repoRoot, "avalonia-node-map.sln")));
        Assert.Contains("AsterGraph.sln", ReadRepoFile("CONTRIBUTING.md"), StringComparison.Ordinal);
    }

    [Fact]
    public void CiScript_CapturesDemoProofArtifactForProofRing()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("demo-proof.txt", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-DemoProof", script, StringComparison.Ordinal);
        foreach (var requiredProofLine in DemoProofContract.CreatePublicSuccessMarkerLines())
        {
            Assert.Contains(requiredProofLine, script, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CiScript_PublishesStressTelemetrySummaryAlongsideDefendedScaleProof()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("'baseline'", script, StringComparison.Ordinal);
        Assert.Contains("'large'", script, StringComparison.Ordinal);
        Assert.Contains("'stress'", script, StringComparison.Ordinal);
        Assert.Contains("'--samples'", script, StringComparison.Ordinal);
        Assert.Contains("'3'", script, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseWorkflowSummaries_SurfaceDemoProofMarkers()
    {
        var ciWorkflow = ReadRepoFile(".github/workflows/ci.yml");
        var releaseWorkflow = ReadRepoFile(".github/workflows/release.yml");

        Assert.Contains("artifacts/proof/demo-proof.txt", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", ciWorkflow, StringComparison.Ordinal);
        foreach (var markerId in DemoProofContract.PublicSuccessMarkerIds)
        {
            Assert.Contains(markerId, ciWorkflow, StringComparison.Ordinal);
        }

        Assert.Contains("artifacts/proof/demo-proof.txt", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("DEMO_OK", releaseWorkflow, StringComparison.Ordinal);
        foreach (var markerId in DemoProofContract.PublicSuccessMarkerIds)
        {
            Assert.Contains(markerId, releaseWorkflow, StringComparison.Ordinal);
        }

        foreach (var markerLine in AdapterMatrixProofMarkerLines)
        {
            Assert.Contains(markerLine, ciWorkflow, StringComparison.Ordinal);
            Assert.Contains(markerLine, releaseWorkflow, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CiScript_CapturesWpfAdapterMatrixProofArtifact()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("helloWorldWpfProofPath", script, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK:True", script, StringComparison.Ordinal);
        Assert.Contains("Validate WPF richer sample proof", script, StringComparison.Ordinal);
    }

    [Fact]
    public void CiWorkflow_IncludesMacOsValidationOnCanonicalSolutionPath()
    {
        var ciWorkflow = ReadRepoFile(".github/workflows/ci.yml");

        Assert.Contains("macos-validation", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("runs-on: macos-latest", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("./eng/ci.ps1 -Lane all -Framework all -Configuration Release", ciWorkflow, StringComparison.Ordinal);
        Assert.Contains("- macos-validation", ciWorkflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ArchitectureDocs_DescribeKernelSceneAdapterSplitAndStabilityLevels()
    {
        var architectureDoc = ReadRepoFile("docs/en/architecture.md");
        var architectureDocZh = ReadRepoFile("docs/zh-CN/architecture.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        foreach (var contents in new[] { architectureDoc, architectureDocZh })
        {
            Assert.Contains("Editor Kernel", contents, StringComparison.Ordinal);
            Assert.Contains("Scene/Interaction", contents, StringComparison.Ordinal);
            Assert.Contains("UI Adapter", contents, StringComparison.Ordinal);
            Assert.Contains("CreateSession(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorSession", contents, StringComparison.Ordinal);
            Assert.Contains("stability", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("architecture", quickStart, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("architecture", quickStartZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PublicDocs_DescribeOfficialCapabilityModulesAcrossRoutes()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");

        foreach (var contents in new[] { readme, readmeZh, hostIntegration, hostIntegrationZh })
        {
            foreach (var moduleName in OfficialCapabilityModules)
            {
                Assert.Contains(moduleName, contents, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void AdvancedEditingDocs_DefineCanonicalModulesAndProofCoverage()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var editorReadme = ReadRepoFile("src/AsterGraph.Editor/README.md");
        var avaloniaReadme = ReadRepoFile("src/AsterGraph.Avalonia/README.md");
        var advancedEditing = ReadRepoFile("docs/en/advanced-editing.md");
        var advancedEditingZh = ReadRepoFile("docs/zh-CN/advanced-editing.md");
        var demoGuide = ReadRepoFile("docs/en/demo-guide.md");
        var demoGuideZh = ReadRepoFile("docs/zh-CN/demo-guide.md");

        foreach (var contents in new[] { readme, readmeZh, advancedEditing, advancedEditingZh })
        {
            foreach (var moduleName in AdvancedEditingCapabilityModules)
            {
                Assert.Contains(moduleName, contents, StringComparison.Ordinal);
            }
        }

        foreach (var contents in new[] { editorReadme, avaloniaReadme })
        {
            Assert.Contains("advanced-editing", contents, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var contents in new[] { demoGuide, demoGuideZh })
        {
            Assert.Contains("HIERARCHY_SEMANTICS_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("EDGE_GEOMETRY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("Node Surface Authoring", contents, StringComparison.Ordinal);
            Assert.Contains("Hierarchy Semantics", contents, StringComparison.Ordinal);
            Assert.Contains("Edge Geometry Tooling", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void QuickStartAndArchitecture_MapCapabilityModulesToProofLanes()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var architectureZh = ReadRepoFile("docs/zh-CN/architecture.md");

        foreach (var contents in new[] { quickStart, quickStartZh })
        {
            Assert.Contains("HostSample", contents, StringComparison.Ordinal);
            Assert.Contains("PackageSmoke", contents, StringComparison.Ordinal);
            Assert.Contains("ScaleSmoke", contents, StringComparison.Ordinal);
            Assert.Contains("Demo", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { architecture, architectureZh })
        {
            Assert.Contains("capability modules", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Host Integration", contents, StringComparison.Ordinal);
            Assert.Contains("Quick Start", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SecondAdapterDocs_LockWpfAdapterMatrixContractAcrossLanguages()
    {
        var readme = ReadRepoFile("README.md");
        var readmeZh = ReadRepoFile("README.zh-CN.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var architectureZh = ReadRepoFile("docs/zh-CN/architecture.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var projectStatusZh = ReadRepoFile("docs/zh-CN/project-status.md");
        var alphaStatus = ReadRepoFile("docs/en/alpha-status.md");
        var alphaStatusZh = ReadRepoFile("docs/zh-CN/alpha-status.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var adapterMatrix = ReadRepoFile("docs/en/adapter-capability-matrix.md");
        var adapterMatrixZh = ReadRepoFile("docs/zh-CN/adapter-capability-matrix.md");

        foreach (var contents in new[]
                 {
                     readme,
                     readmeZh,
                     hostIntegration,
                     hostIntegrationZh,
                     architecture,
                     architectureZh,
                     projectStatus,
                     projectStatusZh,
                     alphaStatus,
                     alphaStatusZh,
                     quickStart,
                     quickStartZh,
                     adapterMatrix,
                     adapterMatrixZh,
                 })
        {
            Assert.Contains("WPF", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { readme, readmeZh, hostIntegration, hostIntegrationZh, architecture, architectureZh, projectStatus, projectStatusZh, alphaStatus, alphaStatusZh, quickStart, quickStartZh })
        {
            Assert.Contains("adapter-capability-matrix.md", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("adapter 2", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Supported", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("Partial", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("Fallback", adapterMatrix, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 154", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CreateSession(...)", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorSession", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Editor", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("adapter-specific runtime APIs", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Retained migration is not", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("Fallback Rule", adapterMatrix, StringComparison.Ordinal);
        Assert.Contains("lower-level documented path", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.True(LineHasAdapterStatus(adapterMatrix, "Avalonia", "supported"));
        Assert.True(LineHasAdapterStatus(adapterMatrix, "WPF", "partial"));
        Assert.True(LineHasAdapterStatus(adapterMatrix, "WPF", "fallback"));

        Assert.Contains("adapter 2", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("supported", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("partial", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("fallback", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("must not exceed", adapterMatrix, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("must not exceed", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Phase 154", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CreateSession(...)", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorSession", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Editor", adapterMatrixZh, StringComparison.Ordinal);
        Assert.Contains("retained", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("runtime API", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fallback", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("proof", adapterMatrixZh, StringComparison.OrdinalIgnoreCase);
        Assert.True(LineHasAdapterStatus(adapterMatrixZh, "Avalonia", "supported"));
        Assert.True(LineHasAdapterStatus(adapterMatrixZh, "WPF", "partial"));
        Assert.True(LineHasAdapterStatus(adapterMatrixZh, "WPF", "fallback"));
    }

    [Fact]
    public void WritePrereleaseNotes_IncludeDemoProofMarkersInProofSummary()
    {
        var tempRoot = CreateTempDirectory();
        var proofRoot = Path.Combine(tempRoot, "proof");
        Directory.CreateDirectory(proofRoot);

        WriteProofFile(proofRoot, "public-repo-hygiene.txt", "PUBLIC_REPO_HYGIENE_OK:True");
        WriteProofFile(proofRoot, "hostsample-packed.txt", "HOST_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "consumer-sample.txt", "CONSUMER_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "hostsample-net10-packed.txt", "HOST_SAMPLE_NET10_OK:True");
        WriteProofFile(proofRoot, "package-smoke.txt", "PACKAGE_SMOKE_OK:True");
        WriteProofFile(
            proofRoot,
            "scale-smoke.txt",
            "SCALE_TIER_BUDGET:baseline`nSCALE_PERFORMANCE_BUDGET_OK:baseline:True:none`nSCALE_TIER_BUDGET:large`nSCALE_PERFORMANCE_BUDGET_OK:large:True:none`nSCALE_TIER_BUDGET:stress:nodes=5000:selection=256:moves=96:budget=informational-only`nSCALE_PERFORMANCE_BUDGET_OK:stress:True:informational-only`nSCALE_PERF_SUMMARY:stress:samples=3:setup-p50=311:setup-p95=569:selection-p50=18:selection-p95=41:connection-p50=582:connection-p95=820:history-p50=912:history-p95=946:viewport-p50=2:viewport-p95=5:save-p50=149:save-p95=156:reload-p50=62:reload-p95=65`nSCALE_HISTORY_CONTRACT_OK:True");
        WriteProofFile(
            proofRoot,
            "demo-proof.txt",
            string.Join("`n", ["DEMO_OK:True", .. DemoProofContract.CreatePublicSuccessMarkerLines()]));
        WriteProofFile(proofRoot, "hello-world-wpf-proof.txt", string.Join("`n", AdapterMatrixProofMarkerLines));

        var coveragePath = Path.Combine(tempRoot, "coverage-summary.json");
        File.WriteAllText(
            coveragePath,
            """
            {
              "coveredLines": 10,
              "totalLines": 20,
              "lineRate": 0.5
            }
            """);

        var outputPath = Path.Combine(tempRoot, "prerelease-notes.md");
        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments =
                $"-NoProfile -ExecutionPolicy Bypass -File \"{Path.Combine(GetRepositoryRoot(), "eng", "write-prerelease-notes.ps1")}\" " +
                $"-RepoRoot \"{GetRepositoryRoot()}\" " +
                $"-ProofRoot \"{proofRoot}\" " +
                $"-OutputPath \"{outputPath}\" " +
                $"-CoverageSummaryPath \"{coveragePath}\"",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        process!.WaitForExit();

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        Assert.True(
            process.ExitCode == 0,
            $"write-prerelease-notes.ps1 failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");

        var notes = File.ReadAllText(outputPath);
        foreach (var requiredProofLine in new[] { "DEMO_OK:True" }.Concat(DemoProofContract.CreatePublicSuccessMarkerLines()))
        {
            Assert.Contains(requiredProofLine, notes, StringComparison.Ordinal);
        }

        foreach (var requiredMatrixProofLine in AdapterMatrixProofMarkerLines)
        {
            Assert.Contains(requiredMatrixProofLine, notes, StringComparison.Ordinal);
        }

        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:baseline:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:samples=3", notes, StringComparison.Ordinal);
    }

    [Fact]
    public void ScaleDocs_DistinguishDefendedBudgetsFromStressTelemetry()
    {
        var scaleBaseline = ReadRepoFile("docs/en/scale-baseline.md");
        var scaleBaselineZh = ReadRepoFile("docs/zh-CN/scale-baseline.md");
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var checklistZh = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        foreach (var contents in new[] { scaleBaseline, scaleBaselineZh })
        {
            Assert.Contains("baseline", contents, StringComparison.Ordinal);
            Assert.Contains("large", contents, StringComparison.Ordinal);
            Assert.Contains("stress", contents, StringComparison.Ordinal);
            Assert.Contains("SCALE_PERF_SUMMARY", contents, StringComparison.Ordinal);
        }

        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:...", checklist, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERFORMANCE_BUDGET_OK:large:True:...", checklistZh, StringComparison.Ordinal);
        Assert.Contains("SCALE_PERF_SUMMARY:stress:...", checklistZh, StringComparison.Ordinal);

        Assert.Contains("ADAPTER_CAPABILITY_MATRIX", checklist, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", checklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX", checklistZh, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK", checklistZh, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Failed to locate repository root from test base directory.");
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void WriteProofFile(string proofRoot, string fileName, string contents)
        => File.WriteAllText(Path.Combine(proofRoot, fileName), contents.Replace("`n", Environment.NewLine, StringComparison.Ordinal));

    private static bool LineHasAdapterStatus(string contents, string subject, string requiredStatus)
    {
        return contents
            .Split('\n')
            .Any(line =>
                line.Contains(subject, StringComparison.OrdinalIgnoreCase) &&
                line.Contains(requiredStatus, StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasLineWith(string contents, string requiredTerm, string requiredCompanion)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line =>
                line.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) &&
                line.Contains(requiredCompanion, StringComparison.OrdinalIgnoreCase));
    }
}
