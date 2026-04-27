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
        Assert.Contains("source_kind:", text, StringComparison.Ordinal);
        Assert.Contains("display_name:", text, StringComparison.Ordinal);
        Assert.Contains("target_framework:", text, StringComparison.Ordinal);
        Assert.Contains("capability_summary:", text, StringComparison.Ordinal);
        Assert.Contains("host_compatibility:", text, StringComparison.Ordinal);
        Assert.Contains("trust: Allowed:ImplicitAllow", text, StringComparison.Ordinal);
        Assert.Contains("signature:", text, StringComparison.Ordinal);
        Assert.Contains("sha256:", text, StringComparison.Ordinal);
        Assert.Contains("node_definitions: 1", text, StringComparison.Ordinal);
        Assert.Contains("parameter_metadata: 1", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_COMPATIBILITY_OK:True", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_MANIFEST_OK:True", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_NODE_DEFINITIONS_OK:True", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_PARAMETER_METADATA_OK:True", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_TRUST_EVIDENCE_OK:True", text, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE_OK:True", text, StringComparison.Ordinal);
    }

    [Fact]
    public void InspectAssembly_WithJsonAndHostVersion_EmitsCompatibilityDefinitionsAndMetadata()
    {
        var assemblyPath = typeof(AsterGraph.TestPlugins.SamplePlugin).Assembly.Location;
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = PluginToolProgram.Run(["inspect", assemblyPath, "--host-version", "0.15.0-beta", "--json"], output, error);

        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());

        using var document = JsonDocument.Parse(output.ToString());
        var root = document.RootElement;
        Assert.Equal("0.15.0-beta", root.GetProperty("hostVersion").GetString());
        var markers = root.GetProperty("proofMarkers");
        Assert.True(markers.GetProperty("PLUGIN_COMPATIBILITY_OK").GetBoolean());
        Assert.True(markers.GetProperty("PLUGIN_MANIFEST_OK").GetBoolean());
        Assert.True(markers.GetProperty("PLUGIN_NODE_DEFINITIONS_OK").GetBoolean());
        Assert.True(markers.GetProperty("PLUGIN_PARAMETER_METADATA_OK").GetBoolean());
        Assert.True(markers.GetProperty("PLUGIN_TRUST_EVIDENCE_OK").GetBoolean());

        var candidate = root.GetProperty("candidates").EnumerateArray().Single();
        Assert.Equal("Compatible", candidate.GetProperty("hostCompatibility").GetProperty("status").GetString());
        Assert.Equal("tests.sample-plugin.node", candidate.GetProperty("nodeDefinitions").EnumerateArray().Single().GetProperty("id").GetString());
        var parameter = candidate
            .GetProperty("nodeDefinitions")
            .EnumerateArray()
            .Single()
            .GetProperty("parameterMetadata")
            .EnumerateArray()
            .Single();
        Assert.Equal("mode", parameter.GetProperty("key").GetString());
        Assert.Equal("Plugin", parameter.GetProperty("groupName").GetString());
        Assert.True(parameter.GetProperty("hasConstraints").GetBoolean());
    }

    [Fact]
    public void HashAssembly_EmitsSha256TrustEvidenceMarker()
    {
        var assemblyPath = typeof(AsterGraph.TestPlugins.SamplePlugin).Assembly.Location;
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = PluginToolProgram.Run(["hash", assemblyPath], output, error);

        var text = output.ToString();
        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        Assert.Contains("PLUGIN_HASH:source=", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_HASH_SHA256:", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_TRUST_EVIDENCE_OK:True", text, StringComparison.Ordinal);
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

    [Fact]
    public void Help_DescribesAcceptedInputsEvidenceMarkersAndNonGoals()
    {
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = PluginToolProgram.Run(["validate", "--help"], output, error);

        var text = output.ToString();
        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        Assert.Contains("Usage: AsterGraph.PluginTool validate <plugin-directory|plugin.dll|plugin.nupkg>", text, StringComparison.Ordinal);
        Assert.Contains("Accepted inputs:", text, StringComparison.Ordinal);
        Assert.Contains("Scans top-level .dll and .nupkg plugin artifacts.", text, StringComparison.Ordinal);
        Assert.Contains("Expected evidence markers:", text, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE:source=<path>", text, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE:candidates=<count>:elapsed_ms=<ms>", text, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE_OK:<bool>", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_COMPATIBILITY_OK:<bool>", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_MANIFEST_OK:<bool>", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_NODE_DEFINITIONS_OK:<bool>", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_PARAMETER_METADATA_OK:<bool>", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN_TRUST_EVIDENCE_OK:<bool>", text, StringComparison.Ordinal);
        Assert.Contains("PLUGIN:<id>", text, StringComparison.Ordinal);
        Assert.Contains("target_framework:, capability_summary:, host_compatibility:, trust:, signature:, sha256:", text, StringComparison.Ordinal);
        Assert.Contains("does not approve marketplace distribution, sandbox code, unload plugins, or isolate untrusted code", text, StringComparison.Ordinal);
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
        var avaloniaReadme = File.ReadAllText(FindRepoPath("templates/astergraph-avalonia/README.md"));
        var avaloniaProject = File.ReadAllText(FindRepoPath("templates/astergraph-avalonia/AsterGraphAvaloniaHost.csproj"));
        var pluginReadme = File.ReadAllText(FindRepoPath("templates/astergraph-plugin/README.md"));
        var pluginProgram = File.ReadAllText(FindRepoPath("templates/astergraph-plugin/SamplePlugin.cs"));
        var pluginManifest = File.ReadAllText(FindRepoPath("templates/astergraph-plugin/astergraph.plugin.json"));
        var pluginProject = File.ReadAllText(FindRepoPath("templates/astergraph-plugin/AsterGraphPlugin.csproj"));

        Assert.Contains(".UsePlatformDetect()", avaloniaProgram, StringComparison.Ordinal);
        Assert.Contains("AsterGraphHostBuilder", avaloniaProgram, StringComparison.Ordinal);
        Assert.Contains("BuildAvaloniaView()", avaloniaProgram, StringComparison.Ordinal);
        Assert.Contains("Builder-first hosted route", avaloniaReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphEditorFactory.Create(...)", avaloniaReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraphAvaloniaViewFactory.Create(...)", avaloniaReadme, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE_OK:True", avaloniaReadme, StringComparison.Ordinal);
        Assert.Contains("minimal trusted in-process AsterGraph plugin", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("SamplePlugin.Descriptor", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("SampleNodeDefinitionProvider", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("astergraph.plugin.json", pluginReadme, StringComparison.Ordinal);
        Assert.Contains(".dll", pluginReadme, StringComparison.Ordinal);
        Assert.Contains(".nupkg", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("directory", pluginReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ASTERGRAPH_PLUGIN_VALIDATE_OK:True", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("target_framework:", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("capability_summary:", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("sha256:", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("does not add marketplace distribution, sandboxing, unload/reload, or untrusted-code isolation", pluginReadme, StringComparison.Ordinal);
        Assert.Contains("PluginId", pluginProgram, StringComparison.Ordinal);
        Assert.Contains("version: \"1.0.0\"", pluginProgram, StringComparison.Ordinal);
        Assert.Contains("public trusted plugin contract", pluginProgram, StringComparison.Ordinal);
        Assert.Contains("\"pluginTypeName\": \"AsterGraphPlugin.SamplePlugin\"", pluginManifest, StringComparison.Ordinal);
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
