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

        AssertConsumerSampleSide(readme);
        AssertConsumerSampleSide(consumerSampleEn);
        AssertConsumerSampleSide(consumerSampleZh);

        AssertHostSeamCopyBlock(readmeCopySeams);
        AssertHostSeamCopyBlock(consumerSampleCopySeamsEn);
        AssertHostSeamCopyBlock(consumerSampleCopySeamsZh);

        AssertContains(readme, "Copy These Host-Owned Seams");
        AssertContains(readme, "Replace These Sample-Owned Details");
        AssertContains(consumerSampleEn, "Copy These Host-Owned Seams");
        AssertContains(consumerSampleEn, "Replace These Sample-Owned Details");
        AssertContains(consumerSampleZh, "复制这些宿主自管 seam");
        AssertContains(consumerSampleZh, "替换这些样例自有内容");

        AssertContains(consumerSampleEn, "Plugin Manifest and Trust Policy Contract v1");
        AssertContains(consumerSampleEn, "Beta Support Bundle");
        AssertContains(consumerSampleZh, "插件信任契约 v1");
        AssertContains(consumerSampleZh, "Beta Support Bundle");

        AssertContains(consumerSampleEn, "The hosted route ladder is `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`.");
        AssertContains(consumerSampleEn, "`HostSample` is the post-ladder proof harness.");
        AssertContains(consumerSampleZh, "这条 hosted route ladder 是 `Starter.Avalonia -> HelloWorld.Avalonia -> ConsumerSample.Avalonia`。");
        AssertContains(consumerSampleZh, "`HostSample` 是这条 ladder 之后的 proof harness。");

        Assert.True(HasLineWith(consumerSampleEn, "AsterGraph.ConsumerSample.Avalonia -- --proof", "first"));
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

        AssertAuthoringRecipeSide(authoringRecipeEn, authoringRecipeZh);
        AssertContains(consumerSampleEn, "[Authoring Inspector Recipe](./authoring-inspector-recipe.md)");
        AssertContains(consumerSampleZh, "[Authoring Inspector Recipe](./authoring-inspector-recipe.md)");
        AssertContains(readme, "Authoring Inspector Recipe");
        AssertContains(readme, "../../docs/en/authoring-inspector-recipe.md");
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
        AssertContains(consumerSampleEn, "reuse the emitted `SUPPORT_BUNDLE_PATH:...` line as the attachment note");
        AssertContains(consumerSampleEn, "./support-bundle.md");
        AssertContains(consumerSampleEn, "./adoption-feedback.md");
        AssertContains(consumerSampleEn, "./public-launch-checklist.md");

        AssertContains(consumerSampleZh, "信任与证明速查");
        AssertContains(consumerSampleZh, "可复制的信任与证明参考");
        AssertContains(consumerSampleZh, "SUPPORT_BUNDLE_OK:True");
        AssertContains(consumerSampleZh, "CONSUMER_SAMPLE_TRUST_OK:True");
        AssertContains(consumerSampleZh, "把输出里的 `SUPPORT_BUNDLE_PATH:...` 作为附件备注");
        AssertContains(consumerSampleZh, "./support-bundle.md");
        AssertContains(consumerSampleZh, "./adoption-feedback.md");
        AssertContains(consumerSampleZh, "./public-launch-checklist.md");

        AssertContains(supportBundleEn, "Local evidence only");
        AssertContains(supportBundleEn, "Copyable local evidence reference");
        ConsumerSampleDocsAssertions.AssertSupportBundleProofMarkers(supportBundleEn);
        AssertContains(supportBundleZh, "仅限本地证据");
        AssertContains(supportBundleZh, "可复制的本地证据参考");
        ConsumerSampleDocsAssertions.AssertSupportBundleProofMarkers(supportBundleZh);

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

    private static void AssertConsumerSampleSide(string contents)
    {
        Assert.Contains("Authoring Inspector Recipe", contents, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.ConsumerSample.Avalonia", contents, StringComparison.Ordinal);
        Assert.True(
            contents.Contains("host-owned", StringComparison.OrdinalIgnoreCase) ||
            contents.Contains("宿主自管", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("seam", contents, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertHostSeamCopyBlock(string contents)
    {
        Assert.True(
            contents.Contains("selected-node parameter read/write seam", StringComparison.OrdinalIgnoreCase) ||
            contents.Contains("选中节点参数读写 seam", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("IGraphEditorSession.Queries.GetSelectedNodeParameterSnapshots()", contents, StringComparison.Ordinal);
        Assert.Contains("IGraphEditorSession.Commands.TrySetSelectedNodeParameterValue(...)", contents, StringComparison.Ordinal);
    }

    private static void AssertAuthoringRecipeSide(string authoringRecipeEn, string authoringRecipeZh)
    {
        Assert.Contains("Canonical Recipe Vocabulary", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("Copyable Definition Example", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("defaultValue", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("helpText", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("placeholderText", authoringRecipeEn, StringComparison.Ordinal);
        Assert.Contains("constraints.IsReadOnly", authoringRecipeEn, StringComparison.Ordinal);

        Assert.Contains("统一的 recipe 词汇", authoringRecipeZh, StringComparison.Ordinal);
        Assert.Contains("可复制的定义示例", authoringRecipeZh, StringComparison.Ordinal);
        Assert.Contains("ConsumerSample.Avalonia", authoringRecipeZh, StringComparison.Ordinal);
        Assert.Contains("defaultValue", authoringRecipeZh, StringComparison.Ordinal);
        Assert.Contains("helpText", authoringRecipeZh, StringComparison.Ordinal);
        Assert.Contains("placeholderText", authoringRecipeZh, StringComparison.Ordinal);
        Assert.Contains("constraints.IsReadOnly", authoringRecipeZh, StringComparison.Ordinal);
    }

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
