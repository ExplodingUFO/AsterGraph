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

        Assert.Contains("Phase 491 closed the accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 491 已关闭 accessibility breadth audit", chineseParity, StringComparison.Ordinal);
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

        Assert.Contains("Phase 491 closed the accessibility breadth audit", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 491 已关闭 accessibility breadth audit", chineseParity, StringComparison.Ordinal);
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

        Assert.Contains("Phase 492 now owns the retained migration removal roadmap", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 492 现在负责 retained migration removal roadmap", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase493CustomNodePresenterProofInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 493", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #109", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-8qv", contents, StringComparison.Ordinal);
            Assert.Contains("custom node presenter cookbook parity proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("NodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("NodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("CUSTOM_EXTENSION_SURFACE_OK", contents, StringComparison.Ordinal);
            Assert.Contains("runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Keep improving docs and samples with concrete host-owned visual presenter examples", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("用具体 host-owned visual presenter 示例继续补文档", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Phase 493 closed the custom node presenter cookbook parity proof", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 493 已关闭 custom node presenter cookbook parity proof", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase494LocalizedShellVisualGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 494", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #111", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-p5z", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-open-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("shell-state language/theme metadata", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("manifest-driven full-window shell", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("runtime UI changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Contains("Phase 494 closed localized full-window shell visual gate coverage", englishParity, StringComparison.Ordinal);
        Assert.Contains("Phase 494 已关闭 localized full-window shell visual gate coverage", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase495PostShellGateIssueWaveRefreshInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 495", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #113", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-wzt", contents, StringComparison.Ordinal);
            Assert.Contains("post-Phase-494 roadmap refresh", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Cookbook example architecture contract", contents, StringComparison.Ordinal);
            Assert.Contains("shell visual gate breadth", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("retained migration removal execution gate", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Core/Editor/Avalonia runtime changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/phase-495-roadmap-refresh", contents, StringComparison.Ordinal);

            Assert.DoesNotContain("| #105 | `avalonia-node-map-44i` | Phase 491", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| #107 | `avalonia-node-map-j8v` | Phase 492", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| #109 | `avalonia-node-map-8qv` | Phase 493", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("| #111 | `avalonia-node-map-p5z` | Phase 494", contents, StringComparison.Ordinal);
        }

        Assert.Contains("at least three concrete follow-up candidates", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("至少三个具体 follow-up candidates", chineseParity, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParityRoadmapDocs_RecordPhase497ClosedDrawerShellVisualGateInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 497", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #117", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-5nl", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-closed", contents, StringComparison.Ordinal);
            Assert.Contains("expectedPaneOpen: false", contents, StringComparison.Ordinal);
            Assert.Contains("strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("broad language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("docs/phase-497-shell-closed-drawer-gate", contents, StringComparison.Ordinal);
        }

        Assert.Contains("four manifest-driven full-window shell captures", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("四条 manifest-driven full-window shell", chineseParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Current owned slice", englishParity, StringComparison.Ordinal);
        Assert.Contains("Current owned slice", chineseParity, StringComparison.Ordinal);
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
