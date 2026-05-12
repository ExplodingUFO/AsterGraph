using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class PixelBaselinePolicyDocsTests
{
    [Fact]
    public void PixelBaselinePolicyDocs_RecordPhase515DeferredStrictHashDecision()
    {
        var englishCookbook = ReadRepoFile("docs/en/demo-cookbook.md");
        var chineseCookbook = ReadRepoFile("docs/zh-CN/demo-cookbook.md");
        var englishParity = ReadRepoFile("docs/en/phase-0-reactflow-parity-audit.md");
        var chineseParity = ReadRepoFile("docs/zh-CN/phase-0-reactflow-parity-audit.md");

        foreach (var contents in new[] { englishCookbook, chineseCookbook, englishParity, chineseParity })
        {
            Assert.Contains("Phase 515", contents, StringComparison.Ordinal);
            Assert.Contains("GitHub #151", contents, StringComparison.Ordinal);
            Assert.Contains("avalonia-node-map-t44", contents, StringComparison.Ordinal);
            Assert.Contains("PngSha256", contents, StringComparison.Ordinal);
            Assert.Contains("DriftMeasurement", contents, StringComparison.Ordinal);
            Assert.Contains("Policy=record-only", contents, StringComparison.Ordinal);
            Assert.Contains("PngHashPurpose=drift-evidence", contents, StringComparison.Ordinal);
            Assert.Contains("StrictPixelBaselineEnforced=false", contents, StringComparison.Ordinal);
            Assert.Contains("HostRuntimeDescription", contents, StringComparison.Ordinal);
            Assert.Contains("OsDescription", contents, StringComparison.Ordinal);
            Assert.Contains("ProcessArchitecture", contents, StringComparison.Ordinal);
            Assert.Contains("two successful Phase 512 release-validation artifact sets", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("25 metadata.json", contents, StringComparison.Ordinal);
            Assert.Contains("shell-cookbook-default-host-command-tooltip-popup", contents, StringComparison.Ordinal);
            Assert.Contains("single-hash", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("deferred", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("host-keyed or tolerant", contents, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("repeatable no-drift evidence", contents, StringComparison.OrdinalIgnoreCase);
        }
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
