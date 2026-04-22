using System;
using System.IO;
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

        Assert.Contains("[Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)", readme, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./stabilization-support-matrix.md)", quickStart, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./stabilization-support-matrix.md)", projectStatus, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./stabilization-support-matrix.md)", versioning, StringComparison.Ordinal);

        Assert.DoesNotContain("public alpha", readme, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public alpha", quickStart, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public alpha", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public alpha", versioning, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("public alpha", supportMatrix, StringComparison.OrdinalIgnoreCase);

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

        Assert.Contains("[稳定化支持矩阵](./docs/zh-CN/stabilization-support-matrix.md)", readme, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./stabilization-support-matrix.md)", quickStart, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./stabilization-support-matrix.md)", projectStatus, StringComparison.Ordinal);
        Assert.Contains("[稳定化支持矩阵](./stabilization-support-matrix.md)", versioning, StringComparison.Ordinal);

        Assert.DoesNotContain("公开 alpha", readme, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("公开 alpha", quickStart, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("公开 alpha", projectStatus, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("公开 alpha", versioning, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("公开 alpha", supportMatrix, StringComparison.OrdinalIgnoreCase);

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
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static void AssertQuickStartPrelude(string quickStart, string expectedTitleLine, string expectedIntroLine, string expectedFirstTimeLine, string expectedBoundaryLine)
    {
        var lines = Array.ConvertAll(quickStart.Split('\n'), line => line.TrimEnd('\r'));

        Assert.True(lines.Length >= 9, "Quick Start should keep the top-of-file support-boundary prelude.");
        Assert.Equal(expectedTitleLine, lines[0]);
        Assert.Equal(expectedIntroLine, lines[2]);
        Assert.Equal(expectedFirstTimeLine, lines[4]);
        Assert.Contains("WPF", lines[5], StringComparison.Ordinal);
        Assert.Contains("adapter-2", lines[5], StringComparison.OrdinalIgnoreCase);
        Assert.Equal(expectedBoundaryLine, lines[6]);
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
