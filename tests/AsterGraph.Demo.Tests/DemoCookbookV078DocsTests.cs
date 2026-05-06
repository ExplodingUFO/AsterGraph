using System;
using System.IO;
using System.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class DemoCookbookV078DocsTests
{
    [Fact]
    public void DemoCookbookDocs_MapV078ProofSurfacesInBothLocales()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.Contains("DEMO_COOKBOOK_V078_PROOF_DOCS_OK", contents, StringComparison.Ordinal);
            Assert.Contains("Rendering and viewport", contents, StringComparison.Ordinal);
            Assert.Contains("Spatial authoring", contents, StringComparison.Ordinal);
            Assert.Contains("ViewportVisibleSceneProjector.Project", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasSceneHostViewportProjectionTests", contents, StringComparison.Ordinal);
            Assert.Contains("NodeCanvasConnectionSceneRendererTests", contents, StringComparison.Ordinal);
            Assert.Contains("CommandRegistry_SpatialAuthoringCommandsExposeCanonicalRouteMenuAndToolPlacements", contents, StringComparison.Ordinal);
            Assert.Contains("Commands_ComposeSelectionMoveSnapGroupRouteAndLayoutResetConstraints", contents, StringComparison.Ordinal);
            Assert.Contains("HostedCommandSurface_RefreshesAfterLayoutPlanApply", contents, StringComparison.Ordinal);
            Assert.Contains("AuthoringToolsChrome_SelectionLayoutActionRestoresCanvasFocusAndKeyboardRouting", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DemoCookbookDocs_KeepV078SupportBoundariesExplicit()
    {
        var english = ReadRepoFile("docs/en/demo-cookbook.md");
        var chinese = ReadRepoFile("docs/zh-CN/demo-cookbook.md");

        foreach (var contents in new[] { english, chinese })
        {
            Assert.True(HasLineWithAll(contents, "Rendering and viewport", "renderer rewrite", "background graph index", "query language", "second viewport model", "new graph-size tier"));
            Assert.True(HasLineWithAll(contents, "Spatial", "session command/query route"));
            Assert.True(HasLineWithAll(contents, "Avalonia", "projection", "focus", "pointer capture", "control lifecycle"));
            Assert.True(HasLineWithAll(contents, "runtime ownership", "AsterGraph.Editor"));
            Assert.True(HasLineWithAll(contents, "runnable-code generation claim"));
            Assert.True(HasLineWithAll(contents, "marketplace behavior", "sandboxing", "collaborative sync", "telemetry", "adapter expansion", "separate workflow engine"));
        }
    }

    [Fact]
    public void DemoCookbookDocs_DoNotAddUnsupportedV078Claims()
    {
        var touchedDocs = new[]
        {
            ReadRepoFile("docs/en/demo-cookbook.md"),
            ReadRepoFile("docs/zh-CN/demo-cookbook.md"),
        };

        foreach (var contents in touchedDocs)
        {
            Assert.DoesNotContain("generated " + "runnable", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Demo-only " + "contract", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("fallback " + "layer", contents, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("external inspiration", contents, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool HasLineWithAll(string contents, params string[] fragments)
        => contents
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Any(line => fragments.All(fragment => line.Contains(fragment, StringComparison.OrdinalIgnoreCase)));

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
