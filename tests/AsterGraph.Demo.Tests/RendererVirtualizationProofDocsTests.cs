using System;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class RendererVirtualizationProofDocsTests
{
    [Fact]
    public void ParityDocs_RecordPhase489AsDocsOnlyDesignSpike()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 489", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #101", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-6sc", contents, StringComparison.Ordinal);
            Assert.Contains("perf/renderer-virtualization-spike", contents, StringComparison.Ordinal);
            Assert.Contains("docs/tests", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("public API", contents, StringComparison.Ordinal);
            Assert.Contains("runtime change", contents, StringComparison.Ordinal);
            Assert.Contains("renderer virtualization design spike", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("viewport-budgeted scene projection/rendering", contents, StringComparison.Ordinal);
            Assert.Contains("telemetry-only", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("not a true renderer virtualization contract", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("不是真正的 renderer virtualization contract", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ScaleDocs_DefineRendererVirtualizationProofContractInBothLocales()
    {
        var englishScale = ReadRepoFile("docs/en/scale-baseline.md");
        var chineseScale = ReadRepoFile("docs/zh-CN/scale-baseline.md");

        foreach (var contents in new[] { englishScale, chineseScale })
        {
            Assert.Contains("Renderer Virtualization Proof Contract", contents, StringComparison.Ordinal);
            Assert.Contains("viewport-budgeted scene projection/rendering", contents, StringComparison.Ordinal);
            Assert.Contains("renderer virtualization contract", contents, StringComparison.Ordinal);
            Assert.Contains("ItemsRepeater/Skia-style", contents, StringComparison.Ordinal);
            Assert.Contains("background graph index", contents, StringComparison.Ordinal);
            Assert.Contains("renderer thresholds", contents, StringComparison.Ordinal);
            Assert.Contains("repeatable proof command", contents, StringComparison.Ordinal);
            Assert.Contains("focused renderer tests", contents, StringComparison.Ordinal);
            Assert.Contains("artifact metadata", contents, StringComparison.Ordinal);
            Assert.Contains("incremental visual lifecycle", contents, StringComparison.Ordinal);
            Assert.Contains("invalidation evidence", contents, StringComparison.Ordinal);
            Assert.Contains("telemetry-only", contents, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var contents in new[] { englishScale, chineseScale })
        {
            Assert.False(
                HasLineWithAll(contents, "xlarge", "is a support claim"),
                "Scale docs must not turn xlarge telemetry into a support claim.");
        }
    }

    [Fact]
    public void ParityDocs_RecordPhase499AsCurrentDocsOnlyExecutionBoundarySlice()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 499", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #121", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-9x7", contents, StringComparison.Ordinal);
            Assert.Contains("perf/phase-499-renderer-virtualization-boundary", contents, StringComparison.Ordinal);
            Assert.Contains("docs/tests", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("viewport-budgeted scene projection/rendering", contents, StringComparison.Ordinal);
            Assert.Contains("renderer virtualization execution", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("xlarge", contents, StringComparison.Ordinal);
            Assert.Contains("telemetry-only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("public API", contents, StringComparison.Ordinal);
            Assert.Contains("UI", contents, StringComparison.Ordinal);
        }

        Assert.Contains("does not authorize a renderer rewrite", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("不授权 renderer rewrite", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ScaleDocs_RecordPhase499ExecutionBoundaryWithoutImplementationAuthorization()
    {
        var englishScale = ReadRepoFile("docs/en/scale-baseline.md");
        var chineseScale = ReadRepoFile("docs/zh-CN/scale-baseline.md");

        foreach (var contents in new[] { englishScale, chineseScale })
        {
            Assert.Contains("Phase 499", contents, StringComparison.Ordinal);
            Assert.Contains("execution boundary", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/tests-only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("benchmark harness implementation", contents, StringComparison.Ordinal);
            Assert.Contains("renderer rewrite", contents, StringComparison.Ordinal);
            Assert.Contains("support-claim expansion", contents, StringComparison.Ordinal);
            Assert.Contains("CI-repeatable evidence", contents, StringComparison.Ordinal);
            Assert.Contains("non-informational renderer thresholds", contents, StringComparison.Ordinal);
            Assert.Contains("repeatable proof command output", contents, StringComparison.Ordinal);
            Assert.Contains("artifact metadata", contents, StringComparison.Ordinal);
            Assert.Contains("incremental visual lifecycle evidence", contents, StringComparison.Ordinal);
            Assert.Contains("invalidation evidence", contents, StringComparison.Ordinal);
            Assert.Contains("connection preview preservation", contents, StringComparison.Ordinal);
            Assert.Contains("full collection scan", contents, StringComparison.Ordinal);
            Assert.Contains("full scene rebuild", contents, StringComparison.Ordinal);
        }

        Assert.Contains("separate implementation issue", englishScale, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("单独的 implementation issue", chineseScale, StringComparison.Ordinal);
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

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
        => contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));
}
