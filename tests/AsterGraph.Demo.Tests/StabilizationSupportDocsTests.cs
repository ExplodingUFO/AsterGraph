using System;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class StabilizationSupportDocsTests
{
    [Fact]
    public void StabilizationSupportDocs_DefendEnglishMatrixAndGuidanceLinks()
    {
        var readme = ReadRepoFile("README.md");
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");
        var versioning = ReadRepoFile("docs/en/versioning.md");
        var supportMatrix = ReadRepoFile("docs/en/stabilization-support-matrix.md");
        var architecture = ReadRepoFile("docs/en/architecture.md");
        var adoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var pluginRecipe = ReadRepoFile("docs/en/plugin-recipe.md");

        Assert.Contains("# Stabilization Support Matrix", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("Published SDK packages", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`AsterGraph.Abstractions`, `AsterGraph.Core`, `AsterGraph.Editor`, `AsterGraph.Avalonia`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`net8.0` and `net9.0`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`CreateSession(...)` + `IGraphEditorSession`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("Avalonia", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("Validation-only and partial-fallback", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("Migration-only", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("promotion of this same defended boundary to stable", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", supportMatrix, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(supportMatrix, "do not add", "WPF parity", "marketplace", "sandbox", "execution-engine", "GA"));

        Assert.Contains("[Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)", readme, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./stabilization-support-matrix.md)", quickStart, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./stabilization-support-matrix.md)", projectStatus, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./stabilization-support-matrix.md)", versioning, StringComparison.Ordinal);

        foreach (var contents in new[] { readme, quickStart, projectStatus, versioning, supportMatrix, architecture, adoptionFeedback, pluginRecipe })
        {
            AssertDoesNotContainStaleAlphaForms(contents);
        }

        Assert.Contains("published packages target `net8.0` and `net9.0`", readme, StringComparison.Ordinal);
        Assert.Contains("Treat `WPF` only as adapter-2 portability validation", quickStart, StringComparison.Ordinal);
        Assert.Contains("active adapter validation target: `WPF` as adapter 2", projectStatus, StringComparison.Ordinal);
        Assert.Contains("The stabilization support matrix freezes the consumer-facing boundary", versioning, StringComparison.Ordinal);
    }

    [Fact]
    public void StabilizationSupportDocs_DefendChineseMatrixAndGuidanceLinks()
    {
        var readme = ReadRepoFile("README.zh-CN.md");
        var quickStart = ReadRepoFile("docs/zh-CN/quick-start.md");
        var projectStatus = ReadRepoFile("docs/zh-CN/project-status.md");
        var versioning = ReadRepoFile("docs/zh-CN/versioning.md");
        var supportMatrix = ReadRepoFile("docs/zh-CN/stabilization-support-matrix.md");
        var architecture = ReadRepoFile("docs/zh-CN/architecture.md");
        var adoptionFeedback = ReadRepoFile("docs/zh-CN/adoption-feedback.md");
        var pluginRecipe = ReadRepoFile("docs/zh-CN/plugin-recipe.md");

        Assert.Contains("# 稳定化支持矩阵", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("公开 SDK 包", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`AsterGraph.Abstractions`、`AsterGraph.Core`、`AsterGraph.Editor`、`AsterGraph.Avalonia`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`net8.0`、`net9.0`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`CreateSession(...)` + `IGraphEditorSession`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("当前真正受支持的 hosted adapter 只有 Avalonia", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("`WPF`", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("仅用于迁移", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("提升为 stable", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", supportMatrix, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", supportMatrix, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(supportMatrix, "不新增", "WPF parity", "marketplace", "sandbox", "execution engine", "GA"));

        Assert.Contains("[稳定化支持矩阵](./docs/zh-CN/stabilization-support-matrix.md)", readme, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./stabilization-support-matrix.md)", quickStart, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./stabilization-support-matrix.md)", projectStatus, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./stabilization-support-matrix.md)", versioning, StringComparison.Ordinal);

        foreach (var contents in new[] { readme, quickStart, projectStatus, versioning, supportMatrix, architecture, adoptionFeedback, pluginRecipe })
        {
            AssertDoesNotContainStaleAlphaForms(contents);
        }

        Assert.Contains("公开发布包目标框架：`net8.0`、`net9.0`", readme, StringComparison.Ordinal);
        Assert.Contains("WPF 仅是 adapter-2 portability validation", quickStart, StringComparison.Ordinal);
        Assert.Contains("`WPF` 作为 adapter 2", projectStatus, StringComparison.Ordinal);
        Assert.Contains("稳定化支持矩阵", versioning, StringComparison.Ordinal);
    }

    [Fact]
    public void StabilizationSupportDocs_DefendQuickStartPreludeAndAlphaStatusEntryLinks()
    {
        var englishQuickStart = ReadRepoFile("docs/en/quick-start.md");
        var chineseQuickStart = ReadRepoFile("docs/zh-CN/quick-start.md");
        var chineseReadme = ReadRepoFile("README.zh-CN.md");

        AssertQuickStartPrelude(
            englishQuickStart,
            "# AsterGraph Quick Start",
            "This guide is the shortest path from a blank host to a running AsterGraph integration.",
            "For first-time adopters, start on the default Avalonia path by default.",
            "For the frozen support boundary and upgrade guidance toward `v1.0.0`, see [Stabilization Support Matrix](./stabilization-support-matrix.md).");
        AssertQuickStartPrelude(
            chineseQuickStart,
            "# AsterGraph 快速开始",
            "这份文档只讲一件事：怎样从空白宿主最快跑起 AsterGraph。",
            "首次接入默认从 Avalonia 路线开始。",
            "关于冻结的支持边界和面向 `v1.0.0` 的升级指引，见 [稳定化支持矩阵](./stabilization-support-matrix.md)。");

        Assert.Contains("[Alpha Status](./alpha-status.md)", englishQuickStart, StringComparison.Ordinal);
        Assert.Contains("[Alpha Status](./alpha-status.md)", chineseQuickStart, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./docs/zh-CN/stabilization-support-matrix.md)", chineseReadme, StringComparison.Ordinal);
        Assert.Contains("[Alpha Status](./docs/zh-CN/alpha-status.md)", chineseReadme, StringComparison.Ordinal);
        Assert.Contains("关于冻结的支持边界和面向 `v1.0.0` 的升级指引，见 [稳定化支持矩阵](./stabilization-support-matrix.md)。", chineseQuickStart, StringComparison.Ordinal);

        AssertAppearsBefore(englishQuickStart, "[Plugin Manifest and Trust Policy Contract v1](./plugin-trust-contracts.md)", "## 1. Pick Your Starting Package");
        AssertAppearsBefore(englishQuickStart, "[Beta Support Bundle](./support-bundle.md)", "## 1. Pick Your Starting Package");
        AssertAppearsBefore(chineseQuickStart, "[插件信任契约 v1](./plugin-trust-contracts.md)", "## 1. 先选起始包");
        AssertAppearsBefore(chineseQuickStart, "[Beta Support Bundle](./support-bundle.md)", "## 1. 先选起始包");

        var section7Start = chineseQuickStart.IndexOf("## 7. 超过“第一跑”之后看哪里", StringComparison.Ordinal);
        var section8Start = chineseQuickStart.IndexOf("## 8. 维护者与源码验证入口", StringComparison.Ordinal);
        Assert.True(section7Start >= 0, "Expected section 7 heading in the zh-CN quick start before asserting its reading list.");
        Assert.True(section8Start > section7Start, "Expected section 8 heading in the zh-CN quick start after section 7 before slicing the reading list.");
        var section7 = chineseQuickStart.Substring(section7Start, section8Start - section7Start);

        Assert.Contains("- [稳定化支持矩阵](./stabilization-support-matrix.md) = 冻结的支持边界和面向 `v1.0.0` 的升级指引", section7, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static void AssertQuickStartPrelude(string quickStart, string expectedTitleLine, string expectedIntroLine, string expectedFirstTimeLine, string expectedBoundaryLine)
    {
        var lines = Array.ConvertAll(quickStart.Split('\n'), line => line.TrimEnd('\r'));
        var section1Start = Array.FindIndex(lines, line => line.StartsWith("## 1.", StringComparison.Ordinal));

        Assert.True(section1Start > 0, "Quick Start should keep the top-of-file support-boundary prelude before section 1.");

        var preludeLines = lines[..section1Start];
        var titleIndex = Array.FindIndex(preludeLines, line => line == expectedTitleLine);
        var introIndex = Array.FindIndex(preludeLines, line => line == expectedIntroLine);
        var firstTimeIndex = Array.FindIndex(preludeLines, line => line == expectedFirstTimeLine);
        var wpfIndex = Array.FindIndex(preludeLines, line => line.Contains("WPF", StringComparison.Ordinal) && line.Contains("adapter-2", StringComparison.OrdinalIgnoreCase));
        var boundaryIndex = Array.FindIndex(preludeLines, line => line == expectedBoundaryLine);

        Assert.True(titleIndex >= 0, "Quick Start should keep the title in the opening support-boundary prelude.");
        Assert.True(introIndex > titleIndex, "Quick Start should keep the intro after the title in the opening support-boundary prelude.");
        Assert.True(firstTimeIndex > introIndex, "Quick Start should keep the first-time adopter guidance after the intro.");
        Assert.True(wpfIndex > firstTimeIndex, "Quick Start should keep the WPF portability note after the first-time adopter guidance.");
        Assert.True(boundaryIndex > wpfIndex, "Quick Start should keep the support-boundary link after the WPF portability note.");
    }

    private static void AssertDoesNotContainStaleAlphaForms(string contents)
    {
        Assert.DoesNotContain("public alpha", contents, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public-alpha", contents, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("公开 alpha", contents, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertAppearsBefore(string contents, string requiredText, string requiredHeading)
    {
        var textIndex = contents.IndexOf(requiredText, StringComparison.Ordinal);
        var headingIndex = contents.IndexOf(requiredHeading, StringComparison.Ordinal);

        Assert.True(textIndex >= 0, $"Expected to find '{requiredText}'.");
        Assert.True(headingIndex >= 0, $"Expected to find '{requiredHeading}'.");
        Assert.True(textIndex < headingIndex, $"Expected '{requiredText}' to appear before '{requiredHeading}'.");
    }

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));
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
