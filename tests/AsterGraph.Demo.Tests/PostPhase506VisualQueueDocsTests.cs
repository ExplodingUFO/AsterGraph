using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class PostPhase506VisualQueueDocsTests
{
    [Fact]
    public void PostPhase506VisualQueueDocs_RefreshCurrentQueueInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        AssertPhase507Boundary(englishParity);
        AssertPhase507Boundary(chineseParity);

        AssertClosedVisualQueue(englishParity);
        AssertClosedVisualQueue(chineseParity);

        AssertCurrentQueue(ExtractIssueWaveTable(englishParity));
        AssertCurrentQueue(ExtractIssueWaveTable(chineseParity));
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static void AssertPhase507Boundary(string contents)
    {
        Assert.Contains("Phase 507", contents, StringComparison.Ordinal);
        Assert.Contains("GitHub #137", contents, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-3tw", contents, StringComparison.Ordinal);
        Assert.Contains("post-Phase-506 visual queue refresh", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("GitHub #139-#143", contents, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-2nu", contents, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-0ff", contents, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-8lu", contents, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-9rq", contents, StringComparison.Ordinal);
        Assert.Contains("avalonia-node-map-1j4", contents, StringComparison.Ordinal);
        Assert.Contains("no runtime UI behavior changes", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no shell-state manifest rows", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no broad visual/language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertClosedVisualQueue(string contents)
    {
        Assert.Contains("Phase 507", contents, StringComparison.Ordinal);
        Assert.Contains("Phase 508", contents, StringComparison.Ordinal);
        Assert.Contains("Phase 509", contents, StringComparison.Ordinal);
        Assert.Contains("Phase 510", contents, StringComparison.Ordinal);
        Assert.Contains("Phase 511", contents, StringComparison.Ordinal);
        Assert.Contains("Phase 512", contents, StringComparison.Ordinal);
        Assert.Contains("closed", contents, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Phases 508-512", contents, StringComparison.Ordinal);
    }

    private static void AssertCurrentQueue(string table)
    {
        Assert.Contains("| #193 | `avalonia-node-map-8l6` | Phase 535: refresh post-lasso visual feedback parity queue", table, StringComparison.Ordinal);
        Assert.Contains("Phase 536: lasso screenshot route and Cookbook proof boundary", table, StringComparison.Ordinal);
        Assert.Contains("Phase 537: lasso toolbar UX and public activation ergonomics boundary", table, StringComparison.Ordinal);
        Assert.Contains("Phase 538: eraser behavior/API feasibility gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 539: rectangle/freehand drawing primitive model gate", table, StringComparison.Ordinal);
        Assert.Contains("Phase 540: whiteboard persistence and render-layer readiness gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #161 | `avalonia-node-map-rs5` | Phase 519: refresh parity roadmap after dynamic announcement proof", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #162 | `avalonia-node-map-vdc` | Phase 520: define declarative host composition API gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #163 | `avalonia-node-map-ayx` | Phase 521: define strict pixel-baseline comparator readiness gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #164 | `avalonia-node-map-ecx` | Phase 522: audit retained migration removal readiness", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #169 | `avalonia-node-map-bp0` | Phase 523: refresh React Flow parity issue wave after retained readiness audit", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 524: built-in component parity matrix for MiniMap, Controls, Background, Panel", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 525: MiniMap interaction and customization parity gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 526: Controls interactivity/custom-button parity gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 527: Background variant public surface gate", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 528: Panel versus viewport-attached overlay boundary", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Phase 529: whiteboard/lasso/eraser feasibility audit", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #137 | `avalonia-node-map-3tw` | Phase 507: post-Phase-506 visual queue refresh", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #143 | `avalonia-node-map-1j4` | Phase 512: pixel-baseline drift measurement", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #149 | `avalonia-node-map-d8q` | Phase 513: post-Phase-512 roadmap refresh", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #150 | `avalonia-node-map-ien` | Phase 514: execute renderer virtualization proof harness", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #151 | `avalonia-node-map-t44` | Phase 515: decide strict pixel baseline policy from drift evidence", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #152 | `avalonia-node-map-821` | Phase 516: record manual assistive-technology validation evidence", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #129 | `avalonia-node-map-mzu`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #131 | `avalonia-node-map-8lf`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #133 | `avalonia-node-map-b4z`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #135 | `avalonia-node-map-h7c`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Issue pending", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Bead pending", table, StringComparison.Ordinal);
    }

    private static string ExtractIssueWaveTable(string contents)
    {
        var headingStart = contents.IndexOf("\n## Next Issue Wave", StringComparison.Ordinal);
        if (headingStart < 0)
        {
            headingStart = contents.IndexOf("\n## 下一轮 Issue Wave", StringComparison.Ordinal);
        }

        Assert.True(headingStart >= 0, "Expected Next Issue Wave heading.");

        var tableStart = contents.IndexOf("| GitHub |", headingStart, StringComparison.Ordinal);
        Assert.True(tableStart >= 0, "Expected Next Issue Wave table header.");
        var nextHeading = contents.IndexOf("\n## Recommended Parallel Worktree Plan", tableStart, StringComparison.Ordinal);
        if (nextHeading < 0)
        {
            nextHeading = contents.IndexOf("\n## 推荐并行 Worktree 计划", tableStart, StringComparison.Ordinal);
        }

        Assert.True(nextHeading > tableStart, "Expected worktree plan heading after Next Issue Wave table.");
        return contents[tableStart..nextHeading];
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
}
