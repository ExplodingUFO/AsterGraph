using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class AuthoringSurfaceRecipeDocsTests
{
    [Fact]
    public void AuthoringSurfaceRecipeDocs_PublishCanonicalNodePortAndEdgeCopyPath()
    {
        var authoringRecipeEn = ReadRepoFile("docs/en/authoring-surface-recipe.md");
        var authoringRecipeZh = ReadRepoFile("docs/zh-CN/authoring-surface-recipe.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var advancedEditingEn = ReadRepoFile("docs/en/advanced-editing.md");
        var advancedEditingZh = ReadRepoFile("docs/zh-CN/advanced-editing.md");
        var hostIntegrationEn = ReadRepoFile("docs/en/host-integration.md");
        var hostIntegrationZh = ReadRepoFile("docs/zh-CN/host-integration.md");
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");

        Assert.Contains("## Copyable custom authoring presentation", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("## 可复制的自定义 authoring presentation", authoringRecipeZh, StringComparison.Ordinal);

        foreach (var contents in new[] { authoringRecipeEn, authoringRecipeZh })
        {
            Assert.Contains("ConsumerSampleAuthoringSurfaceRecipe.CreatePresentationOptions()", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphNodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("INodeParameterEditorRegistry", contents, StringComparison.Ordinal);
            Assert.Contains("TrySetNodeSize(...)", contents, StringComparison.Ordinal);
            Assert.Contains("GetConnectionGeometrySnapshots()", contents, StringComparison.Ordinal);
            Assert.Contains("GraphNodeVisual.ConnectionTargetAnchors", contents, StringComparison.Ordinal);
        }

        Assert.Contains("[Authoring Surface Recipe](./authoring-surface-recipe.md)", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("[Authoring Surface Recipe](./authoring-surface-recipe.md)", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("[Authoring Surface Recipe](./authoring-surface-recipe.md)", advancedEditingEn, StringComparison.Ordinal);
        Assert.Contains("[Authoring Surface Recipe](./authoring-surface-recipe.md)", advancedEditingZh, StringComparison.Ordinal);
        Assert.Contains("[Authoring Surface Recipe](./authoring-surface-recipe.md)", hostIntegrationEn, StringComparison.Ordinal);
        Assert.Contains("[Authoring Surface Recipe](./authoring-surface-recipe.md)", hostIntegrationZh, StringComparison.Ordinal);
        Assert.Contains("../../docs/en/authoring-surface-recipe.md", readme, StringComparison.Ordinal);
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
