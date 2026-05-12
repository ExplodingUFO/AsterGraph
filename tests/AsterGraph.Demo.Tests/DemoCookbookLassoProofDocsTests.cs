using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookLassoProofDocsTests
{
    [Fact]
    public void DemoCookbookDocs_SurfaceLassoScreenshotProofBoundary()
    {
        foreach (var contents in new[] { ReadRepoFile("docs/en/demo-cookbook.md"), ReadRepoFile("docs/zh-CN/demo-cookbook.md") })
        {
            Assert.Contains("interaction-lasso-screenshot-proof-route", contents, StringComparison.Ordinal);
            Assert.Contains("cookbook-interaction-lasso-screenshot-proof", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-lasso-screenshot-proof", contents, StringComparison.Ordinal);
            Assert.Contains("LASSO_SCREENSHOT_PROOF_BOUNDARY_OK", contents, StringComparison.Ordinal);
            Assert.Contains("full-window-shell-lasso-state", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasSelectionMode.Lasso", contents, StringComparison.Ordinal);
            Assert.Contains("LassoSelectionMode_RendersTransientFeedbackPathOnlyDuringDrag", contents, StringComparison.Ordinal);
            Assert.Contains("no toolbar UX", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no eraser", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no drawing primitives", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no persistence", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("no full whiteboard parity", contents, StringComparison.OrdinalIgnoreCase);
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
