using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class CustomNodeHostRecipeDocsTests
{
    [Fact]
    public void CustomNodeHostRecipeDocs_DefineCopyablePresentationSeamsInBothLocales()
    {
        var english = ReadRepoFile("docs/en/custom-node-host-recipe.md");
        var chinese = ReadRepoFile("docs/zh-CN/custom-node-host-recipe.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("NodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("NodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphNodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphNodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("DefaultGraphNodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("NodeDragHandle.SetIsDragHandle(control, true)", contents, StringComparison.Ordinal);
            Assert.Contains("GraphNodeVisual.PortAnchors", contents, StringComparison.Ordinal);
            Assert.Contains("GraphNodeVisual.ConnectionTargetAnchors", contents, StringComparison.Ordinal);
            Assert.Contains("GetConnectionGeometrySnapshots()", contents, StringComparison.Ordinal);
            Assert.Contains("OverlayLayer", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPresentationOptions.NodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPresentationOptions.NodeVisualPresenter", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PublicApiInventoryDocs_ListCustomNodeCopyableSeamsInBothLocales()
    {
        var english = ReadRepoFile("docs/en/public-api-inventory.md");
        var chinese = ReadRepoFile("docs/zh-CN/public-api-inventory.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("Hosted customization surface", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPresentationOptions.NodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPresentationOptions.NodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphNodeBodyPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("GraphNodeBodyVisual", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphNodeVisualPresenter", contents, StringComparison.Ordinal);
            Assert.Contains("NodeDragHandle.SetIsDragHandle(...)", contents, StringComparison.Ordinal);
            Assert.Contains("IGraphEditorQueries.GetConnectionGeometrySnapshots()", contents, StringComparison.Ordinal);
            Assert.Contains("OverlayLayer", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DemoCookbookDocs_KeepCustomNodeConsumerSampleBoundaryInBothLocales()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("v078-customization-route", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraphPresentationOptions", contents, StringComparison.Ordinal);
            Assert.Contains("custom node presenters", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("parameter editor registries", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("host-owned edge overlays", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("ConsumerSample as the copyable customization recipe", contents, StringComparison.Ordinal);
            Assert.Contains("Demo remains visual proof only", contents, StringComparison.Ordinal);
            Assert.Contains("does not widen the runtime model or sample boundary", contents, StringComparison.Ordinal);
        }
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
