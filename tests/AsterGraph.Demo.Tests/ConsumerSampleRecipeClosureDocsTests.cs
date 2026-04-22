using System;
using System.IO;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ConsumerSampleRecipeClosureDocsTests
{
    [Fact]
    public void ConsumerSampleRecipeClosureDocs_DefendCanonicalHostSeamsAndSampleOwnedBoundaries()
    {
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");

        AssertContains(readme, "canonical session/runtime route");
        AssertContains(readme, "action rail / command projection");
        AssertContains(readme, "plugin trust workflow");
        AssertContains(readme, "parameter-editing composition");
        AssertContains(readme, "AsterGraphHostedActionFactory.CreateCommandActions(...)");
        AssertContains(readme, "AsterGraphHostedActionFactory.CreateProjection(...)");
        AssertContains(readme, "GraphEditorPluginDiscoveryOptions");
        AssertContains(readme, "AsterGraphEditorOptions.PluginTrustPolicy");
        AssertContains(readme, "GetSelectedParameterSnapshots()");
        AssertContains(readme, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(readme, "sample-owned details are the review/audit node family");
        AssertContains(readme, "sample action ids and titles");
        AssertContains(readme, "window layout and narrative text");
        AssertContains(readme, "proof labels or copy beyond the defended public markers");

        AssertContains(consumerSampleEn, "canonical session/runtime model only");
        AssertContains(consumerSampleEn, "second editor model");
        AssertContains(consumerSampleEn, "a sandbox");
        AssertContains(consumerSampleEn, "a broader plugin ecosystem");
        AssertContains(consumerSampleEn, "action rail / command projection");
        AssertContains(consumerSampleEn, "plugin trust workflow");
        AssertContains(consumerSampleEn, "parameter-editing composition");
        AssertContains(consumerSampleEn, "AsterGraphHostedActionFactory.CreateCommandActions(...)");
        AssertContains(consumerSampleEn, "AsterGraphHostedActionFactory.CreateProjection(...)");
        AssertContains(consumerSampleEn, "GraphEditorPluginDiscoveryOptions");
        AssertContains(consumerSampleEn, "AsterGraphEditorOptions.PluginTrustPolicy");
        AssertContains(consumerSampleEn, "GetSelectedParameterSnapshots()");
        AssertContains(consumerSampleEn, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(consumerSampleEn, "sample-owned content such as the review/audit node family");
        AssertContains(consumerSampleEn, "action ids and titles");
        AssertContains(consumerSampleEn, "proof labels beyond the defended markers should stay local to your app");

        AssertContains(consumerSampleZh, "canonical session/runtime model");
        AssertContains(consumerSampleZh, "action rail / command projection");
        AssertContains(consumerSampleZh, "plugin trust workflow");
        AssertContains(consumerSampleZh, "parameter-editing composition");
        AssertContains(consumerSampleZh, "AsterGraphHostedActionFactory.CreateCommandActions(...)");
        AssertContains(consumerSampleZh, "AsterGraphHostedActionFactory.CreateProjection(...)");
        AssertContains(consumerSampleZh, "GraphEditorPluginDiscoveryOptions");
        AssertContains(consumerSampleZh, "AsterGraphEditorOptions.PluginTrustPolicy");
        AssertContains(consumerSampleZh, "GetSelectedParameterSnapshots()");
        AssertContains(consumerSampleZh, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(consumerSampleZh, "review/audit 节点族");
        AssertContains(consumerSampleZh, "action ids/titles");
        AssertContains(consumerSampleZh, "窗口布局");
        Assert.Contains("defended markers", consumerSampleZh, StringComparison.Ordinal);
    }

    private static void AssertContains(string contents, string expected)
        => Assert.Contains(expected, contents, StringComparison.Ordinal);

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
