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

        Assert.Contains("公开发布包目标框架：`net8.0`、`net9.0`", readme, StringComparison.Ordinal);
        Assert.Contains("WPF 仅是 adapter-2 portability validation", quickStart, StringComparison.Ordinal);
        Assert.Contains("`WPF` 作为 adapter 2", projectStatus, StringComparison.Ordinal);
        Assert.Contains("稳定化支持矩阵", versioning, StringComparison.Ordinal);
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
