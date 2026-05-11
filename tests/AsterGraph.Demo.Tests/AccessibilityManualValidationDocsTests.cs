using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class AccessibilityManualValidationDocsTests
{
    [Fact]
    public void AccessibilityManualValidationDocs_RecordPhase505ChecklistInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");
        var englishRecipe = ReadRepoFile("docs/en/hosted-accessibility-recipe.md");
        var chineseRecipe = ReadRepoFile("docs/zh-CN/hosted-accessibility-recipe.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 505", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #133", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-b4z", contents, StringComparison.Ordinal);
            Assert.Contains("ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN", contents, StringComparison.Ordinal);
            Assert.Contains("manual assistive-technology validation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("headless automation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("unverified live screen-reader behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no live-region/runtime behavior change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no UI change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no broad screen-reader certification claim", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 505: accessibility manual assistive-technology validation plan", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { englishRecipe, chineseRecipe })
        {
            Assert.Contains("ACCESSIBILITY_MANUAL_AT_VALIDATION_PLAN", contents, StringComparison.Ordinal);
            Assert.Contains("Manual assistive-technology validation checklist", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Narrator", contents, StringComparison.Ordinal);
            Assert.Contains("NVDA", contents, StringComparison.Ordinal);
            Assert.Contains("VoiceOver", contents, StringComparison.Ordinal);
            Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("headless automation", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("unverified live screen-reader behavior", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no broad screen-reader certification claim", contents, StringComparison.OrdinalIgnoreCase);
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
}
