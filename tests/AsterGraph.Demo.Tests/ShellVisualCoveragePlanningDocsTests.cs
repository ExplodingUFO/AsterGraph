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
        Assert.Equal(10, document.RootElement.GetArrayLength());
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
        Assert.Equal(10, document.RootElement.GetArrayLength());
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

    [Fact]
    public void ShellVisualCoveragePlanningDocs_RecordPhase510CanvasContextMenuRow()
    {
        var manifestPath = Path.Combine(GetRepositoryRoot(), "tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        Assert.Equal(10, document.RootElement.GetArrayLength());
        Assert.Contains(
            document.RootElement.EnumerateArray(),
            state =>
                string.Equals(state.GetProperty("id").GetString(), "shell-cookbook-default-canvas-context-menu", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("contextMenuTargetPartName").GetString(), "PART_NodeCanvas", StringComparison.Ordinal)
                && state.GetProperty("requiredContextMenuHeaders").EnumerateArray().Any(text =>
                    string.Equals(text.GetString(), "Add Node", StringComparison.Ordinal)));

        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");
        foreach (var contents in new[] { englishCookbook, chineseCookbook })
        {
            Assert.Contains("Phase 510", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #141", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-8lu", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-canvas-context-menu", contents, StringComparison.Ordinal);
            Assert.Contains("PART_NodeCanvas", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-context-menu-state", contents, StringComparison.Ordinal);
            Assert.Contains("Add Node", contents, StringComparison.Ordinal);
            Assert.Contains("Fit View", contents, StringComparison.Ordinal);
            Assert.Contains("Reset View", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ShellVisualCoveragePlanningDocs_RecordPhase511BoundedLanguageThemeRows()
    {
        var manifestPath = Path.Combine(GetRepositoryRoot(), "tests/AsterGraph.Demo.Tests/CookbookShellVisualGateStates.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        Assert.Equal(10, document.RootElement.GetArrayLength());
        Assert.Contains(
            document.RootElement.EnumerateArray(),
            state =>
                string.Equals(state.GetProperty("id").GetString(), "shell-cookbook-default-closed-zh-cn", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("language").GetString(), "zh-CN", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("theme").GetString(), "canonical-dark", StringComparison.Ordinal)
                && !state.GetProperty("expectedPaneOpen").GetBoolean());
        Assert.Contains(
            document.RootElement.EnumerateArray(),
            state =>
                string.Equals(state.GetProperty("id").GetString(), "shell-runtime-diagnostics-open-zh-cn", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("language").GetString(), "zh-CN", StringComparison.Ordinal)
                && string.Equals(state.GetProperty("theme").GetString(), "canonical-dark", StringComparison.Ordinal)
                && state.GetProperty("expectedPaneOpen").GetBoolean());

        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");
        foreach (var contents in new[] { englishCookbook, chineseCookbook })
        {
            Assert.Contains("Phase 511", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #142", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-9rq", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-closed-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("shell-runtime-diagnostics-open-zh-cn", contents, StringComparison.Ordinal);
            Assert.Contains("zh-CN", contents, StringComparison.Ordinal);
            Assert.Contains("canonical-dark", contents, StringComparison.Ordinal);
            Assert.Contains("no broad visual/language/theme certification", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ShellVisualCoveragePlanningDocs_RecordPhase512DriftMeasurementContract()
    {
        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");
        foreach (var contents in new[] { englishCookbook, chineseCookbook })
        {
            Assert.Contains("Phase 512", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #143", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-1j4", contents, StringComparison.Ordinal);
            Assert.Contains("pixel-baseline drift measurement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("record-only", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("drift-evidence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("DriftMeasurement", contents, StringComparison.Ordinal);
            Assert.Contains("HostRuntimeDescription", contents, StringComparison.Ordinal);
            Assert.Contains("OsDescription", contents, StringComparison.Ordinal);
            Assert.Contains("ProcessArchitecture", contents, StringComparison.Ordinal);
            Assert.Contains("PngSha256", contents, StringComparison.Ordinal);
            Assert.Contains("no strict pixel baseline enforcement", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no visual redesign", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no runtime behavior changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no public API changes", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no retained API removal", contents, StringComparison.OrdinalIgnoreCase);
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
