using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class V075LibraryProofClosureTests
{
    private static readonly string[] ClosureMarkers =
    [
        "V075_LIBRARY_GRADE_PROOF_OK:True",
        "V075_RENDERING_VIEWPORT_PROOF_OK:True",
        "V075_INTERACTION_CONTRACT_PROOF_OK:True",
        "V075_EXTENSION_SURFACE_PROOF_OK:True",
        "V075_HOST_PACKAGING_PROOF_OK:True",
        "V075_COOKBOOK_PROFESSIONAL_PROOF_OK:True",
        "V075_RELEASE_GATE_CLEAN_HANDOFF_OK:True",
    ];

    [Fact]
    public void ProjectStatusAndLaunchChecklist_SurfaceV075ClosureMarkers()
    {
        var docs = new[]
        {
            ReadRepoFile("docs/en/project-status.md"),
            ReadRepoFile("docs/zh-CN/project-status.md"),
            ReadRepoFile("docs/en/public-launch-checklist.md"),
            ReadRepoFile("docs/zh-CN/public-launch-checklist.md"),
        };

        foreach (var contents in docs)
        {
            foreach (var marker in ClosureMarkers)
            {
                Assert.Contains(marker, contents, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void ReleaseChecklist_MapsV075ClosureToCompletedPhaseProofWithoutClaimExpansion()
    {
        var checklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var projectStatus = ReadRepoFile("docs/en/project-status.md");

        foreach (var phase in new[] { "445", "446", "447", "448", "449" })
        {
            Assert.Contains(phase, checklist, StringComparison.Ordinal);
        }

        Assert.Contains("rendering/viewport projection", projectStatus, StringComparison.Ordinal);
        Assert.Contains("interaction contracts", projectStatus, StringComparison.Ordinal);
        Assert.Contains("extension surfaces", projectStatus, StringComparison.Ordinal);
        Assert.Contains("host packaging/template proof", projectStatus, StringComparison.Ordinal);
        Assert.Contains("professional cookbook coverage", projectStatus, StringComparison.Ordinal);
        Assert.Contains("without widening package, adapter, graph-size, marketplace, execution-engine, or GA claims", checklist, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicDocs_DoNotNameExternalInspirationProjects()
    {
        var publicDocs = string.Join(
            Environment.NewLine,
            ReadRepoFile("README.md"),
            ReadRepoFile("README.zh-CN.md"),
            ReadRepoFile("docs/en/project-status.md"),
            ReadRepoFile("docs/zh-CN/project-status.md"),
            ReadRepoFile("docs/en/public-launch-checklist.md"),
            ReadRepoFile("docs/zh-CN/public-launch-checklist.md"),
            ReadRepoFile("docs/en/demo-cookbook.md"),
            ReadRepoFile("docs/zh-CN/demo-cookbook.md"));

        foreach (var prohibited in new[] { "xyflow", "react flow", "svelte flow", "@xyflow", "xy flow" })
        {
            Assert.DoesNotContain(prohibited, publicDocs, StringComparison.OrdinalIgnoreCase);
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
