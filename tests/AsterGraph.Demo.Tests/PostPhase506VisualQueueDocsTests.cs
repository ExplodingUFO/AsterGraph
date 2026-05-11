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

    private static void AssertCurrentQueue(string table)
    {
        Assert.Contains("| #137 | `avalonia-node-map-3tw` | Phase 507: post-Phase-506 visual queue refresh", table, StringComparison.Ordinal);
        Assert.Contains("| #139 | `avalonia-node-map-2nu` | Phase 508: shell flyout visual capture", table, StringComparison.Ordinal);
        Assert.Contains("| #140 | `avalonia-node-map-0ff` | Phase 509: popup visual capture", table, StringComparison.Ordinal);
        Assert.Contains("| #141 | `avalonia-node-map-8lu` | Phase 510: context-menu visual capture", table, StringComparison.Ordinal);
        Assert.Contains("| #142 | `avalonia-node-map-9rq` | Phase 511: additional language/theme shell variants", table, StringComparison.Ordinal);
        Assert.Contains("| #143 | `avalonia-node-map-1j4` | Phase 512: pixel-baseline drift measurement", table, StringComparison.Ordinal);

        Assert.DoesNotContain("| #129 | `avalonia-node-map-mzu`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #131 | `avalonia-node-map-8lf`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #133 | `avalonia-node-map-b4z`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("| #135 | `avalonia-node-map-h7c`", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Issue pending", table, StringComparison.Ordinal);
        Assert.DoesNotContain("Bead pending", table, StringComparison.Ordinal);
    }

    private static string ExtractIssueWaveTable(string contents)
    {
        var tableStart = contents.IndexOf("| GitHub |", StringComparison.Ordinal);
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
