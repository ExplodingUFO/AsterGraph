using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class RetainedRouteDecisionClosureDocsTests
{
    [Fact]
    public void RetainedRouteDecisionGuidance_IsExplicitInEnglishPublicDocs()
    {
        var quickStart = ReadRepoFile("docs/en/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/en/host-integration.md");
        var retainedRecipe = ReadRepoFile("docs/en/retained-migration-recipe.md");

        Assert.Contains("Choose retained only when you are migrating an existing host in batches", quickStart, StringComparison.Ordinal);
        Assert.Contains("Choose retained only when you are migrating an existing host in batches", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("Choose retained only when you are migrating an existing host in batches", retainedRecipe, StringComparison.Ordinal);
    }

    [Fact]
    public void RetainedRouteDecisionGuidance_IsExplicitInChinesePublicDocs()
    {
        var quickStart = ReadRepoFile("docs/zh-CN/quick-start.md");
        var hostIntegration = ReadRepoFile("docs/zh-CN/host-integration.md");
        var retainedRecipe = ReadRepoFile("docs/zh-CN/retained-migration-recipe.md");

        Assert.Contains("只有在现有宿主要分批迁移时才选 retained", quickStart, StringComparison.Ordinal);
        Assert.Contains("只有在现有宿主要分批迁移时才选 retained", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("只有在现有宿主要分批迁移时才选 retained", retainedRecipe, StringComparison.Ordinal);
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
