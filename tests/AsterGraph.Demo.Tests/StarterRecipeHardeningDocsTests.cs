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

        Assert.Contains("copyable", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("first hosted recipe", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HelloWorld.Avalonia", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", starterReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorOptions", starterReadme, StringComparison.Ordinal);
        Assert.Contains("document/catalog/editor/view composition flow", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("top-level window", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shell chrome", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sample title", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sample size", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sample content shell", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sample graph/catalog definitions", starterReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("copyable starter recipe", quickStartEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("keep/copy", quickStartEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("replace", quickStartEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HelloWorld.Avalonia", quickStartEn, StringComparison.Ordinal);
        Assert.Contains("可复制", quickStartZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("保留/复制", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("替换", quickStartZh, StringComparison.Ordinal);
        Assert.Contains("HelloWorld.Avalonia", quickStartZh, StringComparison.Ordinal);

        Assert.True(
            StarterMentionsHelloWorldAfterStarter(starterReadme, quickStartEn, quickStartZh),
            "Starter docs should make HelloWorld.Avalonia the next hosted step after the copyable starter recipe.");
    }

    private static bool StarterMentionsHelloWorldAfterStarter(string starterReadme, string quickStartEn, string quickStartZh)
        => starterReadme.IndexOf("HelloWorld.Avalonia", StringComparison.Ordinal) >= 0
        && quickStartEn.IndexOf("HelloWorld.Avalonia", StringComparison.Ordinal) >= 0
        && quickStartZh.IndexOf("HelloWorld.Avalonia", StringComparison.Ordinal) >= 0;

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


