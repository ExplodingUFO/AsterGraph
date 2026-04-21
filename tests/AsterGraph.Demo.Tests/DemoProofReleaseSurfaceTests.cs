using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoProofReleaseSurfaceTests
{
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
            "SCALE_TIER_BUDGET:baseline`nSCALE_PERFORMANCE_BUDGET_OK:baseline:True:16`nSCALE_HISTORY_CONTRACT_OK:True");
        WriteProofFile(
            proofRoot,
            "demo-proof.txt",
            string.Join("`n", ["DEMO_OK:True", .. DemoProofContract.CreatePublicSuccessMarkerLines()]));

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
}
