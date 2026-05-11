using AsterGraph.Avalonia.Hosting;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class AsterGraphBuiltInComponentCatalogTests
{
    [Fact]
    public void Components_ExposeUniqueStableIdsAndRequiredPhaseThreeTracks()
    {
        var components = AsterGraphBuiltInComponentCatalog.Components;
        var ids = components.Select(component => component.Id).ToArray();

        Assert.Equal(ids.Length, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(AsterGraphBuiltInComponentCatalog.Canvas, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.MiniMap, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.BackgroundGrid, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.Inspector, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.ControlsPanel, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.CommandToolProjection, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.NodeToolbar, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.EdgeToolbar, ids);
        Assert.Contains(AsterGraphBuiltInComponentCatalog.NodeResizer, ids);

        Assert.All(components, component =>
        {
            Assert.False(string.IsNullOrWhiteSpace(component.Id));
            Assert.False(string.IsNullOrWhiteSpace(component.Title));
            Assert.False(string.IsNullOrWhiteSpace(component.EntryPoint));
            Assert.False(string.IsNullOrWhiteSpace(component.CookbookRouteId));
            Assert.False(string.IsNullOrWhiteSpace(component.DocumentationPath));
            Assert.False(string.IsNullOrWhiteSpace(component.Boundary));
        });
    }

    [Fact]
    public void TryGet_ResolvesKnownIdsAndRejectsInvalidIds()
    {
        Assert.True(AsterGraphBuiltInComponentCatalog.TryGet(AsterGraphBuiltInComponentCatalog.Canvas, out var canvas));
        Assert.NotNull(canvas);
        Assert.Equal("Canvas", canvas.Title);
        Assert.Equal(AsterGraphBuiltInComponentStatus.Public, canvas.Status);

        Assert.False(AsterGraphBuiltInComponentCatalog.TryGet("missing-component", out var missing));
        Assert.Null(missing);

        Assert.Throws<ArgumentException>(() => AsterGraphBuiltInComponentCatalog.TryGet(" ", out _));
    }

    [Fact]
    public void PublicComponents_DeclareResolvablePublicSymbols()
    {
        var assembly = typeof(AsterGraphBuiltInComponentCatalog).Assembly;
        var publicComponents = AsterGraphBuiltInComponentCatalog.Components
            .Where(component => component.Status == AsterGraphBuiltInComponentStatus.Public)
            .ToArray();

        Assert.NotEmpty(publicComponents);
        foreach (var component in publicComponents)
        {
            Assert.True(
                component.SurfaceTypeName is not null || component.FactoryTypeName is not null,
                $"{component.Id} should declare at least one public type name.");

            AssertPublicType(assembly, component.SurfaceTypeName);
            AssertPublicType(assembly, component.FactoryTypeName);
        }
    }

    [Fact]
    public void PlannedAndInternalTracks_DoNotClaimStandalonePublicSurfaceTypes()
    {
        AssertStatusWithoutSurfaceType(
            AsterGraphBuiltInComponentCatalog.NodeToolbar,
            AsterGraphBuiltInComponentStatus.Planned);
        AssertStatusWithoutSurfaceType(
            AsterGraphBuiltInComponentCatalog.EdgeToolbar,
            AsterGraphBuiltInComponentStatus.Planned);
        AssertStatusWithoutSurfaceType(
            AsterGraphBuiltInComponentCatalog.NodeResizer,
            AsterGraphBuiltInComponentStatus.InternalWorkbench);
    }

    [Fact]
    public void PublicApiDocs_DescribeCatalogAndPhaseThreeABoundary()
    {
        var englishInventory = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseInventory = ReadRepoFile("docs/zh-CN/public-api-inventory.md");
        var avaloniaReadme = ReadRepoFile("src/AsterGraph.Avalonia/README.md");

        foreach (var contents in new[] { englishInventory, chineseInventory, avaloniaReadme })
        {
            Assert.Contains("AsterGraphBuiltInComponentCatalog", contents, StringComparison.Ordinal);
            Assert.Contains("node-toolbar", contents, StringComparison.Ordinal);
            Assert.Contains("edge-toolbar", contents, StringComparison.Ordinal);
            Assert.Contains("node-resizer", contents, StringComparison.Ordinal);
        }
    }

    private static void AssertStatusWithoutSurfaceType(
        string id,
        AsterGraphBuiltInComponentStatus expectedStatus)
    {
        Assert.True(AsterGraphBuiltInComponentCatalog.TryGet(id, out var descriptor));
        Assert.NotNull(descriptor);
        Assert.Equal(expectedStatus, descriptor.Status);
        Assert.Null(descriptor.SurfaceTypeName);
    }

    private static void AssertPublicType(System.Reflection.Assembly assembly, string? typeName)
    {
        if (typeName is null)
        {
            return;
        }

        var type = assembly.GetType(typeName, throwOnError: false);
        Assert.NotNull(type);
        Assert.True(type!.IsPublic, $"{typeName} should be public.");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../", relativePath));
        return File.ReadAllText(fullPath);
    }
}
