using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class HostSampleExportProofTests
{
    [Fact]
    public void HostSample_EmitsExportProofMarker()
    {
        var program = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "tools", "AsterGraph.HostSample", "Program.cs"));

        Assert.Contains("HOST_SAMPLE_EXPORT_OK", program, StringComparison.Ordinal);
    }

    [Fact]
    public void HostIntegrationDocs_DescribeExportAsDistinctCapability()
    {
        var readme = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "README.md"));
        var hostIntegration = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "docs", "en", "host-integration.md"));

        Assert.Contains("export", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IGraphSceneSvgExportService", hostIntegration, StringComparison.Ordinal);
        Assert.Contains("workspace", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fragment", hostIntegration, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("export", hostIntegration, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HostSample_EmitsReconnectProofMarker()
    {
        var program = File.ReadAllText(Path.Combine(GetRepositoryRoot(), "tools", "AsterGraph.HostSample", "Program.cs"));

        Assert.Contains("HOST_SAMPLE_RECONNECT_OK", program, StringComparison.Ordinal);
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
