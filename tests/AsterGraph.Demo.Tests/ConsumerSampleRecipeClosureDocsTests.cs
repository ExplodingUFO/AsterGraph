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
        var readmeCopySeams = ExtractBlock(readme, "### Copy These Host-Owned Seams", "### Replace These Sample-Owned Details");
        var consumerSampleCopySeamsEn = ExtractBlock(consumerSampleEn, "## Copy These Host-Owned Seams", "## Replace These Sample-Owned Details");
        var consumerSampleCopySeamsZh = ExtractBlock(consumerSampleZh, "## 复制这些宿主自管 seam", "## 替换这些样例自有内容");

        AssertContains(readme, "canonical session/runtime route");
        AssertContains(readme, "action rail / command projection");
        AssertContains(readme, "plugin trust workflow");
        AssertContains(readme, "selected-node parameter read/write seam");
        AssertContains(readme, "AsterGraphHostedActionFactory.CreateCommandActions(...)");
        AssertContains(readme, "AsterGraphHostedActionFactory.CreateProjection(...)");
        AssertContains(readme, "GraphEditorPluginDiscoveryOptions");
        AssertContains(readme, "AsterGraphEditorOptions.PluginTrustPolicy");
        AssertContains(readme, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(readme, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(readmeCopySeams, "selected-node parameter read/write seam");
        AssertContains(readmeCopySeams, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(readmeCopySeams, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
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
        AssertContains(consumerSampleEn, "selected-node parameter read/write seam");
        AssertContains(consumerSampleEn, "Plugin Manifest and Trust Policy Contract v1");
        AssertContains(consumerSampleEn, "Beta Support Bundle");
        AssertContains(consumerSampleEn, "AsterGraphHostedActionFactory.CreateCommandActions(...)");
        AssertContains(consumerSampleEn, "AsterGraphHostedActionFactory.CreateProjection(...)");
        AssertContains(consumerSampleEn, "GraphEditorPluginDiscoveryOptions");
        AssertContains(consumerSampleEn, "AsterGraphEditorOptions.PluginTrustPolicy");
        AssertContains(consumerSampleEn, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(consumerSampleEn, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
        AssertContains(consumerSampleCopySeamsEn, "selected-node parameter read/write seam");
        AssertContains(consumerSampleCopySeamsEn, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(consumerSampleCopySeamsEn, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
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
        AssertContains(consumerSampleCopySeamsZh, "选中节点参数读写 seam");
        AssertContains(consumerSampleCopySeamsZh, "IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()");
        AssertContains(consumerSampleCopySeamsZh, "IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)");
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
    public void AuthoringInspectorRecipeClosureDocs_AlignCanonicalInspectorVocabularyAcrossSampleGuidance()
    {
        var authoringRecipeEn = ReadRepoFile("docs/en/authoring-inspector-recipe.md");
        var authoringRecipeZh = ReadRepoFile("docs/zh-CN/authoring-inspector-recipe.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");

        AssertContains(authoringRecipeEn, "Canonical Recipe Vocabulary");
        AssertContains(authoringRecipeEn, "`defaultValue` seeds new nodes and fallback projection");
        AssertContains(authoringRecipeEn, "`isAdvanced` keeps expert-only parameters collapsed by default");
        AssertContains(authoringRecipeEn, "`helpText` adds short inline guidance next to the field");
        AssertContains(authoringRecipeEn, "`placeholderText` provides short input hints for text-oriented editors");
        AssertContains(authoringRecipeEn, "read-only reasons are shown explicitly when the host or definition locks a field");
        AssertContains(authoringRecipeZh, "统一的 recipe 词汇");
        AssertContains(authoringRecipeZh, "`defaultValue` 作为新节点和投影回退值");
        AssertContains(authoringRecipeZh, "`isAdvanced` 让高级参数默认保持折叠");
        AssertContains(authoringRecipeZh, "`helpText` 在字段旁提供简短说明");
        AssertContains(authoringRecipeZh, "`placeholderText` 为文本型 editor 提供简短输入提示");
        AssertContains(authoringRecipeZh, "只读原因会在宿主或定义锁定字段时明确显示");

        AssertContains(consumerSampleEn, "copyable inspector recipe");
        AssertContains(consumerSampleEn, "shipped definition-driven inspector");
        AssertContains(consumerSampleEn, "defaultValue");
        AssertContains(consumerSampleEn, "isAdvanced");
        AssertContains(consumerSampleEn, "helpText");
        AssertContains(consumerSampleEn, "placeholderText");
        AssertContains(consumerSampleEn, "read-only reasons");
        AssertContains(consumerSampleZh, "可复制的 inspector recipe");
        AssertContains(consumerSampleZh, "shipped definition-driven inspector");
        AssertContains(consumerSampleZh, "defaultValue");
        AssertContains(consumerSampleZh, "isAdvanced");
        AssertContains(consumerSampleZh, "helpText");
        AssertContains(consumerSampleZh, "placeholderText");
        AssertContains(consumerSampleZh, "只读原因");
        AssertContains(readme, "copyable inspector recipe");
        AssertContains(readme, "shipped definition-driven inspector");
        AssertContains(readme, "defaultValue");
        AssertContains(readme, "isAdvanced");
        AssertContains(readme, "helpText");
        AssertContains(readme, "placeholderText");
        AssertContains(readme, "read-only reasons");
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

        AssertAppearsBefore(consumerSampleEn, "Plugin Manifest and Trust Policy Contract v1", "## What It Proves");
        AssertAppearsBefore(consumerSampleEn, "Beta Support Bundle", "## What It Proves");
        AssertAppearsBefore(consumerSampleZh, "插件信任契约 v1", "## 它证明什么");
        AssertAppearsBefore(consumerSampleZh, "Beta Support Bundle", "## 它证明什么");
    }

    [Fact]
    public void ConsumerSampleRecipeClosureDocs_UseOneCopyableIntakeRecordWithoutHardcodedBundleNames()
    {
        var readme = ReadRepoFile("tools/AsterGraph.ConsumerSample.Avalonia/README.md");
        var consumerSampleEn = ReadRepoFile("docs/en/consumer-sample.md");
        var consumerSampleZh = ReadRepoFile("docs/zh-CN/consumer-sample.md");
        var supportBundleEn = ReadRepoFile("docs/en/support-bundle.md");
        var supportBundleZh = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var adoptionFeedbackEn = ReadRepoFile("docs/en/adoption-feedback.md");
        var adoptionFeedbackZh = ReadRepoFile("docs/zh-CN/adoption-feedback.md");
        var quickReferenceHeadingEn = "## Trust and proof quick reference";
        var quickReferenceHeadingZh = "## 信任与证明速查";
        var nextBetaIntakeHeadingEn = "Next beta intake links:";
        var nextBetaIntakeHeadingZh = "下一步 beta intake 文档：";
        var proofMarkerHeadingEn = "Expected proof markers:";
        var proofMarkerHeadingZh = "预期 proof marker：";
        var bundleMarkerHeadingEn = "Expected bundle markers when `--support-bundle <support-bundle-path>` is supplied:";
        var bundleMarkerHeadingZh = "当提供 `--support-bundle <support-bundle-path>` 时，预期 bundle marker：";

        foreach (var contents in new[] { readme, consumerSampleEn, consumerSampleZh, supportBundleEn, supportBundleZh })
        {
            Assert.Contains("--support-bundle", contents, StringComparison.Ordinal);
            Assert.DoesNotContain("artifacts/consumer-support-bundle.json", contents, StringComparison.Ordinal);
        }

        var quickReferenceSectionEn = ExtractBlock(consumerSampleEn, quickReferenceHeadingEn, nextBetaIntakeHeadingEn);
        var quickReferenceSectionZh = ExtractBlock(consumerSampleZh, quickReferenceHeadingZh, nextBetaIntakeHeadingZh);
        var quickReferenceProofBlockEn = ExtractBlock(quickReferenceSectionEn, proofMarkerHeadingEn, bundleMarkerHeadingEn);
        var quickReferenceProofBlockZh = ExtractBlock(quickReferenceSectionZh, proofMarkerHeadingZh, bundleMarkerHeadingZh);
        var quickReferenceBundleBlockEn = ExtractBlock(quickReferenceSectionEn, bundleMarkerHeadingEn, "The support bundle stays local evidence only.");
        var quickReferenceBundleBlockZh = ExtractBlock(quickReferenceSectionZh, bundleMarkerHeadingZh, "support bundle 只保留本地证据，不会扩大 support 边界。");

        Assert.Contains("CONSUMER_SAMPLE_TRUST_OK:True", quickReferenceProofBlockEn, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK:True", quickReferenceProofBlockEn, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:*", quickReferenceProofBlockEn, StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_OK", quickReferenceProofBlockEn, StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_PATH", quickReferenceProofBlockEn, StringComparison.Ordinal);

        Assert.Contains("CONSUMER_SAMPLE_TRUST_OK:True", quickReferenceProofBlockZh, StringComparison.Ordinal);
        Assert.Contains("COMMAND_SURFACE_OK:True", quickReferenceProofBlockZh, StringComparison.Ordinal);
        Assert.Contains("HOST_NATIVE_METRIC:*", quickReferenceProofBlockZh, StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_OK", quickReferenceProofBlockZh, StringComparison.Ordinal);
        Assert.DoesNotContain("SUPPORT_BUNDLE_PATH", quickReferenceProofBlockZh, StringComparison.Ordinal);

        Assert.Contains("SUPPORT_BUNDLE_OK:True", quickReferenceBundleBlockEn, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", quickReferenceBundleBlockEn, StringComparison.Ordinal);
        Assert.DoesNotContain("COMMAND_SURFACE_OK", quickReferenceBundleBlockEn, StringComparison.Ordinal);

        Assert.Contains("SUPPORT_BUNDLE_OK:True", quickReferenceBundleBlockZh, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BUNDLE_PATH:...", quickReferenceBundleBlockZh, StringComparison.Ordinal);
        Assert.DoesNotContain("COMMAND_SURFACE_OK", quickReferenceBundleBlockZh, StringComparison.Ordinal);

        foreach (var contents in new[] { consumerSampleEn, consumerSampleZh })
        {
            var bundleHeading = contents.Contains(bundleMarkerHeadingZh, StringComparison.Ordinal) ? bundleMarkerHeadingZh : bundleMarkerHeadingEn;
            var proofHeading = contents.Contains(bundleMarkerHeadingZh, StringComparison.Ordinal) ? proofMarkerHeadingZh : proofMarkerHeadingEn;
            var proofHandoffHeading = "## Proof Handoff";
            var nextHeading = contents.Contains("## When To Use This Sample", StringComparison.Ordinal)
                ? "## When To Use This Sample"
                : "## 什么时候看它";
            var proofHandoffSection = ExtractBlock(contents, proofHandoffHeading, nextHeading);
            var proofBlock = ExtractBlock(proofHandoffSection, proofHeading, bundleHeading);
            var bundleBlock = ExtractBlock(proofHandoffSection, bundleHeading, "## ");

            Assert.Contains("CONSUMER_SAMPLE_HOST_ACTION_OK:True", proofBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("SUPPORT_BUNDLE_OK", proofBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("SUPPORT_BUNDLE_PATH", proofBlock, StringComparison.Ordinal);

            Assert.Contains("SUPPORT_BUNDLE_OK", bundleBlock, StringComparison.Ordinal);
            Assert.Contains("SUPPORT_BUNDLE_PATH", bundleBlock, StringComparison.Ordinal);
            Assert.Contains("CONSUMER_SAMPLE_OK", bundleBlock, StringComparison.Ordinal);
        }

        Assert.Contains("attachment note", supportBundleEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("附件备注", supportBundleZh, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(adoptionFeedbackEn, "route", "version", "proof", "friction", "support-bundle attachment note"));
        Assert.True(HasLineWithAll(adoptionFeedbackZh, "route", "version", "proof", "摩擦", "support bundle", "附件备注"));
        Assert.Contains("Persona", adoptionFeedbackEn, StringComparison.Ordinal);
        Assert.Contains("maintainer-derived", adoptionFeedbackEn, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Requested next capability", adoptionFeedbackEn, StringComparison.Ordinal);
        Assert.Contains("维护者综合出来的结论", adoptionFeedbackZh, StringComparison.Ordinal);
    }

    private static void AssertContains(string contents, string expected)
        => Assert.Contains(expected, contents, StringComparison.Ordinal);

    private static bool HasLineWith(string contents, string first, string second)
        => contents.Split('\n')
            .Any(line => line.Contains(first, StringComparison.Ordinal) && line.Contains(second, StringComparison.Ordinal));

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

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

    private static void AssertAppearsBefore(string contents, string requiredText, string requiredHeading)
    {
        var textIndex = contents.IndexOf(requiredText, StringComparison.Ordinal);
        var headingIndex = contents.IndexOf(requiredHeading, StringComparison.Ordinal);

        Assert.True(textIndex >= 0, $"Expected to find '{requiredText}'.");
        Assert.True(headingIndex >= 0, $"Expected to find '{requiredHeading}'.");
        Assert.True(textIndex < headingIndex, $"Expected '{requiredText}' to appear before '{requiredHeading}'.");
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
