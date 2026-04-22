using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class StarterRecipeHardeningDocsTests
{
    [Fact]
    public void StarterAvaloniaDocs_SeparateCopyableCanonicalPiecesFromHostOwnedShellPieces()
    {
        var starterReadme = ReadRepoFile("tools/AsterGraph.Starter.Avalonia/README.md");
        var quickStartEn = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        Assert.Contains("first hosted recipe", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Keep/copy from this recipe:", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorOptions", starterReadme, StringComparison.Ordinal);
        Assert.Contains("document/catalog/editor/view composition flow", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Replace/own in your host:", starterReadme, StringComparison.Ordinal);
        Assert.Contains("the top-level window and its title/size", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sample graph/catalog definitions", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HelloWorld.Avalonia", starterReadme, StringComparison.Ordinal);

        Assert.Contains("Use `AsterGraph.Starter.Avalonia` as the starter recipe.", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("Keep/copy `AsterGraphEditorFactory.Create(...)`", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("Replace the top-level window and its title/size", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("The next hosted step is `AsterGraph.HelloWorld.Avalonia`.", quickStartEn, StringComparison.Ordinal);

        Assert.Contains("把 `AsterGraph.Starter.Avalonia` 当作 starter recipe。", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("保留/复制 `AsterGraphEditorFactory.Create(...)`", quickStartZh, StringComparison.Ordinal);
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
