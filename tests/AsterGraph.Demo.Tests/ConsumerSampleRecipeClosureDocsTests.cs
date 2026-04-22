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
        AssertContains(readme, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(readme, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(readme, "sample-owned details are the review/audit node family");
        AssertContains(readme, "sample action ids and titles");
        AssertContains(readme, "window layout and narrative text");
        AssertContains(readme, "proof labels or copy beyond the defended public markers");

        AssertContains(consumerSampleEn, "canonical session/runtime model only");
        AssertContains(consumerSampleEn, "three host-owned seams");
        AssertContains(consumerSampleEn, "action rail / command projection");
        AssertContains(consumerSampleEn, "plugin trust workflow");
        AssertContains(consumerSampleEn, "parameter-editing composition");
        AssertContains(consumerSampleEn, "AsterGraphHostedActionFactory.CreateCommandActions(...)");
        AssertContains(consumerSampleEn, "AsterGraphHostedActionFactory.CreateProjection(...)");
        AssertContains(consumerSampleEn, "GraphEditorPluginDiscoveryOptions");
        AssertContains(consumerSampleEn, "AsterGraphEditorOptions.PluginTrustPolicy");
        AssertContains(consumerSampleEn, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(consumerSampleEn, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(consumerSampleEn, "sample-owned details such as the review/audit node family");
        AssertContains(consumerSampleEn, "sample-owned content such as the review/audit node family");
        AssertContains(consumerSampleEn, "second editor model");
        AssertContains(consumerSampleEn, "sandbox");
        AssertContains(consumerSampleEn, "broader plugin ecosystem");

        AssertContains(consumerSampleZh, "三条宿主管线");
        AssertContains(consumerSampleZh, "样例自有内容");
        AssertContains(consumerSampleZh, "不引入第二套");
        AssertContains(consumerSampleZh, "不提供 sandbox");
        AssertContains(consumerSampleZh, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(consumerSampleZh, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
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
