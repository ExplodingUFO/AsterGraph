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
            Assert.Contains("accessibility breadth audit", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Phase 491", contents, StringComparison.Ordinal);
            Assert.Contains("#105", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 491 now owns the accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 491 现在负责 accessibility breadth audit", chineseParity, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.DoesNotContain("| TBD | TBD | Accessibility breadth audit", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase491AccessibilityBreadthAuditInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 491", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #105", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-44i", contents, StringComparison.Ordinal);
            Assert.Contains("source-backed contract", contents, StringComparison.Ordinal);
            Assert.Contains("dynamic screen-reader announcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Accessibility breadth audit", contents, StringComparison.Ordinal);
        }

        Assert.Contains(
            "| #105 | `avalonia-node-map-44i` | Phase 491: audit accessibility breadth across built-ins and shell states",
            englishParity,
            StringComparison.Ordinal);
        Assert.Contains(
            "| #105 | `avalonia-node-map-44i` | Phase 491: audit accessibility breadth across built-ins and shell states",
            chineseParity,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase492RetainedMigrationRoadmapInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 492", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #107", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-j8v", contents, StringComparison.Ordinal);
            Assert.Contains("Retained migration removal roadmap", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("inventory now, remove later", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Retained migration removal roadmap", contents, StringComparison.Ordinal);
        }

        Assert.Contains(
            "| #107 | `avalonia-node-map-j8v` | Phase 492: inventory retained migration surfaces and define removal roadmap",
            englishParity,
            StringComparison.Ordinal);
        Assert.Contains(
            "| #107 | `avalonia-node-map-j8v` | Phase 492: inventory retained migration surfaces and define removal roadmap",
            chineseParity,
            StringComparison.Ordinal);
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
