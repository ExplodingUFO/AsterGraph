using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ProofFriendlyHostedTemplateClosureDocsTests
{
    [Fact]
    public void HostedRecipeDocs_CloseProofHandoffOnConsumerSampleAndLocalSupportBundle()
    {
        var starterReadme = ReadRepoFile("tools/AsterGraph.Starter.Avalonia/README.md");
        var consumerReadme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var supportBundleEn = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var proofMarkerHeadingEn = "Expected proof markers:";
        var proofMarkerHeadingZh = "预期 proof marker：";
        var bundleMarkerHeadingEn = "Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:";
        var bundleMarkerHeadingZh = "当提供 `--support-bundle <support-bundle-path>` 时，预期 bundle marker：";

        Assert.Contains("proof handoff", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", starterReadme, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", starterReadme, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK:True", starterReadme, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:*", starterReadme, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", starterReadme, StringComparison.Ordinal);
        Assert.Contains("local evidence only", starterReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("--support-bundle", consumerReadme, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", consumerReadme, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", ExtractBlock(consumerReadme, bundleMarkerHeadingEn, "## "), StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", ExtractBlock(consumerReadme, bundleMarkerHeadingEn, "## "), StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_OK", ExtractBlock(consumerReadme, proofMarkerHeadingEn, bundleMarkerHeadingEn), StringComparison.Ordinal);
        Assert.Contains("local evidence only", consumerReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("--support-bundle", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", ExtractBlock(consumerSampleEn, bundleMarkerHeadingEn, "## "), StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", ExtractBlock(consumerSampleEn, bundleMarkerHeadingEn, "## "), StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_OK", ExtractBlock(consumerSampleEn, proofMarkerHeadingEn, bundleMarkerHeadingEn), StringComparison.Ordinal);
        Assert.Contains("local evidence only", consumerSampleEn, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("artifacts/consumer-support-bundle.json", consumerSampleEn, StringComparison.Ordinal);

        Assert.Contains("--support-bundle", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", ExtractBlock(consumerSampleZh, bundleMarkerHeadingZh, "## "), StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", ExtractBlock(consumerSampleZh, bundleMarkerHeadingZh, "## "), StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_OK", ExtractBlock(consumerSampleZh, proofMarkerHeadingZh, bundleMarkerHeadingZh), StringComparison.Ordinal);
        Assert.Contains("本地证据", consumerSampleZh, StringComparison.Ordinal);
        Assert.DoesNotContain("artifacts/consumer-support-bundle.json", consumerSampleZh, StringComparison.Ordinal);

        Assert.Contains("ConsumerSample.Avalonia", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_OK:True", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("local evidence", supportBundleEn, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("artifacts/consumer-support-bundle.json", supportBundleEn, StringComparison.Ordinal);

        Assert.Contains("ConsumerSample.Avalonia", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_OK:True", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("NO_SUPPORT_BUNDLE", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("本地证据", supportBundleZh, StringComparison.Ordinal);
        Assert.DoesNotContain("artifacts/consumer-support-bundle.json", supportBundleZh, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string ExtractBlock(string contents, string startMarker, string endMarkerPrefix)
    {
        var startIndex = contents.IndexOf(startMarker, StringComparison.Ordinal);
        if (startIndex < 0)
        {
            throw new InvalidOperationException($"Could not find start marker '{startMarker}'.");
        }

        startIndex += startMarker.Length;
        var endIndex = contents.IndexOf(endMarkerPrefix, startIndex, StringComparison.Ordinal);
        if (endIndex < 0)
        {
            endIndex = contents.Length;
        }

        return contents[startIndex..endIndex];
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
