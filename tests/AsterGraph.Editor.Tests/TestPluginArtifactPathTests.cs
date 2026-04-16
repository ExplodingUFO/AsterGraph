using System.Reflection;
using Xunit;

namespace AsterGraph.Editor.Tests;

public sealed class TestPluginArtifactPathTests
{
    [Fact]
    public void SamplePluginAssemblyPathResolvers_UseCurrentTestConfiguration()
    {
        var currentConfiguration = GetCurrentConfiguration();
        var resolverTypes = new[]
        {
            typeof(GraphEditorPluginDiscoveryTests),
            typeof(GraphEditorPluginInspectionContractsTests),
            typeof(GraphEditorPluginLoadingTests),
            typeof(GraphEditorPluginPackageStagingTests),
            typeof(GraphEditorProofRingTests),
        };

        foreach (var resolverType in resolverTypes)
        {
            var path = GetSamplePluginAssemblyPath(resolverType);

            Assert.Contains(
                $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}{currentConfiguration}{Path.DirectorySeparatorChar}",
                path,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string GetSamplePluginAssemblyPath(Type declaringType)
    {
        var method = declaringType.GetMethod(
            "GetSamplePluginAssemblyPath",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        return Assert.IsType<string>(method!.Invoke(null, null));
    }

    private static string GetCurrentConfiguration()
    {
        var baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var frameworkDirectory = new DirectoryInfo(baseDirectory);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Test base directory must have a configuration directory.");

        return configurationDirectory.Name;
    }
}
