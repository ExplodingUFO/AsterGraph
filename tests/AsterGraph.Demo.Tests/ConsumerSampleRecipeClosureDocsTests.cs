using System;
using System.IO;
using System.Linq;
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
        AssertContains(readme, "Copy These Host-Owned Seams");
        AssertContains(readme, "Replace These Sample-Owned Details");

        AssertContains(consumerSampleEn, "canonical session/runtime model only");
        AssertContains(consumerSampleEn, "three host-owned seams");
        AssertContains(consumerSampleEn, "action rail / command projection");
        AssertContains(consumerSampleEn, "plugin trust workflow");
        AssertContains(consumerSampleEn, "parameter-editing composition");
        AssertContains(consumerSampleEn, "Plugin Manifest and Trust Policy Contract v1");
        AssertContains(consumerSampleEn, "Beta Support Bundle");
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
        AssertContains(consumerSampleEn, "Copy These Host-Owned Seams");
        AssertContains(consumerSampleEn, "Replace These Sample-Owned Details");
        AssertContains(consumerSampleEn, "review/audit node family");
        AssertContains(consumerSampleEn, "action ids and titles");
        AssertContains(consumerSampleEn, "window layout and narrative text");
        AssertContains(consumerSampleEn, "proof labels beyond the defended markers");
        AssertContains(consumerSampleEn, "The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.");
        AssertContains(consumerSampleEn, "`HostSample` is the post-ladder proof harness.");
        Assert.True(HasLineWith(consumerSampleEn, "AsterGraph.ConsumerSample.Avalonia -- --proof", "first"));

        AssertContains(consumerSampleZh, "三条宿主管线");
        AssertContains(consumerSampleZh, "样例自有内容");
        AssertContains(consumerSampleZh, "不引入第二套");
        AssertContains(consumerSampleZh, "不提供 sandbox");
        AssertContains(consumerSampleZh, "插件信任契约 v1");
        AssertContains(consumerSampleZh, "Beta Support Bundle");
        AssertContains(consumerSampleZh, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(consumerSampleZh, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(consumerSampleZh, "复制这些宿主自管 seam");
        AssertContains(consumerSampleZh, "替换这些样例自有内容");
        AssertContains(consumerSampleZh, "review/audit 节点族");
        AssertContains(consumerSampleZh, "action ids/titles");
        AssertContains(consumerSampleZh, "窗口布局和叙述文本");
        AssertContains(consumerSampleZh, "defended markers 之外的 proof 文案");
        AssertContains(consumerSampleZh, "这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。");
        AssertContains(consumerSampleZh, "`HostSample` 是这条 ladder 之后的 proof harness。");
        Assert.True(HasLineWith(consumerSampleZh, "AsterGraph.ConsumerSample.Avalonia -- --proof", "先跑"));
    }

    [Fact]
    public void ConsumerSampleRecipeClosureDocs_SurfaceDedicatedTrustAndProofCopyBlocks()
    {
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var supportBundleEn = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");

        AssertContains(readme, "Trust and proof quick reference");
        AssertContains(readme, "Copyable trust and proof reference");
        AssertContains(readme, "../../docs/en/support-bundle.md");
        AssertContains(readme, "../../docs/en/adoption-feedback.md");
        AssertContains(readme, "../../docs/en/public-launch-checklist.md");

        AssertContains(consumerSampleEn, "Trust and proof quick reference");
        AssertContains(consumerSampleEn, "Copyable trust and proof reference");
        AssertContains(consumerSampleEn, "SUPPORT_BUNDLE_OK:True");
        AssertContains(consumerSampleEn, "CONSUMER_SAMPLE_TRUST_OK:True");
        AssertContains(consumerSampleEn, "./support-bundle.md");
        AssertContains(consumerSampleEn, "./adoption-feedback.md");
        AssertContains(consumerSampleEn, "./public-launch-checklist.md");

        AssertContains(consumerSampleZh, "信任与证明速查");
        AssertContains(consumerSampleZh, "可复制的信任与证明参考");
        AssertContains(consumerSampleZh, "SUPPORT_BUNDLE_OK:True");
        AssertContains(consumerSampleZh, "CONSUMER_SAMPLE_TRUST_OK:True");
        AssertContains(consumerSampleZh, "./support-bundle.md");
        AssertContains(consumerSampleZh, "./adoption-feedback.md");
        AssertContains(consumerSampleZh, "./public-launch-checklist.md");

        AssertContains(supportBundleEn, "Local evidence only");
        AssertContains(supportBundleEn, "Copyable local evidence reference");
        AssertContains(supportBundleZh, "仅限本地证据");
        AssertContains(supportBundleZh, "可复制的本地证据参考");

        Assert.True(consumerSampleEn.IndexOf("Plugin Manifest and Trust Policy Contract v1", StringComparison.Ordinal) < consumerSampleEn.IndexOf("## What It Proves", StringComparison.Ordinal));
        Assert.True(consumerSampleEn.IndexOf("Beta Support Bundle", StringComparison.Ordinal) < consumerSampleEn.IndexOf("## What It Proves", StringComparison.Ordinal));
        Assert.True(consumerSampleZh.IndexOf("插件信任契约 v1", StringComparison.Ordinal) < consumerSampleZh.IndexOf("## 它证明什么", StringComparison.Ordinal));
        Assert.True(consumerSampleZh.IndexOf("Beta Support Bundle", StringComparison.Ordinal) < consumerSampleZh.IndexOf("## 它证明什么", StringComparison.Ordinal));
    }

    private static void AssertContains(string contents, string expected)
        => Assert.Contains(expected, contents, StringComparison.Ordinal);

    private static bool HasLineWith(string contents, string first, string second)
        => contents.Split('\n')
            .Any(line => line.Contains(first, StringComparison.Ordinal) && line.Contains(second, StringComparison.Ordinal));

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
