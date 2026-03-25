using AsterGraph.Editor.Diagnostics;
using AsterGraph.Editor.Services;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class GraphEditorServiceSeamsTests
{
    [Fact]
    public void ServiceContracts_ArePublicAndHostReplaceable()
    {
        Assert.True(typeof(IGraphWorkspaceService).IsPublic);
        Assert.True(typeof(IGraphFragmentWorkspaceService).IsPublic);
        Assert.True(typeof(IGraphFragmentLibraryService).IsPublic);
        Assert.True(typeof(IGraphClipboardPayloadSerializer).IsPublic);
        Assert.True(typeof(IGraphEditorDiagnosticsSink).IsPublic);
    }

    [Fact]
    public void GraphEditorStorageDefaults_UsePackageNeutralRootWhenOverrideMissing()
    {
        var root = GraphEditorStorageDefaults.ResolveStorageRootPath();
        var expectedRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AsterGraph");

        Assert.Equal(expectedRoot, root);
        Assert.DoesNotContain("Demo", root, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(Path.Combine(expectedRoot, "workspace.json"), GraphEditorStorageDefaults.GetWorkspacePath());
        Assert.Equal(Path.Combine(expectedRoot, "selection-fragment.json"), GraphEditorStorageDefaults.GetFragmentPath());
        Assert.Equal(Path.Combine(expectedRoot, "fragments"), GraphEditorStorageDefaults.GetFragmentLibraryPath());
    }

    [Fact]
    public void GraphEditorStorageDefaults_UseExplicitRootWhenProvided()
    {
        var explicitRoot = Path.Combine(Path.GetTempPath(), "astergraph-service-seams", "custom-root");

        Assert.Equal(explicitRoot, GraphEditorStorageDefaults.ResolveStorageRootPath(explicitRoot));
        Assert.Equal(Path.Combine(explicitRoot, "workspace.json"), GraphEditorStorageDefaults.GetWorkspacePath(explicitRoot));
        Assert.Equal(Path.Combine(explicitRoot, "selection-fragment.json"), GraphEditorStorageDefaults.GetFragmentPath(explicitRoot));
        Assert.Equal(Path.Combine(explicitRoot, "fragments"), GraphEditorStorageDefaults.GetFragmentLibraryPath(explicitRoot));
    }

    [Fact(Skip = "Phase 2 Plan 02-04 wires the default services to the shared storage defaults.")]
    public void DefaultServices_WillUseGraphEditorStorageDefaultsInsteadOfDemoPaths()
    {
    }

    [Fact(Skip = "Phase 2 Plan 02-04/02-05 verifies host replacement behavior and compatibility-service continuity.")]
    public void HostReplacements_WillRemainCompatibleWithCompatibilityServiceContinuity()
    {
    }
}
