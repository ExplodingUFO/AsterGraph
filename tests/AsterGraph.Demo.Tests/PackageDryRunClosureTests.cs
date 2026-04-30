using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class PackageDryRunClosureTests
{
    private static readonly string[] ExpectedPublishedPackageIds =
    [
        "AsterGraph.Abstractions",
        "AsterGraph.Core",
        "AsterGraph.Editor",
        "AsterGraph.Avalonia",
    ];

    [Fact]
    public void PublishablePackageProjects_DefineExpectedNuGetManifestMetadata()
    {
        var version = GetPackageVersion();

        foreach (var packageId in ExpectedPublishedPackageIds)
        {
            var projectPath = Path.Combine(GetRepositoryRoot(), "src", packageId, $"{packageId}.csproj");
            var project = XDocument.Load(projectPath);
            var propertyGroup = project.Root?.Element("PropertyGroup");

            Assert.NotNull(propertyGroup);
            Assert.Equal(packageId, propertyGroup!.Element("PackageId")?.Value);
            Assert.Equal("true", propertyGroup.Element("IsPackable")?.Value);
            Assert.Equal("net8.0;net9.0;net10.0", propertyGroup.Element("TargetFrameworks")?.Value);
            Assert.False(string.IsNullOrWhiteSpace(propertyGroup.Element("Description")?.Value));
            Assert.Equal("README.md", propertyGroup.Element("PackageReadmeFile")?.Value);
            Assert.Contains(
                project.Descendants("None"),
                item => string.Equals(item.Attribute("Include")?.Value, "README.md", StringComparison.Ordinal)
                    && string.Equals(item.Attribute("Pack")?.Value, "true", StringComparison.Ordinal)
                    && string.Equals(item.Attribute("PackagePath")?.Value, "\\", StringComparison.Ordinal));

            Assert.Equal(
                [
                    $"{packageId}.{version}.nupkg",
                    $"{packageId}.{version}.snupkg",
                ],
                GetExpectedPackageArtifactNames(packageId, version));
        }
    }

    [Fact]
    public void SharedPackageMetadata_EnablesSymbolsAndSdkPackageValidation()
    {
        var buildProps = XDocument.Load(Path.Combine(GetRepositoryRoot(), "Directory.Build.props"));
        var propertyGroups = buildProps.Root?.Elements("PropertyGroup").ToArray() ?? [];
        var sharedProperties = propertyGroups.Single(group => group.Attribute("Condition") is null);
        var packableProperties = propertyGroups.Single(group =>
            string.Equals(group.Attribute("Condition")?.Value, "'$(IsPackable)' == 'true'", StringComparison.Ordinal));

        var version = sharedProperties.Element("Version")?.Value;
        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.EndsWith("-beta", version, StringComparison.Ordinal);
        Assert.Equal("true", sharedProperties.Element("IncludeSymbols")?.Value);
        Assert.Equal("snupkg", sharedProperties.Element("SymbolPackageFormat")?.Value);
        Assert.Equal("true", packableProperties.Element("EnablePackageValidation")?.Value);
    }

    [Fact]
    public void ReleaseLane_PacksExactlyFourPublicPackagesAndLeavesWpfValidationOnly()
    {
        var ciScript = ReadRepoFile("eng/ci.ps1");
        var publishableProjects = ParsePublishableProjects(ciScript);

        Assert.Equal(
            [
                "src/AsterGraph.Abstractions/AsterGraph.Abstractions.csproj",
                "src/AsterGraph.Core/AsterGraph.Core.csproj",
                "src/AsterGraph.Editor/AsterGraph.Editor.csproj",
                "src/AsterGraph.Avalonia/AsterGraph.Avalonia.csproj",
            ],
            publishableProjects);
        Assert.DoesNotContain(publishableProjects, project => project.Contains("AsterGraph.Wpf", StringComparison.Ordinal));

        var packFunction = ExtractPowerShellFunction(ciScript, "Invoke-Packages");
        Assert.Contains("foreach ($project in $publishableProjects)", packFunction, StringComparison.Ordinal);
        Assert.Contains("'pack'", packFunction, StringComparison.Ordinal);
        Assert.Contains("$packagesOutputPath", packFunction, StringComparison.Ordinal);
        Assert.DoesNotContain("nuget push", packFunction, StringComparison.OrdinalIgnoreCase);

        var wpfSlice = ExtractPowerShellFunction(ciScript, "Invoke-WindowsHelloWorldWpfSlice");
        Assert.Contains("$asterGraphWpfProject", wpfSlice, StringComparison.Ordinal);
        Assert.Contains("$helloWorldWpfProject", wpfSlice, StringComparison.Ordinal);
        Assert.Contains("'build'", wpfSlice, StringComparison.Ordinal);
        Assert.DoesNotContain("'pack'", wpfSlice, StringComparison.Ordinal);

        var releaseLane = ExtractSwitchCase(ciScript, "'release'");
        Assert.Contains("Invoke-ReleaseValidation", releaseLane, StringComparison.Ordinal);
        Assert.DoesNotContain("nuget push", releaseLane, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReleaseWorkflow_PublishesOnlyAfterValidationAndExplicitNuGetGuard()
    {
        var workflow = ReadRepoFile(".github/workflows/release.yml");
        var validationJob = ExtractYamlBlock(workflow, "  windows-release-validation:");
        var publishJob = ExtractYamlBlock(workflow, "  publish-nuget-prerelease:");

        Assert.Contains("Run release validation lane", validationJob, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet nuget push", validationJob, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NUGET_API_KEY", validationJob, StringComparison.Ordinal);

        Assert.Contains("needs:", publishJob, StringComparison.Ordinal);
        Assert.Contains("windows-release-validation", publishJob, StringComparison.Ordinal);
        Assert.Contains("linux-validation", publishJob, StringComparison.Ordinal);
        Assert.Contains("startsWith(github.ref, 'refs/tags/') || (github.event_name == 'workflow_dispatch' && inputs.publish_to_nuget)", publishJob, StringComparison.Ordinal);
        Assert.Contains("Validate manual publish is limited to beta packages", publishJob, StringComparison.Ordinal);
        Assert.Contains("Manual workflow publish is limited to beta packages", publishJob, StringComparison.Ordinal);
        Assert.Contains("Skip NuGet publish when no API key is configured", publishJob, StringComparison.Ordinal);
        Assert.Contains("NUGET_API_KEY is not configured; skipping NuGet prerelease publish.", publishJob, StringComparison.Ordinal);
        Assert.Contains("dotnet nuget push", publishJob, StringComparison.Ordinal);
        Assert.Contains("--skip-duplicate", publishJob, StringComparison.Ordinal);
        Assert.Contains("Where-Object { $_.Name -notlike '*.snupkg' }", publishJob, StringComparison.Ordinal);
    }

    private static string[] GetExpectedPackageArtifactNames(string packageId, string version)
        => [$"{packageId}.{version}.nupkg", $"{packageId}.{version}.snupkg"];

    private static string[] ParsePublishableProjects(string ciScript)
    {
        var match = Regex.Match(ciScript, @"\$publishableProjects\s*=\s*@\((?<body>.*?)\)", RegexOptions.Singleline);
        Assert.True(match.Success, "eng/ci.ps1 must define $publishableProjects.");

        return Regex.Matches(match.Groups["body"].Value, "'(?<project>[^']+)'")
            .Select(item => item.Groups["project"].Value)
            .ToArray();
    }

    private static string ExtractPowerShellFunction(string script, string functionName)
    {
        var start = script.IndexOf($"function {functionName}", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Function {functionName} was not found.");

        var nextFunction = script.IndexOf("\nfunction ", start + 1, StringComparison.Ordinal);
        return nextFunction < 0 ? script[start..] : script[start..nextFunction];
    }

    private static string ExtractSwitchCase(string script, string caseLabel)
    {
        var start = script.IndexOf($"  {caseLabel} {{", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Switch case {caseLabel} was not found.");

        var nextCase = script.IndexOf("\n  '", start + 1, StringComparison.Ordinal);
        return nextCase < 0 ? script[start..] : script[start..nextCase];
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

    private static string GetPackageVersion()
    {
        var props = XDocument.Load(Path.Combine(GetRepositoryRoot(), "Directory.Build.props"));
        var version = props.Descendants("Version").SingleOrDefault()?.Value;
        Assert.False(string.IsNullOrWhiteSpace(version));
        return version!;
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string GetRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            if (File.Exists(Path.Combine(directory, "AsterGraph.sln")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test output directory.");
    }
}
