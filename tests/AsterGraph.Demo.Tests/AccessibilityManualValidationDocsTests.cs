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

    [Fact]
    public void AccessibilityManualValidationDocs_RecordPhase516EvidencePackageAndLiveAtFollowUp()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");
        var englishRecipe = ReadRepoFile("docs/en/hosted-accessibility-recipe.md");
        var chineseRecipe = ReadRepoFile("docs/zh-CN/hosted-accessibility-recipe.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 516", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #152", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-821", contents, StringComparison.Ordinal);
            Assert.Contains("ACCESSIBILITY_MANUAL_AT_EVIDENCE_PACKAGE", contents, StringComparison.Ordinal);
            Assert.Contains("platform-equivalent", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("headless automation proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("live assistive-technology observations", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GitHub #156", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-1pd", contents, StringComparison.Ordinal);
            Assert.Contains("no live-region/runtime behavior change", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no broad screen-reader certification claim", contents, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var contents in new[] { englishRecipe, chineseRecipe })
        {
            Assert.Contains("ACCESSIBILITY_MANUAL_AT_EVIDENCE_PACKAGE", contents, StringComparison.Ordinal);
            Assert.Contains("Phase 516", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #152", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-821", contents, StringComparison.Ordinal);
            Assert.Contains("platform-equivalent", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("headless automation proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GraphEditorView", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvas", contents, StringComparison.Ordinal);
            Assert.Contains("GraphInspectorView", contents, StringComparison.Ordinal);
            Assert.Contains("PART_CommandPaletteSearchBox", contents, StringComparison.Ordinal);
            Assert.Contains("PART_ParameterSearchBox", contents, StringComparison.Ordinal);
            Assert.Contains("validation focus buttons", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("export/status text", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Narrator", contents, StringComparison.Ordinal);
            Assert.Contains("NVDA", contents, StringComparison.Ordinal);
            Assert.Contains("VoiceOver", contents, StringComparison.Ordinal);
            Assert.Contains("not observed", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("live screen-reader observations were not performed", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("dynamic validation/export/status", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GitHub #156", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-1pd", contents, StringComparison.Ordinal);
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
