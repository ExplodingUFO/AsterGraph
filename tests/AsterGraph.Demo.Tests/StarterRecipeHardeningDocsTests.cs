using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class StarterRecipeHardeningDocsTests
{
    [Fact]
    public void CopyableHostRecipePolishDocs_MakeTheSeamOwnershipAndBetaScopeExplicit()
    {
        var starterReadme = ReadRepoFile("tools/AsterGraph.Starter.Avalonia/README.md");
        var consumerReadme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var quickStartEn = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");

        Assert.Contains("copyable seams", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("host-owned seams", starterReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("copy the host-owned seams in this order", consumerReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("defended beta route", consumerReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("Copy the host-owned seams, not the sample-owned presentation.", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("复制宿主自管 seam，不复制样例自有展示层。", quickStartZh, StringComparison.Ordinal);

        Assert.Contains("action projection", consumerSampleEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trust workflow", consumerSampleEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parameter-editing composition", consumerSampleEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("action projection", consumerSampleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("trust workflow", consumerSampleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("parameter-editing composition", consumerSampleZh, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StarterAvaloniaDocs_SeparateCopyableCanonicalPiecesFromHostOwnedShellPieces()
    {
        var starterReadme = ReadRepoFile("tools/AsterGraph.Starter.Avalonia/README.md");
        var quickStartEn = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        Assert.Contains("first hosted recipe", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Copyable Seams", starterReadme, StringComparison.Ordinal);
        Assert.Contains("Host-Owned Seams", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorOptions", starterReadme, StringComparison.Ordinal);
        Assert.Contains("document/catalog/editor/view composition flow", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("the top-level window and its title/size", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sample graph/catalog definitions", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HelloWorld.Avalonia", starterReadme, StringComparison.Ordinal);

        Assert.Contains("Use `AsterGraph.Starter.Avalonia` as the starter recipe.", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("Keep/copy `AsterGraphEditorFactory.Create(...)`", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("Copy the host-owned seams, not the sample-owned presentation.", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("Replace the top-level window and its title/size", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("The next hosted step is `AsterGraph.HelloWorld.Avalonia`.", quickStartEn, StringComparison.Ordinal);

        Assert.Contains("把 `AsterGraph.Starter.Avalonia` 当作 starter recipe。", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("保留/复制 `AsterGraphEditorFactory.Create(...)`", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("复制宿主自管 seam，不复制样例自有展示层。", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("替换宿主自己的 top-level window 和它的 title/size", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("下一步 hosted step 是 `AsterGraph.HelloWorld.Avalonia`。", quickStartZh, StringComparison.Ordinal);
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
