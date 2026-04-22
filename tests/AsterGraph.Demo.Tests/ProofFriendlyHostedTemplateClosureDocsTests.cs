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

        Assert.Contains("proof handoff", starterReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", starterReadme, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", starterReadme, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK:True", starterReadme, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:*", starterReadme, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", starterReadme, StringComparison.Ordinal);
        Assert.Contains("local evidence only", starterReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("proof handoff", consumerReadme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", consumerReadme, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", consumerReadme, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", consumerReadme, StringComparison.Ordinal);
        Assert.Contains("local evidence only", consumerReadme, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("proof handoff", consumerSampleEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", consumerSampleEn, StringComparison.Ordinal);
        Assert.Contains("local evidence only", consumerSampleEn, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("proof handoff", consumerSampleZh, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia -- --proof", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("CONSUMER_SAMPLE_OK:True", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", consumerSampleZh, StringComparison.Ordinal);
        Assert.Contains("本地证据", consumerSampleZh, StringComparison.Ordinal);

        Assert.Contains("ConsumerSample.Avalonia", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_OK:True", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", supportBundleEn, StringComparison.Ordinal);
        Assert.Contains("local evidence", supportBundleEn, StringComparison.OrdinalIgnoreCase);

        Assert.Contains("ConsumerSample.Avalonia", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_OK:True", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", supportBundleZh, StringComparison.Ordinal);
        Assert.Contains("本地证据", supportBundleZh, StringComparison.Ordinal);
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
