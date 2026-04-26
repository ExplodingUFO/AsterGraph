using System.Text.Json;
using AsterGraph.PluginTool;
using Xunit;

namespace AsterGraph.PluginTool.Tests;

public sealed class PluginToolValidationTests
{
    [Fact]
    public void ValidateAssembly_EmitsManifestTrustHashAndPerformanceMarker()
    {
        var assemblyPath = typeof(AsterGraph.TestPlugins.SamplePlugin).Assembly.Location;
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = PluginToolProgram.Run(["validate", assemblyPath], output, error);

        var text = output.ToString();
        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE:candidates=1:elapsed_ms=", text, StringComparison.Ordinal);
        Assert.True(ReadElapsedMilliseconds(text) < 2_500);
        Assert.Contains("PLUGIN:AsterGraph.TestPlugins", text, StringComparison.Ordinal);
        Assert.Contains("target_framework:", text, StringComparison.Ordinal);
        Assert.Contains("trust: Allowed:ImplicitAllow", text, StringComparison.Ordinal);
        Assert.Contains("sha256:", text, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE_OK:True", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateMissingPath_ReturnsNotFoundWithoutThrowing()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.dll");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = PluginToolProgram.Run(["validate", missingPath], output, error);

        Assert.Equal(2, exitCode);
        Assert.Empty(output.ToString());
        Assert.Contains("Plugin path was not found", error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("templates/astergraph-avalonia/.template.config/template.json", "astergraph-avalonia")]
    [InlineData("templates/astergraph-plugin/.template.config/template.json", "astergraph-plugin")]
    public void Templates_ExposeDotnetNewShortNames(string templatePath, string shortName)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(FindRepoPath(templatePath)));

        var root = document.RootElement;
        Assert.Equal(shortName, root.GetProperty("shortName").GetString());
        Assert.Equal("C#", root.GetProperty("tags").GetProperty("language").GetString());
        Assert.True(root.GetProperty("preferNameDirectory").GetBoolean());
    }

    [Fact]
    public void Templates_StayCrossPlatformAndNative()
    {
        var avaloniaProgram = File.ReadAllText(FindRepoPath("templates/astergraph-avalonia/Program.cs"));
        var avaloniaProject = File.ReadAllText(FindRepoPath("templates/astergraph-avalonia/AsterGraphAvaloniaHost.csproj"));
        var pluginProject = File.ReadAllText(FindRepoPath("templates/astergraph-plugin/AsterGraphPlugin.csproj"));

        Assert.Contains(".UsePlatformDetect()", avaloniaProgram, StringComparison.Ordinal);
        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", avaloniaProject, StringComparison.Ordinal);
        Assert.Contains("<TargetFramework>net8.0</TargetFramework>", pluginProject, StringComparison.Ordinal);
        Assert.DoesNotContain("-windows", avaloniaProject, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("-windows", pluginProject, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoPath(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository path '{relativePath}'.");
    }

    private static long ReadElapsedMilliseconds(string output)
    {
        const string marker = "ASTERGRAPH_PLUGIN_VALIDATE:candidates=1:elapsed_ms=";
        var line = output
            .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
            .Single(value => value.StartsWith(marker, StringComparison.Ordinal));

        return long.Parse(line[marker.Length..], System.Globalization.CultureInfo.InvariantCulture);
    }
}
