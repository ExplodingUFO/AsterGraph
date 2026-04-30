using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class CrossPlatformHostPackagingCiTests
{
    [Fact]
    public void CiWorkflow_DefendsDesktopHostPackagingAcrossOsesAndTargetFrameworks()
    {
        var workflow = ReadRepoFile(".github/workflows/ci.yml");
        var ciScript = ReadRepoFile("eng/ci.ps1");
        var starterReadme = ReadRepoFile("tools/AsterGraph.Starter.Avalonia/README.md");
        var templateReadme = ReadRepoFile("templates/astergraph-avalonia/README.md");
        var quickStartEn = ReadRepoFile("docs/en/quick-start.md");
        var quickStartZh = ReadRepoFile("docs/zh-CN/quick-start.md");

        var frameworkMatrix = ExtractYamlBlock(workflow, "  framework-matrix:");
        Assert.Contains("windows-latest", frameworkMatrix, StringComparison.Ordinal);
        Assert.Contains("- net8.0", frameworkMatrix, StringComparison.Ordinal);
        Assert.Contains("- net9.0", frameworkMatrix, StringComparison.Ordinal);
        Assert.Contains("- net10.0", frameworkMatrix, StringComparison.Ordinal);
        Assert.Contains("-Framework ${{ matrix.framework }}", frameworkMatrix, StringComparison.Ordinal);

        var linuxLane = ExtractYamlBlock(workflow, "  linux-validation:");
        var macosLane = ExtractYamlBlock(workflow, "  macos-validation:");
        Assert.Contains("ubuntu-latest", linuxLane, StringComparison.Ordinal);
        Assert.Contains("-Framework all", linuxLane, StringComparison.Ordinal);
        Assert.Contains("macos-latest", macosLane, StringComparison.Ordinal);
        Assert.Contains("-Framework all", macosLane, StringComparison.Ordinal);

        var releaseLane = ExtractYamlBlock(workflow, "  release-validation:");
        Assert.Contains("framework-matrix", releaseLane, StringComparison.Ordinal);
        Assert.Contains("linux-validation", releaseLane, StringComparison.Ordinal);
        Assert.Contains("macos-validation", releaseLane, StringComparison.Ordinal);
        Assert.Contains("hostsample-net10-packed.txt", releaseLane, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_NET10_OK", releaseLane, StringComparison.Ordinal);
        Assert.Contains("template-smoke.txt", ciScript, StringComparison.Ordinal);
        Assert.Contains("Invoke-TemplateSmoke", ciScript, StringComparison.Ordinal);
        Assert.Contains("Invoke-HostSample -UsePackedPackages -TargetFramework net10.0", ciScript, StringComparison.Ordinal);
        Assert.Contains("dotnet new astergraph-avalonia", templateReadme, StringComparison.Ordinal);

        foreach (var contents in new[] { starterReadme, templateReadme, quickStartEn, quickStartZh })
        {
            Assert.Contains("cross-platform packaging proof", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("net8.0", contents, StringComparison.Ordinal);
            Assert.Contains("net9.0", contents, StringComparison.Ordinal);
            Assert.Contains("net10.0", contents, StringComparison.Ordinal);
        }
    }

    private static string ExtractYamlBlock(string yaml, string heading)
    {
        var start = yaml.IndexOf(heading, StringComparison.Ordinal);
        Assert.True(start >= 0, $"YAML block {heading.Trim()} was not found.");

        var nextJob = yaml.IndexOf("\n  ", start + heading.Length, StringComparison.Ordinal);
        while (nextJob >= 0 && (nextJob + 3 >= yaml.Length || yaml[nextJob + 3] == ' '))
        {
            nextJob = yaml.IndexOf("\n  ", nextJob + 1, StringComparison.Ordinal);
        }

        return nextJob < 0 ? yaml[start..] : yaml[start..nextJob];
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
