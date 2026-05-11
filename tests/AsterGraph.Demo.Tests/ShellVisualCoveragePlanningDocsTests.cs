using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ShellVisualCoveragePlanningDocsTests
{
    [Fact]
    public void ShellVisualCoveragePlanningDocs_RecordPhase506BoundaryInBothLocales()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");
        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("Phase 506", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #135", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-h7c", contents, StringComparison.Ordinal);
            Assert.Contains("SHELL_VISUAL_COVERAGE_PLANNING", contents, StringComparison.Ordinal);
            Assert.Contains("broader shell visual coverage planning", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("five manifest-driven full-window shell captures", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("flyout capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("popup capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("context-menu capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("additional language/theme variants", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pixel-baseline drift measurement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no manifest rows", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime UI changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no broad visual/language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("| TBD | TBD | Phase 506: broader shell visual coverage planning", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { englishCookbook, chineseCookbook })
        {
            Assert.Contains("SHELL_VISUAL_COVERAGE_PLANNING", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-open", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-open-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-closed", contents, StringComparison.Ordinal);
            Assert.Contains("shell-runtime-diagnostics-open", contents, StringComparison.Ordinal);
            Assert.Contains("shell-runtime-diagnostics-closed", contents, StringComparison.Ordinal);
            Assert.Contains("flyout capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("popup capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("context-menu capture", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("additional language/theme variants", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("pixel-baseline drift measurement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no manifest rows", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no strict pixel baselines", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no broad visual/language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ShellVisualCoveragePlanningDocs_KeepPlanningSliceDistinctFromPhase508FlyoutRow()
    {
        var manifestPath = Path.Combine(GetRepositoryRoot(), "tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        Assert.Equal(7, document.RootElement.GetArrayLength());
        Assert.Contains(
            document.RootElement.EnumerateArray(),
            state =>
                string.Equals(state.GetProperty("id").GetString(), "shell-cookbook-default-view-menu-flyout", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("flyoutMenuPartName").GetString(), "PART_ViewMenu", StringComparison.Ordinal));
    }

    [Fact]
    public void ShellVisualCoveragePlanningDocs_RecordPhase509TooltipPopupRow()
    {
        var manifestPath = Path.Combine(GetRepositoryRoot(), "tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        Assert.Equal(7, document.RootElement.GetArrayLength());
        Assert.Contains(
            document.RootElement.EnumerateArray(),
            state =>
                string.Equals(state.GetProperty("id").GetString(), "shell-cookbook-default-host-command-tooltip-popup", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("popupTargetPartName").GetString(), "PART_HostCommand_history.undo", StringComparison.Ordinal)
                && state.GetProperty("requiredPopupText").EnumerateArray().Any(text =>
                    string.Equals(text.GetString(), "Nothing to undo yet.", StringComparison.Ordinal)));

        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");
        foreach (var contents in new[] { englishCookbook, chineseCookbook })
        {
            Assert.Contains("Phase 509", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #140", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-0ff", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-host-command-tooltip-popup", contents, StringComparison.Ordinal);
            Assert.Contains("PART_HostCommand_history.undo", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-popup-state", contents, StringComparison.Ordinal);
            Assert.Contains("Nothing to undo yet.", contents, StringComparison.Ordinal);
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
