namespace AsterGraph.Editor.Tests;

internal static class TestPluginArtifactPathHelper
{
    private const string PluginAssemblyFileName = "AsterGraph.TestPlugins.dll";

    public static string GetSamplePluginAssemblyPath()
    {
        var candidate = Path.Combine(
            GetRepositoryTestsDirectory(),
            "AsterGraph.TestPlugins",
            "bin",
            GetCurrentConfiguration(),
            GetCurrentTargetFramework(),
            PluginAssemblyFileName);

        var fullPath = Path.GetFullPath(candidate);
        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        throw new FileNotFoundException(
            $"Failed to locate sample plugin assembly at '{fullPath}'. Build tests/AsterGraph.TestPlugins for the active configuration and target framework before running plugin integration tests.",
            fullPath);
    }

    public static string GetCurrentConfiguration()
    {
        var baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var frameworkDirectory = new DirectoryInfo(baseDirectory);
        var configurationDirectory = frameworkDirectory.Parent
            ?? throw new InvalidOperationException("Test base directory must have a configuration directory.");

        return configurationDirectory.Name;
    }

    public static string GetCurrentTargetFramework()
    {
        var baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var frameworkDirectory = new DirectoryInfo(baseDirectory);
        if (string.IsNullOrWhiteSpace(frameworkDirectory.Name))
        {
            throw new InvalidOperationException("Test base directory must have a target framework directory.");
        }

        return frameworkDirectory.Name;
    }

    private static string GetRepositoryTestsDirectory()
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
}
