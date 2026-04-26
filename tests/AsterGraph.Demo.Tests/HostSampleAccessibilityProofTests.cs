using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class HostSampleAccessibilityProofTests
{
    [Fact]
    public void HostSampleProof_SurfaceAutomationAndAccessibilityBaselineMarkers()
    {
        var program = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "tools", "AsterGraph.HostSample", "Program.cs"));
        var ciScript = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "eng", "ci.ps1"));

        Assert.Contains("HOST_SAMPLE_AUTOMATION_OK", program, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_ACCESSIBILITY_BASELINE_OK", program, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_ACCESSIBILITY_AUTOMATION_OK", program, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_OK", program, StringComparison.Ordinal);
        Assert.Contains("### Run HostSample ($modeLabel)", ciScript, StringComparison.Ordinal);
        Assert.Contains("HOST_SAMPLE_OK:True", ciScript, StringComparison.Ordinal);
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
