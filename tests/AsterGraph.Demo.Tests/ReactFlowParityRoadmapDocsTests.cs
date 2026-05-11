using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ReactFlowParityRoadmapDocsTests
{
    [Fact]
    public void ParityRoadmapDocs_RecordPhase490RepairInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 490", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #103", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-3x0", contents, StringComparison.Ordinal);
            Assert.Contains("stale-roadmap repair", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/tests only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("No Core/Editor/Avalonia runtime or public API changes", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_RecordClosedPhase485Through489Trackers()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("#93", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-3hc", contents, StringComparison.Ordinal);
            Assert.Contains("PR #94", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 485", contents, StringComparison.Ordinal);

            Assert.Contains("#95", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-0xr", contents, StringComparison.Ordinal);
            Assert.Contains("PR #96", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 486", contents, StringComparison.Ordinal);

            Assert.Contains("#97", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-i8s", contents, StringComparison.Ordinal);
            Assert.Contains("PR #98", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 487", contents, StringComparison.Ordinal);

            Assert.Contains("#99", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-ce1", contents, StringComparison.Ordinal);
            Assert.Contains("PR #100", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 488", contents, StringComparison.Ordinal);

            Assert.Contains("#101", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-6sc", contents, StringComparison.Ordinal);
            Assert.Contains("PR #102", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 489", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_RemoveStaleActiveAndCandidateCompletedWork()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.DoesNotContain("Phase 489 is the current active P2 item", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("TBD | TBD | Cookbook example architecture refresh", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("Current active branch", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("当前 active branch", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("docs/cookbook-example-architecture", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("perf/renderer-virtualization-spike`: active", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ParityRoadmapDocs_SelectAccessibilityBreadthAsNextOpenGap()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Accessibility breadth audit", contents, StringComparison.Ordinal);
            Assert.Contains("Retained migration removal roadmap", contents, StringComparison.Ordinal);
            Assert.Contains("next open parity gap", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("future tracker item", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("accessibility breadth audit is the next open parity gap", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Accessibility breadth audit 是下一项 open parity gap", chineseParity, StringComparison.Ordinal);
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
}
