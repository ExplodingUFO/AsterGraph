using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class LayoutProviderProofDocsTests
{
    [Fact]
    public void LayoutProviderProofDocs_DefineSynchronousProviderBoundaryInBothLocales()
    {
        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");
        var englishApi = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseApi = ReadRepoFile("docs/zh-CN/public-api-inventory.md");

        foreach (var contents in new[] { englishCookbook, chineseCookbook, englishParity, chineseParity, englishApi, chineseApi })
        {
            Assert.Contains("IGraphLayoutProvider", contents, StringComparison.Ordinal);
            Assert.Contains("GraphLayoutRequest", contents, StringComparison.Ordinal);
            Assert.Contains("GraphLayoutPlan", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { englishCookbook, englishParity, englishApi })
        {
            Assert.Contains("synchronous", contents, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var contents in new[] { englishCookbook, chineseCookbook, englishParity, chineseParity })
        {
            Assert.Contains("PreviewLayoutPlan", contents, StringComparison.Ordinal);
            Assert.Contains("TryApplyLayoutPlan", contents, StringComparison.Ordinal);
            Assert.Contains("TryApplyLayoutRequest", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { englishCookbook, chineseCookbook })
        {
            Assert.Contains("LAYOUT_PROVIDER_SEAM_OK", contents, StringComparison.Ordinal);
            Assert.Contains("LAYOUT_PREVIEW_APPLY_CANCEL_OK", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { englishCookbook, chineseCookbook, englishApi, chineseApi })
        {
            Assert.Contains("TrySnapSelectedNodesToGrid", contents, StringComparison.Ordinal);
        }

        Assert.Contains("snap-to-grid commands", englishParity, StringComparison.Ordinal);
        Assert.Contains("snap-to-grid commands", chineseParity, StringComparison.Ordinal);
        Assert.Contains("同步", chineseCookbook, StringComparison.Ordinal);
        Assert.Contains("同步", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void LayoutProviderProofDocs_KeepLayoutCancellationClaimOutOfProviderContract()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");
        var englishApi = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseApi = ReadRepoFile("docs/zh-CN/public-api-inventory.md");

        foreach (var contents in new[] { englishParity, chineseParity, englishApi, chineseApi })
        {
            Assert.Contains("command-surface cancel evidence", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Layout cancellation is not part of the current `IGraphLayoutProvider` contract", englishApi, StringComparison.Ordinal);
        Assert.Contains("Layout cancellation 不是当前 `IGraphLayoutProvider` 契约的一部分", chineseApi, StringComparison.Ordinal);
        Assert.Contains("rather than async-cancellable", englishParity, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("不是 async-cancellable", chineseParity, StringComparison.Ordinal);
    }

    [Fact]
    public void LayoutProviderProofDocs_UpdateActiveTrackerReferences()
    {
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishParity, chineseParity })
        {
            Assert.Contains("GitHub #99", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-ce1", contents, StringComparison.Ordinal);
            Assert.Contains("#97", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-i8s", contents, StringComparison.Ordinal);
        }

        Assert.Contains("Closed by PR #98", englishParity, StringComparison.Ordinal);
        Assert.Contains("已由 PR #98 关闭", chineseParity, StringComparison.Ordinal);
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
