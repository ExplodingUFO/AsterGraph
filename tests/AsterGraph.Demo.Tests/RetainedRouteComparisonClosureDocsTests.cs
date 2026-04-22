using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class RetainedRouteComparisonClosureDocsTests
{
    [Fact]
    public void RetainedRouteComparisonGuidance_IsExplicitInEnglishPublicDocs()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var retainedRecipe = ReadRepoFile("docs/en/retained-migration-recipe.md");

        Assert.Contains("When To Choose Retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Use `CreateSession(...)` when you are starting new work or own your UI.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Use `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)` when you want the shipped Avalonia route.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Use retained only when the host already constructs `GraphEditorViewModel` or `GraphEditorView` and you are migrating in batches.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Retained is not a fourth primary route.", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("The single bounded retained recipe is [Retained-To-Session Migration Recipe](./retained-migration-recipe.md).", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Choose retained only when you are migrating an existing host in batches", quickStart, StringComparison.Ordinal);
        Assert.Contains("This is the single bounded retained recipe set.", retainedRecipe, StringComparison.Ordinal);
    }

    [Fact]
    public void RetainedRouteComparisonGuidance_IsExplicitInChinesePublicDocs()
    {
        var quickStart = ReadRepoFile("docs/zh-CN/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/zh-CN/host-integration.md");
        var retainedRecipe = ReadRepoFile("docs/zh-CN/retained-migration-recipe.md");

        Assert.Contains("何时选择 retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("当你在开始新工作或宿主自己拥有 UI 时，请使用 `CreateSession(...)`", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("当你想使用 shipped Avalonia 路线时，请使用 `Create(...)` + `AsterGraphAvaloniaViewFactory.Create(...)`", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("只有在现有宿主已经构造 `GraphEditorViewModel` 或 `GraphEditorView` 且正在分批迁移时才使用 retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("retained 不是第四条主路线", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("唯一的 retained recipe 是 [Retained 到 Session 的迁移 Recipe](./retained-migration-recipe.md)。", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("只有在现有宿主要分批迁移时才选 retained", quickStart, StringComparison.Ordinal);
        Assert.Contains("这是唯一一个 bounded 的 retained recipe 集合。", retainedRecipe, StringComparison.Ordinal);
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
