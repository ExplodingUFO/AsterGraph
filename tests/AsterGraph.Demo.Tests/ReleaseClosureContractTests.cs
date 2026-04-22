using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ReleaseClosureContractTests
{
    [Fact]
    public void PrereleaseNotes_LockPackageVersionAndSurfacePositiveWpfMatrixProof()
    {
        var tempRoot = CreateTempDirectory();
        var proofRoot = Path.Combine(tempRoot, "proof");
        Directory.CreateDirectory(proofRoot);

        WriteProofFile(proofRoot, "public-repo-hygiene.txt", "PUBLIC_REPO_HYGIENE_OK:True");
        WriteProofFile(proofRoot, "hostsample-packed.txt", "HOST_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "consumer-sample.txt", "CONSUMER_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "demo-proof.txt", "DEMO_OK:True");
        WriteProofFile(proofRoot, "hostsample-net10-packed.txt", "HOST_SAMPLE_NET10_OK:True");
        WriteProofFile(proofRoot, "package-smoke.txt", "PACKAGE_SMOKE_OK:True");
        WriteProofFile(
            proofRoot,
            "scale-smoke.txt",
            "SCALE_TIER_BUDGET:baseline`nSCALE_PERFORMANCE_BUDGET_OK:baseline:True:none`nSCALE_HISTORY_CONTRACT_OK:True");
        WriteProofFile(proofRoot, "hello-world-wpf-proof.txt", "HELLOWORLD_WPF_OK:True");
        WriteProofFile(
            proofRoot,
            "wpf-adapter-capability-matrix.txt",
            string.Join(
                Environment.NewLine,
                [
                    "ADAPTER_CAPABILITY_MATRIX_FORMAT:1",
                    "ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS",
                    "ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS",
                ]));

        var packageVersion = GetPackageVersion();
        var publicTag = $"v{packageVersion}";
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
        var process = RunPowerShell(
            $"-NoProfile -ExecutionPolicy Bypass -File \"{Path.Combine(GetRepositoryRoot(), "eng", "write-prerelease-notes.ps1")}\" " +
            $"-RepoRoot \"{GetRepositoryRoot()}\" " +
            $"-ProofRoot \"{proofRoot}\" " +
            $"-OutputPath \"{outputPath}\" " +
            $"-PublicTag \"{publicTag}\" " +
            $"-CoverageSummaryPath \"{coveragePath}\"");

        Assert.True(
            process.ExitCode == 0,
            $"write-prerelease-notes.ps1 failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{process.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{process.StandardError}");

        var notes = File.ReadAllText(outputPath);
        Assert.Contains($"- installable package version: `{packageVersion}`", notes, StringComparison.Ordinal);
        Assert.Contains($"- matching public tag: `{publicTag}`", notes, StringComparison.Ordinal);
        Assert.Contains("## Support Story", notes, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)", notes, StringComparison.Ordinal);
        Assert.Contains("[Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md)", notes, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("adapter-2 validation only", notes, StringComparison.Ordinal);
        Assert.Contains("does not widen the public publish/package boundary", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", notes, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:MISSING", notes, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:MISSING", notes, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseChecklists_CarrySupportBoundaryStoryAndKeepWpfValidationOnly()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");

        Assert.Contains("frozen support boundary story", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("adapter matrix story", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("`HELLOWORLD_WPF_OK` as adapter-2 validation only", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("historical alpha reference for the current beta support story", englishChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK is Avalonia/WPF parity", englishChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK is public WPF support", englishChecklist, StringComparison.Ordinal);

        Assert.Contains("冻结的 support boundary 叙事", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("adapter matrix 叙事", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("`HELLOWORLD_WPF_OK` 只当成 adapter-2 验证通过", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("历史 alpha 参考，服务于当前 beta support story", chineseChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK 是 Avalonia/WPF parity", chineseChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK 是公开 WPF support", chineseChecklist, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("docs/en/adapter-capability-matrix.md")]
    [InlineData("docs/zh-CN/adapter-capability-matrix.md")]
    public void AdapterMatrixDocs_UseLockedVocabularyAndRejectMissingWpfRows(string relativePath)
    {
        var contents = ReadRepoFile(relativePath);

        Assert.Contains("Matrix Vocabulary", contents, StringComparison.Ordinal);
        Assert.Contains("Matrix Categories", contents, StringComparison.Ordinal);
        Assert.Contains("Supported", contents, StringComparison.Ordinal);
        Assert.Contains("Partial", contents, StringComparison.Ordinal);
        Assert.Contains("Fallback", contents, StringComparison.Ordinal);

        AssertMatrixRowStatus(contents, "Canonical runtime/session route", "Supported", "Supported");
        AssertMatrixRowStatus(contents, "Hosted full editor shell", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Standalone surfaces", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Command and tool projection", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Authoring presentation", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Platform integration", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Proof and sample coverage", "Supported", "Supported");
    }

    [Fact]
    public void CiScript_ReservesMissingForAbsentWpfProofAndUsesProofMarkers()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_PROOF:MISSING", script, StringComparison.Ordinal);
        Assert.Contains("Get-ProofMarkerLine -ProofText $proofText -Marker 'HELLOWORLD_WPF_OK'", script, StringComparison.Ordinal);
        Assert.Contains("Get-ProofMarkerLine -ProofText $proofText -Marker 'COMMAND_SURFACE_OK'", script, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:$(Convert-TextToCapabilityStatus -Value $helloWorldWpfOk)", script, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:$(Convert-TextToCapabilityStatus -Value $commandSurfaceOk)", script, StringComparison.Ordinal);
    }

    [Fact]
    public void WpfAdapterMatrixProof_MapsPositiveMarkersToPassNotMissing()
    {
        var tempRoot = CreateTempDirectory();
        var proofPath = Path.Combine(tempRoot, "hello-world-wpf-proof.txt");
        var outputPath = Path.Combine(tempRoot, "wpf-adapter-capability-matrix.txt");
        File.WriteAllText(proofPath, "HELLOWORLD_WPF_OK:True`nCOMMAND_SURFACE_OK:True".Replace("`n", Environment.NewLine, StringComparison.Ordinal));

        var script = ReadRepoFile("eng/ci.ps1");
        var helperBlock = ExtractWpfMatrixHelperBlock(script);
        var helperScriptPath = Path.Combine(tempRoot, "invoke-wpf-matrix-proof.ps1");
        File.WriteAllText(
            helperScriptPath,
            helperBlock +
            Environment.NewLine +
            "$proofArtifactsRoot = Split-Path -Parent '" + EscapePowerShellSingleQuote(outputPath) + "'" +
            Environment.NewLine +
            $"New-WpfAdapterCapabilityMatrixProof -HelloWorldProofPath '{EscapePowerShellSingleQuote(proofPath)}' -OutputPath '{EscapePowerShellSingleQuote(outputPath)}'");

        var process = RunPowerShell($"-NoProfile -ExecutionPolicy Bypass -File \"{helperScriptPath}\"");

        Assert.True(
            process.ExitCode == 0,
            $"WPF matrix proof helper failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{process.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{process.StandardError}");

        var matrix = File.ReadAllText(outputPath);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", matrix, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", matrix, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", matrix, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:MISSING", matrix, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:MISSING", matrix, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string GetPackageVersion()
    {
        var props = XDocument.Load(Path.Combine(GetRepositoryRoot(), "Directory.Build.props"));
        var version = props.Root?
            .Elements("PropertyGroup")
            .Elements("Version")
            .Select(node => node.Value)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException("Could not read package version from Directory.Build.props.");
        }

        return version.Trim();
    }

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

    private static string ExtractWpfMatrixHelperBlock(string script)
    {
        var start = script.IndexOf("function Convert-TextToCapabilityStatus", StringComparison.Ordinal);
        var end = script.IndexOf("function Reset-Directory", StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start, "The ci.ps1 helper block could not be located.");

        return script.Substring(start, end - start).TrimEnd();
    }

    private static string EscapePowerShellSingleQuote(string value)
        => value.Replace("'", "''", StringComparison.Ordinal);

    private static void WriteProofFile(string proofRoot, string fileName, string contents)
        => File.WriteAllText(Path.Combine(proofRoot, fileName), contents.Replace("`n", Environment.NewLine, StringComparison.Ordinal));

    private static ProcessResult RunPowerShell(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        process!.WaitForExit();

        return new ProcessResult(
            process.ExitCode,
            process.StandardOutput.ReadToEnd(),
            process.StandardError.ReadToEnd());
    }

    private static void AssertMatrixRowStatus(string contents, string rowName, string expectedAvaloniaStatus, string expectedWpfStatus)
    {
        const string matrixHeader = "## Phase 157";
        var matrixStart = contents.IndexOf(matrixHeader, StringComparison.Ordinal);
        Assert.True(matrixStart >= 0, "The adapter capability matrix must include the Phase 157 Matrix section.");

        var matrixContents = contents[(matrixStart + matrixHeader.Length)..];
        var row = matrixContents
            .Split('\n', StringSplitOptions.TrimEntries)
            .FirstOrDefault(line => line.StartsWith($"| {rowName} |", StringComparison.Ordinal));

        Assert.NotNull(row);

        var columns = row!
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        Assert.Equal(rowName, columns[0]);
        Assert.Equal(expectedAvaloniaStatus, columns[1].Trim('`'));
        Assert.Equal(expectedWpfStatus, columns[2].Trim('`'));
        Assert.False(string.Equals(columns[1], "MISSING", StringComparison.OrdinalIgnoreCase));
        Assert.False(string.Equals(columns[2], "MISSING", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
