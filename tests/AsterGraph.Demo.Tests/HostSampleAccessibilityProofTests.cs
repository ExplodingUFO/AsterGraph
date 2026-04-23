using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class HostSampleAccessibilityProofTests
{
    [Fact]
    public void HostSample_EmitsAutomationAndAccessibilityBaselineProofMarkers()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{Path.Combine(GetRepositoryRoot(), "tools", "AsterGraph.HostSample", "AsterGraph.HostSample.csproj")}\" --nologo",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WorkingDirectory = GetRepositoryRoot(),
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        var stdout = process!.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(
            process.ExitCode == 0,
            $"HostSample failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}STDERR:{Environment.NewLine}{stderr}");

        Assert.Contains("HOST_SAMPLE_AUTOMATION_OK:True", stdout, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK:True", stdout, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_OK:True", stdout, StringComparison.Ordinal);
    }

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
